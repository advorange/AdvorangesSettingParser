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
	public abstract class SettingParserBase<T> : ICollection<T> where T : ISettingMetadata
	{
		/// <summary>
		/// The default prefixes used for setting parsing.
		/// </summary>
		public static IEnumerable<string> DefaultPrefixes { get; } = new[] { "-", "--", "/" }.ToImmutableArray();
		/// <summary>
		/// The values used for help.
		/// </summary>
		public static IEnumerable<string> Help { get; } = new[] { "help", "h" }.ToImmutableArray();

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
		/// Returns a dictionary of names and their values. Names are only counted if they begin with passed in a prefix.
		/// </summary>
		/// <param name="prefixes"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Dictionary<string, string> Parse(string[] prefixes, string input)
			=> Parse(prefixes, input.SplitLikeCommandLine());
		/// <summary>
		/// Returns a dictionary of names and their values. Names are only counted if they begin with a passed in prefix.
		/// </summary>
		/// <param name="prefixes"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Dictionary<string, string> Parse(string[] prefixes, string[] input)
		{
			bool HasPrefix(string[] p, string i)
			{
				return p.Any(x => i.CaseInsStartsWith(x));
			}

			var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < input.Length; ++i)
			{
				var part = input[i];
				if (!HasPrefix(prefixes, part))
				{
					continue;
				}

				//Part following is not a setting, so it must be the value.
				if (input.Length - 1 > i && !HasPrefix(prefixes, input[i + 1]))
				{
					dict.Add(part, input[++i]);
				}
				//Otherwise there is no value
				else
				{
					dict.Add(part, null);
				}
			}
			return dict;
		}
		/// <summary>
		/// Determines whether this value is the help command.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		protected bool IsHelp(string input)
		{
			foreach (var prefix in Prefixes)
			{
				if (Deprefix(prefix, input, PrefixState.Required) is string val && Help.CaseInsContains(val))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Determines if the current index leads to a valid setting.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="i"></param>
		/// <param name="setting"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected bool IsValidSetting(string[] input, ref int i, out T setting, out string value)
		{
			var isSetting = TryGetSetting(input[i], PrefixState.Required, out setting);
			//If it's a flag set its value to true then return
			//If it has a getter then check if it's set to toggle it
			if (isSetting && setting.IsFlag)
			{
				value = setting is ISetting<bool> getter && getter.GetValue() ? bool.FalseString : bool.TrueString;
				return true;
			}

			//If there's one more and it's not a setting assign that to value
			value = input.Length - 1 > i && !TryGetSetting(input[i + 1], PrefixState.Required, out _) ? input[++i] : null;
			return isSetting;
		}
		/// <summary>
		/// Parses through all the text and then handles any setting.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="setter"></param>
		/// <returns></returns>
		protected ISettingParserResult Parse(string[] input, Func<T, string, IResult> setter)
		{
			var unusedParts = new List<IResult>();
			var successes = new List<IResult>();
			var errors = new List<IResult>();
			var help = new List<IResult>();
			for (int i = 0; i < input.Length; ++i)
			{
				//No setting was gotten, so just skip this part unless it's help
				var part = input[i];
				if (!IsValidSetting(input, ref i, out var setting, out var value))
				{
					if (IsHelp(part))
					{
						help.Add(this.GetHelp(value));
						continue;
					}
					unusedParts.Add(Result.FromError(part));
					continue;
				}
				var response = setter(setting, value);
				if (response.IsSuccess)
				{
					successes.Add(response);
				}
				else
				{
					errors.Add(response);
				}
			}
			return new SettingParserResult(unusedParts, successes, errors, help);
		}
		/// <summary>
		/// Removes the prefix from the start depending on the prefix state supplied.
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="input"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		private string Deprefix(string prefix, string input, PrefixState state)
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
		/// Gets the setting with a matching name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="state"></param>
		/// <param name="setting"></param>
		/// <returns></returns>
		public bool TryGetSetting(string name, PrefixState state, out T setting)
		{
			foreach (var prefix in Prefixes)
			{
				if (Deprefix(prefix, name, state) is string val && NameMap.TryGetValue(val, out var guid))
				{
					setting = SettingMap[guid];
					return true;
				}
			}
			setting = default;
			return false;
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
	}
}