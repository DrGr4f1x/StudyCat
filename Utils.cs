using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyCat
{
    struct ChapterSectionPair
    {
        public int chapter;
        public int section;
        public SectionType sectionType;
    }

    class Utils
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

        public static T Load<T>(string filepath) where T : class, IPostLoad
        {
            T obj = null;

            // Try to deserialize the T object
            if (File.Exists(filepath))
            {
                try
                {
                    string jsonString = File.ReadAllText(filepath);
                    obj = JsonSerializer.Deserialize<T>(jsonString, GetJsonSerializerOptions());

                    obj.PostLoad();
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

        public static CardSection LoadSection(string path, int chapterNumber, int sectionNumber, SectionType sectionType)
        {
            string sectionTypeStr = sectionType == SectionType.Normal ? "" : "S";
            string filename = string.Format("Section.{0}.{1}{2}.json", chapterNumber, sectionTypeStr, sectionNumber);
           
            return Load<CardSection>(path + "\\" + filename);
        }

        public static int Save<T>(string filepath, T obj, bool promptForOverwrite = true) where T : class
        {
            if (promptForOverwrite && File.Exists(filepath))
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

        public static ChapterSectionPair ParseChapterSection(string str)
        {
            ChapterSectionPair pair;
            pair.chapter = -1;
            pair.section = -1;
            pair.sectionType = SectionType.Normal;

            string[] strs = str.Split('.');
            if (strs.Length == 1)
            {
                pair.chapter = int.Parse(strs[0]);
            }
            else if (strs.Length >= 2)
            {
                pair.chapter = int.Parse(strs[0]);
                if (strs[1].StartsWith('S'))
                {
                    pair.sectionType = SectionType.Special;
                }
                pair.section = int.Parse(strs[1].TrimStart('S'));
                
            }

            return pair;
        }

        public static bool DeckMatchesSession(Deck deck, int session)
        {
            switch(deck)
            {
                case Deck.Current:
                    return true;

                case Deck.Retired:
                    return false;

                case Deck.Progress_0259:
                    return (session == 0 || session == 2 || session == 5 || session == 9);
                       
                case Deck.Progress_1360:
                    return (session == 1 || session == 3 || session == 6 || session == 0);

                case Deck.Progress_2471:
                    return (session == 2 || session == 4 || session == 7 || session == 1);

                case Deck.Progress_3582:
                    return (session == 3 || session == 5 || session == 8 || session == 2);

                case Deck.Progress_4693:
                    return (session == 4 || session == 6 || session == 9 || session == 3);

                case Deck.Progress_5704:
                    return (session == 5 || session == 7 || session == 0 || session == 4);

                case Deck.Progress_6815:
                    return (session == 6 || session == 8 || session == 1 || session == 5);

                case Deck.Progress_7926:
                    return (session == 7 || session == 9 || session == 2 || session == 6);

                case Deck.Progress_8037:
                    return (session == 8 || session == 0 || session == 3 || session == 7);

                case Deck.Progress_9148:
                    return (session == 9 || session == 1 || session == 4 || session == 8);

                default:
                    return true;
            }
        }

        public static Deck ProgressDeckForSession(int session)
        {
            switch (session)
            {
                case 0: return Deck.Progress_0259;
                case 1: return Deck.Progress_1360;
                case 2: return Deck.Progress_2471;
                case 3: return Deck.Progress_3582;
                case 4: return Deck.Progress_4693;
                case 5: return Deck.Progress_5704;
                case 6: return Deck.Progress_6815;
                case 7: return Deck.Progress_7926;
                case 8: return Deck.Progress_8037;
                case 9: return Deck.Progress_9148;
                default: return Deck.Current;
            }
        }

        public static bool ShouldRetireCard(Deck deck, int session)
        {
            switch (session)
            {
                case 0: return deck == Deck.Progress_1360;
                case 1: return deck == Deck.Progress_2471;
                case 2: return deck == Deck.Progress_3582;
                case 3: return deck == Deck.Progress_4693;
                case 4: return deck == Deck.Progress_5704;
                case 5: return deck == Deck.Progress_6815;
                case 6: return deck == Deck.Progress_7926;
                case 7: return deck == Deck.Progress_8037;
                case 8: return deck == Deck.Progress_9148;
                case 9: return deck == Deck.Progress_0259;
                default: return false;
            }
        }
    }
}
