using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdvorangesSettingParser.Utils;

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
		private static MethodInfo _RegMethod { get; } = typeof(TryParserRegistry).GetMethods().Single(x => x.Name == nameof(RegisterNullable));

		/// <summary>
		/// The types registered in this instance.
		/// </summary>
		public IEnumerable<Type> RegisteredTypes
			=> _TryParsers.Keys;

		private IDictionary<Type, object> _TryParsers { get; } = new Dictionary<Type, object>();

		/// <summary>
		/// Creates an instance of <see cref="TryParserRegistry"/> and registers all primitive types.
		/// </summary>
		public TryParserRegistry()
		{
			RegisterNullable(new TryParseDelegate<sbyte>(sbyte.TryParse));
			RegisterNullable(new TryParseDelegate<byte>(byte.TryParse));
			RegisterNullable(new TryParseDelegate<short>(short.TryParse));
			RegisterNullable(new TryParseDelegate<ushort>(ushort.TryParse));
			RegisterNullable(new TryParseDelegate<int>(int.TryParse));
			RegisterNullable(new TryParseDelegate<uint>(uint.TryParse));
			RegisterNullable(new TryParseDelegate<long>(long.TryParse));
			RegisterNullable(new TryParseDelegate<ulong>(ulong.TryParse));
			RegisterNullable(new TryParseDelegate<char>(char.TryParse));
			RegisterNullable(new TryParseDelegate<float>(float.TryParse));
			RegisterNullable(new TryParseDelegate<double>(double.TryParse));
			RegisterNullable(new TryParseDelegate<bool>(bool.TryParse));
			RegisterNullable(new TryParseDelegate<decimal>(decimal.TryParse));
			Register(new TryParseDelegate<string>((string s, out string result) => { result = s; return true; }));
		}

		/// <summary>
		/// Registers the current try parser so it can be used upon request.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tryParser"></param>
		/// <param name="forceRegisterNullable">
		/// Forces <see cref="RegisterNullable{T}(TryParseDelegate{T})"/> to be called even if the generic parameters are not met.
		/// This uses reflection.
		/// </param>
		public void Register<T>(TryParseDelegate<T> tryParser, bool forceRegisterNullable = false)
		{
			if (!forceRegisterNullable || !typeof(T).IsValueType)
			{
				_TryParsers.Add(typeof(T), tryParser);
				return;
			}

			var genericMethod = _RegMethod.MakeGenericMethod(typeof(T));
			genericMethod.Invoke(this, new object[] { tryParser });
		}
		/// <summary>
		/// Registers the current try parser and its nullable form so it can be used upon request.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tryParser"></param>
		public void RegisterNullable<T>(TryParseDelegate<T> tryParser) where T : struct
		{
			_TryParsers.Add(typeof(T), tryParser);
			_TryParsers.Add(typeof(T?), new TryParseDelegate<T?>((string s, out T? result) =>
			{
				//If the value passed in is null, allow that since this is a nullable type.
				if (s == null)
				{
					result = null;
					return true;
				}

				//Otherwise, only allow successfully parsed values
				var success = tryParser(s, out var temp);
				result = success ? (T?)temp : null;
				return success;
			}));
		}
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
				var newParser = new TryParseDelegate<T>(TryParseUtils.TryParseEnum);
				Register(newParser);
				value = newParser;
				return true;
			}
			value = default;
			return false;
		}
	}
}