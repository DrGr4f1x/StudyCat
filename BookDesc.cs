using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace StudyCat
{
	public class SectionDesc
	{
		private List<Card> m_additionalCards = new List<Card>();

		public string Title { get; set; }
		public int Number { get; set; }
		public string Pages { get; set; }
		public int NumProblems { get; set; }
		public List<Card> AdditionalCards 
		{
			get { return m_additionalCards; }
			set { m_additionalCards = value; }
		}
	}

	public class ChapterDesc
	{
		private List<SectionDesc> m_sections = new List<SectionDesc>();

		public string Title { get; set; }
		public int Number { get; set; }
		public List<SectionDesc> Sections
		{
			get	{ return m_sections; }
			set	{ m_sections = value; }
		}
	}

	public class BookDesc
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
	}
}