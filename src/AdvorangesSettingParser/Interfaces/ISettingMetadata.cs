using System;
using System.Collections.Generic;

namespace AdvorangesSettingParser.Interfaces
{
	/// <summary>
	/// Information about the setting.
	/// </summary>
	public interface ISettingMetadata
	{
		/// <summary>
		/// The names of this command.
		/// </summary>
		IReadOnlyCollection<string> Names { get; }
		/// <summary>
		/// The first value in <see cref="Names"/>.
		/// </summary>
		string MainName { get; }
		/// <summary>
		/// String indicating what this setting does.
		/// </summary>
		string Description { get; }
		/// <summary>
		/// String with information about the setting.
		/// </summary>
		string Information { get; }
		/// <summary>
		/// Indicates the setting is a boolean which only requires an attempt at parsing it for it to switch its value.
		/// The passed in string will either be <see cref="bool.TrueString"/> or <see cref="bool.FalseString"/>.
		/// </summary>
		bool IsFlag { get; }
		/// <summary>
		/// Indicates the argument is optional.
		/// </summary>
		bool IsOptional { get; }
		/// <summary>
		/// Indicates the argument is a help command and not a setting.
		/// </summary>
		bool IsHelp { get; }
		/// <summary>
		/// Indicates that the setting is already in a parser.
		/// Attempting to add the same setting to multiple parsers will throw an exception.
		/// </summary>
		bool IsAssociatedWithParser { get; }
		/// <summary>
		/// Indicates that the setting cannot be null.
		/// </summary>
		bool CannotBeNull { get; }
		/// <summary>
		/// Indicates that the setting has been set.
		/// </summary>
		bool HasBeenSet { get; }
		/// <summary>
		/// Whether to unescape the quotes in the supplied string before setting it.
		/// </summary>
		bool UnescapeBeforeSetting { get; }
		/// <summary>
		/// The group number this command is in.
		/// Groups mean commands which are mutually exclusive meaning if one is set then the rest are not prompted for setting any more.
		/// </summary>
		int? Group { get; }
		/// <summary>
		/// The type of object this setting is targeting directly.
		/// For example, if the property to modify is <see cref="ICollection{T}"/> this would be that type.
		/// </summary>
		Type TargetType { get; }
		/// <summary>
		/// The type this setting is trying to convert to.
		/// For example, if <see cref="TargetType"/> is <see cref="ICollection{T}"/> this would be the type parameter.
		/// </summary>
		Type ValueType { get; }

		/// <summary>
		/// Registers this setting to the supplied parser.
		/// Will throw an exception if attempting to register this setting to multiple parsers.
		/// </summary>
		/// <param name="parser"></param>
		void AssociateParser(ISettingParser parser);
	}
}