﻿using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        private Dictionary<string, long> m_WordRarity;

        private long m_WordRarityMin = -1;
        private long m_WordRarityMax = -1;

        public Dictionary<string, long> wordRarity { get { return m_WordRarity; } }

        public static string removeNonAlphanumeric(string aInput)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            return rgx.Replace(aInput, "");
        }

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
                    m_Config.args.showRareWords ||
                    m_Config.args.outputHeatmap
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
                m_Config.args.showRareWords ||
                m_Config.args.outputHeatmap
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

        /// <summary>
        /// Outputs HTML pages showing the full transcript with each word colored relative to the relative importance of each word.
        /// </summary>
        public void outputHeatmap()
        {
            foreach (subFileList curList in m_ParsedSubLists)
            {
                foreach (loadedVideoSubs curSubFile in curList.entries)
                {
                    string output = "";
                    string lastLine = "";

                    foreach (SubtitleItem curSubLine in curSubFile.subtitleLines)
                    {
                        string curLineString = curSubLine.Lines[0];
                        if (curLineString != lastLine && curLineString != "[Music]" && curLineString != "[&nbsp;__&nbsp;]")
                        {
                            double rareTermCount = 0;
                            double uniqueTermCount = 0;
                            double rarityDistScore = 0;
                            double searchTermCount = 0;
                            lastLine = curLineString;


                            
                            List<string> wordsInLine = curLineString.Split(" ").ToList<string>();
                            List<string> lineWordsToSearch = new List<string>();

                            foreach (string curWord in wordsInLine)
                            {
                                lineWordsToSearch.Add(removeNonAlphanumeric(curWord));
                            }

                            bool useStyle = false;

                            string rareTermString = "";
                            string uniqueTermString = "";
                            string searchTermString = "";
                            string rarityScoreString = "";

                            foreach (string curTerm in lowFrequencyTerms.Keys.ToList())
                            {
                                if (lineWordsToSearch.Contains(curTerm))
                                {
                                    if (string.IsNullOrEmpty(rareTermString))
                                    {
                                        rareTermString = "<b>Rare Terms:</b> ";
                                    }

                                    rareTermString += curTerm + ", ";
                                    rareTermCount++;
                                }
                            }

                            foreach(string curTerm in uniqueTerms)
                            {
                                if (lineWordsToSearch.Contains(curTerm))
                                {
                                    if (string.IsNullOrEmpty(uniqueTermString))
                                    {
                                        uniqueTermString = "<b>Unique Terms:</b> ";
                                    }

                                    uniqueTermString += curTerm + ", ";

                                    uniqueTermCount++;
                                    useStyle = true;
                                }
                            }

                            foreach(string curTerm in m_Config.searchTerms)
                            {
                                if (lineWordsToSearch.Contains(curTerm))
                                {
                                    if (string.IsNullOrEmpty(searchTermString))
                                    {
                                        searchTermString = "<b>Search Terms:</b> ";
                                    }

                                    searchTermString += curTerm + ", ";

                                    searchTermCount++;
                                    useStyle = true;
                                }
                            }

                            long rarityDist = (m_WordRarityMax - m_WordRarityMin);

                            foreach (string curWord in lineWordsToSearch)
                            {
                                if (wordRarity.ContainsKey(curWord))
                                {
                                    long rarityAmount = (wordRarity[curWord] - m_WordRarityMin);
                                    double rarityPercent = rarityAmount / rarityDist;

                                    rarityDistScore += 0.4 * (1 - rarityPercent);
                                }
                            }

                            if (rarityDistScore > 0)
                            {
                                rarityScoreString = "<b>Rarity Score:</b> " + rarityDistScore;
                            }

                            if (rarityDistScore + rareTermCount >= 3.5)
                            {
                                useStyle = true;
                            }

                            double redPercent = Math.Min(rarityDistScore + rareTermCount, 8.0f) / 8.0f;
                            double greenPercent = Math.Min(uniqueTermCount, 3.0f) / 3.0f; 
                            double bluePercent = Math.Min(searchTermCount, 2.0f) / 2.0f;

                            int colorRed = (int)(64 + (192 * redPercent));
                            int colorGreen = (int)(64 + (192 * greenPercent));
                            int colorBlue = (int)(64 + (192 * bluePercent));

                            TimeSpan startTimespan = TimeSpan.FromMilliseconds(curSubLine.StartTime);

                            string timeString = string.Format("{0}h {1}m {2}s", startTimespan.Hours, startTimespan.Minutes, startTimespan.Seconds);
                            string timecodeString = string.Format("{0}:{1}:{2}:00", startTimespan.Hours.ToString().PadLeft(2, '0'), startTimespan.Minutes.ToString().PadLeft(2, '0'), startTimespan.Seconds.ToString().PadLeft(2, '0'));

                            output += useStyle ? string.Format("<span class=\"termEntry\" timeData=\"" + string.Join("<br/>", timeString, string.Format("<b>Total Score:</b>{0}", Math.Round(rareTermCount + uniqueTermCount + rarityDistScore + searchTermCount, 2)), rareTermString, uniqueTermString, searchTermString, rarityScoreString) + "\" timeCode=\"" + timecodeString + "\" style=\"color: rgb({0} {1} {2});\" onmouseover=\"hoverText(this);\" onclick=\"selectEntry(this);\">{3}</span> ", colorRed, colorGreen, colorBlue, curLineString) : "<span>"+curLineString+"</span> ";
                        }
                        
                    }

                    string outputPath = System.IO.Path.Combine(curSubFile.filePath.Replace("vtt", "html").Replace("srt", "html").Replace("CardCaptions", "CardCaptions\\Output"));
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));
                    string scriptInfo = "<script>function selectEntry(aElement) { navigator.clipboard.writeText(aElement.getAttribute(\"timeCode\")); } \nfunction hoverText(aElement) { let tipElement = document.getElementById(\"tooltip\"); tooltip.innerHTML = aElement.getAttribute(\"timeData\") != null ? aElement.getAttribute(\"timeData\") : \"???\"; let boundingBox = aElement.getBoundingClientRect(); tooltip.style.left = aElement.offsetLeft; tooltip.style.top = aElement.offsetTop + (boundingBox.bottom - boundingBox.top); }</script>";
                    string styleInfo = "<style>.termEntry { text-decoration: underline; cursor: pointer; }\n\nspan { opacity: 0.75; } span:hover { opacity: 1.0; } #tooltip { width: 20rem; border: 1px solid #DDDDDD; border-radius: 4px; padding: 4px; background: #000000; position: absolute; left: 0px; top: 0px; }\n\n</style>";
                    File.WriteAllText(outputPath, string.Format("<html style=\"background-color: #000000; color: #AAAAAA; font-family: Calibri;\"><head>{1}{2}</head><body>{0}<div id=\"tooltip\"></div></body></html>", output, styleInfo, scriptInfo));
                }
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

                m_WordRarity = new Dictionary<string, long>();

                using (StreamReader wordListReader = new StreamReader(System.IO.Path.Combine(generalConfig.rootPath, "word_rarity_list.csv")))
                {
                    int lineIndex = 0;
                    while (!wordListReader.EndOfStream)
                    {
                        string curLine = wordListReader.ReadLine();

                        lineIndex++;
                        if (lineIndex == 1)
                            continue;


                        string[] values = curLine.Split(",");

                        m_WordRarityMin = m_WordRarityMin == -1 ? long.Parse(values[1]) : Math.Min(m_WordRarityMin, long.Parse(values[1]));
                        m_WordRarityMax = m_WordRarityMax == -1 ? long.Parse(values[1]) : Math.Max(m_WordRarityMax, long.Parse(values[1]));

                        m_WordRarity[values[0]] = long.Parse(values[1]);
                    }
                }

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