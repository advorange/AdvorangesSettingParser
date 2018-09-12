using System;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Acts as the ref keyword for multiple types other than fields.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRef<T>
	{
		/// <summary>
		/// The name of the targeted value.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Sets the targeted value.
		/// </summary>
		Action<T> Setter { get; }
		/// <summary>
		/// Gets the targeted value.
		/// </summary>
		Func<T> Getter { get; }
	}
}