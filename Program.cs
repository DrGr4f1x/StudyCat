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

    [Verb("add", HelpText="Adds a new card to a book, in the specified chapter and section.")]
    class AddCardOptions
    {
        [Option('p', "path", HelpText = "Directory in which to add the new card.")]
        public DirectoryInfo Path { get; set; }
        [Option('c', "chapter", HelpText = "Chapter in which to insert the new card.")]
        public int Chapter { get; set; }
        [Option('s', "section", HelpText = "Section in which to insert the new card.")]
        public int Section { get; set; }
        [Option('t', "type", Required = true, HelpText ="Card type (Definition, Theorem, Lemma, Note, or Algorithm.")]
        public string Type { get; set; }
        [Option("text", Required = true, HelpText = "Card text.")]
        public string Text { get; set; }
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

        static T Load<T>(string filepath) where T : class
        {
            T obj = null;

            // Try to deserialize the T object
            if (File.Exists(filepath))
            {
                try
                {
                    string jsonString = File.ReadAllText(filepath);
                    obj = JsonSerializer.Deserialize<T>(jsonString, GetJsonSerializerOptions());
                }
                catch
                {
                    Console.WriteLine("ERROR: Failed to load input file {0}", filepath);
                }
            }
            else
            {
                Console.WriteLine("ERROR: Input file {0} does not exist.  Exiting.", filepath);
            }

            return obj;
        }

        static int Save<T>(string filepath, T obj) where T : class
        {
            if (File.Exists(filepath))
            {
                if (!PromptForFileOverwrite(filepath))
                {
                    return 1;
                }
            }

            try
            {
                string jsonString = JsonSerializer.Serialize<T>(obj, GetJsonSerializerOptions());
                File.WriteAllText(filepath, jsonString);
            }
            catch
            {
                Console.WriteLine("ERROR: Failed to save to output file {0}", filepath);
                return 1;
            }

            return 0;
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
                        Card card = new Card();
                        card.Number = i;
                        card.Deck = Deck.Current;
                        card.CardType = CardType.Problem;
                        card.TimesReviewed = 0;
                        section.Cards.Add(card);
                    }

                    chapter.Sections.Add(section);
                }

                book.Chapters.Add(chapter);
            }

            return book;
        }

        static int RunNewAndReturnExitCode(NewBookOptions opts)
        {
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

            // Create directory if it doesn't exist
            if (!Directory.Exists(opts.Path.FullName))
            {
                Directory.CreateDirectory(opts.Path.FullName);
            }

            // Serialize the book desc
            return Save<BookDesc>(opts.Path.FullName + "\\book.json", desc);
        }

        static int RunListAndReturnExitCode(ListBookOptions opts)
        {
            var bookDesc = Load<BookDesc>(opts.Path.FullName + "\\book.json");
            if (bookDesc == null)
            {
                return 1;
            }

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
            string cardsFilename = opts.Path.FullName + "\\cardsset.json";

            // Load the BookDesc
            var bookDesc = Load<BookDesc>(opts.Path.FullName + "\\book.desc");
            if (bookDesc == null)
            {
                return 1;
            }

            // Construct a book from the book desc
            Book book = GenerateFromDesc(bookDesc);

            // Save the Book
            return Save<Book>(opts.Path.FullName + "\\cardset.json", book);
        }

        static int RunAddAndReturnExitCode(AddCardOptions opts)
        {
            // Validate the card type
            CardType type = CardType.Definition;
            try
            {
                type = (CardType)Enum.Parse(typeof(CardType), opts.Type, true);
            }
            catch
            {
                Console.WriteLine("Card type '{0}' is not valid.", opts.Type);
                return 1;
            }

            Console.WriteLine("Adding card of type {0} = {1:D} to chapter {2}, section {3}", opts.Type, type, opts.Chapter, opts.Section);
            Console.WriteLine("Text: {0}", opts.Text);

            return 0;
        }

        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<NewBookOptions, ListBookOptions, MakeCardsOptions, AddCardOptions>(args)
                .MapResult(
                    (NewBookOptions opts) => RunNewAndReturnExitCode(opts),
                    (ListBookOptions opts) => RunListAndReturnExitCode(opts),
                    (MakeCardsOptions opts) => RunMakeAndReturnExitCode(opts),
                    (AddCardOptions opts) => RunAddAndReturnExitCode(opts),
                    errs => 1
                );
        }
    }
}
