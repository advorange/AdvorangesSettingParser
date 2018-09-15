using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// The metadata for the setting and some methods shared by both instance and static settings.
	/// </summary>
	/// <typeparam name="TPropertyValue"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public abstract class SettingMetadataBase<TPropertyValue, TValue> : ISettingMetadata
	{
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
		public bool CannotBeNull { get; set; }
		/// <inheritdoc />
		public bool HasBeenSet { get; protected set; }
		/// <inheritdoc />
		public bool IsHelp { get; }
		/// <inheritdoc />
		public IEnumerable<string> Names { get; }
		/// <inheritdoc />
		public string MainName { get; }
		/// <summary>
		/// Parses the type from a string.
		/// </summary>
		public TryParseDelegate<TValue> Parser
		{
			get => _Parser;
			set => _Parser = value ?? throw new ArgumentException(nameof(Parser));
		}
		/// <summary>
		/// Validates the passed in value.
		/// </summary>
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

		private bool _IsFlag { get; set; }
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

			Names = names.ToImmutableArray();
			MainName = names.First();
			_Parser = parser ?? TryParserRegistry.Instance.Retrieve<TValue>();
		}

		/// <inheritdoc />
		public override string ToString() => $"{MainName} ({typeof(TValue).Name})";
		/// <summary>
		/// Validates the value and throws if it's invalid.
		/// </summary>
		/// <param name="value"></param>
		protected void ThrowIfInvalid(TValue value)
		{
			var validationResult = Validation(value);
			if (!validationResult.IsSuccess)
			{
				throw new ArgumentException(validationResult.ToString(), MainName);
			}
		}
		/// <summary>
		/// Attempts to convert the value.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		protected virtual IResult TryConvertValue(string value, out TValue result)
		{
			result = default;
			//Let default just pass on through and set the default value
			if (!value.CaseInsEquals("default") && !Parser(value, out result))
			{
				return SetValueResult.FromError(this, typeof(TValue), value, "Unable to convert.");
			}
			if (result == null && CannotBeNull)
			{
				return SetValueResult.FromError(this, typeof(TValue), value, "Cannot be set to 'NULL'.");
			}

			return Validation(result) ?? Result.FromSuccess("Successfully converted");
		}
	}
}