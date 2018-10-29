using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesSettingParser.Utils;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// The metadata for the setting and some methods shared by both instance and static settings.
	/// </summary>
	/// <typeparam name="TPropertyValue"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public abstract class SettingMetadataBase<TPropertyValue, TValue> : IValueParser<TValue>, ISettingMetadata
	{
		/// <inheritdoc />
		public IReadOnlyCollection<string> Names { get; }
		/// <inheritdoc />
		public string MainName { get; }
		/// <inheritdoc />
		public string Description { get; set; }
		/// <inheritdoc />
		public string Information
		{
			get
			{
				var help = $"{MainName}: {Description}";
				if (typeof(TValue).IsEnum)
				{
					help += $"{Environment.NewLine}Acceptable values: {string.Join(", ", Enum.GetNames(typeof(TValue)))}";
				}
				return help;
			}
		}
		/// <inheritdoc />
		public bool IsFlag
		{
			get => _IsFlag;
			set
			{
				if (value && typeof(TValue) != typeof(bool))
				{
					throw new InvalidOperationException($"The generic type of setting must be bool if {nameof(IsFlag)} is set as true.");
				}
				_IsFlag = value;
			}
		}
		/// <inheritdoc />
		public bool IsOptional { get; set; }
		/// <inheritdoc />
		public virtual bool IsHelp => false;
		/// <inheritdoc />
		public bool IsAssociatedWithParser { get; private set; }
		/// <inheritdoc />
		public bool CannotBeNull { get; set; }
		/// <inheritdoc />
		public bool HasBeenSet { get; protected set; }
		/// <inheritdoc />
		public virtual bool UnescapeBeforeSetting => typeof(TValue) == typeof(string);
		/// <inheritdoc />
		public int? Group { get; set; }
		/// <inheritdoc />
		public Type TargetType => typeof(TPropertyValue);
		/// <inheritdoc />
		public Type ValueType => typeof(TValue);
		/// <inheritdoc />
		public IEqualityComparer<TValue> EqualityComparer
		{
			get => _EqualityComparer;
			set => _EqualityComparer = value ?? throw new ArgumentException(nameof(EqualityComparer));
		}
		/// <inheritdoc />
		public TryParseDelegate<TValue> Parser
		{
			get => _Parser;
			set => _Parser = value ?? throw new ArgumentException(nameof(Parser));
		}
		/// <inheritdoc />
		public Func<TValue, IResult> Validation
		{
			get => _Validation;
			set => _Validation = value ?? throw new ArgumentException(nameof(Validation));
		}
		/// <summary>
		/// Does something with the current value then returns either the modified value or a new value.
		/// </summary>
		public Func<TPropertyValue, TPropertyValue> ResetValueFactory
		{
			get => _ResetValueFactory;
			set => _ResetValueFactory = value ?? throw new ArgumentException(nameof(ResetValueFactory));
		}

		private int _ParserHash { get; set; }
		private bool _IsFlag { get; set; }
		private IEqualityComparer<TValue> _EqualityComparer { get; set; } = EqualityComparer<TValue>.Default;
		private TryParseDelegate<TValue> _Parser { get; set; }
		private Func<TValue, IResult> _Validation { get; set; } = x => Result.FromSuccess("Successfully validated");
		private Func<TPropertyValue, TPropertyValue> _ResetValueFactory { get; set; } = x => x;

		/// <summary>
		/// Creates an instance of <see cref="SettingMetadataBase{TPropertyValue, TValue}"/>.
		/// </summary>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public SettingMetadataBase(IEnumerable<string> names, TryParseDelegate<TValue> parser = default)
		{
			if (!names.Any())
			{
				throw new ArgumentException("Must supply at least one name.");
			}

			var immutable = names.ToImmutableArray();
			Names = immutable;
			MainName = immutable[0];
			_Parser = parser ?? TryParserRegistry.Instance.Retrieve<TValue>();
		}

		/// <summary>
		/// Sets the value after validation and sets HasBeenSet to true;
		/// </summary>
		/// <param name="value"></param>
		/// <param name="setter"></param>
		protected virtual void SetValue(TValue value, Action<TValue> setter)
		{
			this.ThrowIfInvalid(value);
			setter(value);
			HasBeenSet = true;
		}
		/// <summary>
		/// Attempts to set the value.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="context"></param>
		/// <param name="setter"></param>
		/// <returns></returns>
		protected virtual IResult TrySetValue(string value, ITrySetValueContext context, Action<TValue> setter)
		{
			var args = UnescapeBeforeSetting ? value.Replace("\\\"", "\"") : value;
			var convertResult = this.TryConvertValue(args, out var result);
			if (!convertResult.IsSuccess)
			{
				return convertResult;
			}
			setter(result);
			HasBeenSet = true;
			return SetValueResult.FromSuccess(this, result, "Successfully set.");
		}
		/// <inheritdoc />
		public void AssociateParser(ISettingParser parser)
		{
			var parserHash = parser.GetHashCode();
			if (parserHash == _ParserHash)
			{
				return;
			}
			if (IsAssociatedWithParser)
			{
				throw new InvalidOperationException("Cannot associate this setting with multiple parsers.");
			}
			IsAssociatedWithParser = true;
			_ParserHash = parserHash;
		}
		/// <inheritdoc />
		public override string ToString()
			=> $"{MainName} ({typeof(TValue).Name})";
	}
}