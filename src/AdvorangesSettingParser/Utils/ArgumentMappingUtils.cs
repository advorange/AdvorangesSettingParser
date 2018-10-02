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
		/// <param name="input"></param>
		/// <param name="prefixes"></param>
		/// <returns></returns>
		public static IEnumerable<(string Setting, string Args)> Parse(ParseArgs input, IEnumerable<string> prefixes)
		{
			return CreateArgMap(input, (string s, out string result) =>
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
		/// Maps each setting of type <typeparamref name="T"/> to a value.
		/// Can map the same setting multiple times to different values, but will all be in the passed in order.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="args"></param>
		/// <param name="tryParser"></param>
		/// <returns></returns>
		public static IEnumerable<(T Setting, string Args)> CreateArgMap<T>(ParseArgs args, TryParseDelegate<T> tryParser)
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
			var currentSetting = default(T);
			var currentArgs = default(string);
			foreach (var arg in args)
			{
				var (StartsWithQuotes, EndsWithQuotes, Value) = TrimSingle(arg, args);

				//When this is not a setting we add the args onto the current args then return the current values instantly
				//We can't return Value directly because if there were other quote deepness args we need to count those.
				if (!tryParser(Value, out var setting))
				{
					//Trim quotes off the end of a setting value since they're not needed to group anything together anymore
					//And they just complicate future parsing
					var settingValue = TrimSingle(AddArgs(ref currentArgs, arg), args).Value;
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
			}
			//Return any leftover parts which haven't been returned yet
			if (currentSetting != default || currentArgs != null)
			{
				yield return (currentSetting, TrimSingle(currentArgs, args).Value);
			}
		}
		/// <summary>
		/// Removes a single character from the supplied characters from the start and end.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static (bool HasStartQuote, bool HasEndQuote, string Value) TrimSingle(string s, ParseArgs args)
		{
			if (s == null)
			{
				return (false, false, null);
			}

			var start = false;
			foreach (var c in args.StartingQuoteCharacters)
			{
				if (s.StartsWith(c.ToString()))
				{
					s = s.Substring(1);
					start = true;
					break;
				}
			}
			var end = false;
			foreach (var c in args.EndingQuoteCharacters)
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