using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class SettingParserConstructorTests
	{
		private static readonly IEnumerable<string> Names = new[] { "a", "aa", "aaa" };

		[TestMethod]
		public void GetterSetterProperty_Test()
		{
			var property = new TestClass { BoolValue = false, };
			var propertySetting = new Setting<bool>(Names, x => property.BoolValue = x, () => property.BoolValue);
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(false, propertySetting.GetValue());
			propertySetting.Set(true);
			Assert.AreEqual(true, property.BoolValue);
			Assert.AreEqual(true, propertySetting.GetValue());
		}
		[TestMethod]
		public void GetterSetterVariable_Test()
		{
			var variable = false;
			var variableSetting = new Setting<bool>(Names, x => variable = x, () => variable);
			Assert.AreEqual(false, variable);
			Assert.AreEqual(false, variableSetting.GetValue());
			variableSetting.Set(true);
			Assert.AreEqual(true, variable);
			Assert.AreEqual(true, variableSetting.GetValue());
		}
		[TestMethod]
		public void StrongBox_Test()
		{
			var variable = new StrongBox<bool>(false);
			var strongBoxSetting = new Setting<bool>(Names, variable);
			Assert.AreEqual(false, variable.Value);
			Assert.AreEqual(false, strongBoxSetting.GetValue());
			strongBoxSetting.Set(true);
			Assert.AreEqual(true, variable.Value);
			Assert.AreEqual(true, strongBoxSetting.GetValue());
		}
		[TestMethod]
		public void SetterOnly_Test()
		{
			var property = new TestClass { BoolValue = false, };
			var propertySetting = new Setting<bool>(Names, x => property.BoolValue = x);
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(false, propertySetting.GetValue());
			//Set() should update both the target and the cached value
			propertySetting.Set(true);
			Assert.AreEqual(true, property.BoolValue);
			Assert.AreEqual(true, propertySetting.GetValue());
			//GetValue() should return the old cached value
			property.BoolValue = false;
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(true, propertySetting.GetValue());
		}
		[TestMethod]
		public void SelfTargetting_Test()
		{
			var propertySetting = new Setting<bool>(Names);
			Assert.AreEqual(false, propertySetting.GetValue());
			propertySetting.Set(true);
			Assert.AreEqual(true, propertySetting.GetValue());
			propertySetting.Set(false);
			Assert.AreEqual(false, propertySetting.GetValue());
		}
	}
}
