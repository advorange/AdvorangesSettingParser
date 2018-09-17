using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Base class for parsing settings.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class SettingParserBase<T> : ISettingParser, ICollection<T> where T : ISettingMetadata
	{
		/// <summary>
		/// The default prefixes used for setting parsing.
		/// </summary>
		public static IEnumerable<string> DefaultPrefixes { get; } = new[] { "-", "--", "/" }.ToImmutableArray();

		/// <inheritdoc />
		public IEnumerable<string> Prefixes { get; }
		/// <inheritdoc />
		public bool IsReadOnly => false;
		/// <inheritdoc />
		public int Count => SettingMap.Count;

		/// <summary>
		/// Maps the names to a Guid.
		/// </summary>
		protected Dictionary<string, Guid> NameMap { get; } = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		/// <summary>
		/// Maps the Guid from <see cref="NameMap"/> to a setting.
		/// </summary>
		protected Dictionary<Guid, T> SettingMap { get; } = new Dictionary<Guid, T>();

		/// <summary>
		/// Creates an instance of <see cref="SettingParserBase{T}"/> with the supplied prefixes.
		/// </summary>
		/// <param name="prefixes"></param>
		public SettingParserBase(IEnumerable<string> prefixes = default)
		{
			Prefixes = (prefixes ?? DefaultPrefixes).ToImmutableArray();
		}

		/// <summary>
		/// Returns a dictionary of names and their values. Names are only counted if they begin with a passed in prefix.
		/// </summary>
		/// <param name="prefixes"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public static IEnumerable<(string Setting, string Args)> Parse(IEnumerable<string> prefixes, ParseArgs input)
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
		/// Maps each setting of type <typeparamref name="TValue"/> to a value.
		/// Can map the same setting multiple times to different values, but will all be in the passed in order.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="args"></param>
		/// <param name="tryParser"></param>
		/// <returns></returns>
		public static IEnumerable<(TValue Setting, string Args)> CreateArgMap<TValue>(ParseArgs args, TryParseDelegate<TValue> tryParser)
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
				var (StartsWithQuotes, EndsWithQuotes, Value) = TrimSingle(args[i], ParseArgs.DefaultQuotes);

				if (StartsWithQuotes) { ++quoteDeepness; }
				if (EndsWithQuotes) { --quoteDeepness; }
				//New setting found, so return current values and reset state for next setting
				if (quoteDeepness == 0)
				{
					//When this is not a setting we add the args onto the current args then return the current values instantly
					//We can't return Value directly because if there were other quote deepness args we need to count those.
					if (!tryParser(Value, out var setting))
					{
						yield return (currentSetting, AddArgs(ref currentArgs, Value));
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
				AddArgs(ref currentArgs, Value);
			}
			//Return any leftover parts which haven't been returned yet
			if (currentSetting != default || currentArgs != null)
			{
				yield return (currentSetting, currentArgs);
			}
		}
		private static (bool Start, bool End, string Value) TrimSingle(string s, IEnumerable<char> quoteChars)
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
		private static string Deprefix(string prefix, string input, PrefixState state)
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
		/// <summary>
		/// Abstract and protected to handle implementation from an instance and static context.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		protected abstract ISettingParserResult Parse(object source, ParseArgs input);
		/// <summary>
		/// Attempts to get the setting with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="state"></param>
		/// <param name="setting"></param>
		/// <returns></returns>
		public bool TryGetSetting(string name, PrefixState state, out T setting)
		{
			bool InternalTryGetSetting(string deprefixedName, out T returned)
			{
				var valid = NameMap.TryGetValue(deprefixedName, out var guid);
				returned = valid ? SettingMap[guid] : default;
				return valid;
			}

			foreach (var prefix in Prefixes)
			{
				if (Deprefix(prefix, name, state) is string deprefixedName && InternalTryGetSetting(deprefixedName, out setting))
				{
					return true;
				}
			}
			setting = default;
			return false;
		}
		/// <summary>
		/// Gets the setting with the specified name and throws if it's not found.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public T GetSetting(string name, PrefixState state)
			=> TryGetSetting(name, state, out var temp) ? temp : throw new KeyNotFoundException($"There is no setting with the registered name {name}.");
		/// <summary>
		/// Parses through all the text and then handles any setting.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="setter"></param>
		/// <returns></returns>
		public virtual ISettingParserResult Parse(ParseArgs input, Func<T, string, IResult> setter)
		{
			var unusedParts = new List<IResult>();
			var successes = new List<IResult>();
			var errors = new List<IResult>();
			var help = new List<HelpResult>();
			var argMap = CreateArgMap(input, (string s, out T result) => TryGetSetting(s, PrefixState.Required, out result));
			foreach (var (Setting, Args) in argMap)
			{
				if (Setting == null)
				{
					unusedParts.Add(Result.FromError(Args));
					continue;
				}

				var args = Setting.IsFlag && Args == null ? bool.TrueString : Args;
				var response = setter(Setting, args);
				if (response.IsSuccess)
				{
					successes.Add(response);
				}
				else
				{
					errors.Add(response);
				}
			}
			foreach (var success in successes.ToList())
			{
				if (success is HelpResult helpResult)
				{
					successes.Remove(success);
					help.Add(helpResult);
				}
			}
			return new SettingParserResult(unusedParts, successes, errors, help);
		}
		/// <inheritdoc />
		public void Add(T setting)
		{
			var guid = Guid.NewGuid();
			foreach (var name in setting.Names)
			{
				NameMap.Add(name, guid);
			}
			SettingMap.Add(guid, setting);
		}
		/// <inheritdoc />
		public bool Remove(T setting)
		{
			if (!NameMap.TryGetValue(setting.MainName, out var guid))
			{
				return false;
			}
			foreach (var name in setting.Names)
			{
				NameMap.Remove(name);
			}
			return SettingMap.Remove(guid);
		}
		/// <inheritdoc />
		public bool Contains(T setting)
			=> SettingMap.Values.Contains(setting);
		/// <inheritdoc />
		public void Clear()
		{
			NameMap.Clear();
			SettingMap.Clear();
		}
		/// <inheritdoc />
		public void CopyTo(T[] array, int index)
			=> SettingMap.Values.CopyTo(array, index);
		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
			=> SettingMap.Values.GetEnumerator();
		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
			=> SettingMap.Values.GetEnumerator();

		//ISettingParser
		IEnumerable<ISettingMetadata> ISettingParser.GetSettings()
			=> this.OfType<ISettingMetadata>().Where(x => !(x is HelpCommand));
		bool ISettingParser.TryGetSetting(string name, PrefixState state, out ISettingMetadata setting)
		{
			var success = TryGetSetting(name, state, out var temp);
			setting = temp;
			return success;
		}
		ISettingMetadata ISettingParser.GetSetting(string name, PrefixState state)
			=> GetSetting(name, state);
		ISettingParserResult ISettingParser.Parse(object source, ParseArgs input)
			=> Parse(source, input);
	}
}