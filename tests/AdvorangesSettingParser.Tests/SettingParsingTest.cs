using System;
using System.Linq;
using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Implementation.Instance;
using AdvorangesSettingParser.Implementation.Static;
using AdvorangesSettingParser.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class SettingParsingTest
	{
		private static readonly StaticSettingParser<TestClass> StaticSettingParser = new StaticSettingParser<TestClass>
		{
			new StaticSetting<TestClass, string>(x => x.StringValue),
			new StaticSetting<TestClass, int>(x => x.IntValue),
			new StaticSetting<TestClass, bool>(x => x.BoolValue),
			new StaticSetting<TestClass, bool>(x => x.FlagValue) { IsFlag = true, },
			new StaticSetting<TestClass, bool>(x => x.FlagValue2) { IsFlag = true, },
			new StaticSetting<TestClass, ulong>(x => x.UlongValue),
			new StaticSetting<TestClass, DateTime>(x => x.DateTimeValue, parser: DateTime.TryParse),
			new StaticCollectionSetting<TestClass, string>(x => x.CollectionStrings),
		};
		private readonly TestClass Instance;
		private readonly SettingParser SettingParser;
		private readonly string Prefix;

		public SettingParsingTest()
		{
			Instance = new TestClass();
			SettingParser = new SettingParser
			{
				new Setting<string>(() => Instance.StringValue),
				new Setting<int>(() => Instance.IntValue),
				new Setting<bool>(() => Instance.BoolValue),
				new Setting<bool>(() => Instance.FlagValue) { IsFlag = true, },
				new Setting<bool>(() => Instance.FlagValue2) { IsFlag = true, },
				new Setting<ulong>(() => Instance.UlongValue),
				new Setting<DateTime>(() => Instance.DateTimeValue, parser: DateTime.TryParse),
				new CollectionSetting<string>(() => Instance.CollectionStrings),
			};
			Prefix = SettingParser.Prefixes.First();
		}

		[TestMethod]
		public void BasicParsing_Test()
		{
			BasicParsing(SettingParser, Instance);
			var newClass = new TestClass();
			BasicParsing(StaticSettingParser, newClass);
		}
		[TestMethod]
		public void FlagParsing_Test()
		{
			FlagParsing(SettingParser, Instance);
			var newClass = new TestClass();
			FlagParsing(StaticSettingParser, newClass);
		}
		[TestMethod]
		public void ComplicatedParsing_Test()
		{
			ComplicatedParsing(SettingParser, Instance);
			var newClass = new TestClass();
			ComplicatedParsing(StaticSettingParser, newClass);
		}
		[TestMethod]
		public void CollectionParsing_Test()
		{
			CollectionParsing(SettingParser, Instance);
			var newClass = new TestClass();
			CollectionParsing(StaticSettingParser, newClass);
		}
		private static void BasicParsing(ISettingParser parser, TestClass source)
		{
			var prefix = parser.Prefixes.First();
			parser.Parse(source, $"{prefix}{nameof(TestClass.StringValue)} StringValueTest");
			Assert.AreEqual("StringValueTest", source.StringValue);
			parser.Parse(source, $"{prefix}{nameof(TestClass.IntValue)} 1");
			Assert.AreEqual(1, source.IntValue);
			parser.Parse(source, $"{prefix}{nameof(TestClass.BoolValue)} true");
			Assert.AreEqual(true, source.BoolValue);

			parser.Parse(source, $"{prefix}{nameof(TestClass.UlongValue)} 18446744073709551615");
			Assert.AreEqual(ulong.MaxValue, source.UlongValue);
			parser.Parse(source, $"{prefix}{nameof(TestClass.DateTimeValue)} 05/06/2018");
			Assert.AreEqual(new DateTime(2018, 5, 6), source.DateTimeValue);
		}
		private static void FlagParsing(ISettingParser parser, TestClass source)
		{
			var prefix = parser.Prefixes.First();
			Assert.AreEqual(false, source.FlagValue);
			parser.Parse(source, $"{prefix}{nameof(TestClass.FlagValue)}");
			Assert.AreEqual(true, source.FlagValue);
			parser.Parse(source, $"{prefix}{nameof(TestClass.FlagValue)} false");
			Assert.AreEqual(false, source.FlagValue);
		}
		private static void ComplicatedParsing(ISettingParser parser, TestClass source)
		{
			var testStr = "Space \"Double deep quote\" Test";
			var prefix = parser.Prefixes.First();
			var args = $"{prefix}{nameof(TestClass.StringValue)} \"{testStr}\" " +
				$"{prefix}{nameof(TestClass.FlagValue2)} " +
				$"{prefix}{nameof(TestClass.BoolValue)} true " +
				$"{prefix}{nameof(TestClass.UlongValue)} asdf " +
				$"{prefix}help {nameof(TestClass.FlagValue2)} " +
				$"extra";
			var result = parser.Parse(source, args);
			Assert.AreEqual(3, result.Successes.Count());
			Assert.AreEqual(1, result.Errors.Count());
			Assert.AreEqual(1, result.UnusedParts.Count());
			Assert.AreEqual(1, result.Help.Count());
			Assert.AreEqual(testStr, source.StringValue);
			Assert.AreEqual(true, source.FlagValue2);
			Assert.AreEqual(true, source.BoolValue);
			Assert.AreEqual(default(ulong), source.UlongValue);
		}
		private static void CollectionParsing(ISettingParser parser, TestClass source)
		{
			var prefix = parser.Prefixes.First();
			var result = parser.Parse(source, $"{prefix}{nameof(TestClass.CollectionStrings)} CollectionValue");
			Assert.AreEqual(1, result.Successes.Count());
			Assert.AreEqual("CollectionValue", source.CollectionStrings.First());
		}
	}
}