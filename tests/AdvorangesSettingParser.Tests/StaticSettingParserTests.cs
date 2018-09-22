using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Implementation.Static;
using AdvorangesSettingParser.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class StaticSettingParserTests
	{
		static StaticSettingParserTests()
		{
			new StaticSettingParser<TestClass>()
			{
				new StaticSetting<TestClass, string>(x => x.StringValue),
				new StaticSetting<TestClass, ulong>(x => x.UlongValue),
			}.Register();
		}

		[TestMethod]
		public void TestNeededSettings_Test()
		{
			var parser = StaticSettingParserRegistry.Instance.Retrieve<TestClass>();
			var prefix = parser.Prefixes.First();

			var args1 = $"{prefix}{nameof(TestClass.StringValue)} Test";
			var args2 = $"{prefix}{nameof(TestClass.UlongValue)} 10";

			var object1 = new TestClass();
			Assert.AreEqual(2, parser.GetNeededSettings(object1).Count());
			parser.Parse(object1, args1);
			Assert.AreEqual(1, parser.GetNeededSettings(object1).Count());
			parser.Parse(object1, args2);
			Assert.AreEqual(0, parser.GetNeededSettings(object1).Count());

			var object2 = new TestClass();
			Assert.AreEqual(2, parser.GetNeededSettings(object2).Count());
			parser.Parse(object2, args1);
			Assert.AreEqual(1, parser.GetNeededSettings(object2).Count());
			parser.Parse(object2, args2);
			Assert.AreEqual(0, parser.GetNeededSettings(object2).Count());
		}
		[TestMethod]
		public void TestRemovedWhenGC_Test()
		{
			//To account for earlier tests
			GC.Collect();
			GC.WaitForPendingFinalizers();

			var parser = StaticSettingParserRegistry.Instance.Retrieve<TestClass>();
			var prefix = parser.Prefixes.First();

			var args1 = $"{prefix}{nameof(TestClass.StringValue)} Test";
			var args2 = $"{prefix}{nameof(TestClass.UlongValue)} 10";

			var object1 = new TestClass();
			Assert.AreEqual(2, parser.GetNeededSettings(object1).Count());
			parser.Parse(object1, args1);
			Assert.AreEqual(1, parser.GetNeededSettings(object1).Count());
			parser.Parse(object1, args2);
			Assert.AreEqual(0, parser.GetNeededSettings(object1).Count());

			//Use reflection to evaluate how many references are being stored
			var properties = parser.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
			var property = properties.Single(x => x.Name == "UnsetSettings");
			var value = (IEnumerable)property.GetValue(parser);

			new Action(() =>
			{
				var object2 = new TestClass();
				Assert.AreEqual(2, parser.GetNeededSettings(object2).Count());
				parser.Parse(object2, args1);
				Assert.AreEqual(1, parser.GetNeededSettings(object2).Count());
				parser.Parse(object2, args2);
				Assert.AreEqual(0, parser.GetNeededSettings(object2).Count());

				var nonGCCount = 0;
				foreach (var element in value)
				{
					++nonGCCount;
				}
				Assert.AreEqual(2, nonGCCount);
			}).Invoke();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var GCCount = 0;
			foreach (var element in value)
			{
				++GCCount;
			}

			//In release the variable outside the action can be GC'd before it would be in debug.
#if DEBUG
			Assert.AreEqual(1, GCCount);
#else
			Assert.AreEqual(0, GCCount);
#endif
		}
	}
}
