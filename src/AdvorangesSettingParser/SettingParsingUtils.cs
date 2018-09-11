using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Extension methods for <see cref="ISettingParser"/>.
	/// </summary>
	public static class SettingParsingUtils
	{
		/// <summary>
		/// Formats a string describing what settings still need to be set.
		/// </summary>
		/// <param name="parser">The parser.</param>
		/// <returns>A description of what settings still need to be set.</returns>
		public static string FormatNeededSettings(this ISettingParser parser)
		{
			var unsetArguments = parser.GetNeededSettings();
			if (!unsetArguments.Any())
			{
				return $"Every setting which is necessary has been set.";
			}
			var sb = new StringBuilder("The following settings need to be set:" + Environment.NewLine);
			foreach (var setting in unsetArguments)
			{
				sb.AppendLine($"\t{setting.ToString()}");
			}
			return sb.ToString().Trim() + Environment.NewLine;
		}
		/// <summary>
		/// Returns settings which have not been set and are not optional.
		/// </summary>
		/// <param name="parser">The parser.</param>
		/// <returns>The settings which still need to be set.</returns>
		public static IEnumerable<ISetting> GetNeededSettings(this ISettingParser parser)
			=> parser.GetSettings().Where(x => !(x.HasBeenSet || x.IsOptional));
		/// <summary>
		/// Returns true if every setting is either set or optional.
		/// </summary>
		/// <param name="parser">The parser.</param>
		/// <returns>Whether every setting has been set.</returns>
		public static bool AreAllSet(this ISettingParser parser)
			=> parser.GetSettings().All(x => x.HasBeenSet || x.IsOptional);
	}
}