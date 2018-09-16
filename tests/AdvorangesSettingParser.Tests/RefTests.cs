using System;
using System.Runtime.CompilerServices;
using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Implementation.Instance;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class RefTests
	{
		private static int Runs => IsBenchmark ? 50000000 : 1;
		private static bool IsBenchmark { get; } = false;

		private bool InstanceProperty { get; set; }
		private TestClass TestClass { get; set; } = new TestClass();
		private StrongBox<bool> StrongBox { get; set; } = new StrongBox<bool>(false);

		[TestMethod]
		public void ExpressionClassField_Test()
		{
			var valueRef = new Ref<bool>(() => TestClass.BoolFieldValue);
			Test(valueRef, false, true);
		}
		[TestMethod]
		public void ExpressionClassPropertyPrivate_Test()
		{
			var valueRef = new Ref<bool>(() => TestClass.PrivateSetBoolValue);
			Test(valueRef, false, true);
		}
		[TestMethod]
		public void ExpressionClassPropertyPublic_Test()
		{
			var valueRef = new Ref<bool>(() => TestClass.BoolValue);
			Test(valueRef, false, true);
		}
		[TestMethod]
		public void ExpressionInstanceVariable_Test()
		{
			var valueRef = new Ref<bool>(() => InstanceProperty);
			Test(valueRef, false, true);
		}
		[TestMethod]
		public void ExpressionLocalVariable_Test()
		{
			var local = false;
			var valueRef = new Ref<bool>(() => local);
			Test(valueRef, false, true);
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ExpressionStructProperty_Test()
		{
			var localStruct = new TestStruct();
			var valueRef = new Ref<bool>(() => localStruct.BoolValue);
			Test(valueRef, false, true);
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ExpressionException_Test()
		{
			var valueRef = new Ref<bool>(() => TestClass.BoolValue ? true : false);
		}

		[TestMethod]
		public void DirectSettingAndGetting_Test()
			=> Test(TestClass, false, true);
		[TestMethod]
		public void StrongBox_Test()
		{
			var valueRef = new Ref<bool>(StrongBox, "a");
			Test(valueRef, false, true);
		}

		private void Test(TestClass source, bool firstVal, bool secondVal)
		{
			if (IsBenchmark)
			{
				for (int i = 0; i < Runs; ++i)
				{
					if (i % 2 == 0)
					{
						source.BoolValue = firstVal;
					}
					else
					{
						source.BoolValue = secondVal;
					}
					var get = source.BoolValue;
				}
			}
			else
			{
				Assert.AreEqual(firstVal, source.BoolValue);
				source.BoolValue = secondVal;
				Assert.AreEqual(secondVal, source.BoolValue);
				source.BoolValue = firstVal;
				Assert.AreEqual(firstVal, source.BoolValue);
			}
		}
		private void Test<T>(Ref<T> reference, T firstVal, T secondVal)
		{
			if (IsBenchmark)
			{
				for (int i = 0; i < Runs; ++i)
				{
					if (i % 2 == 0)
					{
						reference.SetValue(firstVal);
					}
					else
					{
						reference.SetValue(secondVal);
					}
					var get = reference.GetValue();
				}
			}
			else
			{
				Assert.AreEqual(firstVal, reference.GetValue());
				reference.SetValue(secondVal);
				Assert.AreEqual(secondVal, reference.GetValue());
				reference.SetValue(firstVal);
				Assert.AreEqual(firstVal, reference.GetValue());
			}
		}
	}
}
