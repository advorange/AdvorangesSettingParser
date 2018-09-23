using System.Collections.Generic;
using AdvorangesSettingParser.Interfaces;

namespace AdvorangesSettingParser.Implementation.Instance
{
	/// <summary>
	/// Base class of an instance setting.
	/// Allows easy implementation of either a singular property or a collection property.
	/// /// </summary>
	/// <typeparam name="TPropertyValue">This can be the same type as <typeparamref name="TValue"/>.</typeparam>
	/// <typeparam name="TValue">This can be the same type as <typeparamref name="TPropertyValue"/>.</typeparam>
	public abstract class SettingBase<TPropertyValue, TValue>
		: SettingMetadataBase<TPropertyValue, TValue>, ISetting<TPropertyValue>, ISetting
	{
		/// <summary>
		/// Creates an instance of <see cref="SettingBase{TSource, TValue}"/>.
		/// </summary>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public SettingBase(IEnumerable<string> names, TryParseDelegate<TValue> parser = default)
			: base(names, parser) { }

		/// <inheritdoc />
		public abstract TPropertyValue GetValue();
		/// <inheritdoc />
		public abstract void ResetValue();
		/// <inheritdoc />
		public abstract void SetValue(TPropertyValue value);
		/// <inheritdoc />
		public abstract IResult TrySetValue(string value);
		/// <inheritdoc />
		public abstract IResult TrySetValue(string value, ITrySetValueContext context);

		//ISetting
		object ISetting.GetValue()
			=> GetValue();
		void ISetting.SetValue(object value)
			=> SetValue((TPropertyValue)value);
	}
}
