using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace CardHelper2
{
    class generalConfig
    {

        private configData loadedConfig;
        public argumentConfig args;

        public List<string> searchTerms { get { return loadedConfig.searchTerms; } }
        public List<string> skipTerms { get { return loadedConfig.skipTerms; } }

        public int lowFrequencyTermCountMaxGlobal { get { return loadedConfig.lowFrequencyTermCountMaxGlobal; } }
        public int lowFrequencyTermCountMaxList { get { return loadedConfig.lowFrequencyTermCountMaxList; } }

        public static string rootPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CardCaptions");
        public static int timeBuffer = 5000;

        private bool m_loadSuccess;
        public bool loadSuccess { get { return m_loadSuccess; } }

        public generalConfig(string[] aArgs)
        {
            args = new argumentConfig(aArgs);

            if (Directory.Exists(rootPath))
            {
                string termsPath = System.IO.Path.Combine(rootPath, "search_terms.json");
                if (File.Exists(termsPath))
                {
                    string termsFileUnparsed = File.ReadAllText(termsPath);
                    loadedConfig = JsonConvert.DeserializeObject<configData>(termsFileUnparsed);

                    m_loadSuccess = true;
                }
                else
                {
                    Console.WriteLine("Cannot find terms config: " + termsPath);
                }
            }
            else
            {
                Console.WriteLine("Cannot find path: " + rootPath);
            }
        }
    }
}
