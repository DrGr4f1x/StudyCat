using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace StudyCat
{
    class CardTypeStats
    {
        public int NumberStudied { get; set; } = 0;
        public int NumberCorrect { get; set; } = 0;
        public int NumberIncorrect
        {
            get { return NumberStudied - NumberCorrect; }
        }
    }

    class SessionStats
    {
        private List<CardTypeStats> m_stats = new List<CardTypeStats>();

        public int NumberCardsStudied
        {
            get
            {
                int total = 0;
                for(int i = 0; i < (int)CardType.Max; ++i)
                {
                    total += m_stats[i].NumberStudied;
                }
                return total;
            }
        }

        public SessionStats()
        {
            for (int i = 0; i < (int)CardType.Max; ++i)
            {
                m_stats.Add(new CardTypeStats());
            }
        }

        public CardTypeStats CardStats(CardType type)
        {
            return m_stats[(int)type];
        }

        public void PrintStats()
        {
            for (int i = 0; i < (int)CardType.Max; ++i)
            {
                var stats = m_stats[i];
                if (stats.NumberStudied > 0)
                {
                    string cardStr = stats.NumberStudied > 1 ? "cards" : "card";
                    if (stats.NumberStudied == stats.NumberCorrect)
                    {
                        Console.WriteLine("  {0} {1} {2} studied, all correct.", stats.NumberStudied, ((CardType)i).ToString(), cardStr);
                    }
                    else
                    {
                        Console.WriteLine(
                            "  {0} {1} {2} cards studied, {3} correct, {4} incorrect.", 
                            stats.NumberStudied, 
                            ((CardType)i).ToString(), 
                            cardStr,
                            stats.NumberCorrect, 
                            stats.NumberIncorrect);
                    }
                }
            }
            Console.WriteLine();
        }
    }

    class ReviewCard
    {
        public CardSection CardSection { get; set; }
        public Card Card { get; set; }
        public CardTypeStats Stats { get; set; }
    }

    class SessionManager
    {
        private List<ChapterSectionPair> m_chapterSections = new List<ChapterSectionPair>();
        private List<CardType> m_cardTypes = new List<CardType>();
        private List<ReviewCard> m_cardList = new List<ReviewCard>();
        private List<CardSection> m_cardSections = new List<CardSection>();

        private static bool IsOdd(int value)
        {
            return (value % 2) != 0;
        }

        private static bool IsEven(int value)
        {
            return (value % 2) == 0;
        }

        private bool HasCardType(CardType type)
        {
            foreach (var cardType in m_cardTypes)
            {
                if (cardType == type)
                    return true;
            }
            return false;
        }

        private bool PresentCard(ReviewCard card, bool isSimulating)
        {
            bool bQuit = false;

            Console.WriteLine(card.CardSection.GetDesc());

            if (card.Card.CardType == CardType.Problem)
            {
                Console.WriteLine("  Problem: {0}", card.Card.Number);
            }
            else
            {
                Console.WriteLine("  {0}: {1}", card.Card.CardType.ToString(), card.Card.Text);
            }

            bool bValidInput = false;
            bool bCorrect = false;
            while (!bValidInput)
            {
                Console.Write("  Correct?  [Y]es, [N]o, [Q]uit: ");
                string val = Console.ReadLine();
                if (val == "Y" || val == "y")
                {
                    card.Stats.NumberStudied += 1;
                    card.Stats.NumberCorrect += 1;

                    bValidInput = true;
                    bCorrect = true;
                }
                else if (val == "N" || val == "n")
                {
                    card.Stats.NumberStudied += 1;

                    bValidInput = true;
                    bCorrect = false;
                }
                else if (val == "Q" || val == "q")
                {
                    bValidInput = true;
                    bQuit = true;
                    break;
                }
                else
                {
                    Console.WriteLine("Input {0} is not valid", val);
                }
            }

            // Update the card's review count if we aren't quitting
            if (!bQuit)
                card.Card.TimesReviewed += 1;

            // Update the card's deck, if we aren't quitting or simulating
            if (!bQuit && !isSimulating)
            {
                if(bCorrect)
                {
                    if (Utils.ShouldRetireCard(card.Card.Deck, card.CardSection.SessionNumber))
                    {
                        card.Card.Deck = Deck.Retired;
                    }
                    else if (card.Card.Deck == Deck.Current)
                    {
                        card.Card.Deck = Utils.ProgressDeckForSession(card.CardSection.SessionNumber);
                    }
                }
                else
                {
                    card.Card.Deck = Deck.Current;
                }
            }

            return bQuit;
        }

        public int Run(string path, IEnumerable<string> sections, bool isSerial, bool isSimulating, string types, int number, bool oddsOnly, bool evensOnly)
        {
            // Parse the requested sections
            foreach (var sectionStr in sections)
            {
                ChapterSectionPair pair = Utils.ParseChapterSection(sectionStr);
                m_chapterSections.Add(pair);
            }

            if (m_chapterSections.Count == 0)
            {
                Console.WriteLine("ERROR: Failed to parse chapters/sections for study session.");
                return 1;
            }

            // Parse the requested card types
            types = types.ToLower();
            if (types.Contains("all"))
            {
                m_cardTypes.Add(CardType.Problem);
                m_cardTypes.Add(CardType.Definition);
                m_cardTypes.Add(CardType.Lemma);
                m_cardTypes.Add(CardType.Theorem);
                m_cardTypes.Add(CardType.Note);
                m_cardTypes.Add(CardType.Algorithm);
            }
            else
            {
                if (types.Contains("def"))
                    m_cardTypes.Add(CardType.Definition);

                if (types.Contains("prob"))
                    m_cardTypes.Add(CardType.Problem);

                if (types.Contains("lem"))
                    m_cardTypes.Add(CardType.Lemma);

                if (types.Contains("note"))
                    m_cardTypes.Add(CardType.Note);

                if (types.Contains("algo"))
                    m_cardTypes.Add(CardType.Algorithm);

                if (types.Contains("theo"))
                    m_cardTypes.Add(CardType.Theorem);
            }

            if (m_cardTypes.Count == 0)
            {
                Console.WriteLine("ERROR: Failed to parse any valid card types.");
                return 1;
            }

            // Determine which section files to open
            string[] files = Directory.GetFiles(path);
            HashSet<string> filesToOpen = new HashSet<string>();
            foreach(var chapterSection in m_chapterSections)
            {
                if (chapterSection.section == -1)
                {
                    foreach (var file in files)
                    {
                        if (file.Contains(string.Format("Section.{0}", chapterSection.chapter)))
                            filesToOpen.Add(file);
                    }
                }
                else
                {
                    foreach (var file in files)
                    {
                        if (file.Contains(string.Format("Section.{0}.{1}", chapterSection.chapter, chapterSection.section)))
                            filesToOpen.Add(file);
                    }
                }
            }

            if (filesToOpen.Count == 0)
            {
                Console.WriteLine("ERROR: Failed to find any section files to open.");
                return 1;
            }

            // Open section files
            foreach(var file in filesToOpen)
            {
                var cardSection = Utils.Load<CardSection>(file);
                if (cardSection != null)
                {
                    m_cardSections.Add(cardSection);
                }
                else
                {
                    Console.WriteLine("WARNING: Failed to load section file {0}", file);
                }
            }

            if (m_cardSections.Count == 0)
            {
                Console.WriteLine("ERROR: Failed to load any section files.");
                return 1;
            }

            List<SessionStats> sessionStatsList = new List<SessionStats>();

            // Load cards into the base list
            DateTime now = DateTime.Now;

            foreach(var cardSection in m_cardSections)
            {
                // Track statistics
                SessionStats sessionStats = new SessionStats();
                sessionStatsList.Add(sessionStats);

                // Determine whether to advance the session counter
                TimeSpan elapsed = now - cardSection.LastReviewDate;
                if (elapsed.Days > 0)
                {
                    cardSection.SessionNumber = (cardSection.SessionNumber + 1) % 10;
                }

                foreach(var card in cardSection.Cards)
                {
                    if (oddsOnly && !evensOnly && IsEven(card.Number))
                        continue;

                    if (evensOnly && !oddsOnly && IsOdd(card.Number))
                        continue;

                    if (Utils.DeckMatchesSession(card.Deck, cardSection.SessionNumber) && HasCardType(card.CardType))
                    {
                        ReviewCard revCard = new ReviewCard();
                        revCard.Card = card;
                        revCard.CardSection = cardSection;
                        revCard.Stats = sessionStats.CardStats(card.CardType);

                        m_cardList.Add(revCard);
                    }
                }
            }

            // Run the study session
            bool bQuit = false;
            int currentCard = 0;
            int currentCardAbs = 0;

            while (!bQuit)
            {
                if (number > 0 && currentCardAbs >= number)
                {
                    bQuit = true;
                    break;
                }

                // Randomize if necessary
                if (currentCard == 0 && !isSerial)
                {
                    Random rng = new Random();
                    for(int i =  m_cardList.Count - 1; i > -1; i--)
                    {
                        int j = rng.Next(i);
                        var temp = m_cardList[i];
                        m_cardList[i] = m_cardList[j];
                        m_cardList[j] = temp;
                    }
                }

                // Present card and handle user input
                if (number > 0)
                    Console.WriteLine("Item {0}/{1}:", currentCardAbs + 1, number);
                else
                    Console.WriteLine("Item {0}", currentCardAbs + 1);

                bQuit = PresentCard(m_cardList[currentCard], isSimulating);

                // Advance to the next card
                currentCard = (currentCard + 1) % m_cardList.Count;
                currentCardAbs += 1;
            }

            // Save the results
            int sectionIndex = 0;
            Console.WriteLine();
            foreach (var cardSection in m_cardSections)
            {
                if (!isSimulating)
                {
                    string filename = string.Format("Section.{0}.{1}.json", cardSection.ChapterNumber, cardSection.SectionNumber);
                    Utils.Save<CardSection>(path + "\\" + filename, cardSection, false);
                }

                Console.WriteLine("Stats for {0}", cardSection.GetDesc());
                var sessionStats = sessionStatsList[sectionIndex];
                if (sessionStats.NumberCardsStudied == 0)
                {
                    Console.WriteLine("  No cards studied.\n");
                }
                else
                {
                    sessionStats.PrintStats();
                }

                ++sectionIndex;
            }

            return 0;
        }
    }
}
