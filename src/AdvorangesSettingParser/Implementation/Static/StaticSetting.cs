using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Utils;

namespace AdvorangesSettingParser.Implementation.Static
{
	/// <summary>
	/// A generic class for a static setting, allowing full getter and setter capabilities on the target with any passed in source.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class StaticSetting<TSource, TValue> : StaticSettingBase<TSource, TValue, TValue>
	{
		private Func<TSource, TValue> _Getter { get; set; }
		private Action<TSource, TValue> _Setter { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="StaticSetting{TSource, TValue}"/> with full setter and getter capabilities targeting the supplied value.
		/// </summary>
		/// <param name="selector">The targeted value. This can be any property/field local/instance/global/static in a class. It NEEDS to have both a getter and setter.</param>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public StaticSetting(Expression<Func<TSource, TValue>> selector, IEnumerable<string> names = default, TryParseDelegate<TValue> parser = default)
			: base(SettingParsingUtils.DistinctNames(selector.GetMemberExpression().Member.Name, names), parser)
		{
			_Getter = selector.Compile();
			_Setter = selector.GenerateSetter();
		}

		/// <inheritdoc />
		public override TValue GetValue(TSource source)
			=> _Getter(source);
		/// <inheritdoc />
		public override void ResetValue(TSource source)
			=> SetValue(source, ResetValueFactory(GetValue(source)));
		/// <inheritdoc />
		public override void SetValue(TSource source, TValue value)
			=> SetValue(value, x => _Setter(source, x));
		/// <inheritdoc />
		public override IResult TrySetValue(TSource source, string value)
			=> TrySetValue(source, value, null);
		/// <inheritdoc />
		public override IResult TrySetValue(TSource source, string value, ITrySetValueContext context)
			=> TrySetValue(value, context, x => SetValue(source, x));
	}
}