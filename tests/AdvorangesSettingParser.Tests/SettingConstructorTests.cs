using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class SettingConstructorTests
	{
		private static readonly IEnumerable<string> Names = new[] { "a", "aa", "aaa" };

		private bool InstanceValue { get; set; }

		[TestMethod]
		public void GetterSetterProperty_Test()
		{
			var property = new TestClass { BoolValue = false, };
			var propertySetting = new Setting<bool>(x => property.BoolValue = x, () => property.BoolValue, Names);
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
			var variableSetting = new Setting<bool>(x => variable = x, () => variable, Names);
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
			var strongBoxSetting = new Setting<bool>(variable, Names);
			Assert.AreEqual(false, variable.Value);
			Assert.AreEqual(false, strongBoxSetting.GetValue());
			strongBoxSetting.Set(true);
			Assert.AreEqual(true, variable.Value);
			Assert.AreEqual(true, strongBoxSetting.GetValue());
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
		[TestMethod]
		public void ExpressionStructPropertySetting_Test()
		{
			var property = new TestStruct { BoolValue = false, };
			var propertySetting = new Setting<bool>(() => property.BoolValue);
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(false, propertySetting.GetValue());
			propertySetting.Set(true);
			Assert.AreEqual(true, property.BoolValue);
			Assert.AreEqual(true, propertySetting.GetValue());
		}
		[TestMethod]
		public void ExpressionPropertySetting_Test()
		{
			var property = new TestClass { BoolValue = false, };
			var propertySetting = new Setting<bool>(() => property.BoolValue);
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(false, propertySetting.GetValue());
			propertySetting.Set(true);
			Assert.AreEqual(true, property.BoolValue);
			Assert.AreEqual(true, propertySetting.GetValue());
		}
		[TestMethod]
		public void ExpressionFieldSetting_Test()
		{
			var property = new TestClass { BoolFieldValue = false, };
			var propertySetting = new Setting<bool>(() => property.BoolFieldValue);
			Assert.AreEqual(false, property.BoolFieldValue);
			Assert.AreEqual(false, propertySetting.GetValue());
			propertySetting.Set(true);
			Assert.AreEqual(true, property.BoolFieldValue);
			Assert.AreEqual(true, propertySetting.GetValue());
		}
		[TestMethod]
		public void ExpressionLocalVariableSetting_Test()
		{
			var variable = false;
			var variableSetting = new Setting<bool>(() => variable);
			Assert.AreEqual(false, variable);
			Assert.AreEqual(false, variableSetting.GetValue());
			variableSetting.Set(true);
			Assert.AreEqual(true, variable);
			Assert.AreEqual(true, variableSetting.GetValue());
		}
		[TestMethod]
		public void ExpressionInstanceVariableSetting_Test()
		{
			var variableSetting = new Setting<bool>(() => InstanceValue);
			Assert.AreEqual(false, InstanceValue);
			Assert.AreEqual(false, variableSetting.GetValue());
			variableSetting.Set(true);
			Assert.AreEqual(true, InstanceValue);
			Assert.AreEqual(true, variableSetting.GetValue());
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ExpressionException_Test()
		{
			var exceptionSetting = new Setting<bool>(() => InstanceValue ? InstanceValue : false);
		}
	}
}
