using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Utils;

namespace AdvorangesSettingParser.Implementation.Instance
{
	/// <summary>
	/// Acts as the ref keyword for multiple types other than fields.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Ref<T> : IRef<T>
	{
		/// <summary>
		/// Creates a reference to nothing.
		/// Setting will do nothing.
		/// Getting will return the default value.
		/// </summary>
		public static Ref<T> Nothing { get; } = new Ref<T>(x => { }, () => default);

		/// <inheritdoc />
		public string Name { get; }
		/// <summary>
		/// Sets the value.
		/// </summary>
		protected Action<T> Setter { get; set; }
		/// <summary>
		/// Gets the value.
		/// </summary>
		protected Func<T> Getter { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="Ref{T}"/>.
		/// </summary>
		/// <param name="strongbox">Holds the value.</param>
		/// <param name="name">The name of the targeted value.</param>
		public Ref(StrongBox<T> strongbox, string name = null)
			: this(x => strongbox.Value = x, () => strongbox.Value, name) { }
		/// <summary>
		/// Creates an instance of <see cref="Ref{T}"/>. The targeted value MUST have both a getter and setter and must NOT be in a value type.
		/// </summary>
		/// <param name="selector">Allows getting and setting the value plus gives the name.</param>
		/// <remarks>
		/// This cannot be in a value type because unless the struct is local none of the fields will be set on it correctly.
		/// This results in a compile time error with <see cref="Action{T}"/> whereas here it's extremely easy to miss and will never throw an exception.
		/// </remarks>
		public Ref(Expression<Func<T>> selector)
			: this(selector.GenerateSetter(), selector.Compile(), selector.GetMemberExpression().Member.Name) { }
		/// <summary>
		/// Creates an instance of <see cref="Ref{T}"/>.
		/// </summary>
		/// <param name="setter">Sets the value.</param>
		/// <param name="getter">Gets the value.</param>
		/// <param name="name">The name of the targeted value.</param>
		protected Ref(Action<T> setter, Func<T> getter, string name = null)
		{
			Name = name;
			Setter = setter;
			Getter = getter;
		}

		/// <inheritdoc />
		public T GetValue()
			=> Getter();
		/// <inheritdoc />
		public void SetValue(T value)
			=> Setter(value);
		/// <summary>
		/// Creates a <see cref="Ref{T}"/> from an action and func targeting a something on a struct.
		/// THIS WILL ONLY WORK ON LOCAL STRUCTS.
		/// </summary>
		/// <param name="setter"></param>
		/// <param name="getter"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Ref<T> FromStruct(Action<T> setter, Func<T> getter, string name = null)
			=> new Ref<T>(setter, getter, name);
		/// <summary>
		/// Converts <see cref="Ref{T}"/> to its held value.
		/// </summary>
		/// <param name="instance"></param>
		public static implicit operator T(Ref<T> instance) => instance.GetValue();
		/// <summary>
		/// Converts an expression into <see cref="Ref{T}"/>.
		/// </summary>
		/// <param name="selector"></param>
		/// <remarks>Generally useless because lambda expressions default to the non expression value.</remarks>
		public static implicit operator Ref<T>(Expression<Func<T>> selector) => new Ref<T>(selector);
	}
}