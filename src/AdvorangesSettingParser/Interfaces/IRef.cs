namespace AdvorangesSettingParser
{
	/// <summary>
	/// Acts as the ref keyword for multiple types other than fields.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRef<T> : IDirectGetter<T>, IDirectSetter<T>
	{
		/// <summary>
		/// The name of the targeted value.
		/// </summary>
		string Name { get; }
	}
}