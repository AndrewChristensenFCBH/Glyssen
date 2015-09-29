﻿using Glyssen;
using Glyssen.Character;
using NUnit.Framework;
using Rhino.Mocks;
using SIL.Scripture;
using SIL.Xml;
using ScrVers = Paratext.ScrVers;

namespace GlyssenTests.Character
{
	[TestFixture]
	public class CharacterAssignerTests
	{
		private BookScript m_bookScript;

		[SetUp]
		public void SetUp()
		{
			const string bookScript = @"
<book id=""MRK"">
  <block style=""p"" chapter=""1"" initialStartVerse=""4"" characterId=""narrator-MRK"" userConfirmed=""false"">
    <verse num=""4"" />
    <text>Mantsa tama, ka zlagaptá Yuhwana, mnda maga Batem ma mtak, kaʼa mantsa: </text>
  </block>
  <block style=""p"" chapter=""1"" initialStartVerse=""4"" characterId=""Made Up Guy"" userConfirmed=""false"">
    <text>«Mbəɗanafwa mbəɗa ta nzakwa ghuni, ka magaghunafta lu ta batem, ka plighunista Lazglafta ta dmakuha ghuni,» </text>
  </block>
  <block style=""p"" chapter=""1"" initialStartVerse=""5"" characterId=""Thomas/Andrew/Bartholomew"" userConfirmed=""true"">
    <text>«Gobbledy-gook» </text>
  </block>
</book>";

			m_bookScript = XmlSerializationHelper.DeserializeFromString<BookScript>(bookScript);
		}

		[Test]
		public void Assign_SetDefaultForMultipleChoiceCharactersFalseOverwriteUserConfirmedFalse_OverwritesOnlyUncomfirmedBlocks()
		{
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "King Saul", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 5, 0, 5, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "Jesus", null, null, false) });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, ScrVers.English);
			Assert.AreEqual("King Saul", m_bookScript[1].CharacterId);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", m_bookScript[2].CharacterId);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", m_bookScript[2].CharacterIdInScript);
		}

		[Test]
		public void Assign_OverwriteUserConfirmedTrue_OverwritesAll()
		{
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "John the Baptist", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 5, 0, 5, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "King Saul", null, null, false) });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, ScrVers.English, false, true);
			Assert.AreEqual("John the Baptist", m_bookScript[1].CharacterId);
			Assert.AreEqual("King Saul", m_bookScript[2].CharacterId);
		}

		[Test]
		public void Assign_Overwriting_ControlFileHasMultipleChoiceCharacters_SetsImplicitOrExplicitDefault()
		{
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "Thomas/Andrew/Bartholomew", null, null, false, QuoteType.Normal, "Andrew") });
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 5, 0, 5, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "James/John", null, null, false) });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, ScrVers.English, true, true);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", m_bookScript[1].CharacterId);
			Assert.AreEqual("Andrew", m_bookScript[1].CharacterIdInScript);
			Assert.AreEqual("James/John", m_bookScript[2].CharacterId);
			Assert.AreEqual("James", m_bookScript[2].CharacterIdInScript);
		}

		[Test]
		public void Assign_NotOverwriting_SetDefaultForMultipleChoiceCharactersTrue_ControlFileDoesNotHaveExplicitDefault_SetsImplicitDefault()
		{
			Assert.AreEqual("Thomas/Andrew/Bartholomew", m_bookScript[2].CharacterId);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", m_bookScript[2].CharacterIdInScript);

			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "Made Up Guy", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters(41, 1, 5, 0, versification: ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "Thomas/Andrew/Bartholomew", null, null, false) });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, ScrVers.English, true);
			Assert.AreEqual("Made Up Guy", m_bookScript[1].CharacterId);
			Assert.AreEqual("Made Up Guy", m_bookScript[1].CharacterIdInScript);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", m_bookScript[2].CharacterId);
			Assert.AreEqual("Thomas", m_bookScript[2].CharacterIdInScript);
		}

		[Test]
		public void Assign_NotOverwriting_SetDefaultForMultipleChoiceCharactersTrue_ControlFileHasExplicitDefault_SetsExplicitDefault()
		{
			Assert.AreEqual("Thomas/Andrew/Bartholomew", m_bookScript[2].CharacterId);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", m_bookScript[2].CharacterIdInScript);

			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "Made Up Guy", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters(41, 1, 5, 0, versification: ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "Thomas/Andrew/Bartholomew", null, null, false, QuoteType.Normal, "Andrew") });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, ScrVers.English, true);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", m_bookScript[2].CharacterId);
			Assert.AreEqual("Andrew", m_bookScript[2].CharacterIdInScript);
		}

		[Test]
		public void Assign_BlockIsStandardCharacter_DoesNotOverwrite()
		{
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "John the Baptist", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 5, 0, 5, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "King Saul", null, null, false) });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, ScrVers.English, false, true);
			Assert.AreEqual("narrator-MRK", m_bookScript[0].CharacterId);
		}
	}
}
