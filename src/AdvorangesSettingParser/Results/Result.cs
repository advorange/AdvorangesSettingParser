using AdvorangesSettingParser.Interfaces;

namespace AdvorangesSettingParser.Results
{
	/// <summary>
	/// Indicates the status of something.
	/// </summary>
	public class Result : IResult
	{
		/// <inheritdoc />
		public string Response { get; }
		/// <inheritdoc />
		public bool IsSuccess { get; }

		/// <summary>
		/// Creates an instance of <see cref="Result"/>.
		/// </summary>
		/// <param name="isSuccess"></param>
		/// <param name="response"></param>
		protected Result(bool isSuccess, string response)
		{
			IsSuccess = isSuccess;
			Response = response;
		}

		/// <summary>
		/// Returns a successful result.
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public static Result FromError(string response)
			=> new Result(false, response);
		/// <summary>
		/// Returns a successful result.
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public static Result FromSuccess(string response)
			=> new Result(true, response);
		/// <summary>
		/// Returns the response.
		/// </summary>
		/// <returns></returns>
		public override string ToString() => Response;
	}
}
