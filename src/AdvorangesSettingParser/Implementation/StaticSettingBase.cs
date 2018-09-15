using System;
using System.Collections.Generic;
using AdvorangesSettingParser.Interfaces;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Abstract implementation of a static setting.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	/// <typeparam name="TPropertyValue"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public abstract class StaticSettingBase<TSource, TPropertyValue, TValue>
		: SettingMetadataBase<TPropertyValue, TValue>, IStaticSetting<TSource, TPropertyValue>, IStaticSetting<TSource>
	{
		/// <summary>
		/// Sets the default value for this setting;
		/// </summary>
		public Func<TPropertyValue> DefaultValueFactory
		{
			get => _DefaultValueFactory;
			set => _DefaultValueFactory = value;
		}

		private Func<TPropertyValue> _DefaultValueFactory { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="StaticSettingBase{TSource, TPropertyValue, TValue}"/>.
		/// </summary>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public StaticSettingBase(IEnumerable<string> names, TryParseDelegate<TValue> parser = default)
			: base(names, parser) { }

		/// <inheritdoc />
		public abstract TPropertyValue GetValue(TSource source);
		/// <inheritdoc />
		public abstract void ResetValue(TSource source);
		/// <inheritdoc />
		public abstract void SetValue(TSource source, TPropertyValue value);
		/// <inheritdoc />
		public abstract IResult TrySetValue(TSource source, string value);
		/// <inheritdoc />
		public abstract IResult TrySetValue(TSource source, string value, ITrySetValueContext context);

		//IStaticSetting
		object IStaticSetting<TSource>.GetValue(TSource source) => GetValue(source);
		void IStaticSetting<TSource>.SetValue(TSource source, object value) => SetValue(source, (TPropertyValue)value);
	}
}