using System;
using System.Collections.Generic;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Registers try parsers for easy retrieval.
	/// </summary>
	public class TryParserRegistry
	{
		/// <summary>
		/// The singleton instance of this.
		/// </summary>
		public static TryParserRegistry Instance { get; } = new TryParserRegistry();

		private IDictionary<Type, object> _TryParsers { get; } = new Dictionary<Type, object>();

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
		public void Register<T>(TryParseDelegate<T> tryParser)
			=> _TryParsers[typeof(T)] = tryParser;
		/// <summary>
		/// Removes the try parser for the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Remove<T>()
			=> _TryParsers.Remove(typeof(T));
		/// <summary>
		/// Retries the try parser for the spcified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public TryParseDelegate<T> Retrieve<T>()
			=> TryRetrieve<T>(out var value) ? value : throw new KeyNotFoundException($"There is no try parser registered for {typeof(T).Name}.");
		/// <summary>
		/// Attempts to retrieve the try parser for the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryRetrieve<T>(out TryParseDelegate<T> value)
		{
			if (_TryParsers.TryGetValue(typeof(T), out var stored))
			{
				value = (TryParseDelegate<T>)stored;
				return true;
			}
			if (typeof(T).IsEnum)
			{
				var newParser = new TryParseDelegate<T>(EnumTryParse);
				Register(newParser);
				value = newParser;
				return true;
			}
			value = default;
			return false;
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
}