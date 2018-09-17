using System.Collections.Generic;
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
		public ISettingParserResult Parse(ParseArgs input)
			=> Parse(input, (setting, value) => setting.TrySetValue(value));

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