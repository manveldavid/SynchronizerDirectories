using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CompareDir
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string fileConf = File.ReadAllText("LinkedDirs.conf");
            List<string> listOfDir = fileConf.Split('\n').ToList();
            listOfDir = listOfDir.Select(i => i.Length > 0? i.Substring(0,i.Length-1) : i).ToList();
            Dictionary<string, string> pathDirs = new Dictionary<string, string>();

            if (listOfDir.Count > 3 && listOfDir.Count % 3 == 0)
            {

                for(int i = 0; i < listOfDir.Count;i++)
                {
                    if (listOfDir[i] == "")
                    {
                        pathDirs.Add(listOfDir[i-2], listOfDir[i-1]);
                    }
                }
            }

            if (pathDirs.Count > 0)
            {
                foreach (var key in pathDirs.Keys)
                {
                    ActionsOnDIrAndFiles.MainCompareAction(new Dir(key), new Dir(pathDirs[key]));
                    ActionsOnDIrAndFiles.MainCompareAction(new Dir(pathDirs[key]), new Dir(key));
                }
            }
            
            Console.WriteLine("The end");

            Console.ReadKey();
        }
    }
}
