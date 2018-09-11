using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Holds the results of parsing settings.
	/// </summary>
	public struct SettingParserResults : ISettingParserResults
	{
		/// <inheritdoc />
		public IEnumerable<string> UnusedParts { get; }
		/// <inheritdoc />
		public IEnumerable<string> Successes { get; }
		/// <inheritdoc />
		public IEnumerable<string> Errors { get; }
		/// <inheritdoc />
		public IEnumerable<string> Help { get; }
		/// <inheritdoc />
		public bool IsSuccess { get; }

		/// <summary>
		/// Creates an instance of <see cref="SettingParserResults"/>.
		/// </summary>
		/// <param name="unusedParts"></param>
		/// <param name="successes"></param>
		/// <param name="errors"></param>
		/// <param name="help"></param>
		public SettingParserResults(IEnumerable<string> unusedParts, IEnumerable<string> successes, IEnumerable<string> errors, IEnumerable<string> help)
		{
			UnusedParts = (unusedParts ?? Enumerable.Empty<string>()).Where(x => x != null).ToImmutableArray();
			Successes = (successes ?? Enumerable.Empty<string>()).Where(x => x != null).ToImmutableArray();
			Errors = (errors ?? Enumerable.Empty<string>()).Where(x => x != null).ToImmutableArray();
			Help = (help ?? Enumerable.Empty<string>()).Where(x => x != null).ToImmutableArray();
			IsSuccess = !UnusedParts.Any() && !Errors.Any();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var responses = new List<string>();
			if (Help.Any())
			{
				responses.Add(string.Join("\n", Help));
			}
			if (Successes.Any())
			{
				responses.Add(string.Join("\n", Successes));
			}
			if (Errors.Any())
			{
				responses.Add($"The following errors occurred:\n{string.Join("\n\t", Errors)}");
			}
			if (UnusedParts.Any())
			{
				responses.Add($"The following parts were extra; was an argument mistyped? '{string.Join("', '", UnusedParts)}'");
			}
			return string.Join("\n", responses) + "\n";
		}
	}
}