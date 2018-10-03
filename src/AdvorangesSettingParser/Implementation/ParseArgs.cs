using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Used to remove the need for two seperate methods each time for string and string[].
	/// </summary>
	public class ParseArgs : IReadOnlyCollection<string>
	{
		/// <summary>
		/// How to validate a start or end quote.
		/// </summary>
		/// <param name="previousChar"></param>
		/// <param name="currentChar"></param>
		/// <param name="nextChar"></param>
		/// <returns></returns>
		public delegate bool ValidateQuote(char? previousChar, char currentChar, char? nextChar);

		private static char[] _DefautQuotes { get; } = new[] { '"' };

		/// <inheritdoc />
		public int Count => _Arguments.Length;
		/// <summary>
		/// The characters used to start quotes with.
		/// </summary>
		public ImmutableArray<char> StartingQuoteCharacters { get; }
		/// <summary>
		/// The characters used to end quotes with.
		/// </summary>
		public ImmutableArray<char> EndingQuoteCharacters { get; }
		/// <summary>
		/// The arguments to be used.
		/// </summary>
		private string[] _Arguments { get; }

		/// <summary>
		/// Creates an instance of <see cref="ParseArgs"/> to remove the need for two methods every time string and string[] are interchangeable.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="startingQuotes"></param>
		/// <param name="endingQuotes"></param>
		public ParseArgs(IEnumerable<string> args, IEnumerable<char> startingQuotes, IEnumerable<char> endingQuotes)
		{
			_Arguments = args.ToArray();
			StartingQuoteCharacters = startingQuotes.ToImmutableArray();
			EndingQuoteCharacters = endingQuotes.ToImmutableArray();
		}

		/// <summary>
		/// Accessor for the array.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public string this[int i]
		{
			get => _Arguments[i];
		}

		/// <inheritdoc />
		public IEnumerator<string> GetEnumerator() => ((IReadOnlyCollection<string>)_Arguments).GetEnumerator();

		/// <summary>
		/// Parses a <see cref="ParseArgs"/> from the passed in string or throws an exception.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static ParseArgs Parse(string input)
			=> TryParse(input, out var result) ? result : throw new ArgumentException("There is a quote mismatch.");
		/// <summary>
		/// Attempts to parse a <see cref="ParseArgs"/> using the default quote character " for beginning and ending quotes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(string input, out ParseArgs result)
			=> TryParse(input, _DefautQuotes, _DefautQuotes, out result);
		/// <summary>
		/// Attempts to parse a <see cref="ParseArgs"/> from characters indicating the start of a quote and characters indicating the end of a quote.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startQuotes"></param>
		/// <param name="endQuotes"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(string input, char[] startQuotes, char[] endQuotes, out ParseArgs result)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				throw new ArgumentException("Cannot be null or whitespace.", nameof(input));
			}

			var startIndexes = GetIndexes(input, startQuotes, allowEscaping: true, (previous, current, next) =>
			{
				return previous == null || char.IsWhiteSpace(previous.Value);
			});
			var endIndexes = GetIndexes(input, endQuotes, allowEscaping: true, (previous, current, next) =>
			{
				return next == null || char.IsWhiteSpace(next.Value) || endQuotes.Contains(next.Value);
			});
			return TryParse(input, startQuotes, endQuotes, startIndexes, endIndexes, out result);
		}
		/// <summary>
		/// Attempts to parse a <see cref="ParseArgs"/> from start and end indexes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startQuotes"></param>
		/// <param name="endQuotes"></param>
		/// <param name="startIndexes"></param>
		/// <param name="endIndexes"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(string input, char[] startQuotes, char[] endQuotes, int[] startIndexes, int[] endIndexes, out ParseArgs result)
		{
			if (startIndexes.Length != endIndexes.Length)
			{
				result = null;
				return false;
			}
			//No quotes means just return splitting on space
			if (startIndexes.Length == 0)
			{
				result = new ParseArgs(input.Split(' '), startQuotes, endQuotes);
				return true;
			}

			var minStart = startIndexes.Min(x => x);
			var maxEnd = endIndexes.Max(x => x);
			if (minStart == 0 && maxEnd == input.Length - 1)
			{
				result = new ParseArgs(new[] { GetTrimmedString(input, minStart + 1, maxEnd) }, startQuotes, endQuotes);
				return true;
			}

			var args = new List<string>();
			if (minStart != 0)
			{
				args.AddRange(input.Substring(0, minStart).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			}
			//If all start indexes are less than any end index this is fairly easy
			//Just pair them off from the outside in
			if (!startIndexes.Any(s => endIndexes.Any(e => s > e)))
			{
				AddIfNotWhitespace(args, input, startIndexes[0], endIndexes[endIndexes.Length - 1] + 1);
			}
			else
			{
				for (int i = 0, previousEnding = int.MaxValue; i < startIndexes.Length; ++i)
				{
					var start = startIndexes[i];
					var end = endIndexes[i];

					//If the last ending is before the current start that means everything in between is ignored unless we manually add it
					if (previousEnding < start)
					{
						AddIfNotWhitespace(args, input, previousEnding + 1, start);
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
					AddIfNotWhitespace(args, input, start, previousEnding = endIndexes[i += startsBetweenStartAndEnd] + 1);
				}
			}
			if (maxEnd != input.Length - 1)
			{
				args.AddRange(input.Substring(maxEnd + 1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			}
			result = new ParseArgs(args, startQuotes, endQuotes);
			return true;
		}
		/// <summary>
		/// Gets either start or end indexes from the supplied string using the supplied quotes.
		/// </summary>
		/// <param name="quotes"></param>
		/// <param name="input"></param>
		/// <param name="allowEscaping"></param>
		/// <param name="valid"></param>
		/// <returns></returns>
		public static int[] GetIndexes(string input, char[] quotes, bool allowEscaping, ValidateQuote valid)
		{
			var indexes = new List<int>();
			for (int i = 0; i < input.Length; ++i)
			{
				var currentChar = input[i];
				if (!quotes.Contains(currentChar))
				{
					continue;
				}

				var previousChar = i == 0 ? default(char?) : input[i - 1];
				if (allowEscaping && previousChar == '\\')
				{
					continue;
				}

				var nextChar = i == input.Length - 1 ? default(char?) : input[i + 1];
				if (valid(previousChar, currentChar, nextChar))
				{
					indexes.Add(i);
				}
			}
			return indexes.ToArray();
		}
		private static string GetTrimmedString(string input, int start, int end)
			=> input.Substring(start, end - start).Trim();
		private static void AddIfNotWhitespace(List<string> list, string input, int start, int end)
		{
			var trimmed = GetTrimmedString(input, start, end);
			if (trimmed.Length != 0)
			{
				list.Add(trimmed);
			}
		}
		/// <summary>
		/// Splits the input like command line and uses those as the arguments.
		/// </summary>
		/// <param name="input"></param>
		public static implicit operator ParseArgs(string input)
			=> Parse(input);
		/// <summary>
		/// Returns the array of arguments.
		/// </summary>
		/// <param name="args"></param>
		public static implicit operator string[] (ParseArgs args)
			=> args._Arguments.ToArray();

		//IReadOnlyCollection
		IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyCollection<string>)_Arguments).GetEnumerator();
	}
}