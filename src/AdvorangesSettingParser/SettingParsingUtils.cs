using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AdvorangesUtils;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Extension methods for <see cref="ISettingParser"/>.
	/// </summary>
	public static class SettingParsingUtils
	{
		/// <summary>
		/// Formats a string describing what settings still need to be set.
		/// </summary>
		/// <param name="parser">The parser.</param>
		/// <returns>A description of what settings still need to be set.</returns>
		public static string FormatNeededSettings(this ISettingParser parser)
		{
			var unsetArguments = parser.GetNeededSettings();
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
		/// Returns settings which have not been set and are not optional.
		/// </summary>
		/// <param name="parser">The parser.</param>
		/// <returns>The settings which still need to be set.</returns>
		public static IEnumerable<IBasicSetting> GetNeededSettings(this ISettingParser parser)
			=> parser.GetSettings().Where(x => !(x.HasBeenSet || x.IsOptional));
		/// <summary>
		/// Returns true if every setting is either set or optional.
		/// </summary>
		/// <param name="parser">The parser.</param>
		/// <returns>Whether every setting has been set.</returns>
		public static bool AreAllSet(this ISettingParser parser)
			=> parser.GetSettings().All(x => x.HasBeenSet || x.IsOptional);
		/// <summary>
		/// Attempts to get a parser for an enum. On failure will return null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static TryParseDelegate<T> TryGetEnumParser<T>()
		{
			bool EnumTryParse<TEnum>(string s, out TEnum value)
			{
				foreach (var name in Enum.GetNames(typeof(TEnum)))
				{
					if (name.CaseInsEquals(s))
					{
						value = (TEnum)Enum.Parse(typeof(TEnum), s, true);
						return true;
					}
				}
				value = default;
				return false;
			}

			return typeof(T).IsEnum ? new TryParseDelegate<T>(EnumTryParse) : null;
		}
		/// <summary>
		/// Attempts to get a parser for a primitive type. On failure will return null.
		/// </summary>
		/// <returns></returns>
		public static TryParseDelegate<T> TryGetPrimitiveParser<T>()
		{
			bool StringTryParse(string s, out string value)
			{
				value = s;
				return true;
			}

			switch (typeof(T).Name)
			{
				//I know this looks bad, but it's the easiest way to do this. It is still type safe anyways.
				case nameof(SByte):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<sbyte>(sbyte.TryParse);
				case nameof(Byte):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<byte>(byte.TryParse);
				case nameof(Int16):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<short>(short.TryParse);
				case nameof(UInt16):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<ushort>(ushort.TryParse);
				case nameof(Int32):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<int>(int.TryParse);
				case nameof(UInt32):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<uint>(uint.TryParse);
				case nameof(Int64):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<long>(long.TryParse);
				case nameof(UInt64):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<ulong>(ulong.TryParse);
				case nameof(Char):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<char>(char.TryParse);
				case nameof(Single):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<float>(float.TryParse);
				case nameof(Double):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<double>(double.TryParse);
				case nameof(Boolean):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<bool>(bool.TryParse);
				case nameof(Decimal):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<decimal>(decimal.TryParse);
				case nameof(String):
					//Instead of having to do special checks, just use this dumb delegate
					return (TryParseDelegate<T>)(object)new TryParseDelegate<string>(StringTryParse);
			}
			return null;
		}
		/// <summary>
		/// Checks for an enum parser, then a primitive parser, then throws an exception unless one was found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static TryParseDelegate<T> GetParser<T>()
		{
			return TryGetEnumParser<T>()
				?? TryGetPrimitiveParser<T>()
				?? throw new ArgumentException($"Unable to find a primitive converter for the supplied type {typeof(T).Name}.");
		}
		/// <summary>
		/// Gets the member expression from <paramref name="expression"/>.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static MemberExpression GetMemberExpression(this LambdaExpression expression)
			=> expression.Body as MemberExpression ?? throw new ArgumentException($"Supplied expression is not a {nameof(MemberExpression)}.");
		/// <summary>
		/// Generates a setter method from a getter expression. If there is no setter with will throw.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static Action<T> GenerateSetter<T>(this Expression<Func<T>> selector)
		{
			var body = selector.GetMemberExpression();
			switch (body.Member)
			{
				case PropertyInfo property:
				case FieldInfo field:
					try
					{
						var valueExp = Expression.Parameter(typeof(T));
						var assignExp = Expression.Assign(body, valueExp);
						return Expression.Lambda<Action<T>>(assignExp, valueExp).Compile();
					}
					catch (ArgumentException e)
					{
						throw new ArgumentException("Target is readonly or has no setter.", body.Member.Name, e);
					}
				default:
					throw new ArgumentException("Can only target properties and fields.", body.Member.Name);
			}
		}
	}
}