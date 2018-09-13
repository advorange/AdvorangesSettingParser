using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// A generic class for a setting, allowing full getter and setter capabilities on the target.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Setting<T> : SettingBase<T, T>, ISetting<T>, ISetting
	{
		private IRef<T> _Ref { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="Setting{T}"/> with full setter and getter capabilities targeting the supplied value.
		/// </summary>
		/// <param name="selector">The targeted value. This can be any property/field local/instance/global/static in a class/struct. It NEEDS to have both a getter and setter.</param>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public Setting(Expression<Func<T>> selector, IEnumerable<string> names = default, TryParseDelegate<T> parser = default)
			: this(new Ref<T>(selector), names, parser) { }
		/// <summary>
		/// Creates an instance of <see cref="Setting{T}"/> with full setter and getter capabilities targeting the supplied value.
		/// </summary>
		/// <param name="reference">The targeted value. This can be any property/field local/instance/global/static in a class/struct.</param>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public Setting(IRef<T> reference, IEnumerable<string> names = default, TryParseDelegate<T> parser = default)
			: base(DistinctNames(reference.Name, names), parser)
		{
			_Ref = reference ?? throw new ArgumentException(nameof(reference));
		}

		/// <inheritdoc />
		public override T GetValue()
			=> _Ref.GetValue();
		/// <inheritdoc />
		public override void ResetValue()
			=> SetValue(ResetValueFactory(GetValue()));
		/// <inheritdoc />
		public override void SetValue(T value)
		{
			if (!Validation(value))
			{
				throw new ArgumentException($"Validation failed for {MainName} with supplied value {value}.");
			}
			_Ref.SetValue(value);
			HasBeenSet = true;
		}
		/// <inheritdoc />
		public override bool TrySetValue(string value, out IResult response)
			=> TrySetValue(value, null, out response);
		/// <inheritdoc />
		public override bool TrySetValue(string value, ITrySetValueContext context, out IResult response)
		{
			if (!TryConvertValue(value, out var result, out response))
			{
				return false;
			}

			SetValue(result);
			response = SettingValueResult.FromSuccess(this, typeof(T), result, "Successfully set.");
			return true;
		}

		//ISetting
		void IDirectSetter.SetValue(object value) => SetValue((T)value);
	}
}