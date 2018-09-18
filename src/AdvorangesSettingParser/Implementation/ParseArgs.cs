using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Used to remove the need for two seperate methods each time for string and string[].
	/// </summary>
	public class ParseArgs
	{
		/// <summary>
		/// The characters to use for quotes.
		/// </summary>
		public static char[] QuoteChars { get; set; } = new[] { '\"' };
		/// <summary>
		/// Matches the default quote character if it's not escaped and there is whitespace or the start of the line before it.
		/// </summary>
		public static Regex StartRegex { get; set; } = new Regex($"(\\s|^)(?<!\\\\)\"", RegexOptions.Compiled | RegexOptions.Multiline);
		/// <summary>
		/// Mathces the default quote character if it's not escaped and there is whitespace, the end of the line, or another quote after it.
		/// </summary>
		public static Regex EndRegex { get; set; } = new Regex($"(?<!\\\\)\"(\\s|$|\")", RegexOptions.Compiled | RegexOptions.Multiline);

		/// <summary>
		/// The arguments to be used.
		/// </summary>
		public string[] Arguments { get; }
		/// <summary>
		/// The argument's length.
		/// </summary>
		public int Length => Arguments.Length;

		/// <summary>
		/// Creates an instance of <see cref="ParseArgs"/> to remove the need for two methods every time string and string[] are interchangeable.
		/// </summary>
		/// <param name="args"></param>
		public ParseArgs(IEnumerable<string> args)
		{
			Arguments = args.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
		}

		/// <summary>
		/// Accessor for the array.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public string this[int i]
		{
			get => Arguments[i];
			set => Arguments[i] = value;
		}

		/// <summary>
		/// Splits the input like command line and uses those as the arguments.
		/// </summary>
		/// <param name="input"></param>
		public static implicit operator ParseArgs(string input)
		{
			var startIndexes = GetIndexes(StartRegex.Matches(input)).ToArray();
			var endIndexes = GetIndexes(EndRegex.Matches(input)).ToArray();
			if (startIndexes.Length != endIndexes.Length)
			{
				throw new ArgumentException("There is a quote mismatch.");
			}
			//No quotes means just return splitting on space
			if (startIndexes.Length == 0)
			{
				return new ParseArgs(input.Split(' '));
			}

			var args = new List<string>();
			var minStart = startIndexes.DefaultIfEmpty(0).Min(x => x);
			if (minStart != 0)
			{
				args.AddRange(input.Substring(0, minStart).Split(' '));
			}
			//If all start indexes are less than any end index this is fairly easy
			//Just pair them off from the outside in
			if (!startIndexes.Any(s => endIndexes.Any(e => s > e)))
			{
				var start = startIndexes[0];
				var end = endIndexes[endIndexes.Length - 1];
				args.Add(input.Substring(start + 1, end - start));
			}
			else
			{
				for (int i = 0, lastEnding = int.MaxValue; i < startIndexes.Length; ++i)
				{
					var start = startIndexes[i];
					var end = endIndexes[i];

					//If the last ending is before the current start that means everything in between is ignored unless we manually add it
					if (lastEnding < start)
					{
						args.Add(input.Substring(lastEnding + 1, start - lastEnding));
					}

					//Determine how many quotes start before the end of the end at index i
					var startsBetweenStartAndEnd = 0;
					for (int k = i + 1; k < startIndexes.Length; ++k)
					{
						if (startIndexes[k] > end)
						{
							break;
						}
						++startsBetweenStartAndEnd;
						continue;
					}

					//No starts before next end means the argument is a basic "not nested quotes"
					//Some starts before next end means the argument is a "there "are nested quotes""
					args.Add(input.Substring(start + 1, (lastEnding = endIndexes[i += startsBetweenStartAndEnd]) - start));
				}
			}
			var maxEnd = endIndexes.DefaultIfEmpty(input.Length - 1).Max(x => x);
			if (maxEnd != input.Length - 1)
			{
				args.AddRange(input.Substring(maxEnd + 1).Split(' '));
			}
			return new ParseArgs(args);
		}
		/// <summary>
		/// Creates the arguments from the passed in array.
		/// </summary>
		/// <param name="input"></param>
		public static implicit operator ParseArgs(string[] input)
			=> new ParseArgs(input);
		/// <summary>
		/// Returns the array of arguments.
		/// </summary>
		/// <param name="args"></param>
		public static implicit operator string[] (ParseArgs args)
			=> args.Arguments;

		private static IEnumerable<int> GetIndexes(MatchCollection matches)
		{
			foreach (Match match in matches)
			{
				foreach (Group group in match.Groups)
				{
					if (!string.IsNullOrWhiteSpace(group.Value))
					{
						yield return group.Index;
					}
				}
			}
		}
	}
}