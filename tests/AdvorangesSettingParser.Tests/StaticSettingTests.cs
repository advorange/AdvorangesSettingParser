using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Implementation.Static;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class StaticSettingTests
	{
		[TestMethod]
		public void Test()
		{
			var setting = new StaticSetting<TestClass, bool>(x => x.BoolValue);
			var instance = new TestClass();
			setting.SetValue(instance, true);
			Assert.AreEqual(true, setting.GetValue(instance));
		}
	}
}
