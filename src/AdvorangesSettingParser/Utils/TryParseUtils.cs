using System;
using System.IO;
using System.Linq;
using AdvorangesSettingParser.Implementation;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Utils
{
	/// <summary>
	/// Implementations for some try parses which are not already existent by default.
	/// </summary>
	public static class TryParseUtils
	{
		/// <summary>
		/// Try parser for any static setting parser registered type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParseStaticSetting<T>(string s, out T result) where T : new()
		{
			result = new T();
			var parser = StaticSettingParserRegistry.Instance.Retrieve<T>();
			var response = parser.Parse(result, s);
			return !response.Errors.Any() && !response.UnusedParts.Any() && parser.AreAllSet();
		}
		/// <summary>
		/// Returns true if this directory has a valid name, returns false if not.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParseDirectoryInfo(string s, out DirectoryInfo result)
		{
			try
			{
				result = new DirectoryInfo(s);
				if (!result.Exists)
				{
					result.Create();
				}
				return true;
			}
			//Catch all invalid input exceptions
			//Let all invalid operation exceptions throw (ioexception, securityexception)
			catch (Exception e) when (e is ArgumentNullException || e is ArgumentException || e is PathTooLongException)
			{
				result = default;
				return false;
			}
		}
		/// <summary>
		/// Attempts to parse the datetime and then set it to UTC.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParseUTCDateTime(string s, out DateTime result)
		{
			var valid = DateTime.TryParse(s, out var temp);
			result = valid ? temp.ToUniversalTime() : default;
			return valid;
		}
		/// <summary>
		/// Attempts to parse an enum.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryParseEnum<T>(string s, out T value)
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