using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Holds the results of parsing settings.
	/// </summary>
	public class SettingParserResult : Result, ISettingParserResult
	{
		/// <inheritdoc />
		public IEnumerable<string> UnusedParts { get; }
		/// <inheritdoc />
		public IEnumerable<string> Successes { get; }
		/// <inheritdoc />
		public IEnumerable<string> Errors { get; }
		/// <inheritdoc />
		public IEnumerable<string> Help { get; }

		/// <summary>
		/// Creates an instance of <see cref="SettingParserResult"/>.
		/// </summary>
		/// <param name="unusedParts"></param>
		/// <param name="successes"></param>
		/// <param name="errors"></param>
		/// <param name="help"></param>
		public SettingParserResult(IEnumerable<string> unusedParts, IEnumerable<string> successes, IEnumerable<string> errors, IEnumerable<string> help)
			: base(!unusedParts.Any() && !errors.Any(), GenerateResponse(unusedParts, successes, errors, help))
		{
			UnusedParts = (unusedParts ?? Enumerable.Empty<string>()).Where(x => x != null).ToImmutableArray();
			Successes = (successes ?? Enumerable.Empty<string>()).Where(x => x != null).ToImmutableArray();
			Errors = (errors ?? Enumerable.Empty<string>()).Where(x => x != null).ToImmutableArray();
			Help = (help ?? Enumerable.Empty<string>()).Where(x => x != null).ToImmutableArray();
		}

		private static string GenerateResponse(IEnumerable<string> unusedParts, IEnumerable<string> successes, IEnumerable<string> errors, IEnumerable<string> help)
		{
			var responses = new List<string>();
			if (help.Any())
			{
				responses.Add($"HELP:\n{string.Join("\n\t", help)}");
			}
			if (successes.Any())
			{
				responses.Add($"SUCCESSES:\n{string.Join("\n\t", successes)}");
			}
			if (errors.Any())
			{
				responses.Add($"The following errors occurred:\n{string.Join("\n\t", errors)}");
			}
			if (unusedParts.Any())
			{
				responses.Add($"The following parts were extra; was an argument mistyped? '{string.Join("', '", unusedParts)}'");
			}
			return string.Join("\n\n", responses) + "\n";
		}
	}
}