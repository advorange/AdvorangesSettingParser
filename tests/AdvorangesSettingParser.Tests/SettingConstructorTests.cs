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
			var propertySetting = new Setting<bool>(new Ref<bool>(x => property.BoolValue = x, () => property.BoolValue, "a"));
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(false, propertySetting.GetValue());
			propertySetting.SetValue(true);
			Assert.AreEqual(true, property.BoolValue);
			Assert.AreEqual(true, propertySetting.GetValue());
			property.BoolValue = false;
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(false, propertySetting.GetValue());
		}
		[TestMethod]
		public void GetterSetterVariable_Test()
		{
			var local = false;
			var localSetting = new Setting<bool>(new Ref<bool>(x => local = x, () => local, "a"));
			Assert.AreEqual(false, local);
			Assert.AreEqual(false, localSetting.GetValue());
			localSetting.SetValue(true);
			Assert.AreEqual(true, local);
			Assert.AreEqual(true, localSetting.GetValue());
			local = false;
			Assert.AreEqual(false, local);
			Assert.AreEqual(false, localSetting.GetValue());
		}
		[TestMethod]
		public void StrongBox_Test()
		{
			var strongBox = new StrongBox<bool>(false);
			var strongBoxSetting = new Setting<bool>(new Ref<bool>(strongBox, "a"));
			Assert.AreEqual(false, strongBox.Value);
			Assert.AreEqual(false, strongBoxSetting.GetValue());
			strongBoxSetting.SetValue(true);
			Assert.AreEqual(true, strongBox.Value);
			Assert.AreEqual(true, strongBoxSetting.GetValue());
			strongBox.Value = false;
			Assert.AreEqual(false, strongBox.Value);
			Assert.AreEqual(false, strongBoxSetting.GetValue());
		}
		[TestMethod]
		public void ExpressionStructPropertySetting_Test()
		{
			var property = new TestStruct { BoolValue = false, };
			var propertySetting = new Setting<bool>(() => property.BoolValue);
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(false, propertySetting.GetValue());
			propertySetting.SetValue(true);
			Assert.AreEqual(true, property.BoolValue);
			Assert.AreEqual(true, propertySetting.GetValue());
			property.BoolValue = false;
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(false, propertySetting.GetValue());
		}
		[TestMethod]
		public void ExpressionPropertySetting_Test()
		{
			var property = new TestClass { BoolValue = false, };
			var propertySetting = new Setting<bool>(() => property.BoolValue);
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(false, propertySetting.GetValue());
			propertySetting.SetValue(true);
			Assert.AreEqual(true, property.BoolValue);
			Assert.AreEqual(true, propertySetting.GetValue());
			property.BoolValue = false;
			Assert.AreEqual(false, property.BoolValue);
			Assert.AreEqual(false, propertySetting.GetValue());
		}
		[TestMethod]
		public void ExpressionFieldSetting_Test()
		{
			var field = new TestClass { BoolFieldValue = false, };
			var fieldSetting = new Setting<bool>(() => field.BoolFieldValue);
			Assert.AreEqual(false, field.BoolFieldValue);
			Assert.AreEqual(false, fieldSetting.GetValue());
			fieldSetting.SetValue(true);
			Assert.AreEqual(true, field.BoolFieldValue);
			Assert.AreEqual(true, fieldSetting.GetValue());
			field.BoolFieldValue = false;
			Assert.AreEqual(false, field.BoolFieldValue);
			Assert.AreEqual(false, fieldSetting.GetValue());
		}
		[TestMethod]
		public void ExpressionLocalVariableSetting_Test()
		{
			var local = false;
			var localSetting = new Setting<bool>(() => local);
			Assert.AreEqual(false, local);
			Assert.AreEqual(false, localSetting.GetValue());
			localSetting.SetValue(true);
			Assert.AreEqual(true, local);
			Assert.AreEqual(true, localSetting.GetValue());
			local = false;
			Assert.AreEqual(false, local);
			Assert.AreEqual(false, localSetting.GetValue());
		}
		[TestMethod]
		public void ExpressionInstanceVariableSetting_Test()
		{
			var instanceSetting = new Setting<bool>(() => InstanceValue);
			Assert.AreEqual(false, InstanceValue);
			Assert.AreEqual(false, instanceSetting.GetValue());
			instanceSetting.SetValue(true);
			Assert.AreEqual(true, InstanceValue);
			Assert.AreEqual(true, instanceSetting.GetValue());
			InstanceValue = false;
			Assert.AreEqual(false, InstanceValue);
			Assert.AreEqual(false, instanceSetting.GetValue());
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ExpressionException_Test()
		{
			var exceptionSetting = new Setting<bool>(() => InstanceValue ? InstanceValue : false);
		}
	}
}
