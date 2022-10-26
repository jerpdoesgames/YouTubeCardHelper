using System;
using System.Collections.Generic;
using System.Text;

namespace CardHelper2
{
    class argumentConfig
    {
        private bool m_ShowWordList;
        private bool m_ShowWordCounts;
        private bool m_ShowUniqueWords;
        private bool m_ShowRareWords;
        private bool m_SearchUniqueWords;
        private bool m_SearchRareWords;
        private bool m_EmptyArgs;

        public bool showWordList { get { return m_ShowWordList; } }
        public bool showWordCounts { get { return m_ShowWordCounts; } }
        public bool showUniqueWords { get { return m_ShowUniqueWords; } }
        public bool showRareWords { get { return m_ShowRareWords; } }
        public bool searchUniqueWords { get { return m_SearchUniqueWords; } }
        public bool searchRareWords { get { return m_SearchRareWords; } }
        public bool emptyArgs { get { return m_EmptyArgs; } }
        public argumentConfig(string[] args)
        {
            if (args.Length == 0)
            {
                // Display Help
                Console.WriteLine("The following options are available:");
                Console.WriteLine("====================================");
                Console.WriteLine("--words                Show find times for search terms.");
                Console.WriteLine("--rare                 Show list of rare words.");
                Console.WriteLine("--unique               Show list of unique words.");
                Console.WriteLine("--count                Show word counts.");
                Console.WriteLine("--unique_search        Show find times for unique terms.");
                Console.WriteLine("--rare_search          Show find times for rare terms.");
                Console.WriteLine("--display_all          Enable all prior options.");
                m_EmptyArgs = true;
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--words":
                            m_ShowWordList = true;
                            break;

                        case "--count":
                            m_ShowWordCounts = true;
                            break;

                        case "--rare":
                            m_ShowRareWords = true;
                            break;

                        case "--unique":
                            m_ShowUniqueWords = true;
                            break;

                        case "--unique_search":
                            m_SearchUniqueWords = true;
                            break;
                        case "--rare_search":
                            m_SearchRareWords = true;
                            break;
                        case "--display_all":
                            m_ShowRareWords = true;
                            m_ShowUniqueWords = true;
                            m_ShowWordCounts = true;
                            m_ShowWordList = true;
                            m_SearchRareWords = true;
                            m_SearchUniqueWords = true;
                            break;
                    }
                }
            }
        }
    }
}
