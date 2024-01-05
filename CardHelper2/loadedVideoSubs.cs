using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CardHelper2
{
    class loadedVideoSubs
    {
        private generalConfig m_Config;
        private string m_FilePath;
        public Dictionary<string, foundTermData> termData;
        public string filePath { get { return m_FilePath; } }

        public static int timeBuffer = 5000;
        private bool m_Loadsuccess;
        public bool loadSuccess { get { return m_Loadsuccess; } }

        private int m_LastTermTime = 0;
        private string m_LastTerm;

        private List<SubtitlesParser.Classes.SubtitleItem> m_SubtitleLines;
        public List<SubtitlesParser.Classes.SubtitleItem> subtitleLines { get { return m_SubtitleLines; } }

        private bool isValidTerm(string aTerm)
        {
            return Regex.Match(aTerm, @"[a-zA-Z]{4,}").Success;
        }

        private foundTermData checkCreateFoundTerm(string aTerm)
        {
            if (!termData.ContainsKey(aTerm))
            {
                lock (termData)
                {
                    termData[aTerm] = new foundTermData(aTerm);
                }
            }

            return termData[aTerm];
        }

        private void collectTermTimesFromLine(SubtitlesParser.Classes.SubtitleItem aSubLine, List<string> aWordListOverride = null)
        {
            List<string> useSearchTerms = aWordListOverride != null ? aWordListOverride: m_Config.searchTerms;
            foreach (string curSearchTerm in useSearchTerms)
            {
                if (aWordListOverride != null && m_Config.searchTerms.Contains(curSearchTerm))
                {
                    continue;   // Skip anything that's already been covered
                }

                string curTermLower = curSearchTerm.ToLower();
                string[] curLineTermsLower = aSubLine.Lines[0].ToLower().Split(" ");
                if (!m_Config.skipTerms.Contains(curTermLower) && curLineTermsLower.Contains(curTermLower))
                {
                    
                    foundTermData curTerm = checkCreateFoundTerm(curTermLower);

                    bool foundNearbyTerm = false;
                    TimeSpan curSpan = TimeSpan.FromMilliseconds(aSubLine.StartTime);

                    foreach (termTimeData curFoundData in curTerm.foundTimes)
                    {
                        TimeSpan curFoundTime = curFoundData.time;
                        if (curSpan.TotalMilliseconds >= curFoundTime.TotalMilliseconds && curSpan.TotalMilliseconds <= curFoundTime.TotalMilliseconds + timeBuffer)
                        {
                            foundNearbyTerm = true;
                            break;
                        }
                    }

                    // Skip if terms match and there's a nearby time
                    if (!foundNearbyTerm)
                    {
                        lock(curTerm.foundTimes)
                        {
                            curTerm.foundTimes.Add(new termTimeData(curSpan, aSubLine.Lines[0], m_FilePath));
                        }

                        m_LastTerm = curTermLower;
                        m_LastTermTime = aSubLine.StartTime;
                    }

                }
            }
        }

        private void collectWordCountsFromLine(SubtitlesParser.Classes.SubtitleItem aSubLine)
        {
            string[] curWordList = aSubLine.Lines[0].Split(" ");
            string curWord;
            for (int wordIndex = 0; wordIndex < curWordList.Length; wordIndex++)
            {
                curWord = curWordList[wordIndex].ToLower();
                if (isValidTerm(curWord) && m_Config.skipTerms.IndexOf(curWord) == -1)
                {
                    foundTermData curTerm = checkCreateFoundTerm(curWord);
                    curTerm.foundCount++;
                }
            }
        }

        public void aggregateTermData(bool aCollectWordCounts = true, List<string> aWordListOverride = null)
        {
            SubtitlesParser.Classes.Parsers.SubParser parser = new SubtitlesParser.Classes.Parsers.SubParser();

            m_LastTerm = "";
            m_LastTermTime = 0;

            using (FileStream currentSubFile = File.OpenRead(m_FilePath))
            {
                m_SubtitleLines = parser.ParseStream(currentSubFile, Encoding.UTF8);

                for (int lineIndex = 0; lineIndex < m_SubtitleLines.Count; lineIndex++)
                {
                    SubtitlesParser.Classes.SubtitleItem curSubLine = m_SubtitleLines[lineIndex];

                    if (aCollectWordCounts)
                        collectWordCountsFromLine(curSubLine);

                    collectTermTimesFromLine(curSubLine, aWordListOverride);
                }
            }
        }

        public loadedVideoSubs(string aFilePath, generalConfig aConfig)
        {
            if (File.Exists(aFilePath))
            {
                termData = new Dictionary<string, foundTermData>();
                m_FilePath = aFilePath;
                m_Config = aConfig;

                bool wordCounts = m_Config.args.showWordCounts || m_Config.args.searchRareWords || m_Config.args.showRareWords || m_Config.args.searchUniqueWords || m_Config.args.showUniqueWords || m_Config.args.outputHeatmap;

                aggregateTermData(wordCounts);

                m_Loadsuccess = true;
            }
        }
    }
}
