using AdvorangesUtils;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Used to remove the need for two seperate methods each time for string and string[].
	/// </summary>
	public class ParseArgs
	{
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
		public ParseArgs(string[] args)
		{
			Arguments = args;
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
			=> new ParseArgs(input.SplitLikeCommandLine());
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