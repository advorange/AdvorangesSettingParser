﻿using System;
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
	/// <typeparam name="TValue"></typeparam>
	public class Setting<TValue> : ICompleteSetting<TValue>, ICompleteSetting
	{
		/// <inheritdoc />
		public string Description { get; set; }
		/// <inheritdoc />
		public string Information
		{
			get
			{
				var help = $"{MainName}: {Description}";
				if (typeof(TValue).IsEnum)
				{
					help += $"{Environment.NewLine}Acceptable values: {string.Join(", ", Enum.GetNames(typeof(TValue)))}";
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
				if (value && typeof(TValue) != typeof(bool))
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
		/// Parses the type from a string.
		/// </summary>
		public TryParseDelegate<TValue> Parser
		{
			get => _Parser;
			set => _Parser = value ?? throw new ArgumentException(nameof(Parser));
		}
		/// <summary>
		/// Validates the passed in value.
		/// </summary>
		public Func<TValue, bool> Validation
		{
			get => _Validation;
			set => _Validation = value ?? throw new ArgumentException(nameof(Validation));
		}
		/// <summary>
		/// Does something with the current value then returns either the modified value or a new value.
		/// </summary>
		public Func<TValue, TValue> ResetValueFactory
		{
			get => _ResetValueFactory;
			set => _ResetValueFactory = value ?? throw new ArgumentException(nameof(ResetValueFactory));
		}
		/// <summary>
		/// Sets the default value for this.
		/// </summary>
		public TValue DefaultValue
		{
			get => _DefaultValue;
			set => Set(_DefaultValue = value);
		}

		private TryParseDelegate<TValue> _Parser { get; set; }
		private Func<TValue, bool> _Validation { get; set; } = x => true;
		private Func<TValue, TValue> _ResetValueFactory { get; set; } = x => x;
		private Action<TValue> _Setter { get; set; }
		private Func<TValue> _Getter { get; set; }
		private TValue _DefaultValue { get; set; }
		private bool _IsFlag { get;set; }

		/// <summary>
		/// Creates an instance of <see cref="Setting{TValue}"/> with full setter and getter capabilities.
		/// <paramref name="getter"/> and <paramref name="setter"/> should lead to the same value for best results.
		/// </summary>
		/// <param name="setter">The setter to use for this setting.</param>
		/// <param name="getter">The getter to use for this setting.</param>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type.</param>
		public Setting(Action<TValue> setter, Func<TValue> getter, IEnumerable<string> names, TryParseDelegate<TValue> parser = default)
			: this(names, parser, false)
		{
			_Getter = getter ?? throw new ArgumentException(nameof(getter));
			_Setter = setter ?? throw new ArgumentException(nameof(setter));
		}
		/// <summary>
		/// Creates an instance of <see cref="Setting{TValue}"/> with full setter and getter capabilities targetting the supplied value.
		/// The selector's targeted value will be the main name.
		/// </summary>
		/// <param name="selector">The targeted value. This can be any property/field local/instance/global/static in a class/struct.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type.</param>
		public Setting(Expression<Func<TValue>> selector, TryParseDelegate<TValue> parser = default)
			: this(selector, new[] { GetMemberExpression(selector).Member.Name }, parser) { }
		/// <summary>
		/// Creates an instance of <see cref="Setting{TValue}"/> with full setter and getter capabilities targetting the supplied value.
		/// </summary>
		/// <param name="names">The names to use for this setting. Must supply at least one name. The first name will be designated the main name.</param>
		/// <param name="selector">The targeted value. This can be any property/field local/instance/global/static in a class/struct.</param>
		/// <param name="parser">The converter to convert from a string to the value. Can be null if a primitive type.</param>
		public Setting(Expression<Func<TValue>> selector, IEnumerable<string> names, TryParseDelegate<TValue> parser = default)
			: this(GenerateSetter(selector), selector.Compile(), names, parser) { }
		/// <summary>
		/// Creates an instance of <see cref="Setting{TValue}"/> with full setter and getter capabilities targetting the strongbox.
		/// </summary>
		/// <param name="names"></param>
		/// <param name="strongBox"></param>
		/// <param name="parser"></param>
		public Setting(StrongBox<TValue> strongBox, IEnumerable<string> names, TryParseDelegate<TValue> parser = default)
			: this(() => strongBox.Value, names, parser) { }
		/// <summary>
		/// Creates an instance of <see cref="Setting{TValue}"/> with setter and getter capabilities on itself.
		/// This is generally not useful and defeats the purpose of this class which is modifying external values.
		/// </summary>
		/// <param name="names"></param>
		/// <param name="parser"></param>
		public Setting(IEnumerable<string> names, TryParseDelegate<TValue> parser = default)
			: this(names, parser, true) { }
		/// <summary>
		/// Sets the names and parser. If <paramref name="targetsSelf"/> is true links the getter and setter to a <see cref="StrongBox{T}"/>.
		/// </summary>
		/// <param name="names"></param>
		/// <param name="parser"></param>
		/// <param name="targetsSelf"></param>
		protected Setting(IEnumerable<string> names, TryParseDelegate<TValue> parser, bool targetsSelf)
		{
			if (names == null || !names.Any())
			{
				throw new ArgumentException("Must supply at least one name.");
			}

			Names = names.ToImmutableArray();
			MainName = names.First();
			IsHelp = names.Any(x => x.CaseInsEquals("help") || x.CaseInsEquals("h"));
			_Parser = parser ?? GetPrimitiveParser();

			if (targetsSelf)
			{
				var sb = new StrongBox<TValue>();
				_Getter = () => sb.Value;
				_Setter = x => sb.Value = x;
			}
		}

		/// <inheritdoc />
		public void Reset()
			=> PrivateSet(ResetValueFactory(_Getter()));
		/// <inheritdoc />
		public void Set(TValue value)
			=> PrivateSet(value);
		/// <summary>
		/// If a constructor is used which provides full getter capabilities, this will return the direct object.
		/// Otherwise this will return a value which is only updated when the value is changed from within this class.
		/// </summary>
		/// <returns></returns>
		public TValue GetValue()
			=> _Getter();
		/// <inheritdoc />
		public bool TrySetValue(string value, out string response)
		{
			TValue result = default;
			if (value.CaseInsEquals("default")) //Let default just pass on through
			{
				result = default;
			}
			else
			{
				if (!_Parser(value, out var converted))
				{
					response = $"Unable to convert '{value}' to type {typeof(TValue).Name}.";
					return false;
				}
				result = converted;
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
			=> $"{MainName} ({typeof(TValue).Name})";
		/// <summary>
		/// Attempts to get a parser for a primitive type.
		/// </summary>
		/// <returns></returns>
		private TryParseDelegate<TValue> GetPrimitiveParser()
		{
			bool StringTryParse(string s, out string value)
			{
				value = s;
				return true;
			}

			switch (typeof(TValue).Name)
			{
				//I know this looks bad, but it's the easiest way to do this. It is still type safe anyways.
				case nameof(SByte):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<sbyte>(sbyte.TryParse);
				case nameof(Byte):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<byte>(byte.TryParse);
				case nameof(Int16):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<short>(short.TryParse);
				case nameof(UInt16):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<ushort>(ushort.TryParse);
				case nameof(Int32):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<int>(int.TryParse);
				case nameof(UInt32):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<uint>(uint.TryParse);
				case nameof(Int64):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<long>(long.TryParse);
				case nameof(UInt64):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<ulong>(ulong.TryParse);
				case nameof(Char):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<char>(char.TryParse);
				case nameof(Single):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<float>(float.TryParse);
				case nameof(Double):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<double>(double.TryParse);
				case nameof(Boolean):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<bool>(bool.TryParse);
				case nameof(Decimal):
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<decimal>(decimal.TryParse);
				case nameof(String):
					//Instead of having to do special checks, just use this dumb delegate
					return (TryParseDelegate<TValue>)(object)new TryParseDelegate<string>(StringTryParse);
				default:
					throw new ArgumentException($"Unable to find a primitive converter for the supplied type {typeof(TValue).Name}.");
			}
		}
		/// <summary>
		/// Validates the value then sets if valid.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private void PrivateSet(TValue value)
		{
			if (!Validation(value))
			{
				throw new ArgumentException($"Validation failed for {MainName}.");
			}
			_Setter(value);
			HasBeenSet = true;
		}
		private static MemberExpression GetMemberExpression(LambdaExpression expression)
			=> expression.Body as MemberExpression ?? throw new ArgumentException($"Supplied expression is not a {nameof(MemberExpression)}.");
		private static Action<TValue> GenerateSetter(Expression<Func<TValue>> selector)
		{
			var body = GetMemberExpression(selector);
			switch (body.Member)
			{
				case PropertyInfo property:
				case FieldInfo field:
					var valueExp = Expression.Parameter(typeof(TValue));
					var assignExp = Expression.Assign(body, valueExp);
					return Expression.Lambda<Action<TValue>>(assignExp, valueExp).Compile();
				default:
					throw new ArgumentException("Can only target properties and fields.", nameof(selector));
			}
		}

		//ICompleteSetting
		object IDirectGetter.GetValue() => GetValue();
		void IDirectSetter.Set(object value) => _Setter((TValue)value);
	}
}