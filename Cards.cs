using System;
using System.Collections.Generic;
using System.Text;

namespace StudyCat
{
    public enum Deck
    {
        Current,
        Progress_0259,
        Progress_1360,
        Progress_2471,
        Progress_3582,
        Progress_4693,
        Progress_5704,
        Progress_6815,
        Progress_7926,
        Progress_8037,
        Progress_9146,
        Retired
    }

    public enum CardType
    {
        Problem,
        Definition,
        Theorem,
        Lemma,
        Note,
        Algorithm,

        Max
    }

    public class Card
    {
        public int Number { get; set; }
        public Deck Deck { get; set; } = Deck.Current;
        public CardType CardType { get; set; } = CardType.Problem;
        public int TimesReviewed { get; set; } = 0;
        public string Text { get; set; }
    }

    public class Section
    {
        private List<Card> m_cards = new List<Card>();

        public string Title { get; set; }
        public int Number { get; set; }
        public string Pages { get; set; }
        public int SessionNum { get; set; }
        public List<Card> Cards
        {
            get { return m_cards; }
            set { m_cards = value; }
        }
    }

    public class Chapter
    {
        private List<Section> m_sections = new List<Section>();

        public string Title { get; set; }
        public int Number { get; set; }
        public List<Section> Sections
        {
            get { return m_sections; }
            set { m_sections = value; }
        }

        public bool AddCard(Card card, int iSection)
        {
            foreach (var section in Sections)
            {
                if (section.Number == iSection)
                {
                    section.Cards.Add(card);
                    return true;
                }
            }

            return false;
        }
    }
    public class Book : IPostLoad
    {
        private List<Chapter> m_chapters = new List<Chapter>();

        public string Title { get; set; }
        public string Authors { get; set; }
        public string Publisher { get; set; }
        public string Year { get; set; }
        public List<Chapter> Chapters
        {
            get { return m_chapters; }
            set { m_chapters = value; }
        }

        public void PostLoad() { }

        public bool AddCard(Card card, int iChapter, int iSection)
        {
            bool bCardAdded = false;

            foreach (var chapter in Chapters)
            {
                if (chapter.Number == iChapter)
                {
                    bCardAdded = chapter.AddCard(card, iSection);
                    break;
                }
            }
            return bCardAdded;
        }
    }
}
