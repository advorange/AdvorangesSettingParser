using System;
using System.Linq.Expressions;
using System.Reflection;
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
		/// <inheritdoc />
		public Action<T> Setter { get; }
		/// <inheritdoc />
		public Func<T> Getter { get; }

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
		/// Creates an instance of <see cref="Ref{T}"/>.
		/// </summary>
		/// <param name="selector">Allows getting and setting the value plus gives the name.</param>
		public Ref(Expression<Func<T>> selector)
			: this(GenerateSetter(selector), selector.Compile(), GetMemberExpression(selector).Member.Name) { }

		private static MemberExpression GetMemberExpression(LambdaExpression expression)
			=> expression.Body as MemberExpression ?? throw new ArgumentException($"Supplied expression is not a {nameof(MemberExpression)}.");
		private static Action<T> GenerateSetter(Expression<Func<T>> selector)
		{
			var body = GetMemberExpression(selector);
			switch (body.Member)
			{
				case PropertyInfo property:
				case FieldInfo field:
					var valueExp = Expression.Parameter(typeof(T));
					var assignExp = Expression.Assign(body, valueExp);
					return Expression.Lambda<Action<T>>(assignExp, valueExp).Compile();
				default:
					throw new ArgumentException("Can only target properties and fields.", nameof(selector));
			}
		}
	}
}