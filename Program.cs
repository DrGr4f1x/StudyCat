using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyCat
{
    class Program
    {
        static Book GenerateFromDesc(BookDesc bookDesc)
        {
            Book book = new Book();
            book.Authors = bookDesc.Authors;
            book.Title = bookDesc.Title;
            book.Year = bookDesc.Year;
            book.Publisher = bookDesc.Publisher;

            foreach (var chapterDesc in bookDesc.Chapters)
            {
                Chapter chapter = new Chapter();
                chapter.Title = chapterDesc.Title;
                chapter.Number = chapterDesc.Number;

                foreach (var sectionDesc in chapterDesc.Sections)
                {
                    Section section = new Section();
                    section.Title = sectionDesc.Title;
                    section.Number = sectionDesc.Number;
                    section.Pages = sectionDesc.Pages;
                    section.SessionNum = 0;

                    for(int i = 0; i < sectionDesc.NumProblems; ++i)
                    {
                        Problem problem = new Problem();
                        problem.Number = i;
                        problem.Deck = Deck.Current;
                        problem.TimesReviewed = 0;
                        section.Problems.Add(problem);
                    }

                    chapter.Sections.Add(section);
                }

                book.Chapters.Add(chapter);
            }

            return book;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            BookDesc bookDesc = new BookDesc();
            bookDesc.Title = "Calculus";
            bookDesc.Authors = "Rogawski, Jon; Adams, Colin";
            bookDesc.Year = "2015";
            bookDesc.Publisher = "W. H. Freeman and Company";

            ChapterDesc chap2 = new ChapterDesc();
            chap2.Title = "Limits";
            chap2.Number = 2;

            SectionDesc sec2_1 = new SectionDesc();
            sec2_1.Title = "Limits, Rates of Change, and Tangent Lines";
            sec2_1.Number = 1;
            sec2_1.Pages = "pp. 44-47";
            sec2_1.NumProblems = 36;

            SectionDesc sec2_2 = new SectionDesc();
            sec2_2.Title = "Limits: A Numerical and Graphical Approach";
            sec2_2.Number = 2;
            sec2_2.Pages = "pp. 54-56";
            sec2_2.NumProblems = 70;

            chap2.Sections.Add(sec2_1);
            chap2.Sections.Add(sec2_2);

            ChapterDesc chap3 = new ChapterDesc();
            chap3.Title = "Differentiation";
            chap3.Number = 3;

            SectionDesc sec3_1 = new SectionDesc();
            sec3_1.Title = "Definition of the Derivative";
            sec3_1.Number = 1;
            sec3_1.Pages = "pp. 102-105";
            sec3_1.NumProblems = 73;

            SectionDesc sec3_2 = new SectionDesc();
            sec3_2.Title = "The Derivative as a Function";
            sec3_2.Number = 2;
            sec3_2.Pages = "pp. 114-117";
            sec3_2.NumProblems = 98;

            chap3.Sections.Add(sec3_1);
            chap3.Sections.Add(sec3_2);

            bookDesc.Chapters.Add(chap2);
            bookDesc.Chapters.Add(chap3);

            string jsonString;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            jsonString = JsonSerializer.Serialize(bookDesc, options);
            string filename = "book.json";
            File.WriteAllText(filename, jsonString);

            Book book = GenerateFromDesc(bookDesc);
            jsonString = JsonSerializer.Serialize(book, options);
            filename = "problems.json";
            File.WriteAllText(filename, jsonString);
        }
    }
}
