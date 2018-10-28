using System.Collections.Generic;
using System.Linq;
using AdvorangesSettingParser.Interfaces;

namespace AdvorangesSettingParser.Implementation.Instance
{
	/// <summary>
	/// Parses settings and then sets them.
	/// This implementation is case-insensitive.
	/// </summary>
	/// <remarks>Putting a command with the name help or h will overwrite the help command.</remarks>
	public class SettingParser : SettingParserBase<ISetting>
	{
		/// <summary>
		/// A list of the settings the instance this is registered in has not set which are required.
		/// </summary>
		protected HashSet<ISetting> SetSettings { get; private set; } = new HashSet<ISetting>();

		/// <summary>
		/// Creates an instance of <see cref="SettingParser"/> with the supplied prefixes.
		/// </summary>
		/// <param name="prefixes"></param>
		public SettingParser(IEnumerable<string> prefixes = default) : base(prefixes)
		{
			Add(new SettingHelpCommand(this));
		}

		/// <inheritdoc />
		protected override ISettingParserResult Parse(object source, ParseArgs input)
			=> Parse(input);
		/// <inheritdoc />
		protected override IReadOnlyCollection<ISetting> GetNeededSettings(object source)
			=> GetNeededSettings();
		/// <summary>
		/// Parses the arguments into the parent.
		/// </summary>
		/// <param name="input"></param>
		/// <returns>The results of this parsing.</returns>
		public ISettingParserResult Parse(ParseArgs input)
		{
			return Parse(input, (setting, value) =>
			{
				var result = setting.TrySetValue(value);
				if (result.IsSuccess)
				{
					SetSettings.Add(setting);
				}
				return result;
			});
		}
		/// <summary>
		/// Returns settings which have not been set and are not optional.
		/// </summary>
		/// <returns>The settings which still need to be set.</returns>
		public IReadOnlyCollection<ISetting> GetNeededSettings()
			=> this.Where(x => !x.IsOptional && !SetSettings.Contains(x)).ToArray();

		private class SettingHelpCommand : HelpCommand, ISetting
		{
			public SettingHelpCommand(ISettingParser parent) : base(parent) { }

			//ISetting
			object ISetting.GetValue() => null;
			void ISetting.SetValue(object value) { }
			void IBasicSetting.ResetValue() { }
			IResult IBasicSetting.TrySetValue(string value) => GetHelp(value);
			IResult IBasicSetting.TrySetValue(string value, ITrySetValueContext context) => GetHelp(value);
		}
	}
}