using System;
using System.Collections.Generic;
using AdvorangesSettingParser.Implementation;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Utils
{
	/// <summary>
	/// Methods for putting a setting and its arguments together.
	/// </summary>
	public static class ArgumentMappingUtils
	{
		/// <summary>
		/// Returns a dictionary of names and their values. Names are only counted if they begin with a passed in prefix.
		/// </summary>
		/// <param name="prefixes"></param>
		/// <param name="throwQuoteError">Whether to throw if there are mismatched quotes after parsing.</param>
		/// <param name="input"></param>
		/// <returns></returns>
		public static IEnumerable<(string Setting, string Args)> Parse(IEnumerable<string> prefixes, bool throwQuoteError, ParseArgs input)
		{
			return CreateArgMap(input, throwQuoteError, (string s, out string result) =>
			{
				foreach (var prefix in prefixes)
				{
					if (s.StartsWith(prefix))
					{
						result = Deprefix(prefix, s, PrefixState.Required);
						return true;
					}
				}
				result = null;
				return false;
			});
		}
		/// <summary>
		/// Maps each setting of type <typeparamref name="TValue"/> to a value.
		/// Can map the same setting multiple times to different values, but will all be in the passed in order.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="args"></param>
		/// <param name="throwQuoteError">Whether to throw if there are mismatched quotes after parsing.</param>
		/// <param name="tryParser"></param>
		/// <returns></returns>
		public static IEnumerable<(TValue Setting, string Args)> CreateArgMap<TValue>(
			ParseArgs args,
			bool throwQuoteError,
			TryParseDelegate<TValue> tryParser)
		{
			string AddArgs(ref string current, string n)
				=> current += current != null ? $" {n}" : n;

			//Something like -FieldInfo "-Name "Test Value" -Text TestText" (with or without quotes around the whole thing)
			//Should parse into:
			//-FieldInfo
			//	-Name
			//		Test Value
			//	-Text
			//		TestText
			//Which with the current data structure should be (FieldInfo, "-Name "Test Value" -Text TestText")
			//then when those get parsed would turn into (Name, Test Value) and (Text, TestText)
			var currentSetting = default(TValue);
			var currentArgs = default(string);
			var quoteDeepness = 0;
			for (int i = 0; i < args.Length; ++i)
			{
				var fullCurrentValue = args[i];
				var (StartsWithQuotes, EndsWithQuotes, Value) = TrimSingle(fullCurrentValue, ParseArgs.QuoteChars);

				if (StartsWithQuotes) { ++quoteDeepness; }
				if (EndsWithQuotes) { --quoteDeepness; }
				//New setting found, so return current values and reset state for next setting
				if (quoteDeepness == 0)
				{
					//When this is not a setting we add the args onto the current args then return the current values instantly
					//We can't return Value directly because if there were other quote deepness args we need to count those.
					if (!tryParser(Value, out var setting))
					{
						//Trim quotes off the end of a setting value since they're not needed to group anything together anymore
						//And they just complicate future parsing
						var settingValue = TrimSingle(AddArgs(ref currentArgs, fullCurrentValue), ParseArgs.QuoteChars).Value;
						yield return (currentSetting, settingValue);
					}
					//When this is a setting we only check if there's a setting from the last iteration
					//If there is, we send that one because there's a chance it could be a parameterless setting
					else if (currentSetting != null)
					{
						yield return (currentSetting, currentArgs);
					}
					currentSetting = setting;
					currentArgs = null;
					continue;
				}

				//If inside any quotes at all, keep adding until we run out of args
				AddArgs(ref currentArgs, fullCurrentValue);
			}
			//Return any leftover parts which haven't been returned yet
			if (currentSetting != default || currentArgs != null)
			{
				yield return (currentSetting, TrimSingle(currentArgs, ParseArgs.QuoteChars).Value);
			}
			if (quoteDeepness > 0 && throwQuoteError)
			{
				throw new InvalidOperationException("Some quotes were not closed correctly and messed up the parsing.");
			}
		}
		/// <summary>
		/// Removes a single character from the supplied characters from the start and end.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="quoteChars"></param>
		/// <returns></returns>
		public static (bool Start, bool End, string Value) TrimSingle(string s, IEnumerable<char> quoteChars)
		{
			if (s == null)
			{
				return (false, false, null);
			}

			var start = false;
			foreach (var c in quoteChars)
			{
				if (s.StartsWith(c.ToString()))
				{
					s = s.Substring(1);
					start = true;
					break;
				}
			}
			var end = false;
			foreach (var c in quoteChars)
			{
				if (s.EndsWith(c.ToString()))
				{
					s = s.Substring(0, s.Length - 1);
					end = true;
					break;
				}
			}
			return (start, end, s);
		}
		/// <summary>
		/// Removes the prefix from the start of the string if it exists.
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="input"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public static string Deprefix(string prefix, string input, PrefixState state)
		{
			switch (state)
			{
				case PrefixState.Required:
					return input.CaseInsStartsWith(prefix) ? input.Substring(prefix.Length) : null;
				case PrefixState.Optional:
					return input.CaseInsStartsWith(prefix) ? input.Substring(prefix.Length) : input;
				case PrefixState.NotPrefixed:
					return input;
				default:
					throw new InvalidOperationException("Invalid prefix state provided.");
			}
		}
	}
}