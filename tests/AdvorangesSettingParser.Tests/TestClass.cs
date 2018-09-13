using System;
using System.Collections.Generic;
using System.Linq;

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

		public IEnumerable<string> EnumerableStrings { get; } = Enumerable.Empty<string>();
		public ICollection<string> CollectionStrings { get; } = new List<string>();
		public IList<string> ListStrings { get; } = new List<string>();
		public IList<string> FilledListStrings { get; } = new List<string>();
		public IList<int> ListInts { get; } = new List<int>();
	}

	internal struct TestStruct
	{
		public bool BoolValue { get; set; }
	}
}
