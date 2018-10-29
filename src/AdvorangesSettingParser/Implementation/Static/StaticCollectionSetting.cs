using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesSettingParser.Utils;

namespace AdvorangesSettingParser.Implementation.Static
{
	/// <summary>
	/// A generic class for a setting which is a collection, allowing full getter and modification but no real setter capabilities on the target.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class StaticCollectionSetting<TSource, TValue> : StaticSettingBase<TSource, ICollection<TValue>, TValue>
	{
		private Func<TSource, ICollection<TValue>> _Getter { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="StaticCollectionSetting{TSource, TValue}"/>.
		/// </summary>
		/// <param name="selector">The targeted value. If this doesn't have a setter it will still work.</param>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public StaticCollectionSetting(
			Expression<Func<TSource, ICollection<TValue>>> selector,
			IEnumerable<string> names = default,
			TryParseDelegate<TValue> parser = default)
			: base(SettingParsingUtils.DistinctNames(selector.GetMemberExpression().Member.Name, names), parser)
		{
			_Getter = selector?.Compile() ?? throw new ArgumentException(nameof(selector));
			ResetValueFactory = x => { x.Clear(); return x; }; //Clear the list when reset by default
			HasBeenSet = true; //Set to prevent it from stopping things
		}

		/// <inheritdoc />
		public override ICollection<TValue> GetValue(TSource source)
			=> _Getter(source);
		/// <summary>
		/// This will invoke the reset value factory then the setter, meaning to clear the list simply have the factory return an empty list.
		/// </summary>
		/// <param name="source"></param>
		public override void ResetValue(TSource source)
			=> SetValue(source, ResetValueFactory(GetValue(source)));
		/// <summary>
		/// Clears the source list and adds all the supplied values.
		/// Will clear the collection before adding the values, but will not change the reference.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value"></param>
		public override void SetValue(TSource source, ICollection<TValue> value)
			=> this.SetCollection(GetValue(source), value);
		/// <inheritdoc />
		public override IResult TrySetValue(TSource source, string value)
			=> TrySetValue(source, value, new CollectionModificationContext { Action = CollectionModificationAction.Toggle });
		/// <inheritdoc />
		public override IResult TrySetValue(TSource source, string value, ITrySetValueContext context)
		{
			if (!(context is CollectionModificationContext modificationContext))
			{
				return SettingContextResult.FromError(this, typeof(CollectionModificationContext), context?.GetType(), "Invalid context provided.");
			}
			return TrySetValue(source, value, modificationContext);
		}
		/// <summary>
		/// Invokes TrySetValue with the correct context type.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public IResult TrySetValue(TSource source, string value, CollectionModificationContext context)
			=> this.TrySetValue(value, context, () => GetValue(source));
	}
}