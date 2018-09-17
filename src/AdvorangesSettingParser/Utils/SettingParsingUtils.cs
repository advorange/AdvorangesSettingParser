using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Utils
{
	/// <summary>
	/// Extension methods for <see cref="SettingParserBase{T}"/>.
	/// </summary>
	public static class SettingParsingUtils
	{
		/// <summary>
		/// Formats a string describing what settings still need to be set.
		/// </summary>
		/// <param name="settings">The parser.</param>
		/// <returns>A description of what settings still need to be set.</returns>
		public static string FormatNeededSettings<T>(this IEnumerable<T> settings) where T : ISettingMetadata
		{
			var unsetArguments = settings.GetNeededSettings();
			if (!unsetArguments.Any())
			{
				return $"Every setting which is necessary has been set.";
			}
			var sb = new StringBuilder("The following settings need to be set:" + Environment.NewLine);
			foreach (var setting in unsetArguments)
			{
				sb.AppendLine($"\t{setting.ToString()}");
			}
			return sb.ToString().Trim() + Environment.NewLine;
		}
		/// <summary>
		/// Returns information either about the settings in general, or the specified setting.
		/// </summary>
		/// <param name="parser"></param>
		/// <param name="name">The setting to target. Can be null if wanting to list out every setting.</param>
		/// <returns>Help information about either the setting or all settings.</returns>
		public static IResult GetHelp(this ISettingParser parser, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				var values = parser.GetSettings().Select(x => x.Names.Count() < 2 ? x.MainName : $"{x.MainName} ({string.Join(", ", x.Names.Skip(1))})");
				return new HelpResult($"All Settings:{Environment.NewLine}\t{string.Join($"{Environment.NewLine}\t", values)}");
			}
			return parser.TryGetSetting(name, PrefixState.NotPrefixed, out var setting)
				? new HelpResult(setting.Information)
				: Result.FromError($"'{name}' is not a valid setting.");
		}
		/// <summary>
		/// Returns settings which have not been set and are not optional.
		/// </summary>
		/// <param name="settings">The parser.</param>
		/// <returns>The settings which still need to be set.</returns>
		public static IEnumerable<T> GetNeededSettings<T>(this IEnumerable<T> settings) where T : ISettingMetadata
			=> settings.Where(x => !(x.HasBeenSet || x.IsOptional));
		/// <summary>
		/// Returns true if every setting is either set or optional.
		/// </summary>
		/// <param name="settings">The parser.</param>
		/// <returns>Whether every setting has been set.</returns>
		public static bool AreAllSet<T>(this IEnumerable<T> settings) where T : ISettingMetadata
			=> settings.All(x => x.HasBeenSet || x.IsOptional);
		/// <summary>
		/// Gets the member expression from <paramref name="expression"/>.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static MemberExpression GetMemberExpression(this LambdaExpression expression)
			=> expression.Body as MemberExpression ?? throw new ArgumentException($"Supplied expression is not a {nameof(MemberExpression)}.");
		private static TSetter GenerateSetter<TGetter, TSetter>(
			this Expression<TGetter> selector,
			Func<MemberExpression, TSetter> setterGenerator)
		{
			var body = selector.GetMemberExpression();
			body.Member.DeclaringType.ThrowIfStruct(body.Member.Name);
			switch (body.Member)
			{
				case PropertyInfo property:
				case FieldInfo field:
					try
					{
						return setterGenerator(body);
					}
					catch (ArgumentException e)
					{
						throw new ArgumentException("Target is readonly or has no setter.", body.Member.Name, e);
					}
				default:
					throw new ArgumentException("Can only target properties and fields.", body.Member.Name);
			}
		}
		/// <summary>
		/// Generates a setter method from a getter expression. If there is no setter with will throw.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static Action<TValue> GenerateSetter<TValue>(this Expression<Func<TValue>> selector)
		{
			return selector.GenerateSetter(x =>
			{
				var valueExp = Expression.Parameter(typeof(TValue));
				var assignExp = Expression.Assign(x, valueExp);
				return Expression.Lambda<Action<TValue>>(assignExp, valueExp).Compile();
			});
		}
		/// <summary>
		/// Generates a setter method from a getter expression. If there is no setter this will throw.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static Action<TSource, TValue> GenerateSetter<TSource, TValue>(this Expression<Func<TSource, TValue>> selector)
		{
			return selector.GenerateSetter(x =>
			{
				var sourceExpr = Expression.Parameter(typeof(TSource));
				var valueExp = Expression.Parameter(typeof(TValue));
				var propertyExp = Expression.PropertyOrField(sourceExpr, x.Member.Name);
				var assignExp = Expression.Assign(propertyExp, valueExp);
				return Expression.Lambda<Action<TSource, TValue>>(assignExp, sourceExpr, valueExp).Compile();
			});
		}
		/// <summary>
		/// Throws if the type is a struct.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		public static void ThrowIfStruct(this Type type, string name = null)
		{
			if (type.IsValueType)
			{
				throw new ArgumentException($"Cannot target values on a struct ({type.Name}).", name ?? "Anonymous");
			}
		}
		/// <summary>
		/// Distincts the names into a single enumerable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="aliases"></param>
		/// <returns></returns>
		public static IEnumerable<string> DistinctNames(string name, IEnumerable<string> aliases)
		{
			//Distinct is not 100% guaranteed to be in the passed in order, GroupBy is
			var concatted = new[] { name }.Concat(aliases ?? Enumerable.Empty<string>());
			return concatted.Where(x => x != null).GroupBy(x => x).Select(x => x.First());
		}
		/// <summary>
		/// Gets the setting parser either registered for the type or in the type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="registry"></param>
		/// <param name="source"></param>
		/// <param name="parsableFirst"></param>
		/// <returns></returns>
		public static ISettingParser GetSettingParser<T>(this StaticSettingParserRegistry registry, T source, bool parsableFirst = true)
		{
			if (parsableFirst)
			{
				if (source is IParsable parsable)
				{
					return parsable.SettingParser;
				}
				if (registry.TryRetrieve<T>(out var value))
				{
					return value;
				}
			}
			else
			{
				if (registry.TryRetrieve<T>(out var value))
				{
					return value;
				}
				if (source is IParsable parsable)
				{
					return parsable.SettingParser;
				}
			}
			throw new KeyNotFoundException($"There is no setting parser registered for {typeof(T).Name}.");
		}
		/// <summary>
		/// If <paramref name="source"/> is <see cref="IParsable"/> will use that parser, otherwise searches for a registered parser.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="registry"></param>
		/// <param name="source"></param>
		/// <param name="args"></param>
		/// <param name="parsableFirst">Whether to check for the object being parsable first before trying to get the registered setting parser.</param>
		/// <returns></returns>
		public static ISettingParserResult Parse<T>(this StaticSettingParserRegistry registry, T source, ParseArgs args, bool parsableFirst = true)
			=> registry.GetSettingParser(source).Parse(source, args);
	}
}