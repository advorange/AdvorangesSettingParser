using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvorangesSettingParser.Tests
{
	internal class TestClass
	{
		public string StringValue { get; set; }
		public string StringValue2 { get; set; }
		public int IntValue { get; set; }
		public bool BoolValue { get; set; }
		public bool PrivateSetBoolValue { get; private set; }
		public bool FlagValue { get; set; }
		public bool FlagValue2 { get; set; }
		public ulong UlongValue { get; set; }
		public DateTime DateTimeValue { get; set; }

		public bool BoolFieldValue = false;

		public IEnumerable<string> EnumerableStrings { get; } = Enumerable.Empty<string>();
		public ICollection<string> CollectionStrings { get; } = new List<string>();
		public IList<string> ListStrings { get; } = new List<string>();
		public IList<string> FilledListStrings { get; } = new List<string> { "a", "b", "c", "d" };
		public IList<int> FilledListInts { get; } = new List<int> { 1, 2, 3 };
	}

	internal struct TestStruct
	{
		public bool BoolValue { get; set; }
	}
}
