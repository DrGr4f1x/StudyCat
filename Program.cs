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
    {
        [Option('p', "path", HelpText = "Directory of the book whose contents we are listing.")]
        public DirectoryInfo Path { get; set; }
    }

    [Verb("make", HelpText="Makes a new set of cards from an existing book desc.")]
    class MakeCardsOptions
    {
        [Option('p', "path", HelpText = "Directory in which to put the generated book desc file.")]
        public DirectoryInfo Path { get; set; }
    }

    class Program
    {
        static JsonSerializerOptions GetJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                WriteIndented = true
            };
            return options;
        }

        static bool PromptForFileOverwrite(string filename)
        {
            bool bOverwriteFile = true;

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
                    bOverwriteFile = false;
                }
                else if (val == "Q" || val == "q")
                {
                    bValidChoice = true;
                    bOverwriteFile = false;
                    break;
                }
                else
                {
                    Console.WriteLine("Input {0} is not valid", val);
                }
            }

            return bOverwriteFile;
        }

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

            string filename = opts.Path.FullName + "\\book.json";

            // See if the book.json file already exists.  Prompt for overwrite.
            bool bCreateFile = true;
            if(File.Exists(filename))
            {
                bCreateFile = PromptForFileOverwrite(filename);
            }

            if(!bCreateFile)
            {
                return 0;
            }

            // Create a stubbed-out book desc that the user can fill in
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

            // Serialize the book desc
            string jsonString = JsonSerializer.Serialize(desc, GetJsonSerializerOptions());
            File.WriteAllText(filename, jsonString);

            return 0;
        }

        static int RunListAndReturnExitCode(ListBookOptions opts)
        {
            string bookFilename = opts.Path.FullName + "\\book.json";

            // If the book desc file doesn't exist, bail out
            if (!File.Exists(bookFilename))
            {
                Console.WriteLine("Input book desc {0} does not exist.  Exiting.", bookFilename);
                return 1;
            }

            // Try to deserialize the book desc
            string jsonString = File.ReadAllText(bookFilename);
            BookDesc bookDesc = JsonSerializer.Deserialize<BookDesc>(jsonString, GetJsonSerializerOptions());

            Console.WriteLine("\nTitle: {0}", bookDesc.Title);
            Console.WriteLine("Authors: {0}", bookDesc.Authors);
            Console.WriteLine("Publisher and year: {0} - {1}\n", bookDesc.Publisher, bookDesc.Year);

            int numProblemsBook = 0;
            foreach (var chapter in bookDesc.Chapters)
            {
                int numProblemsChapter = 0;
                Console.WriteLine("{0}. {1}", chapter.Number, chapter.Title);
                
                foreach(var section in chapter.Sections)
                {
                    Console.WriteLine("    {0}. {1} - {2} problems", section.Number, section.Title, section.NumProblems);
                    numProblemsChapter += section.NumProblems;
                }
                Console.WriteLine("Chapter problems: {0}\n", numProblemsChapter);
                numProblemsBook += numProblemsChapter;
            }
            Console.WriteLine("Book problems: {0}\n", numProblemsBook);

            return 0;
        }

        static int RunMakeAndReturnExitCode(MakeCardsOptions opts)
        {
            string bookFilename = opts.Path.FullName + "\\book.json";
            string cardsFilename = opts.Path.FullName + "\\cards.json";

            // If the book desc file doesn't exist, bail out
            if (!File.Exists(bookFilename))
            {
                Console.WriteLine("Input book desc {0} does not exist.  Exiting.", bookFilename);
                return 1;
            }

            // Try to deserialize the book desc
            string jsonString = File.ReadAllText(bookFilename);
            BookDesc bookDesc = JsonSerializer.Deserialize<BookDesc>(jsonString, GetJsonSerializerOptions());

            // Construct a cardset from the book desc
            Book book = GenerateFromDesc(bookDesc);

            // See if the cards.json file already exists.  Prompt for overwrite.
            bool bCreateFile = true;
            if (File.Exists(cardsFilename))
            {
                bCreateFile = PromptForFileOverwrite(cardsFilename);
            }

            if (!bCreateFile)
            {
                return 0;
            }

            jsonString = JsonSerializer.Serialize(book, GetJsonSerializerOptions());
            File.WriteAllText(cardsFilename, jsonString);

            return 0;
        }

        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<NewBookOptions, ListBookOptions, MakeCardsOptions>(args)
                .MapResult(
                    (NewBookOptions opts) => RunNewAndReturnExitCode(opts),
                    (ListBookOptions opts) => RunListAndReturnExitCode(opts),
                    (MakeCardsOptions opts) => RunMakeAndReturnExitCode(opts),
                    errs => 1
                );
        }
    }
}
