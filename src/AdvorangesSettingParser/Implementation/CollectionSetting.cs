using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdvorangesUtils;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// A generic class for a setting which is a collection, allowing full getter and modification but no setter capabilities on the target.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CollectionSetting<T> : SettingBase<ICollection<T>, T>
	{
		/// <summary>
		/// Equality comparer to use for checking for duplicates.
		/// </summary>
		public IEqualityComparer<T> EqualityComparer
		{
			get => _EqualityComparer;
			set => _EqualityComparer = value ?? throw new ArgumentException(nameof(EqualityComparer));
		}

		private IEqualityComparer<T> _EqualityComparer { get; set; } = EqualityComparer<T>.Default;
		private Func<ICollection<T>> _Getter { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="CollectionSetting{T}"/>.
		/// </summary>
		/// <param name="selector">The targeted value. If this doesn't have a setter it will still work.</param>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type or enum.</param>
		public CollectionSetting(
			Expression<Func<ICollection<T>>> selector,
			IEnumerable<string> names = default,
			TryParseDelegate<T> parser = default)
			: base(DistinctNames(selector.GetMemberExpression().Member.Name, names), parser)
		{
			_Getter = selector?.Compile() ?? throw new ArgumentException(nameof(selector));
			if (GetValue() == null)
			{
				throw new InvalidOperationException($"{MainName} must be initialized before being used in a setting.");
			}
			ResetValueFactory = x => { x.Clear(); return x; }; //Clear the list when reset by default
			MaxParamCount = 2; //For action then variable
			HasBeenSet = true; //Set to prevent it from stopping things
		}

		/// <inheritdoc />
		public override ICollection<T> GetValue() => _Getter();
		/// <summary>
		/// This will invoke the reset value factory then the setter, meaning to clear the list simply have the factory return an empty list.
		/// </summary>
		public override void ResetValue() => SetValue(ResetValueFactory(GetValue()));
		/// <summary>
		/// Clears the source list and adds all the supplied values.
		/// Will clear the collection before adding the values, but will not change the reference.
		/// </summary>
		/// <param name="value"></param>
		public override void SetValue(ICollection<T> value)
		{
			var source = GetValue();
			source.Clear();
			foreach (var val in value)
			{
				if (!Validation(val))
				{
					throw new ArgumentException($"Validation failed for {MainName} with supplied value {val}.");
				}
				source.Add(val);
			}
			HasBeenSet = true;
		}
		/// <summary>
		/// Attempts to modify the collection. Returns true if the collection was modified successfully.
		/// Returns false if the collection already had the value added or removed depending on what action was supplied.
		/// </summary>
		/// <param name="add"></param>
		/// <param name="value"></param>
		public bool ModifyCollection(CollectionModificationContext context, T value)
		{
			if (!Validation(value))
			{
				throw new ArgumentException($"Validation failed for {MainName} with supplied value {value}.");
			}

			var source = GetValue();
			switch (context.Action)
			{
				case CMAction.Toggle:
					var rCount = RemoveAll(source, value, context.MaxRemovalCount);
					if (rCount <= 0) //Only add if nothing was removed
					{
						source.Add(value);
					}
					return true;
				case CMAction.Add:
					source.Add(value);
					return true;
				case CMAction.AddIfMissing:
					var contains = source.Contains(value, EqualityComparer);
					if (!contains) //Only add if not contained in collection
					{
						source.Add(value);
					}
					return !contains;
				case CMAction.Remove:
					return RemoveAll(source, value, context.MaxRemovalCount) > 0; //Failure if removed nothing
				default:
					throw new ArgumentException("Invalid action supplied.", nameof(context));
			}
		}
		/// <inheritdoc />
		public override bool TrySetValue(string value, out IResult response)
			=> TrySetValue(value, new CollectionModificationContext { Action = CMAction.Toggle }, out response);
		/// <inheritdoc />
		public override bool TrySetValue(string value, ITrySetValueContext context, out IResult response)
		{
			if (!(context is CollectionModificationContext mod))
			{
				var text = "Invalid context provided.";
				response = SettingContextResult.FromSuccess(this, typeof(CollectionModificationContext), context?.GetType(), text);
				return false;
			}
			return TrySetValue(value, mod, out response);
		}
		/// <summary>
		/// Invokes TrySetValue with the correct context type.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="context"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public bool TrySetValue(string value, CollectionModificationContext context, out IResult response)
		{
			if (!TryGetCMAction(context, value, out value))
			{
				response = SettingValueResult.FromError(this, typeof(T), value, "Invalid arg count.");
				return false;
			}
			if (!TryConvertValue(value, out var result, out response))
			{
				return false;
			}

			var m = ModifyCollection(context, result);
			response = m
				? SettingValueResult.FromSuccess(this, typeof(T), result, $"Successfully {context.ActionString}.")
				: SettingValueResult.FromError(this, typeof(T), result, $"Already {context.ActionString}.");
			return m;
		}
		private bool TryGetCMAction(CollectionModificationContext context, string value, out string newString)
		{
			var split = value.Split(new[] { ' ' }, 2);
			var names = Enum.GetNames(typeof(CMAction)); //Do not allow numbers being parsed as the enum in case someone wants to add numbers to a list
			if (split.Length == 1)
			{
				newString = value;
				//Only return success if this only value is NOT CMAction
				//b/c if someone messes up quotes it would attempt to set this otherwise
				return !names.CaseInsContains(split[0]);
			}
			if (names.CaseInsContains(split[0]))
			{
				context.Action = (CMAction)Enum.Parse(typeof(CMAction), split[0], true);
				newString = split[1];
				return true;
			}
			newString = value;
			return false;
		}
		private int RemoveAll(ICollection<T> source, T value, int limit)
		{
			var rCount = 0;
			for (int i = source.Count - 1; i >= 0; --i)
			{
				var sourceValue = source.ElementAt(i);
				if (EqualityComparer.Equals(sourceValue, value))
				{
					source.Remove(sourceValue);
					++rCount;
				}
				if (rCount >= limit)
				{
					break;
				}
			}
			return rCount;
		}
	}
}