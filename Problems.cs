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

    public class Problem
    {
        public int Number { get; set; }
        public Deck Deck { get; set; }
        public int TimesReviewed { get; set; }
    }

    public class Section
    {
        private List<Problem> m_problems = new List<Problem>();

        public string Title { get; set; }
        public int Number { get; set; }
        public string Pages { get; set; }
        public int SessionNum { get; set; }
        public List<Problem> Problems
        {
            get { return m_problems; }
            set { m_problems = value; }
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
    }
    public class Book
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
    }
}
