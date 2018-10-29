namespace AdvorangesSettingParser
{
	/// <summary>
	/// Instructs how to modify the targeted value in the collection.
	/// </summary>
	public enum CollectionModificationAction
	{
		/// <summary>
		/// If in list, will remove. If out of list, will add.
		/// </summary>
		Toggle,
		/// <summary>
		/// Add no matter what.
		/// </summary>
		Add,
		/// <summary>
		/// Will only return success if missing then added.
		/// </summary>
		AddIfMissing,
		/// <summary>
		/// Removes any matching values.
		/// </summary>
		Remove,
	}
}