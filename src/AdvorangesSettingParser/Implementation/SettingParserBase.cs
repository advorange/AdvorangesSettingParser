using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesSettingParser.Utils;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Base class for parsing settings.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class SettingParserBase<T> : ISettingParser, IReadOnlyCollection<T>, ICollection<T> where T : ISettingMetadata
	{
		/// <summary>
		/// The default prefixes used for setting parsing.
		/// </summary>
		public static IReadOnlyCollection<string> DefaultPrefixes { get; } = new[] { "-", "--", "/" }.ToImmutableArray();

		/// <inheritdoc />
		public IReadOnlyCollection<string> Prefixes { get; }
		/// <inheritdoc />
		public string MainPrefix { get; }
		/// <inheritdoc />
		public bool IsReadOnly { get; private set; }
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
		/// Whether this parser has the help command inside it.
		/// </summary>
		protected bool HasHelpCommand { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="SettingParserBase{T}"/> with the supplied prefixes.
		/// </summary>
		/// <param name="prefixes"></param>
		public SettingParserBase(IEnumerable<string> prefixes = default)
		{
			var immutable = (prefixes ?? DefaultPrefixes).ToImmutableArray();
			Prefixes = immutable;
			MainPrefix = immutable[0];
		}

		/// <summary>
		/// Parses through all the text and then handles any setting.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="setter"></param>
		/// <returns></returns>
		protected ISettingParserResult Parse(ParseArgs input, Func<T, string, IResult> setter)
		{
			var unusedParts = new List<IResult>();
			var successes = new List<IResult>();
			var errors = new List<IResult>();
			var help = new List<HelpResult>();

			bool tryParser(string s, out T result)
				=> TryGetSetting(s, PrefixState.Required, out result);

			var argMap = input.CreateArgMap<T>(tryParser);
			foreach (var (Setting, Args) in argMap)
			{
				if (Setting == null)
				{
					unusedParts.Add(Result.FromError(Args));
					continue;
				}

				var args = Setting.IsFlag && Args == null ? bool.TrueString : Args;
				var response = setter(Setting, args);
				if (response.IsSuccess && response is HelpResult helpResult)
				{
					help.Add(helpResult);
				}
				else if (response.IsSuccess)
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
		/// Abstract and protected to handle implementation from an instance and static context.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		protected abstract ISettingParserResult Parse(object source, ParseArgs input);
		/// <summary>
		/// Abstract and protected to handle implementation from an instance and static context.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		protected abstract IReadOnlyCollection<T> GetNeededSettings(object source);
		/// <summary>
		/// Returns settings which are not optional or have not been set/had a mutually exclusive setting set.
		/// </summary>
		/// <param name="set"></param>
		/// <returns></returns>
		protected IReadOnlyCollection<T> GetNeededSettings(IEnumerable<T> set)
		{
			return this.Where(x =>
			{
				return !x.IsOptional
					&& !set.Contains(x)
					&& !(x.Group != null && set.Any(y => y.Group == x.Group));
			}).ToArray();
		}
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
				var deprefixed = ArgumentMappingUtils.Deprefix(prefix, name, state);
				if (deprefixed != null && InternalTryGetSetting(deprefixed, out setting))
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
		/// Removes the ability to modify this collection.
		/// </summary>
		public void Freeze()
			=> IsReadOnly = true;
		/// <inheritdoc />
		public void Add(T setting)
		{
			ThrowIfReadOnly();

			if (setting.IsHelp)
			{
				if (HasHelpCommand)
				{
					throw new InvalidOperationException("Do not add more than one help command into a setting parser.");
				}
				HasHelpCommand = true;
			}

			var guid = Guid.NewGuid();
			foreach (var name in setting.Names)
			{
				NameMap.Add(name, guid);
			}
			SettingMap.Add(guid, setting);
			setting.AssociateParser(this);
		}
		/// <inheritdoc />
		public bool Remove(T setting)
		{
			ThrowIfReadOnly();
			if (!NameMap.TryGetValue(setting.MainName, out var guid))
			{
				return false;
			}

			if (setting.IsHelp)
			{
				HasHelpCommand = false;
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
			ThrowIfReadOnly();
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
		private void ThrowIfReadOnly()
		{
			if (IsReadOnly)
			{
				throw new InvalidOperationException("Cannot modify a setting parser after it has been frozen.");
			}
		}

		//ISettingParser
		IReadOnlyCollection<ISettingMetadata> ISettingParser.GetSettings()
			=> (IReadOnlyCollection<ISettingMetadata>)this;
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
		IReadOnlyCollection<ISettingMetadata> ISettingParser.GetNeededSettings(object source)
			=> (IReadOnlyCollection<ISettingMetadata>)GetNeededSettings(source);
	}
}