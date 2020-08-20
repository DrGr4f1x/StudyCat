using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace StudyCat
{
	public class SectionDesc : IPostLoad
	{
		private List<Card> m_additionalCards = new List<Card>();

		private int[] m_maxCardsByType = new int[(int)CardType.Max];

		public string Title { get; set; }
		public int Number { get; set; }
		public string Pages { get; set; }
		public int NumProblems { get; set; }
		public List<Card> AdditionalCards 
		{
			get { return m_additionalCards; }
			set { m_additionalCards = value; }
		}

		public void PostLoad()
        {
			for(int i = 0; i < (int)CardType.Max; ++i)
            {
				m_maxCardsByType[i] = 0;
            }
			m_maxCardsByType[(int)CardType.Problem] = NumProblems;

			foreach(var card in AdditionalCards)
            {
				int cardNum = card.Number;
				int cardType = (int)card.CardType;

				m_maxCardsByType[cardType] = Math.Max(m_maxCardsByType[cardType], cardNum);
            }
        }

		public Card AddCard(CardType type, string text)
        {
			Card card = new Card();
			card.CardType = type;
			card.Number = NextCardNumber(type);
			card.Text = text;

			AdditionalCards.Add(card);

			return card;
        }

		private int NextCardNumber(CardType type)
        {
			return m_maxCardsByType[(int)type] + 1;
        }
	}

	public class ChapterDesc : IPostLoad
	{
		private List<SectionDesc> m_sections = new List<SectionDesc>();

		public string Title { get; set; }
		public int Number { get; set; }
		public List<SectionDesc> Sections
		{
			get	{ return m_sections; }
			set	{ m_sections = value; }
		}

		public void PostLoad()
        {
			foreach(var section in Sections)
            {
				section.PostLoad();
            }
        }

		public Card AddCard(CardType type, int iSection, string text)
        {
			foreach(var section in Sections)
            {
				if (section.Number == iSection)
                {
					return section.AddCard(type, text);
                }
            }

			return null;
        }
	}

	public class BookDesc : IPostLoad
	{
		private List<ChapterDesc> m_chapters = new List<ChapterDesc>();

		public string Title { get; set; }
		public string Authors { get; set; }
		public string Publisher { get; set; }
		public string Year { get; set; }

		public List<ChapterDesc> Chapters
		{
			get
			{
				return m_chapters;
			}
			set
			{
				m_chapters = value;
			}
		}

		public void PostLoad() 
		{ 
			foreach(var chapter in Chapters)
            {
				chapter.PostLoad();
            }
		}

		public Card AddCard(CardType type, int iChapter, int iSection, string text)
		{
			Card card = null;

			foreach (var chapter in Chapters)
			{
				if (chapter.Number == iChapter)
				{
					card = chapter.AddCard(type, iSection, text);
					break;
				}
			}
			return card;
		}
	}
}