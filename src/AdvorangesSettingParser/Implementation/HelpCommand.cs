using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Utils;

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
		public IResult GetHelp(string args) => Parent.GetHelp(args);
	}
}