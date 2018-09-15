using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// A generic class for a setting, allowing full getter and setter capabilities on the target.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public class Setting<TValue> : SettingBase<TValue, TValue>, ISetting<TValue>, ISetting
	{
		private IRef<TValue> _Ref { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="Setting{T}"/> with full setter and getter capabilities targeting the supplied value.
		/// </summary>
		/// <param name="selector">The targeted value. This can be any property/field local/instance/global/static in a class. It NEEDS to have both a getter and setter.</param>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public Setting(Expression<Func<TValue>> selector, IEnumerable<string> names = default, TryParseDelegate<TValue> parser = default)
			: this(new Ref<TValue>(selector), names, parser) { }
		/// <summary>
		/// Creates an instance of <see cref="Setting{T}"/> with full setter and getter capabilities targeting the supplied value.
		/// </summary>
		/// <param name="reference">The targeted value. This can be any property/field local/instance/global/static in a class.</param>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public Setting(IRef<TValue> reference, IEnumerable<string> names = default, TryParseDelegate<TValue> parser = default)
			: base(SettingParsingUtils.DistinctNames(reference.Name, names), parser)
		{
			_Ref = reference ?? throw new ArgumentException(nameof(reference));
		}

		/// <inheritdoc />
		public override TValue GetValue()
			=> _Ref.GetValue();
		/// <inheritdoc />
		public override void ResetValue()
			=> SetValue(ResetValueFactory(GetValue()));
		/// <inheritdoc />
		public override void SetValue(TValue value)
		{
			ThrowIfInvalid(value);
			_Ref.SetValue(value);
			HasBeenSet = true;
		}
		/// <inheritdoc />
		public override IResult TrySetValue(string value)
			=> TrySetValue(value, null);
		/// <inheritdoc />
		public override IResult TrySetValue(string value, ITrySetValueContext context)
		{
			var convertResult = TryConvertValue(value, out var result);
			if (!convertResult.IsSuccess)
			{
				return convertResult;
			}
			SetValue(result);
			return SetValueResult.FromSuccess(this, result, "Successfully set.");
		}

		//ISetting
		object ISetting.GetValue() => GetValue();
		void ISetting.SetValue(object value) => SetValue((TValue)value);
	}
}