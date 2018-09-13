namespace AdvorangesSettingParser
{
	/// <summary>
	/// Allows try setting, regular setting, getting, and resetting the value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISetting<T> : IBasicSetting, IResettable, IDirectGetter<T>, IDirectSetter<T> { }
	/// <summary>
	/// Allows try setting, regular setting, getting, and resetting the value.
	/// </summary>
	public interface ISetting : IBasicSetting, IResettable, IDirectGetter, IDirectSetter { }
}