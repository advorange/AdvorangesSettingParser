using System;
using System.Linq;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// How to get help for the commands.
	/// </summary>
	public class HelpCommand : SettingMetadataBase<string, string>
	{
		private ISettingParser Parent { get; }

		/// <summary>
		/// Creates an instance of <see cref="HelpCommand"/>.
		/// </summary>
		/// <param name="parent"></param>
		public HelpCommand(ISettingParser parent) : base(new[] { "help", "h" })
		{
			Parent = parent;
			IsOptional = true;
			HasBeenSet = true;
		}

		/// <summary>
		/// Gets a help result for the supplied arguments.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public IResult GetHelp(string args)
			=> GetHelp(Parent, args);
		/// <summary>
		/// Returns information either about the settings in general, or the specified setting.
		/// </summary>
		/// <param name="parser"></param>
		/// <param name="name">The setting to target. Can be null if wanting to list out every setting.</param>
		/// <returns>Help information about either the setting or all settings.</returns>
		public static IResult GetHelp(ISettingParser parser, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				var values = parser.GetSettings().Select(x => x.Names.Count < 2 ? x.MainName : $"{x.MainName} ({string.Join(", ", x.Names.Skip(1))})");
				return new HelpResult($"All Settings:{Environment.NewLine}\t{string.Join($"{Environment.NewLine}\t", values)}");
			}
			return parser.TryGetSetting(name, PrefixState.NotPrefixed, out var setting)
				? new HelpResult(setting.Information)
				: Result.FromError($"'{name}' is not a valid setting.");
		}
	}
}