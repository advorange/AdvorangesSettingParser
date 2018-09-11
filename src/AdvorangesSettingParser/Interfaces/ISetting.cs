using System;
using System.Collections.Generic;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// An interface for settings.
	/// </summary>
	public interface ISetting
	{
		/// <summary>
		/// The names of this command.
		/// </summary>
		IEnumerable<string> Names { get; }
		/// <summary>
		/// The first value in <see cref="Names"/>.
		/// </summary>
		string MainName { get; }
		/// <summary>
		/// String indicating what this setting does.
		/// </summary>
		string Description { get; set; }
		/// <summary>
		/// String with information about the setting.
		/// </summary>
		string Information { get; }
		/// <summary>
		/// Indicates the setting is a boolean which only requires an attempt at parsing it for it to switch its value.
		/// The passed in string will either be <see cref="bool.TrueString"/> or <see cref="bool.FalseString"/>.
		/// </summary>
		bool IsFlag { get; set; }
		/// <summary>
		/// Indicates the argument is optional.
		/// </summary>
		bool IsOptional { get; set; }
		/// <summary>
		/// Indicates that the setting cannot be null.
		/// </summary>
		bool CannotBeNull { get; set; }
		/// <summary>
		/// Indicates whether or not the setting has been set yet.
		/// </summary>
		bool HasBeenSet { get; }
		/// <summary>
		/// Indicates this is for providing help, and is not necessarily a setting.
		/// </summary>
		bool IsHelp { get; }

		/// <summary>
		/// Converts the value to the required type and sets the property/field.
		/// </summary>
		/// <param name="value">The passed in argument to convert.</param>
		/// <param name="response">Response which can either be a success or failure string.</param>
		/// <returns>Whether the value has successfully been set.</returns>
		bool TrySetValue(string value, out string response);
	}
}