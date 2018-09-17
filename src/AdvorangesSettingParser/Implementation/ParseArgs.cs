using System.Collections.Generic;
using System.Collections.Immutable;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Used to remove the need for two seperate methods each time for string and string[].
	/// </summary>
	public class ParseArgs
	{
		/// <summary>
		/// What to seperate by.
		/// </summary>
		public static ImmutableArray<char> DefaultSeperators => _DefaultSeperators.ToImmutableArray();
		private static char[] _DefaultSeperators { get; } = { ' ' };
		/// <summary>
		/// The quotes to not split inside.
		/// </summary>
		public static ImmutableArray<char> DefaultQuotes => _DefaultQuotes.ToImmutableArray();
		private static char[] _DefaultQuotes { get; } = { '"' };

		/// <summary>
		/// The arguments to be used.
		/// </summary>
		public string[] Arguments { get; }
		/// <summary>
		/// The values to split by except when in quotes.
		/// </summary>
		public IEnumerable<char> Seperators { get; }
		/// <summary>
		/// The values to treat as quotes.
		/// </summary>
		public IEnumerable<char> Quotes { get; }
		/// <summary>
		/// The argument's length.
		/// </summary>
		public int Length => Arguments.Length;

		/// <summary>
		/// Creates an instance of <see cref="ParseArgs"/> to remove the need for two methods every time string and string[] are interchangeable.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="seperators"></param>
		/// <param name="quotes"></param>
		public ParseArgs(string[] args, IEnumerable<char> seperators = null, IEnumerable<char> quotes = null)
		{
			Arguments = args;
			Seperators = seperators ?? DefaultSeperators;
			Quotes = quotes ?? DefaultQuotes;
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
			=> new ParseArgs(input.ComplexSplit(_DefaultSeperators, _DefaultQuotes, false));
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
	}
}