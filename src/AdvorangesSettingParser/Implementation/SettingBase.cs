using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvorangesUtils;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Abstract implementation of a setting.
	/// </summary>
	/// <typeparam name="TSource">This can be the same type as <typeparamref name="TValue"/>.</typeparam>
	/// <typeparam name="TValue">This can be the same type as <typeparamref name="TSource"/>.</typeparam>
	public abstract class SettingBase<TSource, TValue> : ISetting<TSource>, ISetting
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
		/// <inheritdoc />
		public int MaxParamCount { get; set; } = 1;
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
		public Func<TSource, TSource> ResetValueFactory
		{
			get => _ResetValueFactory;
			set => _ResetValueFactory = value ?? throw new ArgumentException(nameof(ResetValueFactory));
		}
		/// <summary>
		/// Sets the default value for this setting;
		/// </summary>
		public TSource DefaultValue
		{
			get => _DefaultValue;
			set => SetValue(_DefaultValue = value);
		}

		private bool _IsFlag { get; set; }
		private TryParseDelegate<TValue> _Parser { get; set; }
		private Func<TValue, IResult> _Validation { get; set; } = x => Result.FromSuccess("Successfully validated");
		private Func<TSource, TSource> _ResetValueFactory { get; set; } = x => x;
		private TSource _DefaultValue { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="SettingBase{TSource, TValue}"/>.
		/// </summary>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public SettingBase(IEnumerable<string> names, TryParseDelegate<TValue> parser)
		{
			if (!names.Any())
			{
				throw new ArgumentException("Must supply at least one name.");
			}

			Names = names.ToImmutableArray();
			MainName = names.First();
			IsHelp = names.Any(x => x.CaseInsEquals("help") || x.CaseInsEquals("h"));
			_Parser = parser ?? SettingParsingUtils.GetParser<TValue>();
		}

		/// <inheritdoc />
		public abstract TSource GetValue();
		/// <inheritdoc />
		public abstract void ResetValue();
		/// <inheritdoc />
		public abstract void SetValue(TSource value);
		/// <inheritdoc />
		public abstract bool TrySetValue(string value, out IResult response);
		/// <inheritdoc />
		public abstract bool TrySetValue(string value, ITrySetValueContext context, out IResult response);
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
		protected virtual bool TryConvertValue(string value, out TValue result, out IResult response)
		{
			result = default;
			response = default;
			//Let default just pass on through and set the default value
			//
			if (!value.CaseInsEquals("default") && !Parser(value, out result))
			{
				response = SetValueResult.FromError(this, value, "Unable to convert.");
				return false;
			}
			if (result == null && CannotBeNull)
			{
				response = SetValueResult.FromError(this, value, "Cannot be set to 'NULL'.");
				return false;
			}

			response = Validation(result);
			return response?.IsSuccess ?? true;
		}
		/// <summary>
		/// Distincts the names into a single enumerable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="aliases"></param>
		/// <returns></returns>
		protected static IEnumerable<string> DistinctNames(string name, IEnumerable<string> aliases)
		{
			//Distinct is not 100% guaranteed to be in the passed in order, GroupBy is
			var concatted = new[] { name }.Concat(aliases ?? Enumerable.Empty<string>());
			return concatted.Where(x => x != null).GroupBy(x => x).Select(x => x.First());
		}

		//IDirectGetter
		object IDirectGetter.GetValue()
			=> GetValue();
		//IDirectSetter
		void IDirectSetter.SetValue(object value)
			=> SetValue((TSource)value);
	}
}
