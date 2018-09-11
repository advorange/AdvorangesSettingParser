using System;
using System.Linq;
using AdvorangesSettingParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class SettingParsingTest
	{
		private readonly TestClass TestClass;
		private readonly SettingParser SettingParser;
		private readonly string Prefix;

		public SettingParsingTest()
		{
			TestClass = new TestClass();
			SettingParser = new SettingParser
			{
				new Setting<string>(new[] { nameof(TestClass.StringValue), }, x => TestClass.StringValue = x),
				new Setting<int>(new[] { nameof(TestClass.IntValue), }, x => TestClass.IntValue = x),
				new Setting<bool>(new[] { nameof(TestClass.BoolValue), }, x => TestClass.BoolValue = x),
				new Setting<bool>(new[] { nameof(TestClass.FlagValue), }, x => TestClass.FlagValue = x) { IsFlag = true, },
				new Setting<bool>(new[] { nameof(TestClass.FlagValue2), }, x => TestClass.FlagValue2 = x) { IsFlag = true, },
				new Setting<ulong>(new[] { nameof(TestClass.UlongValue), }, x => TestClass.UlongValue = x),
				new Setting<DateTime>(new[] { nameof(TestClass.DateTimeValue), }, x => TestClass.DateTimeValue = x, parser: ConvertDatetime),
			};
			Prefix = SettingParser.Prefixes.First();
		}
		private (bool Valid, DateTime value) ConvertDatetime(string input) => (DateTime.TryParse(input, out var result), result);

		[TestMethod]
		public void BasicParsing_Test()
		{
			SettingParser.Parse($"{Prefix}{nameof(TestClass.StringValue)} StringValueTest");
			Assert.AreEqual("StringValueTest", TestClass.StringValue);
			SettingParser.Parse($"{Prefix}{nameof(TestClass.IntValue)} 1");
			Assert.AreEqual(1, TestClass.IntValue);
			SettingParser.Parse($"{Prefix}{nameof(TestClass.BoolValue)} true");
			Assert.AreEqual(true, TestClass.BoolValue);

			SettingParser.Parse($"{Prefix}{nameof(TestClass.UlongValue)} 18446744073709551615");
			Assert.AreEqual(ulong.MaxValue, TestClass.UlongValue);
			SettingParser.Parse($"{Prefix}{nameof(TestClass.DateTimeValue)} 05/06/2018");
			Assert.AreEqual(new DateTime(2018, 5, 6), TestClass.DateTimeValue);
		}
		[TestMethod]
		public void FlagParsing_Test()
		{
			Assert.AreEqual(false, TestClass.FlagValue);
			SettingParser.Parse($"{Prefix}{nameof(TestClass.FlagValue)}");
			Assert.AreEqual(true, TestClass.FlagValue);
			SettingParser.Parse($"{Prefix}{nameof(TestClass.FlagValue)}");
			Assert.AreEqual(false, TestClass.FlagValue);
		}
		[TestMethod]
		public void ComplicatedParsing_Test()
		{
			var result = SettingParser.Parse($"{Prefix}{nameof(TestClass.StringValue)} StringValueTest2 " +
				$"{Prefix}{nameof(TestClass.FlagValue2)} " +
				$"{Prefix}{nameof(TestClass.BoolValue)} true " +
				$"{Prefix}{nameof(TestClass.UlongValue)} asdf " +
				$"{Prefix}help {nameof(TestClass.FlagValue2)} " +
				$"extra");
			Assert.AreEqual(3, result.Successes.Count());
			Assert.AreEqual(1, result.Errors.Count());
			Assert.AreEqual(1, result.UnusedParts.Count());
			Assert.AreEqual(1, result.Help.Count());
			Assert.AreEqual("StringValueTest2", TestClass.StringValue);
			Assert.AreEqual(true, TestClass.FlagValue2);
			Assert.AreEqual(true, TestClass.BoolValue);
			Assert.AreEqual(default(ulong), TestClass.UlongValue);
		}
	}
}