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
		/// Attempts to parse the supplied arguments into the specified class through a parser registered in <see cref="StaticSettingParserRegistry"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParseStaticSetting<T>(string s, out T result) where T : class, new()
		{
			if (!ParseArgs.TryParse(s, out var args))
			{
				result = default;
				return false;
			}

			var instance = new T();
			var parser = StaticSettingParserRegistry.Instance.Retrieve<T>();
			var response = parser.Parse(instance, args);
			var isSuccess = response.IsSuccess && !parser.GetNeededSettings(instance).Any();
			result = isSuccess ? instance : default;
			return isSuccess;
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
		/// <summary>
		/// Attempts to parse an absolue url.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParseUri(string s, out Uri result)
			=> Uri.TryCreate(s, UriKind.Absolute, out result);
		/// <summary>
		/// Acts as an empty <see cref="TryParseDelegate{T}"/> always returning true and the default value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static bool TryParseEmpty<T>(string _, out T value)
		{
			value = default;
			return true;
		}
		/// <summary>
		/// Functionally the exact same as <see cref="TryParseEmpty{T}(string, out T)"/> except this leaves a warning to actually implement it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[Obsolete("This try parser is only temporary and should have a valid implementation supplied at some point.")]
		public static bool TryParseTemporary<T>(string s, out T value)
			=> TryParseEmpty(s, out value);
	}
}