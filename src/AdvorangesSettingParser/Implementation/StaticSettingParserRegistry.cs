using System;
using System.Collections.Generic;
using AdvorangesSettingParser.Implementation.Static;
using AdvorangesSettingParser.Interfaces;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Registers setting parsers for easy retrieval.
	/// </summary>
	public class StaticSettingParserRegistry
	{
		/// <summary>
		/// The singleton instance of this.
		/// </summary>
		public static StaticSettingParserRegistry Instance { get; } = new StaticSettingParserRegistry();

		private IDictionary<Type, object> _SettingParsers { get; } = new Dictionary<Type, object>();

		/// <summary>
		/// Registers the current try parser so it can be used upon request.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parser"></param>
		public void Register<T>(StaticSettingParser<T> parser)
			=> _SettingParsers[typeof(T)] = parser;
		/// <summary>
		/// Removes the try parser for the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Remove<T>()
			=> _SettingParsers.Remove(typeof(T));
		/// <summary>
		/// Retries the try parser for the spcified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public StaticSettingParser<T> Retrieve<T>()
			=> TryRetrieve<T>(out var value) ? value : throw new KeyNotFoundException($"There is no setting parser registered for {typeof(T).Name}.");
		/// <summary>
		/// Attempts to retrieve the try parser for the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryRetrieve<T>(out StaticSettingParser<T> value)
		{
			var valid = _SettingParsers.TryGetValue(typeof(T), out var stored);
			value = valid ? (StaticSettingParser<T>)stored : default;
			return valid;
		}
		/// <summary>
		/// If <paramref name="source"/> is <see cref="IParsable"/> will use that parser, otherwise searches for a registered parser.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="args"></param>
		/// <param name="parsableFirst">Whether to check for the object being parsable first before trying to get the registered setting parser.</param>
		/// <returns></returns>
		public ISettingParserResult Parse<T>(T source, string args, bool parsableFirst = true)
		{
			if (parsableFirst)
			{
				return source is IParsable parsable ? parsable.SettingParser.Parse(args) : Retrieve<T>().Parse(source, args);
			}
			else
			{
				if (TryRetrieve<T>(out var value))
				{
					return value.Parse(source, args);
				}
				if (source is IParsable parsable)
				{
					return parsable.SettingParser.Parse(args);
				}
				throw new KeyNotFoundException($"There is no setting parser registered for {typeof(T).Name}.");
			}
		}
	}
}
