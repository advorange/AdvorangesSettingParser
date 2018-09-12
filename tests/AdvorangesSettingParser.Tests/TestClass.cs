using System;

namespace AdvorangesSettingParser.Tests
{
	internal class TestClass
	{
		public string StringValue { get; set; }
		public int IntValue { get; set; }
		public bool BoolValue { get; set; }
		public bool FlagValue { get; set; }
		public bool FlagValue2 { get; set; }
		public ulong UlongValue { get; set; }
		public DateTime DateTimeValue { get; set; }

		public bool BoolFieldValue;
	}

	internal struct TestStruct
	{
		public bool BoolValue { get; set; }
	}
}
