using System;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Used in <see cref="CollectionSetting{T}"/> for determining whether to add or remove.
	/// </summary>
	public class CollectionModificationContext : ITrySetValueContext
	{
		/// <summary>
		/// Returns a string for the action.
		/// </summary>
		public string ActionString
		{
			get
			{
				switch (Action)
				{
					case CMAction.Toggle:
						return "toggled";
					case CMAction.AddAlways:
					case CMAction.AddIfMissing:
						return "added";
					case CMAction.RemoveAlways:
					case CMAction.RemoveIfExists:
						return "removed";
					default:
						throw new ArgumentException($"Unable to convert to string.", nameof(Action));
				}
			}
		}
		/// <summary>
		/// How to modify the targeted value.
		/// </summary>
		public CMAction Action { get; set; }
	}

	/// <summary>
	/// Instructs how to modify the targeted value in the collection.
	/// </summary>
	public enum CMAction
	{
		/// <summary>
		/// If in list, will remove. If out of list, will add.
		/// </summary>
		Toggle,
		/// <summary>
		/// Add no matter what.
		/// </summary>
		AddAlways,
		/// <summary>
		/// Will only return success if missing then added.
		/// </summary>
		AddIfMissing,
		/// <summary>
		/// Remove no matter what.
		/// </summary>
		RemoveAlways,
		/// <summary>
		/// Will only return success if existing then removed.
		/// </summary>
		RemoveIfExists,
	}
}