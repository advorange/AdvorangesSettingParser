using System;
using AdvorangesSettingParser.Implementation.Instance;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class MutuallyExclusiveTest
	{
		[TestMethod]
		public void MutuallyExclusive_Test()
		{
			var instanceInstance = new TestClass();
			var instanceParser = new SettingParser
			{
				new Setting<string>(() => instanceInstance.StringValue),
				new Setting<int>(() => instanceInstance.IntValue),
				new Setting<bool>(() => instanceInstance.BoolValue),
				new Setting<bool>(() => instanceInstance.FlagValue) { IsFlag = true, Group = 1, },
				new Setting<bool>(() => instanceInstance.FlagValue2) { IsFlag = true, Group = 1, },
				new Setting<ulong>(() => instanceInstance.UlongValue),
				new Setting<DateTime>(() => instanceInstance.DateTimeValue, parser: DateTime.TryParse),
				new CollectionSetting<string>(() => instanceInstance.CollectionStrings),
			};

			var requiredAmt = instanceParser.GetNeededSettings().Count;
			instanceParser.Parse($"{instanceParser.MainPrefix}{nameof(TestClass.FlagValue)} true");
			Assert.AreEqual(requiredAmt - 2, instanceParser.GetNeededSettings().Count);
		}
	}
}
