namespace AdvorangesSettingParser
{
	/// <summary>
	/// Completely allows modifying the targeted value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICompleteSetting<T> : ISetting, IResettable, IDirectGetter<T>, IDirectSetter<T> { }

	/// <summary>
	/// Completely allows modifying the targeted value.
	/// </summary>
	public interface ICompleteSetting : ISetting, IResettable, IDirectGetter, IDirectSetter { }
}
