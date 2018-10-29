using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdvorangesSettingParser.Interfaces;

namespace AdvorangesSettingParser.Implementation.Static
{
	/// <summary>
	/// Allows creation of the settings statically, then providing an instance of the target type to fill up.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	public class StaticSettingParser<TSource> : SettingParserBase<IStaticSetting<TSource>> where TSource : class
	{
		/// <summary>
		/// Keeps a list of any objects parsed and which settings have been set on them.
		/// </summary>
		protected ConditionalWeakTable<TSource, HashSet<IStaticSetting<TSource>>> SetSettings { get; } = new ConditionalWeakTable<TSource, HashSet<IStaticSetting<TSource>>>();

		/// <summary>
		/// Creates an instance of <see cref="StaticSettingParser{TSource}"/> with the supplied prefixes.
		/// </summary>
		/// <param name="prefixes"></param>
		public StaticSettingParser(IEnumerable<string> prefixes = default) : base(prefixes)
		{
			Add(new StaticSettingHelpCommand(this));
		}

		/// <inheritdoc />
		protected override ISettingParserResult Parse(object source, ParseArgs input)
			=> Parse((TSource)source, input);
		/// <inheritdoc />
		protected override IReadOnlyCollection<IStaticSetting<TSource>> GetNeededSettings(object source)
			=> GetNeededSettings((TSource)source);
		/// <summary>
		/// Parses the arguments into the supplied instance.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public ISettingParserResult Parse(TSource source, ParseArgs input)
		{
			return Parse(input, (setting, value) =>
			{
				var result = setting.TrySetValue(source, value);
				if (result.IsSuccess)
				{
					SetSettings.GetValue(source, x => new HashSet<IStaticSetting<TSource>>()).Add(setting);
				}
				return result;
			});
		}
		/// <summary>
		/// Returns settings which have not been set and are not optional.
		/// </summary>
		/// <param name="source">The targeted source.</param>
		/// <returns>The settings which still need to be set.</returns>
		public IReadOnlyCollection<IStaticSetting<TSource>> GetNeededSettings(TSource source)
			=> GetNeededSettings(SetSettings.GetValue(source, x => new HashSet<IStaticSetting<TSource>>()));

		private class StaticSettingHelpCommand : HelpCommand, IStaticSetting<TSource>
		{
			public StaticSettingHelpCommand(ISettingParser parent) : base(parent) { }

			//IStaticSetting
			object IStaticSetting<TSource>.GetValue(TSource source) => null;
			void IBasicStaticSetting<TSource>.ResetValue(TSource source) { }
			void IStaticSetting<TSource>.SetValue(TSource source, object value) { }
			IResult IBasicStaticSetting<TSource>.TrySetValue(TSource source, string value) => GetHelp(value);
			IResult IBasicStaticSetting<TSource>.TrySetValue(TSource source, string value, ITrySetValueContext context) => GetHelp(value);
		}
	}
}