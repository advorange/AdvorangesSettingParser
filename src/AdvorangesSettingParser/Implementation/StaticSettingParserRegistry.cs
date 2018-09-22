using System;
using System.Collections.Generic;
using AdvorangesSettingParser.Implementation.Static;
using AdvorangesSettingParser.Interfaces;
using AdvorangesUtils;

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

		/// <summary>
		/// The types registered in this instance.
		/// </summary>
		public IEnumerable<Type> RegisteredTypes => _SettingParsers.Keys;

		private IDictionary<Type, object> _SettingParsers { get; } = new Dictionary<Type, object>();

		/// <summary>
		/// Registers the current try parser so it can be used upon request.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parser"></param>
		public void Register<T>(StaticSettingParser<T> parser) where T : class
			=> _SettingParsers[typeof(T)] = parser;
		/// <summary>
		/// Removes the try parser for the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Remove<T>() where T : class
			=> _SettingParsers.Remove(typeof(T));
		/// <summary>
		/// Retries the try parser for the spcified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public StaticSettingParser<T> Retrieve<T>() where T : class
			=> TryRetrieve<T>(out var value) ? value : throw new KeyNotFoundException($"There is no setting parser registered for {typeof(T).Name}.");
		/// <summary>
		/// Attempts to retrieve the try parser for the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryRetrieve<T>(out StaticSettingParser<T> value) where T : class
		{
			var valid = _SettingParsers.TryGetValue(typeof(T), out var stored);
			value = valid ? (StaticSettingParser<T>)stored : default;
			return valid;
		}
	}
}
