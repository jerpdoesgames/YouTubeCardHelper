using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace CardHelper2
{
    class subFileList
    {
        public List<loadedVideoSubs> entries;
        private bool m_Loadsuccess;
        public bool loadSuccess { get { return m_Loadsuccess; } }
        private generalConfig m_Config;
        private string m_DirectoryPath;
        public string directoryPath { get { return m_DirectoryPath; } }

        private Dictionary<string, int> m_WordCounts;
        public Dictionary<string, int> wordCounts { get { return m_WordCounts; } }

        public void aggregateWordCountsIntoOtherVideos()
        {
            foreach (loadedVideoSubs curSubFile in entries)
            {
                List<KeyValuePair<string, foundTermData>> foundTermsList = curSubFile.termData.ToList();

                foreach (KeyValuePair<string, foundTermData> curTerm in foundTermsList)
                {
                    if (wordCounts.ContainsKey(curTerm.Key))
                    {
                        wordCounts[curTerm.Key]++;
                    }
                    else
                    {
                        wordCounts[curTerm.Key] = 1;
                    }

                    foreach (loadedVideoSubs otherSubFile in entries)
                    {
                        if (otherSubFile != curSubFile)
                        {
                            if (otherSubFile.termData.ContainsKey(curTerm.Key))
                            {
                                otherSubFile.termData[curTerm.Key].foundInSameListOtherVideo += curTerm.Value.foundCount;
                            }
                        }
                    }
                }
            }
        }

        public subFileList(string aDirectoryPath, generalConfig aConfig)
        {
            m_Config = aConfig;
            m_DirectoryPath = aDirectoryPath;
            if (Directory.Exists(aDirectoryPath))
            {
                m_WordCounts = new Dictionary<string, int>();
                entries = new List<loadedVideoSubs>();
                string[] fileList = Directory.GetFiles(aDirectoryPath);
                List<Task> videoFileTasks = new List<Task>();

                foreach (string curFile in fileList)
                {
                    if (Path.GetExtension(curFile) == ".vtt" || Path.GetExtension(curFile) == ".srt")
                    {
                        videoFileTasks.Add(
                            Task.Run(() => {
                                loadedVideoSubs newLoadedSubs = new loadedVideoSubs(curFile, m_Config);
                                if (newLoadedSubs.loadSuccess)
                                {
                                    lock (entries)
                                    {
                                        entries.Add(newLoadedSubs);
                                    }
                                }
                            })
                        );
                    }
                }

                Task.WaitAll(videoFileTasks.ToArray());
                m_Loadsuccess = true;
            }
        }
    }
}
