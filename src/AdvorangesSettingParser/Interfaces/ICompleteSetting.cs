namespace AdvorangesSettingParser
{
	/// <summary>
	/// Completely allows modifying the targeted value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICompleteSetting<T> : ISetting, IDirectGetter<T>, IDirectSetter<T> { }

	/// <summary>
	/// Completely allows modifying the targeted value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICompleteSetting : ISetting, IDirectGetter, IDirectSetter { }
}
