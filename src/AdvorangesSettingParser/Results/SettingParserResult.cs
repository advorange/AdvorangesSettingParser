using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvorangesSettingParser.Interfaces;

namespace AdvorangesSettingParser.Results
{
	/// <summary>
	/// Holds the results of parsing settings.
	/// </summary>
	public class SettingParserResult : Result, ISettingParserResult
	{
		/// <inheritdoc />
		public IEnumerable<IResult> UnusedParts { get; }
		/// <inheritdoc />
		public IEnumerable<IResult> Successes { get; }
		/// <inheritdoc />
		public IEnumerable<IResult> Errors { get; }
		/// <inheritdoc />
		public IEnumerable<IResult> Help { get; }

		/// <summary>
		/// Creates an instance of <see cref="SettingParserResult"/>.
		/// </summary>
		/// <param name="unusedParts"></param>
		/// <param name="successes"></param>
		/// <param name="errors"></param>
		/// <param name="help"></param>
		public SettingParserResult(IEnumerable<IResult> unusedParts, IEnumerable<IResult> successes, IEnumerable<IResult> errors, IEnumerable<IResult> help)
			: base(!unusedParts.Any() && !errors.Any(), GenerateResponse(unusedParts, successes, errors, help))
		{
			UnusedParts = MakeImmutable(unusedParts);
			Successes = MakeImmutable(successes);
			Errors = MakeImmutable(errors);
			Help = MakeImmutable(help);
		}

		private static ImmutableArray<IResult> MakeImmutable(IEnumerable<IResult> source)
			=> (source ?? Enumerable.Empty<IResult>()).Where(x => x != null).ToImmutableArray();
		private static string GenerateResponse(IEnumerable<IResult> unusedParts, IEnumerable<IResult> successes, IEnumerable<IResult> errors, IEnumerable<IResult> help)
		{
			var responses = new List<string>();
			if (help.Any())
			{
				responses.Add($"Help:\n\t{string.Join("\n\t", help)}");
			}
			if (successes.Any())
			{
				responses.Add($"Successes:\n\t{string.Join("\n\t", successes)}");
			}
			if (errors.Any())
			{
				responses.Add($"Errors:\n\t{string.Join("\n\t", errors)}");
			}
			if (unusedParts.Any())
			{
				responses.Add($"The following parts were extra; was an argument mistyped? '{string.Join("', '", unusedParts)}'");
			}
			return string.Join("\n\n", responses) + "\n";
		}
	}
}