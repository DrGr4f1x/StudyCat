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
        Progress_9148,
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


    public class CardSection : IPostLoad
    {
        private List<Card> m_cards = new List<Card>();

        // Public chapter/section info properties
        public string ChapterTitle { get; set; }
        public int ChapterNumber { get; set; }
        public string SectionTitle { get; set; }
        public int SectionNumber { get; set; }

        // Public study session properties
        public int SessionNumber { get; set; } = -1;
        public DateTime LastReviewDate { get; set; }

        // Cards
        public List<Card> Cards
        {
            get { return m_cards; }
            set { m_cards = value; }
        }

        // IPostLoad implementation
        public void PostLoad() { }

        public string GetDesc()
        {
            return string.Format("{0}.{1} - {2} - {3}", ChapterNumber, SectionNumber, ChapterTitle, SectionTitle);
        }
    }
}
