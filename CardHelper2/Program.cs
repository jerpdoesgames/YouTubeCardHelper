using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace CardHelper
{

    public class termConfig
    {
        public List<string> terms;
    }

    class Program
    {
        public static string inputPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CardCaptions");
        public static int timeBuffer = 5000;
        

        static void Main(string[] args)
        {
            Dictionary<string, int> wordCounts = new Dictionary<string, int>();
            string termsPath = System.IO.Path.Combine(inputPath, "search_terms.json");
            if (Directory.Exists(inputPath))
            {
                if (File.Exists(termsPath))
                {
                    string termsFileUnparsed = File.ReadAllText(termsPath);
                    termConfig newTerms = JsonConvert.DeserializeObject<termConfig>(termsFileUnparsed);

                    List<string> sourceList = newTerms.terms;

                    int lastTime = 0;
                    string lastTerm;

                    string[] fileList = Directory.GetFiles(inputPath); ;
                    Console.WriteLine(fileList.Length + " files found.");

                    SubtitlesParser.Classes.Parsers.VttParser parser = new SubtitlesParser.Classes.Parsers.VttParser();

                    for (int fileIndex = 0; fileIndex < fileList.Length; fileIndex++)
                    {
                        if (Path.GetExtension(fileList[fileIndex]) != ".vtt")
                            continue;

                        lastTime = 0;
                        lastTerm = "";
                        using (FileStream currentSubFile = File.OpenRead(fileList[fileIndex]))
                        {
                            List<SubtitlesParser.Classes.SubtitleItem> subList = parser.ParseStream(currentSubFile, Encoding.UTF8);

                            for (int lineIndex = 0; lineIndex < subList.Count; lineIndex++)
                            {
                                SubtitlesParser.Classes.SubtitleItem curSubLine = subList[lineIndex];

                                string[] curWordList = curSubLine.Lines[0].Split(" ");
                                for (int wordIndex = 0; wordIndex < curWordList.Length; wordIndex++)
                                {
                                    if (!wordCounts.ContainsKey(curWordList[wordIndex]))
                                    {
                                        wordCounts[curWordList[wordIndex]] = 1;
                                    }
                                    else
                                    {
                                        wordCounts[curWordList[wordIndex]]++;
                                    }
                                }

                                for (int sourceIndex = 0; sourceIndex < sourceList.Count; sourceIndex++)
                                {
                                    if (curSubLine.Lines[0].Contains(sourceList[sourceIndex]) && (sourceList[sourceIndex] != lastTerm || ((curSubLine.StartTime > lastTime + timeBuffer) && lastTime > 0)))
                                    {

                                        if (lastTime == 0)
                                        {
                                            Console.WriteLine("=====> " + fileList[fileIndex] + " -----------------------");
                                        }

                                        TimeSpan curSpan = TimeSpan.FromMilliseconds(curSubLine.StartTime);
                                        string curTime = curSpan.ToString(@"hh\:mm\:ss");

                                        Console.WriteLine(curTime + " | " + sourceList[sourceIndex] + " | " + curSubLine.Lines[0]);
                                        lastTerm = sourceList[sourceIndex];
                                        lastTime = curSubLine.StartTime;
                                    }
                                }

                            }

                            if (lastTime > 0)
                            {
                                Console.WriteLine("-----------------------------------------");
                            }
                        }
                    }

                    List<KeyValuePair<string, int>> wordCountList = wordCounts.ToList();

                    wordCountList.Sort(
                        delegate (KeyValuePair<string, int> pair1,
                        KeyValuePair<string, int> pair2)
                        {
                            return pair1.Value.CompareTo(pair2.Value);
                        }
                    );

                    Console.WriteLine("word counts logged::");
                    Console.WriteLine("-------------------------");

                    foreach (KeyValuePair<string, int> curWord in wordCountList)
                    {
                        Console.WriteLine(curWord.Value + " | " + curWord.Key);
                    }
                }
                else
                {
                    Console.WriteLine("Can't find directory " + inputPath);
                }
            }
        }
    }
}
