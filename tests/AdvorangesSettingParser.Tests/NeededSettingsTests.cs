using System;
using AdvorangesSettingParser.Implementation.Instance;
using AdvorangesSettingParser.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class NeededSettingsTests
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
		[TestMethod]
		public void InterfaceAndImplementation_Test()
		{
			var instanceInstance = new TestClass();
			var instanceParser = new SettingParser
			{
				new Setting<string>(() => instanceInstance.StringValue),
			};

			var requiredAmt1 = instanceParser.GetNeededSettings().Count;
			Assert.AreEqual(1, requiredAmt1);
			var requiredAmt2 = ((ISettingParser)instanceParser).GetNeededSettings(null).Count;
			Assert.AreEqual(1, requiredAmt2);
		}
	}
}
