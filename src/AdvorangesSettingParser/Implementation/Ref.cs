using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Acts as the ref keyword for multiple types other than fields.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class Ref<T> : IRef<T>
	{
		/// <summary>
		/// Creates a reference to nothing.
		/// Setting will do nothing.
		/// Getting will return the default value.
		/// </summary>
		public static Ref<T> Nothing { get; } = new Ref<T>(x => { }, () => default);

		/// <inheritdoc />
		public string Name { get; }
		private Action<T> Setter { get; }
		private Func<T> Getter { get; }

		/// <summary>
		/// Creates an instance of <see cref="Ref{T}"/>.
		/// </summary>
		/// <param name="setter">Sets the value.</param>
		/// <param name="getter">Gets the value.</param>
		/// <param name="name">The name of the targeted value.</param>
		public Ref(Action<T> setter, Func<T> getter, string name = null)
		{
			Name = name;
			Setter = setter;
			Getter = getter;
		}
		/// <summary>
		/// Creates an instance of <see cref="Ref{T}"/>.
		/// </summary>
		/// <param name="strongbox">Holds the value.</param>
		/// <param name="name">The name of the targeted value.</param>
		public Ref(StrongBox<T> strongbox, string name = null)
			: this(x => strongbox.Value = x, () => strongbox.Value, name) { }
		/// <summary>
		/// Creates an instance of <see cref="Ref{T}"/>. The targeted value MUST have both a getter and setter.
		/// </summary>
		/// <param name="selector">Allows getting and setting the value plus gives the name.</param>
		public Ref(Expression<Func<T>> selector)
			: this(selector.GenerateSetter(), selector.Compile(), selector.GetMemberExpression().Member.Name) { }

		/// <inheritdoc />
		public T GetValue() => Getter();
		/// <inheritdoc />
		public void SetValue(T value) => Setter(value);
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