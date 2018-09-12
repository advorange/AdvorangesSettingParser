using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using AdvorangesUtils;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Parses settings and then sets them.
	/// This implementation is case-insensitive.
	/// </summary>
	/// <remarks>Reserved setting names: help, h (unless help command is not added)</remarks>
	public sealed class SettingParser : ISettingParser, ICollection<ISetting>
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
		public int Count => _SettingMap.Count;

		private readonly Dictionary<string, Guid> _NameMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<Guid, ISetting> _SettingMap = new Dictionary<Guid, ISetting>();

		/// <summary>
		/// Creates an instance of <see cref="SettingParser"/> with -, --, and / as valid prefixes and adds a help command.
		/// </summary>
		public SettingParser() : this(true, DefaultPrefixes) { }
		/// <summary>
		/// Creates an instance of <see cref="SettingParser"/> with -, --, and / as valid prefixes and optionally adds a help command.
		/// </summary>
		/// <param name="addHelp"></param>
		public SettingParser(bool addHelp) : this(addHelp, DefaultPrefixes) { }
		/// <summary>
		/// Creates an instance of <see cref="SettingParser"/> with the supplied prefixes and optionally adds a help command.
		/// </summary>
		/// <param name="addHelp"></param>
		/// <param name="prefixes"></param>
		public SettingParser(bool addHelp, params char[] prefixes) : this(addHelp, prefixes.Select(x => x.ToString())) { }
		/// <summary>
		/// Creates an instance of <see cref="SettingParser"/> with the supplied prefixes and optionally adds a help command.
		/// </summary>
		/// <param name="addHelp"></param>
		/// <param name="prefixes"></param>
		public SettingParser(bool addHelp, params string[] prefixes) : this(addHelp, (IEnumerable<string>)prefixes) { }
		/// <summary>
		/// Creates an instance of <see cref="SettingParser"/> with the supplied prefixes and optionally adds a help command.
		/// </summary>
		/// <param name="addHelp"></param>
		/// <param name="prefixes"></param>
		public SettingParser(bool addHelp, IEnumerable<string> prefixes)
		{
			if (addHelp)
			{
				Add(new Setting<string>(new[] { "Help", "h" }, x => { }, () => null)
				{
					Description = "Gives you help. Can't fix your life.",
					IsOptional = true,
				});
			}
			Prefixes = prefixes.ToImmutableArray();
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
		/// <inheritdoc />
		public IEnumerable<ISetting> GetSettings()
			=> _SettingMap.Values;
		/// <inheritdoc />
		public ISetting GetSetting(string name, PrefixState state)
		{
			var settings = GetSettings();
			foreach (var prefix in Prefixes)
			{
				string val;
				switch (state)
				{
					//Break switch if match found, else continue loop
					case PrefixState.Required:
						if (name.CaseInsStartsWith(prefix))
						{
							val = name.Substring(prefix.Length);
							break;
						}
						continue;
					//Break switch if match found, also break switch if match found
					case PrefixState.Optional:
						if (name.CaseInsStartsWith(prefix))
						{
							val = name.Substring(prefix.Length);
							break;
						}
						val = name;
						break;
					case PrefixState.NotPrefixed:
						val = name;
						break;
					default:
						throw new InvalidOperationException("Invalid prefix state provided.");
				}
				if (_NameMap.TryGetValue(val, out var guid))
				{
					return _SettingMap[guid];
				}
			}
			return null;
		}
		/// <inheritdoc />
		public string GetHelp(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				var values = GetSettings().Select(x =>
				{
					return x.Names.Count() < 2 ? x.MainName : $"{x.MainName} ({string.Join(", ", x.Names.Skip(1))})";
				});
				return $"All Settings:{Environment.NewLine}\t{string.Join($"{Environment.NewLine}\t", values)}";
			}
			return GetSetting(name, PrefixState.NotPrefixed) is ISetting setting
				? setting.Information
				: $"'{name}' is not a valid setting.";
		}
		/// <inheritdoc />
		public ISettingParserResults Parse(string input)
			=> Parse(input.SplitLikeCommandLine());
		/// <inheritdoc />
		public ISettingParserResults Parse(string[] input)
		{
			var unusedParts = new List<string>();
			var successes = new List<string>();
			var errors = new List<string>();
			var help = new List<string>();
			for (int i = 0; i < input.Length; ++i)
			{
				var part = input[i];
				string value;
				//No setting was gotten, so just skip this part
				if (!(GetSetting(part, PrefixState.Required) is ISetting setting))
				{
					unusedParts.Add(part);
					continue;
				}

				//If it's a flag set its value to true then go to the next part
				if (setting.IsFlag)
				{
					value = setting is IDirectGetter<bool> getter && getter.GetValue() ? bool.FalseString : bool.TrueString;
				}
				//If there's one more and it's not a setting use that
				else if (input.Length - 1 > i && !(GetSetting(input[i + 1], PrefixState.Required) is ISetting throwaway))
				{
					value = input[++i]; //Make sure to increment i since the part is being used as a setting
				}
				//If help and had argument, would have gone into the above statement.
				//This means it has gotten to the flag aspect of it, so null can just be passed in.
				else if (setting.IsHelp)
				{
					value = null;
				}
				//Otherwise this part is unused
				else
				{
					unusedParts.Add(part);
					continue;
				}

				if (setting.IsHelp)
				{
					help.Add(GetHelp(value));
				}
				else if (setting.TrySetValue(value, out var response))
				{
					successes.Add(response);
				}
				else
				{
					errors.Add(response);
				}
			}
			return new SettingParserResults(unusedParts, successes, errors, help);
		}
		/// <inheritdoc />
		public void Add(ISetting setting)
		{
			var guid = Guid.NewGuid();
			foreach (var name in setting.Names)
			{
				_NameMap.Add(name, guid);
			}
			_SettingMap.Add(guid, setting);
		}
		/// <inheritdoc />
		public bool Remove(ISetting setting)
		{
			if (!_NameMap.TryGetValue(setting.MainName, out var guid))
			{
				return false;
			}
			foreach (var name in setting.Names)
			{
				_NameMap.Remove(name);
			}
			return _SettingMap.Remove(guid);
		}
		/// <inheritdoc />
		public bool Contains(ISetting setting)
			=> _SettingMap.Values.Contains(setting);
		/// <inheritdoc />
		public void Clear()
		{
			_NameMap.Clear();
			_SettingMap.Clear();
		}
		/// <inheritdoc />
		public void CopyTo(ISetting[] array, int index)
			=> _SettingMap.Values.CopyTo(array, index);
		/// <inheritdoc />
		public IEnumerator<ISetting> GetEnumerator()
			=> _SettingMap.Values.GetEnumerator();
		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
			=> _SettingMap.Values.GetEnumerator();
	}
}