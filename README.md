# AdvorangesSettingParser
Basic setting parser for C# which doesn't use any attributes meaning it's easy to use even on classes you don't have the ability to add attributes to.

```
public class Example
{
	public int Property { get; set; }
	public string Field;
	public IList<string> Collection { get; } = new List<string>();
	public DateTime NeedsParser { get; set; }

	public ISettingParser SettingParser { get; }

	public Example()
	{
		int localVariable = 1;
		SettingParser = new SettingParser
		{
			//Can target properties (cannot be getter only unless you implement it yourself via reflection)
			new Setting<int>(() => Property),
			//Can target fields (cannot be readonly unless you implement it yourself via reflection)
			new Setting<string>(() => Field),
			//Can target getter only collections
			new CollectionSetting<string>(() => Collection),
			//Can target local variables
			new Setting<int>(() => localVariable),
			//Supply your own parsers for things other than primitives and enums
			new Setting<DateTime>(() => NeedsParser, parser: DateTime.TryParse),
		};

		string prefix = SettingParser.Prefixes.First();
		string input = $"{prefix}{nameof(Property)} 20 " +
			$"{prefix}{nameof(Field)} asdf " +
			$"{prefix}{nameof(localVariable)} 32";
		ISettingParserResult result = SettingParser.Parse(input);
		Console.WriteLine(string.Join("\n", result.Errors));
		Console.WriteLine(string.Join("\n", result.Successes));
		Console.WriteLine(string.Join("\n", result.Help));
		Console.WriteLine(string.Join("\n", result.UnusedParts));

		//Modifying a collection is slightly trickier than the rest
		//An action will need to be supplied in quotes with the value unless you're fine with using the default value of Toggle
		//Valid actions are: Toggle, AddAlways, AddIfMissing, RemoveAlways, and RemoveIfExists
		string collectionInput = $"{prefix}{nameof(Collection)} first " +
			$"{prefix}{nameof(Collection)} \"AddAlways second\" " +
			$"{prefix}{nameof(Collection)} \"Toggle first\" " +
			$"{prefix}{nameof(Collection)} \"AddIfMissing first\"";
		SettingParser.Parse(collectionInput);
		//Will result in having: second, first
	}
}
```

This works by abusing `Expression<Func<T>>` inside the `Ref<T>` class.

Speaking of `Ref<T>`, here's a table of its speeds on 50,000,000 gets and sets:

| Test Name | Time (ms) |
|---|---|
| Direct getter and setter | 52 |
| `Expression<Func<T>>` | 479 |
| `StrongBox<T>` | 381 |

While `Expression<Func<T>>` is the slowest at roughly 9x the speed of direct getting and setting, it's the easiest to use and can even work for private setter properties. However, `Expression<Func<T>>` will throw a runtime exception if the source of the property/field is a struct because it's impossible to tell if the struct is a local value or not local and thus will lead to a copy.