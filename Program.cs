using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareDir
{
    
    public delegate void VoidFileByPath(JustFile file, string path);
    public delegate void VoidDirByPath(Dir dir, Dir dependDir, bool recursive);
    internal class Program
    {
        

        static void Main(string[] args)
        {
            List<string> listOfDir = new List<string>();
            string tempPath = "";
            do
            {
                tempPath = Console.ReadLine();
                if (tempPath != "") listOfDir.Add(tempPath);
            }
            while (tempPath != "");

            List<Dir> AllDirs = ActionsOnDIrAndFiles.BuildDirsClassFromPathList(listOfDir);

            var problemFiles = Dir.CompareFilesInDirs(AllDirs.First(), AllDirs.Last());
            var problemDirs = Dir.CompareDirs(AllDirs.First(), AllDirs.Last());

            ActionsOnDIrAndFiles.SyncDirActionDependKey(problemDirs, AllDirs, ConsoleKey.Enter);
            ActionsOnDIrAndFiles.SyncFileActionDependKey(problemFiles, AllDirs, ConsoleKey.Enter);
            
            Console.WriteLine("Sync? (press \"a\" to add | \"r\" to remove | other key to nothing)");
            ConsoleKey key = Console.ReadKey().Key;

            ActionsOnDIrAndFiles.SyncDirActionDependKey(problemDirs, AllDirs, key);
            ActionsOnDIrAndFiles.SyncFileActionDependKey(problemFiles, AllDirs, key);

            Console.WriteLine("The end");

            Console.ReadKey();
        }
    }
}
