using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace CardHelper2
{
    class subProcessor
    {
        private generalConfig m_Config;
        private List<subFileList> m_ParsedSubLists;
        private Dictionary<string, int> m_GlobalWordCounts;
        public Dictionary<string, int> globalWordCounts { get { return m_GlobalWordCounts; } }
        private List<string> m_UniqueTerms;
        public List<string> uniqueTerms { get { return m_UniqueTerms; } }

        private Dictionary<string, int> m_LowFrequencyTerms;
        public Dictionary<string, int> lowFrequencyTerms { get { return m_LowFrequencyTerms; } }



        public void aggregateGlobalWordCounts()
        {
            List<Task> wordCountTasks = new List<Task>();
            foreach (subFileList curList in m_ParsedSubLists)
            {
                foreach (loadedVideoSubs curSubFile in curList.entries)
                {
                    wordCountTasks.Add(Task.Run(() =>
                        {
                            List<KeyValuePair<string, foundTermData>> foundTermsList = curSubFile.termData.ToList();

                            foreach (KeyValuePair<string, foundTermData> curTerm in foundTermsList)
                            {
                                if (m_GlobalWordCounts.ContainsKey(curTerm.Key))
                                {
                                    m_GlobalWordCounts[curTerm.Key]++;
                                }
                                else
                                {
                                    lock(m_GlobalWordCounts)
                                    {
                                        m_GlobalWordCounts[curTerm.Key] = 1;
                                    }
                                }
                            }
                        }
                    ));
                }

                if (
                    m_Config.args.searchRareWords ||
                    m_Config.args.searchUniqueWords ||
                    m_Config.args.showUniqueWords ||
                    m_Config.args.showRareWords
                )
                {
                    curList.aggregateWordCountsIntoOtherVideos();
                }
            }

            Task.WaitAll(wordCountTasks.ToArray());
        }

        public void aggregateRareTerms()
        {
            foreach (subFileList curList in m_ParsedSubLists)
            {
                foreach (loadedVideoSubs curSubFile in curList.entries)
                {
                    List<KeyValuePair<string, foundTermData>> foundTermsList = curSubFile.termData.ToList();

                    foreach (KeyValuePair<string, foundTermData> curTerm in foundTermsList)
                    {
                        if (curTerm.Value.foundInOtherLists == 0 && curTerm.Value.foundInSameListOtherVideo == 0)
                        {
                            m_UniqueTerms.Add(curTerm.Key);
                        }
                        else if (curTerm.Value.foundInOtherLists <= m_Config.lowFrequencyTermCountMaxGlobal && curTerm.Value.foundInSameListOtherVideo <= m_Config.lowFrequencyTermCountMaxList)
                        {
                            m_LowFrequencyTerms[curTerm.Key] = globalWordCounts[curTerm.Key];
                        }
                    }
                }
            }
        }

        public void searchUniqueTerms()
        {
            foreach (subFileList curList in m_ParsedSubLists)
            {
                foreach (loadedVideoSubs curSubFile in curList.entries)
                {
                    List<string> uniqueTerms = new List<string>();

                    List<KeyValuePair<string, foundTermData>> foundTermsList = curSubFile.termData.ToList();
                    foreach (KeyValuePair<string, foundTermData> curTerm in foundTermsList)
                    {
                        if (curTerm.Value.foundInOtherLists == 0 && curTerm.Value.foundInSameListOtherVideo == 0)
                            uniqueTerms.Add(curTerm.Key);
                    }

                    curSubFile.aggregateTermData(false, uniqueTerms);
                }
            }
        }

        public void searchLowFrequencyTerms()
        {
            foreach (subFileList curList in m_ParsedSubLists)
            {
                foreach (loadedVideoSubs curSubFile in curList.entries)
                {
                    List<string> rareTerms = new List<string>();

                    List<KeyValuePair<string, foundTermData>> foundTermsList = curSubFile.termData.ToList();
                    foreach (KeyValuePair<string, foundTermData> curTerm in foundTermsList)
                    {
                        if (curTerm.Value.foundInOtherLists == 0 && curTerm.Value.foundInSameListOtherVideo == 0)
                            rareTerms.Add(curTerm.Key);
                    }

                    curSubFile.aggregateTermData(false, rareTerms);
                }
            }
        }

        public void aggregateWordCountsBetweenLists()
        {
            foreach (subFileList curList in m_ParsedSubLists)
            {
                foreach (subFileList otherList in m_ParsedSubLists)
                {
                    if (otherList != curList)
                    {
                        foreach (loadedVideoSubs curSubFile in otherList.entries)
                        {
                            List<KeyValuePair<string, int>> thisListCounts = curList.wordCounts.ToList();

                            foreach (KeyValuePair<string, int> curTerm in thisListCounts)
                            {
                                if (curSubFile.termData.ContainsKey(curTerm.Key))
                                {
                                    curSubFile.termData[curTerm.Key].foundInOtherLists += curTerm.Value;
                                }
                            }
                        }

                    }
                }
            }
        }

        public void aggregateWordCounts()
        {
            aggregateGlobalWordCounts();
            
            if (
                m_Config.args.searchRareWords ||
                m_Config.args.searchUniqueWords ||
                m_Config.args.showUniqueWords ||
                m_Config.args.showRareWords
            )
            {
                aggregateWordCountsBetweenLists();
                aggregateRareTerms();
            }
        }

        public void outputWordCounts()
        {
            List<KeyValuePair<string, int>> wordCountList = m_GlobalWordCounts.ToList();

            wordCountList.Sort(
                delegate (KeyValuePair<string, int> pair1,
                KeyValuePair<string, int> pair2)
                {
                    int output = pair1.Value.CompareTo(pair2.Value);
                    if (output == 0)
                    {
                        output = pair1.Key.CompareTo(pair2.Key);    // Fallback alpha sort if same # of occurrances
                    }
                    return output;
                }
            );

            Console.WriteLine(" ========================================================");
            Console.WriteLine("Word counts logged::");
            Console.WriteLine("-------------------------");

            foreach (KeyValuePair<string, int> curWord in wordCountList)
            {
                Console.WriteLine(curWord.Value + " | " + curWord.Key);
            }
        }

        public void outputRareTerms()
        {
            List<KeyValuePair<string, int>> wordCountList = m_LowFrequencyTerms.ToList();

            wordCountList.Sort(
                delegate (KeyValuePair<string, int> pair1,
                KeyValuePair<string, int> pair2)
                {
                    int output = pair1.Value.CompareTo(pair2.Value);
                    if (output == 0)
                    {
                        output = pair1.Key.CompareTo(pair2.Key);    // Fallback alpha sort if same # of occurrances
                    }
                    return output;
                }
            );

            Console.WriteLine(" ========================================================");
            Console.WriteLine("Rare Terms:");
            Console.WriteLine("-------------------------");

            foreach (KeyValuePair<string, int> curWord in wordCountList)
            {
                Console.WriteLine(curWord.Value + " | " + curWord.Key);
            }
        }

        public void outputUniqueTerms()
        {
            m_UniqueTerms.Sort();

            Console.WriteLine(" ========================================================");
            Console.WriteLine("Unique Terms::");
            Console.WriteLine("-------------------------");

            foreach(string curTerm in m_UniqueTerms)
            {
                Console.WriteLine(curTerm);
            }
        }

        public void outputFoundTimes()
        {
            foreach (subFileList curList in m_ParsedSubLists)
            {
                Console.WriteLine(curList.directoryPath + " ========================================================");

                foreach (loadedVideoSubs curSubFile in curList.entries)
                {
                    Console.WriteLine(curSubFile.filePath + " -----------------------------------------------");

                    List<KeyValuePair<string, foundTermData>> foundTermsList = curSubFile.termData.ToList();

                    foreach(KeyValuePair<string, foundTermData> curTerm in foundTermsList)
                    {
                        foreach (termTimeData curTimeEntry in curTerm.Value.foundTimes)
                        {
                            Console.WriteLine(formatTime(curTimeEntry.time) + "\t" + curTerm.Key + "\t" + curTimeEntry.context);
                        }
                    }

                    Console.WriteLine("// End File -----------------------------------------------");
                }

                Console.WriteLine("// End SubList ========================================================");
            }
        }

        public static string formatTime(TimeSpan aTime)
        {
            return aTime.ToString(@"hh\:mm\:ss");
        }

        public subProcessor(generalConfig aConfig)
        {
            m_Config = aConfig;

            if (m_Config.loadSuccess)
            {
                m_ParsedSubLists = new List<subFileList>();
                m_GlobalWordCounts = new Dictionary<string, int>();
                m_UniqueTerms = new List<string>();
                m_LowFrequencyTerms = new Dictionary<string, int>();

                string[] dirList = Directory.GetDirectories(generalConfig.rootPath);
                List<Task> taskList = new List<Task>();
                foreach (string curDir in dirList)
                {
                    taskList.Add(
                        Task.Run(() => {
                            subFileList curSubList = new subFileList(curDir, m_Config);
                            if (curSubList.loadSuccess)
                            {
                                lock (m_ParsedSubLists)
                                {
                                    m_ParsedSubLists.Add(curSubList);
                                }
                            }
                        })
                    );
                }

                Task.WaitAll(taskList.ToArray());
            }
        }
    }
}