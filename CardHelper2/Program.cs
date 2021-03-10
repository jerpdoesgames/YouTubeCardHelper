using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CardHelper
{

    class Program
    {
        public static string inputPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CardCaptions");
        public static int timeBuffer = 5000;

        static void Main(string[] args)
        {

            List<string> sourceList = new List<string>();
            // sourceList.Add("dark souls");
            // sourceList.Add("dark souls 2");
            // sourceList.Add("dark souls two");
            // sourceList.Add("dark souls 3");
            // sourceList.Add("dark souls three");
            sourceList.Add("code vein");
            sourceList.Add("undermine");
            sourceList.Add("going under");
            sourceList.Add("hydorah");
            sourceList.Add("zeroranger");
            sourceList.Add("zero ranger");
            sourceList.Add("shmup");
            sourceList.Add("monolith");
            sourceList.Add("jarvis");
            sourceList.Add("mortal shell");
            sourceList.Add("subnautica");
            sourceList.Add("cuphead");
            sourceList.Add("cup head");
            // sourceList.Add("contra");
            // sourceList.Add("super c");
            sourceList.Add("super see");
            sourceList.Add("shattered");
            sourceList.Add("kenshi");
            sourceList.Add("noita");
            sourceList.Add("outer wilds");
            sourceList.Add("opus magnum");
            sourceList.Add("olija");
            sourceList.Add("olia");
            sourceList.Add("bloodstained");
            sourceList.Add("curse of the moon");
            sourceList.Add("ritual of the night");
            sourceList.Add("primal light");
            sourceList.Add("wizard of legend");
            sourceList.Add("xcom");
            sourceList.Add("rogue legacy");
            sourceList.Add("absolver");
            sourceList.Add("panzer paladin");
            sourceList.Add("sojourner");
            sourceList.Add("rival megagun");
            sourceList.Add("mega gun");
            sourceList.Add("hyper light drifter");
            sourceList.Add("small saga");
            sourceList.Add("a short hike");
            sourceList.Add("ape out");
            sourceList.Add("streets of rage");
            sourceList.Add("vernal edge");
            sourceList.Add("rigid force alpha");
            sourceList.Add("rigid force redux");
            sourceList.Add("iron meat");
            sourceList.Add("oniken");
            sourceList.Add("blazing chrome");
            sourceList.Add("goose game");
            sourceList.Add("atomicrops");
            sourceList.Add("gungeon");
            sourceList.Add("mind seize");
            sourceList.Add("mindseize");
            sourceList.Add("itta");
            sourceList.Add("will of the wisps");
            sourceList.Add("blind forest");
            sourceList.Add("hell is other demons");
            sourceList.Add("ashen");
            sourceList.Add("mario maker");
            sourceList.Add("river city");
            sourceList.Add("katana zero");
            sourceList.Add("night in the woods");
            sourceList.Add("near automata");
            sourceList.Add("nier");
            sourceList.Add("overwhelm");
            sourceList.Add("dandara");
            sourceList.Add("sekiro");
            sourceList.Add("sundered");
            sourceList.Add("sinner");
            sourceList.Add("timespinner");
            sourceList.Add("shovel knight");
            sourceList.Add("doom 2016");
            sourceList.Add("doom eternal");
            sourceList.Add("into the breach");
            sourceList.Add("invisible ink");
            sourceList.Add("iconoclasts");
            sourceList.Add("the mummy");
            sourceList.Add("demastered");
            sourceList.Add("jotun");
            sourceList.Add("zelda");
            sourceList.Add("breath of the wild");
            sourceList.Add("my site");
            sourceList.Add("my website");
            sourceList.Add("my web site");
            sourceList.Add("jerp.tv");
            sourceList.Add("jerp dot tv");
            sourceList.Add("twitter");
            sourceList.Add("instagram");
            sourceList.Add("extras channel");
            sourceList.Add("discord");
            sourceList.Add("the messenger");
            sourceList.Add("cyber shadow");
            sourceList.Add("narita");
            sourceList.Add("steel assault");
            sourceList.Add("mindseize");
            sourceList.Add("mind seize");
            sourceList.Add("hell is other demons");
            sourceList.Add("weeb");
            sourceList.Add("cannon fodder");
            sourceList.Add("mcqueeb");

            // sourceList.Add("messenger");    // possibly delete

            int lastTime = 0;
            string lastTerm = "";

            if (Directory.Exists(inputPath))
            {
                string[] fileList = Directory.GetFiles(inputPath); ;
                Console.WriteLine(fileList.Length + " files found.");

                SubtitlesParser.Classes.Parsers.VttParser parser = new SubtitlesParser.Classes.Parsers.VttParser();

                for (int fileIndex=0; fileIndex < fileList.Length; fileIndex++)
                {
                    lastTime = 0;
                    lastTerm = "";
                    using (FileStream currentSubFile = File.OpenRead(fileList[fileIndex]))
                    {
                        List<SubtitlesParser.Classes.SubtitleItem> subList = parser.ParseStream(currentSubFile, Encoding.UTF8);

                        for (int lineIndex=0; lineIndex < subList.Count; lineIndex++)
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
                // 
            }
            else
            {
                Console.WriteLine("Can't find directory " + inputPath);

            }


            /*
            config followConfig = new config();

            if (followConfig.loaded)
            {
                Console.WriteLine("config loaded, good to go");
                logger requestLog = new logger(followConfig, "cardHelperLog.txt");
                if (Directory.Exists(""))
                {

                }

            }
            else
            {
                Console.WriteLine("Failed to load config");
            }
            */


        }
    }
}
