using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace StudyCat
{
    struct ChapterSectionPair
    {
        public int chapter;
        public int section;
    }

    class ReviewCard
    {
        public CardSection CardSection { get; set; }
        public Card Card { get; set; }
    }

    class SessionManager
    {
        private List<ChapterSectionPair> m_chapterSections = new List<ChapterSectionPair>();
        private List<CardType> m_cardTypes = new List<CardType>();
        private List<ReviewCard> m_cardList = new List<ReviewCard>();
        private List<CardSection> m_cardSections = new List<CardSection>();

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

            Console.WriteLine(
                "{0}.{1} - {2} - {3}", 
                card.CardSection.ChapterNumber, 
                card.CardSection.SectionNumber, 
                card.CardSection.ChapterTitle, 
                card.CardSection.SectionTitle);

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
                    bValidInput = true;
                    bCorrect = true;
                }
                else if (val == "N" || val == "n")
                {
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

            if(!bQuit && !isSimulating)
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

        public int Run(string path, IEnumerable<string> sections, bool isSerial, bool isSimulating, string types)
        {
            // Parse the requested sections
            foreach (var sectionStr in sections)
            {
                ChapterSectionPair pair;
                string[] strs = sectionStr.Split('.');
                if (strs.Length == 1)
                {
                    pair.chapter = int.Parse(strs[0]);
                    pair.section = -1;

                    m_chapterSections.Add(pair);
                }
                else if (strs.Length >= 2)
                {
                    pair.chapter = int.Parse(strs[0]);
                    pair.section = int.Parse(strs[1]);

                    m_chapterSections.Add(pair);
                }
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

            if(filesToOpen.Count == 0)
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

            // Load cards into the base list
            DateTime now = DateTime.Now;

            foreach(var cardSection in m_cardSections)
            {
                TimeSpan elapsed = now - cardSection.LastReviewDate;
                
                if (elapsed.Days > 0)
                {
                    cardSection.SessionNumber = (cardSection.SessionNumber + 1) % 10;
                }

                foreach(var card in cardSection.Cards)
                {
                    if (Utils.DeckMatchesSession(card.Deck, cardSection.SessionNumber) && HasCardType(card.CardType))
                    {
                        ReviewCard revCard = new ReviewCard();
                        revCard.Card = card;
                        revCard.CardSection = cardSection;
                        m_cardList.Add(revCard);
                    }
                }
            }

            // Run the study session
            bool bQuit = false;
            int currentCard = 0;
            
            while (!bQuit)
            {
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
                bQuit = PresentCard(m_cardList[currentCard], isSimulating);

                // Advance to the next card
                currentCard = (currentCard + 1) % m_cardList.Count;
            }

            // Save the results
            if (!isSimulating)
            {
                foreach(var cardSection in m_cardSections)
                {
                    string filename = string.Format("Section.{0}.{1}.json", cardSection.ChapterNumber, cardSection.SectionNumber);
                    Utils.Save<CardSection>(path + "\\" + filename, cardSection, false);
                }
            }

            return 0;
        }
    }
}
