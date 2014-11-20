﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Palaso.Xml;
using ProtoScript.Bundle;
using ProtoScript.ToPalaso;

namespace ProtoScript
{
	public class UsxParser
	{
		private readonly XmlNodeList m_nodeList;

		private string m_bookLevelChapterLabel;
		private bool m_chapterNodeFound;
		private int m_currentChapter = 0;
		private int m_currentVerse = 0;

		public UsxParser(XmlNodeList nodeList)
		{
			m_nodeList = nodeList;
		}

		public IEnumerable<Block> Parse()
		{
			IList<Block> blocks = new List<Block>();
			foreach (XmlNode node in m_nodeList)
			{
				Block block = null;
				switch (node.Name)
				{
					case "chapter":
						m_chapterNodeFound = true;
						block = ProcessChapterNode(node);
						if (block == null)
							continue;
						break;
					case "para":
						var usxPara = new UsxPara(node);
						if (usxPara.StyleTag == "cl")
						{
							block = ProcessChapterLabelNode(node.InnerText, usxPara);
							if (block == null)
								continue;

							// The node before this was the chapter. We already added it, then found this label.
							// Remove that block so it will be replaced with this one.
							blocks.RemoveAt(blocks.Count-1);
							break;
						}
						block = new Block(usxPara.StyleTag, m_currentChapter, m_currentVerse);
						var sb = new StringBuilder();
						// <verse number="1" style="v" />
						// Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,
						// <verse number="2" style="v" />
						// <note caller="-" style="x"><char style="xo" closed="false">1.2: </char><char style="xt" closed="false">Mal 3.1</char></note>
						// kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>
						foreach (XmlNode childNode in usxPara.ChildNodes)
						{
							switch (childNode.Name)
							{
								case "verse":
									if (sb.Length > 0)
									{
										block.BlockElements.Add(new ScriptText(sb.ToString()));
										sb.Clear();
									}
									var verseNumStr = childNode.Attributes.GetNamedItem("number").Value;
									int verseNum = ScrReference.VerseToIntStart(verseNumStr);
									m_currentVerse = verseNum;
									if (!block.BlockElements.Any())
										block.InitialVerseNumber = verseNum;

									block.BlockElements.Add(new Verse(verseNumStr));
									break;
								case "#text":
									sb.Append(childNode.InnerText);
									break;
								case "#whitespace":
									if (sb.Length > 0)
										sb.Append(childNode.InnerText);
									break;
							}
						}
						if (sb.Length > 0)
						{
							block.BlockElements.Add(new ScriptText(sb.ToString()));
							sb.Clear();
						}
						break;
				}
				blocks.Add(block);
			}
			return blocks;
		}

		private Block ProcessChapterNode(XmlNode node)
		{
			var usxChapter = new UsxChapter(node);
			string chapterText;
			if (m_bookLevelChapterLabel != null)
			{
				//TODO what if this isn't the right order? Is there any way we can know?
				chapterText = m_bookLevelChapterLabel + " " + usxChapter.ChapterNumber;
			}
			else
				chapterText = usxChapter.ChapterNumber;

			int chapterNum;
			if (Int32.TryParse(usxChapter.ChapterNumber, out chapterNum))
				m_currentChapter = chapterNum;
			else
				Debug.Fail("TODO: Deal with bogus chapter number in USX data!");
			m_currentVerse = 0;
			var block = new Block(usxChapter.StyleTag, m_currentChapter);
			block.BlockElements.Add(new ScriptText(chapterText));
			return block;
		}

		private Block ProcessChapterLabelNode(string nodeText, UsxNode usxNode)
		{
			Block block = null;

			// Chapter label before the first chapter means we have a chapter label which applies to all chapters
			if (!m_chapterNodeFound)
				m_bookLevelChapterLabel = nodeText;

			if (m_bookLevelChapterLabel == null)
			{
				block = new Block(usxNode.StyleTag, m_currentChapter);
				block.BlockElements.Add(new ScriptText(nodeText));
				m_currentVerse = 0;
			}
			return block;
		}
	}
}