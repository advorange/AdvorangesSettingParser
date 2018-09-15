using System;
using System.Collections.Generic;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Registers parsers for easy retrieval.
	/// </summary>
	public class TryParserRegistry
	{
		/// <summary>
		/// The singleton instance of this.
		/// </summary>
		public static TryParserRegistry Instance { get; } = new TryParserRegistry();

		private IDictionary<Type, object> TryParsers { get; } = new Dictionary<Type, object>();

		/// <summary>
		/// Creates an instance of <see cref="TryParserRegistry"/> and registers all primitive types.
		/// </summary>
		public TryParserRegistry()
		{
			Register(new TryParseDelegate<sbyte>(sbyte.TryParse));
			Register(new TryParseDelegate<byte>(byte.TryParse));
			Register(new TryParseDelegate<short>(short.TryParse));
			Register(new TryParseDelegate<ushort>(ushort.TryParse));
			Register(new TryParseDelegate<int>(int.TryParse));
			Register(new TryParseDelegate<uint>(uint.TryParse));
			Register(new TryParseDelegate<long>(long.TryParse));
			Register(new TryParseDelegate<ulong>(ulong.TryParse));
			Register(new TryParseDelegate<char>(char.TryParse));
			Register(new TryParseDelegate<float>(float.TryParse));
			Register(new TryParseDelegate<double>(double.TryParse));
			Register(new TryParseDelegate<bool>(bool.TryParse));
			Register(new TryParseDelegate<decimal>(decimal.TryParse));
			Register(new TryParseDelegate<string>(StringTryParse));
		}

		/// <summary>
		/// Registers the current try parser so it can be used upon request.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tryParser"></param>
		public void Register<T>(TryParseDelegate<T> tryParser) => TryParsers[typeof(T)] = tryParser;
		/// <summary>
		/// Removes the try parser for the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Remove<T>() => TryParsers.Remove(typeof(T));
		/// <summary>
		/// Retries the try parser for the spcified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public TryParseDelegate<T> Retrieve<T>()
		{
			try
			{
				if (typeof(T).IsEnum)
				{
					if (TryParsers.TryGetValue(typeof(T), out var stored))
					{
						return (TryParseDelegate<T>)stored;
					}

					var newParser = new TryParseDelegate<T>(EnumTryParse);
					Register(newParser);
					return newParser;
				}

				return (TryParseDelegate<T>)TryParsers[typeof(T)];
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException($"There is no try parser registered for {typeof(T).Name}.");
			}
		}
		private static bool StringTryParse(string s, out string value)
		{
			value = s;
			return true;
		}
		private static bool EnumTryParse<T>(string s, out T value)
		{
			foreach (var name in Enum.GetNames(typeof(T)))
			{
				if (name.CaseInsEquals(s))
				{
					value = (T)Enum.Parse(typeof(T), s, true);
					return true;
				}
			}
			value = default;
			return false;
		}
	}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public static class Test
	{
		public static bool TryParseAClass(string s, out AClass result)
		{
			var instance = new AClass();
			var parser = new SettingParser
			{
				new Setting<string>(() => instance.Dog),
			};
			var valid = parser.AreAllSet();
			result = valid ? instance : default;
			return valid;
		}
	}

	public class AClass
	{
		public string Dog { get; set; }
	}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}