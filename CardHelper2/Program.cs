using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

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
            string termsPath = "../../../../search_terms.json";
            if (File.Exists(termsPath))
            {
                string termsFileUnparsed = File.ReadAllText(termsPath);
                termConfig newTerms = JsonConvert.DeserializeObject<termConfig>(termsFileUnparsed);
                Console.WriteLine(newTerms.terms[0]);

                List<string> sourceList = newTerms.terms;

                int lastTime = 0;
                string lastTerm;

                if (Directory.Exists(inputPath))
                {
                    string[] fileList = Directory.GetFiles(inputPath); ;
                    Console.WriteLine(fileList.Length + " files found.");

                    SubtitlesParser.Classes.Parsers.VttParser parser = new SubtitlesParser.Classes.Parsers.VttParser();

                    for (int fileIndex = 0; fileIndex < fileList.Length; fileIndex++)
                    {
                        lastTime = 0;
                        lastTerm = "";
                        using (FileStream currentSubFile = File.OpenRead(fileList[fileIndex]))
                        {
                            List<SubtitlesParser.Classes.SubtitleItem> subList = parser.ParseStream(currentSubFile, Encoding.UTF8);

                            for (int lineIndex = 0; lineIndex < subList.Count; lineIndex++)
                            {
                                SubtitlesParser.Classes.SubtitleItem curSubLine = subList[lineIndex];

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
                }
                else
                {
                    Console.WriteLine("Can't find directory " + inputPath);
                }
            }
        }
    }
}
