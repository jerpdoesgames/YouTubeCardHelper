using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace CardHelper2
{
    class Program
    {
        public static string rootPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CardCaptions");
        public static int timeBuffer = 5000;

        static void Main(string[] args)
        {
            generalConfig configData = new generalConfig(args);
            if (configData.loadSuccess)
            {
                if (!configData.args.emptyArgs)
                {
                    subProcessor mainProcessor = new subProcessor(configData);

                    if (
                        configData.args.showWordCounts ||
                        configData.args.searchRareWords ||
                        configData.args.searchUniqueWords ||
                        configData.args.showRareWords ||
                        configData.args.showUniqueWords
                    )
                    {
                        mainProcessor.aggregateWordCounts();
                    }

                    if (configData.args.searchUniqueWords)
                        mainProcessor.searchUniqueTerms();

                    if (configData.args.searchRareWords)
                        mainProcessor.searchLowFrequencyTerms();

                    if (configData.args.showWordCounts)
                        mainProcessor.outputWordCounts();

                    if (configData.args.showWordList)
                        mainProcessor.outputFoundTimes();

                    if (configData.args.showUniqueWords)
                        mainProcessor.outputUniqueTerms();

                    if (configData.args.showRareWords)
                        mainProcessor.outputRareTerms();
                }
            }
        }
    }
}
