using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Implementation.Static;
using AdvorangesSettingParser.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvorangesSettingParser.Tests
{
	[TestClass]
	public class QuoteChaosTest
	{
		static QuoteChaosTest()
		{
			new StaticSettingParser<Child>
			{
				new StaticSetting<Child, string>(x => x.Name),
				new StaticSetting<Child, string>(x => x.Text),
			}.Register();
			new StaticSettingParser<Parent>
			{
				new StaticSetting<Parent, Child>(x => x.Child),
			}.Register();
			new StaticSettingParser<Grandparent>
			{
				new StaticSetting<Grandparent, Parent>(x => x.Parent),
				new StaticSetting<Grandparent, string>(x => x.Text),
			}.Register();
			new StaticSettingParser<GreatGrandparent>
			{
				new StaticSetting<GreatGrandparent, Grandparent>(x => x.Grandparent),
				new StaticSetting<GreatGrandparent, Grandparent>(x => x.Grandparent2),
				new StaticSetting<GreatGrandparent, int>(x => x.Number),
				new StaticSetting<GreatGrandparent, string>(x => x.Stringy),
			}.Register();
		}

		public static readonly string FirstStr = "-Child \"-Name \"-Name \\\"Test Value\\\"\" -Text TestText\"";
		public static readonly string SecondStr = $"-Parent \"{FirstStr}\" -Text Dog";
		public static readonly string ThirdStr = $"-Stringy \"Space Value\" -Grandparent2 \"{SecondStr}\" -Grandparent \"{SecondStr}\" -Number 1";

		[TestMethod]
		public void QuoteChaos_Test()
		{
			var firstObj = new Parent();
			var firstResult = StaticSettingParserRegistry.Instance.Retrieve<Parent>().Parse(firstObj, FirstStr);
			Assert.AreEqual(true, firstResult.IsSuccess);
			Assert.AreEqual("-Name \"Test Value\"", firstObj.Child.Name);
			Assert.AreEqual("TestText", firstObj.Child.Text);
		}
		[TestMethod]
		public void QuoteHell_Test()
		{
			var secondObj = new Grandparent();
			var secondResult = StaticSettingParserRegistry.Instance.Retrieve<Grandparent>().Parse(secondObj, SecondStr);
			Assert.AreEqual(true, secondResult.IsSuccess);
			Assert.AreEqual("-Name \"Test Value\"", secondObj.Parent.Child.Name);
			Assert.AreEqual("TestText", secondObj.Parent.Child.Text);
			Assert.AreEqual("Dog", secondObj.Text);
		}
		[TestMethod]
		public void QuotesIsThisEvenPossible_Test()
		{
			var thirdObj = new GreatGrandparent();
			var thirdResult = StaticSettingParserRegistry.Instance.Retrieve<GreatGrandparent>().Parse(thirdObj, ThirdStr);
			Assert.AreEqual(true, thirdResult.IsSuccess);
			Assert.AreEqual("-Name \"Test Value\"", thirdObj.Grandparent.Parent.Child.Name);
			Assert.AreEqual("TestText", thirdObj.Grandparent.Parent.Child.Text);
			Assert.AreEqual("Dog", thirdObj.Grandparent.Text);
			Assert.AreEqual("-Name \"Test Value\"", thirdObj.Grandparent2.Parent.Child.Name);
			Assert.AreEqual("TestText", thirdObj.Grandparent2.Parent.Child.Text);
			Assert.AreEqual("Dog", thirdObj.Grandparent2.Text);
			Assert.AreEqual("Space Value", thirdObj.Stringy);
			Assert.AreEqual(1, thirdObj.Number);
		}
	}

	public class GreatGrandparent
	{
		public Grandparent Grandparent { get; private set; }
		public Grandparent Grandparent2 { get; private set; }
		public int Number { get; private set; }
		public string Stringy { get; private set; }
	}

	public class Grandparent
	{
		public Parent Parent { get; private set; }
		public string Text { get; private set; }
	}

	public class Parent
	{
		public Child Child { get; private set; }
	}

	public class Child
	{
		public string Name { get; private set; }
		public string Text { get; private set; }
	}
}
