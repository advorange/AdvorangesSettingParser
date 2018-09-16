using System;
using System.Collections.Generic;
using System.Linq;
using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Implementation.Instance;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class CollectionSettingTests
	{
		private readonly TestClass TestClass;
		private readonly SettingParser SettingParser;
		private readonly string Prefix;

		public CollectionSettingTests()
		{
			TestClass = new TestClass();
			SettingParser = new SettingParser
			{
				new CollectionSetting<string>(() => TestClass.CollectionStrings)
				{
					ResetValueFactory = x => { x.Clear(); return x; },
					EqualityComparer = StringComparer.OrdinalIgnoreCase,
				},
				new CollectionSetting<string>(() => TestClass.ListStrings)
				{
					ResetValueFactory = x => new List<string>() { "a", "q", "k" },
					EqualityComparer = StringComparer.OrdinalIgnoreCase,
				},
				new CollectionSetting<string>(() => TestClass.FilledListStrings)
				{
					DefaultValue = new List<string> { "a", "b", "c", "d" },
					EqualityComparer = StringComparer.OrdinalIgnoreCase,
				},
				new CollectionSetting<int>(() => TestClass.ListInts)
				{
					DefaultValue = new List<int> { 1, 2, 3 },
				}
			};
			Prefix = SettingParser.Prefixes.First();
		}

		[TestMethod]
		public void Init_Test()
		{
			Assert.AreEqual(0, TestClass.CollectionStrings.Count);
			Assert.AreEqual(0, TestClass.ListStrings.Count);
			Assert.AreEqual(4, TestClass.FilledListStrings.Count);
			Assert.AreEqual(3, TestClass.ListInts.Count);
		}
		[TestMethod]
		public void Modify_Test()
		{
			var target = nameof(TestClass.ListInts);
			var str = Prefix + target;
			SettingParser.TryGetSetting(target, PrefixState.NotPrefixed, out var setting);
			var value = (IList<int>)setting.GetValue();
			Assert.AreEqual(3, value.Count);
			SettingParser.Parse($"{str} 50"); //add 50
			Assert.AreEqual(4, value.Count);
			SettingParser.Parse($"{str} 1"); //remove 1
			Assert.AreEqual(3, value.Count);
			SettingParser.Parse($"{str} 2 {str} 3"); //remove 2 and 3
			Assert.AreEqual(1, value.Count);
			SettingParser.Parse($"{str} 100"); //add 100
			Assert.AreEqual(2, value.Count);
			SettingParser.Parse($"{str} 50 {str} 10"); //remove 50 add 10
			Assert.AreEqual(2, value.Count);
			//Should end up with 100 and 10
			Assert.AreEqual(100, value[0]);
			Assert.AreEqual(10, value[1]);
		}
		[TestMethod]
		public void Set_Test()
		{
			var list = new[] { "dog", "cat", "fish" };
			SettingParser.TryGetSetting(nameof(TestClass.ListStrings), PrefixState.NotPrefixed, out var setting);
			Assert.AreNotEqual(null, setting);
			var value = (ICollection<string>)setting.GetValue();
			Assert.AreEqual(0, value.Count);
			setting.SetValue(list);
			for (int i = 0; i < list.Length; ++i)
			{
				Assert.AreEqual(list[i], value.ElementAt(i));
			}
		}
		[TestMethod]
		public void Reset_Test()
		{
			SettingParser.TryGetSetting(nameof(TestClass.FilledListStrings), PrefixState.NotPrefixed, out var setting);
			Assert.AreNotEqual(null, setting);
			var value = (ICollection<string>)setting.GetValue();
			Assert.AreNotEqual(0, value.Count);
			setting.ResetValue();
			Assert.AreEqual(0, value.Count);
		}
		[TestMethod]
		public void CMAction_Test()
		{
			var target = nameof(TestClass.CollectionStrings);
			var str = Prefix + target;
			SettingParser.TryGetSetting(target, PrefixState.NotPrefixed, out var setting);
			Assert.AreNotEqual(null, setting);
			var value = (ICollection<string>)setting.GetValue();
			Assert.AreEqual(0, value.Count);
			SettingParser.Parse($@"{str} ""{CMAction.Add} dog""");
			Assert.AreEqual(1, value.Count);
			SettingParser.Parse($@"{str} ""{CMAction.AddIfMissing} dog""");
			Assert.AreEqual(1, value.Count);
			SettingParser.Parse($@"{str} ""{CMAction.AddIfMissing} cat"" {str} ""{CMAction.Toggle} fish""");
			Assert.AreEqual(3, value.Count);
			var result = SettingParser.Parse($@"{str} ""{CMAction.Remove} asdflkj""");
			Assert.AreEqual(1, result.Errors.Count());
		}
	}
}
