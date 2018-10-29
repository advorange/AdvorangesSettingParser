using System;
using AdvorangesSettingParser.Interfaces;

namespace AdvorangesSettingParser.Implementation
{
	/// <summary>
	/// Used in collection settings for determining whether to add or remove.
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
					case CollectionModificationAction.Toggle:
						return "toggled";
					case CollectionModificationAction.Add:
					case CollectionModificationAction.AddIfMissing:
						return "added";
					case CollectionModificationAction.Remove:
						return "removed";
					default:
						throw new ArgumentException($"Unable to convert to string.", nameof(Action));
				}
			}
		}
		/// <summary>
		/// How to modify the targeted value.
		/// </summary>
		public CollectionModificationAction Action { get; set; }
		/// <summary>
		/// Limits how many items can be removed when finding existing matching values.
		/// </summary>
		/// <value></value>
		public int MaxRemovalCount { get; set; } = int.MaxValue;
	}
}