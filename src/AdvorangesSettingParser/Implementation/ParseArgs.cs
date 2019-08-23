using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Used to remove the need for two seperate methods each time for string and string[].
	/// </summary>
	public struct ParseArgs : IReadOnlyCollection<string>
	{
		/// <summary>
		/// How to validate a start or end quote.
		/// </summary>
		/// <param name="previousChar"></param>
		/// <param name="currentChar"></param>
		/// <param name="nextChar"></param>
		/// <returns></returns>
		public delegate bool ValidateQuote(char? previousChar, char currentChar, char? nextChar);

		private static readonly char[] _DefautQuotes = new[] { '"' };

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

		private readonly string[] _Arguments;

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
			=> _Arguments[i];

		/// <summary>
		/// Maps each setting of type <typeparamref name="T"/> to a value.
		/// Can map the same setting multiple times to different values, but will all be in the passed in order.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tryParser"></param>
		/// <returns></returns>
		public IEnumerable<(T Setting, string Args)> CreateArgMap<T>(TryParseDelegate<T> tryParser)
		{
			if (Count == 0)
			{
				yield break;
			}

			//Something like -FieldInfo "-Name "Test Value" -Text TestText" (with or without quotes around the whole thing)
			//Should parse into:
			//-FieldInfo
			//	-Name
			//		Test Value
			//	-Text
			//		TestText
			//Which with the current data structure should be (FieldInfo, "-Name "Test Value" -Text TestText")
			//then when those get parsed would turn into (Name, Test Value) and (Text, TestText)
			var currentSetting = default(T);
			var current = new StringBuilder();
			foreach (var arg in this)
			{
				var trimmed = TrimSingle(arg);
				//When this is not a setting we add the args onto the current args then return the current values instantly
				//We can't return Value directly because if there were other quote deepness args we need to count those.
				if (!tryParser(trimmed, out var setting))
				{
					//Trim quotes off the end of a setting value since they're not needed to group anything together anymore
					//And they just complicate future parsing
					if (current.Length > 0)
					{
						current.Append(" ");
					}
					current.Append(arg);

					yield return (currentSetting, TrimSingle(current));
				}
				//When this is a setting we only check if there's a setting from the last iteration
				//If there is, we send that one because there's a chance it could be a parameterless setting
				else if (currentSetting != null)
				{
					yield return (currentSetting, TrimSingle(current));
				}

				currentSetting = setting;
				current = new StringBuilder();
			}
			//Return any leftover parts which haven't been returned yet
			if (currentSetting != default || current.Length > 0)
			{
				yield return (currentSetting, TrimSingle(current));
			}
		}
		private string TrimSingle(string s)
		{
			if (s == null)
			{
				return null;
			}

			foreach (var c in StartingQuoteCharacters)
			{
				if (s.StartsWith(c))
				{
					s = s.Substring(1);
					break;
				}
			}
			foreach (var c in EndingQuoteCharacters)
			{
				if (s.EndsWith(c))
				{
					s = s.Substring(0, s.Length - 1);
					break;
				}
			}
			return s;
		}
		private string TrimSingle(StringBuilder sb)
		{
			if (sb.Length == 0)
			{
				return null;
			}

			foreach (var c in StartingQuoteCharacters)
			{
				if (sb[0] == c)
				{
					sb.Remove(0, 1);
					break;
				}
			}
			foreach (var c in EndingQuoteCharacters)
			{
				if (sb[sb.Length - 1] == c)
				{
					--sb.Length;
					break;
				}
			}
			return sb.ToString();
		}
		/// <inheritdoc />
		public IEnumerator<string> GetEnumerator()
			=> ((IReadOnlyCollection<string>)_Arguments).GetEnumerator();
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
		public static bool TryParse(
			string input,
			char[] startQuotes,
			char[] endQuotes,
			out ParseArgs result)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				result = new ParseArgs(Enumerable.Empty<string>(), startQuotes, endQuotes);
				return true;
			}

			bool ValidateStart(char? p, char c, char? n)
				=> p == null || char.IsWhiteSpace(p.Value);
			bool ValidateEnd(char? p, char c, char? n)
				=> n == null || char.IsWhiteSpace(n.Value) || endQuotes.Contains(n.Value);

			var startIndexes = GetIndices(input, startQuotes, allowEscaping: true, ValidateStart);
			var endIndexes = GetIndices(input, endQuotes, allowEscaping: true, ValidateEnd);
			return TryParse(input, startQuotes, endQuotes, startIndexes, endIndexes, out result);
		}
		/// <summary>
		/// Attempts to parse a <see cref="ParseArgs"/> from start and end indexes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startQuotes"></param>
		/// <param name="endQuotes"></param>
		/// <param name="startIndexes">Assumed to be in order.</param>
		/// <param name="endIndexes">Assumed to be in order</param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(
			string input,
			char[] startQuotes,
			char[] endQuotes,
			int[] startIndexes,
			int[] endIndexes,
			out ParseArgs result)
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

			var minStart = startIndexes[0];
			var maxEnd = endIndexes[endIndexes.Length - 1];
			if (minStart == 0 && maxEnd == input.Length - 1)
			{
				var trimmed = GetTrimmedString(input, minStart + 1, maxEnd);
				result = new ParseArgs(new[] { trimmed }, startQuotes, endQuotes);
				return true;
			}

			var args = new List<string>();
			if (minStart != 0)
			{
				args.AddRange(Split(input.Substring(0, minStart)));
			}
			//If all start indexes are less than any end index this is fairly easy
			//Just pair them off from the outside in
			if (!startIndexes.Any(s => endIndexes.Any(e => s > e)))
			{
				AddIfNotWhitespace(args, input, startIndexes[0], endIndexes[endIndexes.Length - 1] + 1);
			}
			else
			{
				for (int i = 0, previousEnd = int.MaxValue; i < startIndexes.Length; ++i)
				{
					var start = startIndexes[i];
					var end = endIndexes[i];

					//If the last ending is before the current start that means everything
					//in between is ignored unless we manually add it
					//And we need to split it
					if (previousEnd < start)
					{
						var diff = start - previousEnd - 1;
						args.AddRange(Split(input.Substring(previousEnd + 1, diff)));
					}

					//No starts before next end means simple quotes
					//Some starts before next end means nested quotes
					var skip = startIndexes.Skip(i + 1).Count(x => x < end);
					previousEnd = endIndexes[i += skip] + 1;
					AddIfNotWhitespace(args, input, start, previousEnd);
				}
			}

			if (maxEnd != input.Length - 1)
			{
				args.AddRange(Split(input.Substring(maxEnd + 1)));
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
		public static int[] GetIndices(string input, char[] quotes, bool allowEscaping, ValidateQuote valid)
		{
			var indexes = new List<int>();
			for (var i = 0; i < input.Length; ++i)
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
		private static string[] Split(string input)
			=> input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
		public static implicit operator string[](ParseArgs args)
			=> args._Arguments.ToArray();

		//IReadOnlyCollection
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}