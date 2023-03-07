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
            Dictionary<Dir, Dir> linkedDirs = new Dictionary<Dir, Dir>();

            if (listOfDir.Count > 3 && listOfDir.Count % 3 == 0)
            {
                Dictionary<string, string> pathDirs = new Dictionary<string, string>();

                for(int i = 0; i < listOfDir.Count;i++)
                {
                    if (listOfDir[i] == "")
                    {
                        pathDirs.Add(listOfDir[i-2], listOfDir[i-1]);
                    }
                }

                foreach(var key in pathDirs.Keys)
                {
                    linkedDirs.Add(new Dir(key), new Dir(pathDirs[key]));
                }

            }

            if (linkedDirs.Count > 0)
            {
                foreach (var key in linkedDirs.Keys)
                {
                    ActionsOnDIrAndFiles.MainCompareAction(key, linkedDirs[key]);
                    ActionsOnDIrAndFiles.MainCompareAction(linkedDirs[key], key);
                }
            }
            
            Console.WriteLine("The end");

            Console.ReadKey();
        }
    }
}
