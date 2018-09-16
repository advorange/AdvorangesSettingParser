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
			};
			Prefix = SettingParser.Prefixes.First();
		}

		[TestMethod]
		public void BasicParsing_Test()
		{
			BasicParsing(SettingParser, (setting, value) => setting.TrySetValue(value), Instance);
			var newClass = new TestClass();
			BasicParsing(StaticSettingParser, (setting, value) => setting.TrySetValue(newClass, value), newClass);
		}
		[TestMethod]
		public void FlagParsing_Test()
		{
			FlagParsing(SettingParser, (setting, value) => setting.TrySetValue(value), Instance);
			var newClass = new TestClass();
			FlagParsing(StaticSettingParser, (setting, value) => setting.TrySetValue(newClass, value), newClass);
		}
		[TestMethod]
		public void ComplicatedParsing_Test()
		{
			CopmlicatedParsing(SettingParser, (setting, value) => setting.TrySetValue(value), Instance);
			var newClass = new TestClass();
			CopmlicatedParsing(StaticSettingParser, (setting, value) => setting.TrySetValue(newClass, value), newClass);
		}
		private static void BasicParsing<T>(SettingParserBase<T> parser, Func<T, string, IResult> setter, TestClass source) where T : ISettingMetadata
		{
			var prefix = parser.Prefixes.First();
			parser.Parse($"{prefix}{nameof(TestClass.StringValue)} StringValueTest", setter);
			Assert.AreEqual("StringValueTest", source.StringValue);
			parser.Parse($"{prefix}{nameof(TestClass.IntValue)} 1", setter);
			Assert.AreEqual(1, source.IntValue);
			parser.Parse($"{prefix}{nameof(TestClass.BoolValue)} true", setter);
			Assert.AreEqual(true, source.BoolValue);

			parser.Parse($"{prefix}{nameof(TestClass.UlongValue)} 18446744073709551615", setter);
			Assert.AreEqual(ulong.MaxValue, source.UlongValue);
			parser.Parse($"{prefix}{nameof(TestClass.DateTimeValue)} 05/06/2018", setter);
			Assert.AreEqual(new DateTime(2018, 5, 6), source.DateTimeValue);
		}
		private static void FlagParsing<T>(SettingParserBase<T> parser, Func<T, string, IResult> setter, TestClass source) where T : ISettingMetadata
		{
			var prefix = parser.Prefixes.First();
			Assert.AreEqual(false, source.FlagValue);
			parser.Parse($"{prefix}{nameof(TestClass.FlagValue)}", setter);
			Assert.AreEqual(true, source.FlagValue);
			parser.Parse($"{prefix}{nameof(TestClass.FlagValue)} false", setter);
			Assert.AreEqual(false, source.FlagValue);
		}
		private static void CopmlicatedParsing<T>(SettingParserBase<T> parser, Func<T, string, IResult> setter, TestClass source) where T : ISettingMetadata
		{
			var prefix = parser.Prefixes.First();
			var args = $"{prefix}{nameof(TestClass.StringValue)} StringValueTest2 " +
				$"{prefix}{nameof(TestClass.FlagValue2)} " +
				$"{prefix}{nameof(TestClass.BoolValue)} true " +
				$"{prefix}{nameof(TestClass.UlongValue)} asdf " +
				$"{prefix}help {nameof(TestClass.FlagValue2)} " +
				$"extra";
			var result = parser.Parse(args, setter);
			Assert.AreEqual(3, result.Successes.Count());
			Assert.AreEqual(1, result.Errors.Count());
			Assert.AreEqual(1, result.UnusedParts.Count());
			Assert.AreEqual(1, result.Help.Count());
			Assert.AreEqual("StringValueTest2", source.StringValue);
			Assert.AreEqual(true, source.FlagValue2);
			Assert.AreEqual(true, source.BoolValue);
			Assert.AreEqual(default(ulong), source.UlongValue);
		}
	}
}