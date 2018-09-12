using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AdvorangesUtils;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// A generic class for a setting, specifying what the setting type is.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Setting<T> : ICompleteSetting<T>, ICompleteSetting
	{
		/// <inheritdoc />
		public string Description { get; set; }
		/// <inheritdoc />
		public string Information
		{
			get
			{
				var help = $"{MainName}: {Description}";
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
		/// Validates the passed in value.
		/// </summary>
		public Func<T, bool> Validation { get; set; } = x => true;
		/// <summary>
		/// Generates a new default value.
		/// </summary>
		public Func<T> DefaultValueFactory
		{
			get => _DefaultValueFactory;
			set
			{
				_DefaultValueFactory = value;
				HasBeenSet = true;
				SetDefault();
			}
		}
		/// <summary>
		/// Updates the current value.
		/// </summary>
		public Func<T, T> UpdateValueFactory { get; set; } = x => x;

		private readonly TryParseDelegate<T> _Parser;
		private readonly Action<T> _Setter;
		private readonly Func<T> _Getter;
		//Only used when getter is null, this caches changes from inside this class
		private T _LastSetValue;
		private Func<T> _DefaultValueFactory = () => default(T);
		private bool _IsFlag;

		/// <summary>
		/// Creates an instance of <see cref="Setting{T}"/> with full setter capabilities and optional getter capabilities.
		/// </summary>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="setter">The setter to use for this setting.</param>
		/// <param name="getter">The getter to use for this setting.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type.</param>
		public Setting(IEnumerable<string> names, Action<T> setter, Func<T> getter = default, TryParseDelegate<T> parser = default)
			: this(names, parser, false)
		{
			_Getter = getter;
			_Setter = setter ?? throw new ArgumentException(nameof(setter));
		}
		/// <summary>
		/// Creates an instance of <see cref="Setting{T}"/> with full getter and setter capabilities.
		/// </summary>
		/// <param name="names"></param>
		/// <param name="strongBox"></param>
		/// <param name="parser"></param>
		public Setting(IEnumerable<string> names, StrongBox<T> strongBox, TryParseDelegate<T> parser = default)
			: this(names, parser, false)
		{
			if (strongBox == null)
			{
				throw new ArgumentException(nameof(strongBox));
			}

			_Getter = () => strongBox.Value;
			_Setter = x => strongBox.Value = x;
		}
		/// <summary>
		/// Creates an instance of <see cref="Setting{T}"/> with setter and getter capabilities on itself.
		/// This is generally not useful and defeats the purpose of this class (modifying external values).
		/// </summary>
		/// <param name="dog"></param>
		/// <param name="names"></param>
		/// <param name="parser"></param>
		public Setting(IEnumerable<string> names, TryParseDelegate<T> parser = default)
			: this(names, parser, true) { }
		private Setting(IEnumerable<string> names, TryParseDelegate<T> parser, bool targetsSelf)
		{
			if (names == null || !names.Any())
			{
				throw new ArgumentException("Must supply at least one name.");
			}

			Names = names.ToImmutableArray();
			MainName = Names.First();
			IsHelp = Names.Any(x => x.CaseInsEquals("help") || x.CaseInsEquals("h"));
			_Parser = parser ?? GetPrimitiveParser();

			if (targetsSelf)
			{
				var sb = new StrongBox<T>();
				_Getter = () => sb.Value;
				_Setter = x => sb.Value = x;
			}
		}

		/// <inheritdoc />
		public void SetDefault()
			=> PrivateSet(DefaultValueFactory());
		public void Update()
			=> PrivateSet(UpdateValueFactory(_Getter()))
		/// <inheritdoc />
		public void Set(T value)
			=> PrivateSet(value);
		/// <summary>
		/// If a constructor is used which provides full getter capabilities, this will return the direct object.
		/// Otherwise this will return a value which is only updated when the value is changed from within this class.
		/// </summary>
		/// <returns></returns>
		public T GetValue()
			=> _Getter == null ? _LastSetValue : _Getter();
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
				PrivateSet(result);
			}
			catch (ArgumentException e)
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
		/// <summary>
		/// Validates the value then sets if valid.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private void PrivateSet(T value)
		{
			var valid = Validation(value);
			if (!valid)
			{
				throw new ArgumentException($"Validation failed for {MainName}.");
			}
			if (_Getter == null)
			{
				_LastSetValue = value;
			}
			_Setter(value);
		}

		//ICompleteSetting
		object IDirectGetter.GetValue() => GetValue();
		void IDirectSetter.Set(object value) => _Setter((T)value);
	}
}