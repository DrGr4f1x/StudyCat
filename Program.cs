using CommandLine;
using CommandLine.Text;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyCat
{
    [Verb("new", HelpText = "Make a new book desc.  You fill in the details to generate a card set for the book.")]
    class NewBookOptions
    {
        [Option('t', "title", HelpText="Book title.")]
        public string Title { get; set; }

        [Option('a', "authors", HelpText="Book authors.")]
        public string Authors { get; set; }

        [Option('y', "year", HelpText="Book publication year.")]
        public string Year { get; set; }

        [Option('p', "publisher", HelpText="Book publisher.")]
        public string Publisher { get; set; }

        [Option('c', "chapters", HelpText="Number of chapters.")]
        public int NumChapters { get; set; }

        [Option("path", HelpText="Directory in which to put the generated book desc file.")]
        public DirectoryInfo Path { get; set; }
    }

    [Verb("list", HelpText = "Prints information about the book.")]
    class ListBookOptions
    { }

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

        static int RunNewAndReturnExitCode(NewBookOptions opts)
        {
            // Create directory if it doesn't exist
            if(!Directory.Exists(opts.Path.FullName))
            {
                Directory.CreateDirectory(opts.Path.FullName);
            }

            string filename = opts.Path.FullName + "\\" + "book.json";

            // See if the book.json file already exists.  Prompt for overwrite.
            bool bCreateFile = true;
            if(File.Exists(filename))
            {
                bool bValidChoice = false;
                while (!bValidChoice)
                {
                    Console.Write("The file {0} already exists.  Overwrite it?  [Y]es, [N]o, [Q]uit: ", filename);
                    string val = Console.ReadLine();
                    if (val == "Y" || val == "y")
                    {
                        bValidChoice = true;
                    }
                    else if (val == "N" || val == "n")
                    {
                        bValidChoice = true;
                        bCreateFile = false;
                    }
                    else if (val == "Q" || val == "q")
                    {
                        bValidChoice = true;
                        bCreateFile = false;
                        return 0;
                    }
                    else
                    {
                        Console.WriteLine("Input {0} is not valid", filename);
                    }
                }
            }

            if(!bCreateFile)
            {
                return 0;
            }

            BookDesc desc = new BookDesc();
            desc.Title = opts.Title.Length == 0 ? "Book Title" : opts.Title;
            desc.Authors = opts.Authors.Length == 0 ? "Authors" : opts.Authors;
            desc.Publisher = opts.Publisher.Length == 0 ? "Publisher" : opts.Publisher;
            desc.Year = opts.Year.Length == 0 ? "2020" : opts.Year;

            int numChapters = opts.NumChapters <= 0 ? 3 : opts.NumChapters;
            for(int i = 0; i < numChapters; ++i)
            {
                ChapterDesc chapterDesc = new ChapterDesc();
                chapterDesc.Title = "Chapter " + (i + 1).ToString();
                chapterDesc.Number = (i + 1);

                for(int j = 0; j < 2; ++j)
                {
                    SectionDesc sectionDesc = new SectionDesc();
                    sectionDesc.Title = "Section " + (j + 1).ToString();
                    sectionDesc.Pages = "pp. 20-22";
                    sectionDesc.Number = (j + 1);
                    sectionDesc.NumProblems = 25;

                    chapterDesc.Sections.Add(sectionDesc);
                }

                desc.Chapters.Add(chapterDesc);
            }

            

            string jsonString;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            jsonString = JsonSerializer.Serialize(desc, options);
            File.WriteAllText(filename, jsonString);

            return 0;
        }

        static int RunListAndReturnExitCode(ListBookOptions opts)
        {
            return 0;
        }

        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<NewBookOptions, ListBookOptions>(args)
                .MapResult(
                    (NewBookOptions opts) => RunNewAndReturnExitCode(opts),
                    (ListBookOptions opts) => RunListAndReturnExitCode(opts),
                    errs => 1
                );

            //Console.WriteLine("Hello World!");

            //BookDesc bookDesc = new BookDesc();
            //bookDesc.Title = "Calculus";
            //bookDesc.Authors = "Rogawski, Jon; Adams, Colin";
            //bookDesc.Year = "2015";
            //bookDesc.Publisher = "W. H. Freeman and Company";

            //ChapterDesc chap2 = new ChapterDesc();
            //chap2.Title = "Limits";
            //chap2.Number = 2;

            //SectionDesc sec2_1 = new SectionDesc();
            //sec2_1.Title = "Limits, Rates of Change, and Tangent Lines";
            //sec2_1.Number = 1;
            //sec2_1.Pages = "pp. 44-47";
            //sec2_1.NumProblems = 36;

            //SectionDesc sec2_2 = new SectionDesc();
            //sec2_2.Title = "Limits: A Numerical and Graphical Approach";
            //sec2_2.Number = 2;
            //sec2_2.Pages = "pp. 54-56";
            //sec2_2.NumProblems = 70;

            //chap2.Sections.Add(sec2_1);
            //chap2.Sections.Add(sec2_2);

            //ChapterDesc chap3 = new ChapterDesc();
            //chap3.Title = "Differentiation";
            //chap3.Number = 3;

            //SectionDesc sec3_1 = new SectionDesc();
            //sec3_1.Title = "Definition of the Derivative";
            //sec3_1.Number = 1;
            //sec3_1.Pages = "pp. 102-105";
            //sec3_1.NumProblems = 73;

            //SectionDesc sec3_2 = new SectionDesc();
            //sec3_2.Title = "The Derivative as a Function";
            //sec3_2.Number = 2;
            //sec3_2.Pages = "pp. 114-117";
            //sec3_2.NumProblems = 98;

            //chap3.Sections.Add(sec3_1);
            //chap3.Sections.Add(sec3_2);

            //bookDesc.Chapters.Add(chap2);
            //bookDesc.Chapters.Add(chap3);

            //string jsonString;
            //var options = new JsonSerializerOptions
            //{
            //    WriteIndented = true
            //};
            //jsonString = JsonSerializer.Serialize(bookDesc, options);
            //string filename = "book.json";
            //File.WriteAllText(filename, jsonString);

            //Book book = GenerateFromDesc(bookDesc);
            //jsonString = JsonSerializer.Serialize(book, options);
            //filename = "problems.json";
            //File.WriteAllText(filename, jsonString);
        }
    }
}
