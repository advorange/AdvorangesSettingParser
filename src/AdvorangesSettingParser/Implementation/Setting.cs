using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvorangesUtils;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// A generic class for a setting, specifying what the setting type is.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class Setting<T> : ISetting
	{
		/// <inheritdoc />
		public string Description { get; set; }
		/// <inheritdoc />
		public string Information
		{
			get
			{
				var help = $"{MainName}: {Description}";
				if (!EqualityComparer<T>.Default.Equals(DefaultValue, default))
				{
					help += $" Default value is {DefaultValue}.";
				}
				if (typeof(T).IsEnum)
				{
					help += $"{Environment.NewLine}Acceptable values: {string.Join(", ", Enum.GetNames(typeof(T)))}";
				}
				return help;
			}
		}
		/// <inheritdoc />
		public bool IsFlag
		{
			get => _IsFlag;
			set
			{
				if (value && typeof(T) != typeof(bool))
				{
					throw new InvalidOperationException($"The generic type of setting must be bool if {nameof(IsFlag)} is set as true.");
				}
				_IsFlag = value;
			}
		}
		/// <inheritdoc />
		public bool IsOptional { get; set; }
		/// <inheritdoc />
		public bool CannotBeNull { get; set; }
		/// <inheritdoc />
		public bool HasBeenSet { get; private set; }
		/// <inheritdoc />
		public bool IsHelp { get; }
		/// <inheritdoc />
		public IEnumerable<string> Names { get; }
		/// <inheritdoc />
		public string MainName { get; }
		/// <summary>
		/// The current value of the setting.
		/// </summary>
		public T CurrentValue { get; private set; }
		/// <summary>
		/// Default value of the setting. This will indicate the setting is optional, but has a value other than the default value of the type.
		/// </summary>
		public T DefaultValue
		{
			get => _DefaultValue;
			set
			{
				_DefaultValue = value;
				HasBeenSet = true;
				SetDefault();
			}
		}

		private readonly TryParseDelegate<T> _Parser;
		private readonly Action<T> _Setter;
		private T _DefaultValue;
		private bool _IsFlag;

		/// <summary>
		/// Creates an instance of <see cref="Setting{T}"/>.
		/// </summary>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="setter">The setter to use for this setting.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type.</param>
		public Setting(IEnumerable<string> names, Action<T> setter, TryParseDelegate<T> parser = default)
		{
			if (names == null || !names.Any())
			{
				throw new ArgumentException("Must supply at least one name.");
			}

			Names = names.ToImmutableArray();
			MainName = Names.First();
			IsHelp = Names.Any(x => x.CaseInsEquals("help") || x.CaseInsEquals("h"));
			_Setter = setter ?? throw new ArgumentException("Invalid setter supplied.");
			_Parser = parser ?? GetPrimitiveParser();
		}

		/// <inheritdoc />
		public void SetDefault()
		{
			_Setter(DefaultValue);
			CurrentValue = DefaultValue;
		}
		/// <inheritdoc />
		public bool TrySetValue(string value, out string response)
		{
			T result = default;
			if (value.CaseInsEquals("default")) //Let default just pass on through
			{
				result = default;
			}
			else
			{
				var (Success, Value) = _Parser(value);
				if (!Success)
				{
					response = $"Unable to convert '{value}' to type {typeof(T).Name}.";
					return false;
				}
				result = Value;
			}
			if (result == null && CannotBeNull)
			{
				response = $"{MainName} cannot be set to 'NULL'.";
				return false;
			}

			try
			{
				_Setter(result);
				CurrentValue = result;
			}
			//Catch all because who knows what exceptions will happen, and it's user input
			catch (Exception e)
			{
				response = e.Message;
				return false;
			}
			HasBeenSet = true;
			response = $"Successfully set {MainName} to '{result?.ToString() ?? "NULL"}'.";
			return true;
		}
		/// <inheritdoc />
		public override string ToString()
			=> $"{MainName} ({typeof(T).Name})";
		/// <summary>
		/// Attempts to get a parser for a primitive type.
		/// </summary>
		/// <returns></returns>
		private TryParseDelegate<T> GetPrimitiveParser()
		{
			switch (typeof(T).Name)
			{
				//I know this looks bad, but it's the easiest way to do this. It is still type safe anyways.
				case nameof(SByte):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<sbyte>(s => (sbyte.TryParse(s, out var result), result));
				case nameof(Byte):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<byte>(s => (byte.TryParse(s, out var result), result));
				case nameof(Int16):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<short>(s => (short.TryParse(s, out var result), result));
				case nameof(UInt16):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<ushort>(s => (ushort.TryParse(s, out var result), result));
				case nameof(Int32):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<int>(s => (int.TryParse(s, out var result), result));
				case nameof(UInt32):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<uint>(s => (uint.TryParse(s, out var result), result));
				case nameof(Int64):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<long>(s => (long.TryParse(s, out var result), result));
				case nameof(UInt64):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<ulong>(s => (ulong.TryParse(s, out var result), result));
				case nameof(Char):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<char>(s => (char.TryParse(s, out var result), result));
				case nameof(Single):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<float>(s => (float.TryParse(s, out var result), result));
				case nameof(Double):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<double>(s => (double.TryParse(s, out var result), result));
				case nameof(Boolean):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<bool>(s => (bool.TryParse(s, out var result), result));
				case nameof(Decimal):
					return (TryParseDelegate<T>)(object)new TryParseDelegate<decimal>(s => (decimal.TryParse(s, out var result), result));
				case nameof(String):
					//Instead of having to do special checks, just use this dumb delegate
					return (TryParseDelegate<T>)(object)new TryParseDelegate<string>(s => (true, s));
				default:
					throw new ArgumentException($"Unable to find a primitive converter for the supplied type {typeof(T).Name}.");
			}
		}

		///ISetting
		object ISetting.CurrentValue => CurrentValue;
		object ISetting.DefaultValue => DefaultValue;
	}
}