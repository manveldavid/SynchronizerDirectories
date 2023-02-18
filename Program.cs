using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareDir
{
    public class Dir
    {
        public Dir(string path) 
        {
            Path = path;
            Name = Path.Substring(Path.LastIndexOf('\\') + 1);

            Files = new List<JustFile>();
            ChildDirs = new List<Dir>();

            var childsSourse = Directory.GetDirectories(path).ToList();
            var filesSourse = Directory.GetFiles(path).ToList();
            
            foreach (var childPath in childsSourse) 
                if(childPath!="") ChildDirs.Add(new Dir(childPath,this));
            foreach (var filePath in filesSourse)
                if (filePath != "") Files.Add(new JustFile(filePath,this));
        }
        public Dir(string path, Dir Parent) : this(path) { ParentDir = Parent; }

        public static List<Dir> CompareDirs(Dir mainDir, Dir compareDir)
        {
            List<Dir> problemDirs = new List<Dir>();
            if (mainDir.ChildDirs != null)
                if (compareDir.ChildDirs != null)
                    foreach (Dir mainDirChild in mainDir.ChildDirs)
                    {
                        if (!compareDir.ChildDirs.Select(o => o.Name).Contains(mainDirChild.Name))
                            problemDirs.Add(mainDirChild);

                        else problemDirs.AddRange(CompareDirs(mainDirChild,
                            compareDir.ChildDirs.FirstOrDefault(x => x.Name == mainDirChild.Name)));
                    }
            return problemDirs;
        }

        public static List<JustFile> CompareFilesInDirs(Dir mainDir, Dir compareDir)
        {
            List<JustFile> problemFiles = new List<JustFile>();

            bool dirsExist = mainDir!= null && compareDir!=null;
            if (dirsExist)
            {
                foreach (var fileInMainDir in mainDir.Files)
                {
                    if (!compareDir.Files.Select(o => o.Name).Contains(fileInMainDir.Name))
                        problemFiles.Add(fileInMainDir);
                    else if (compareDir.Files
                      .FirstOrDefault(o => o.Name == fileInMainDir.Name)
                      .Info.Length != fileInMainDir.Info.Length) 
                        problemFiles.Add(fileInMainDir);
                }

                if (compareDir.ChildDirs != null) 
                    foreach (Dir mainDirChild in mainDir.ChildDirs)
                    {
                        problemFiles.AddRange(CompareFilesInDirs(mainDirChild,
                               compareDir.ChildDirs.FirstOrDefault(x => x.Name == mainDirChild.Name)));
                    }
            }
            return problemFiles;
        }

        public int CountAllDir()
        {
            int DirsCount = 1;
            if (ChildDirs != null) foreach (var child in ChildDirs)
                    DirsCount += child.CountAllDir();
            return DirsCount;
        }
        public int CountAllFiles()
        {
            int FilesCount = Files.Count;
            if (ChildDirs != null) foreach (var child in ChildDirs)
                    FilesCount += child.CountAllFiles();
            return FilesCount;
        }
        public ulong CountAllFileSize()
        {
            ulong FilesSize = 0;

            foreach (var somefile in Files) FilesSize += (ulong)somefile.Info.Length;
            foreach (var child in ChildDirs) if (child != null)
                    FilesSize += child.CountAllFileSize();
            return FilesSize;
        }

        public string Name { get; private set; }
        public string Path { get; private set; }
        public List<JustFile> Files { get; private set; }
        public Dir ParentDir { get; private set; }
        public List<Dir> ChildDirs { get; private set; }
        public new string ToString() => Name;
    }
    public class JustFile
    {
        public JustFile(string path) 
        {
            Info = new FileInfo(path);
            Path = path;
            Name = Path.Substring(Path.LastIndexOf('\\') + 1);
            Ending = Name.LastIndexOf('.') != -1 ? Name.Substring(Name.LastIndexOf('.') + 1) : null;
        }
        public JustFile(string name, Dir Sourse):this(name) { SourseDir = Sourse; }

        public Dir SourseDir { get; private set; }
        public string Name { get; private set; }
        public string Ending { get; private set; }
        public FileInfo Info { get; private set; }
        public string Path { get; private set; }
        public new string ToString() => Name;
    }
    
    public delegate void VoidFileByPath(JustFile file, string path);
    public delegate void VoidDirByPath(Dir dir, Dir dependDir, bool recursive);
    internal class Program
    {
        #region HelpFunctions
        public static List<Dir> BuildDirsClassFromPathList(List<string> listOfPath)
        {
            List<Dir> allDirs = new List<Dir>();
            foreach (string path in listOfPath) allDirs.Add(new Dir(path));
            return allDirs;
        }
        
        public static void ReplaceOrCopyFileByPath(JustFile file, string path)
        {
            if (File.Exists(path))
            {
                if (file.Info.IsReadOnly != true)
                {
                    File.Delete(path);
                    file.Info.CopyTo(path);
                }
            }
            else file.Info.CopyTo(path);
        }
        public static void DeleteFileByPath(JustFile file, string path)
        {
            if (file.Info.IsReadOnly != true)
            {
                File.Delete(path);
            }
        }
        public static void PrintProblemFileByPath(JustFile file, string path)
        {
            Console.WriteLine(path);
        }
        public static void SyncFileActionDependKey(List<JustFile> problemFiles, List<Dir> AllDirs, ConsoleKey key)
        {

            foreach (var file in problemFiles)
            {
                string PathNew = file.Path;
                VoidFileByPath NeedFileVoid = new VoidFileByPath(PrintProblemFileByPath);
                
                if (key == ConsoleKey.A)
                {
                    PathNew = AllDirs.Last().Path + "\\" + 
                        file.Path.Substring(AllDirs.First().Path.Length);
                    NeedFileVoid += new VoidFileByPath(ReplaceOrCopyFileByPath);
                }
                else if(key == ConsoleKey.R)
                {
                    NeedFileVoid += new VoidFileByPath(DeleteFileByPath);
                }
                
                NeedFileVoid(file, PathNew);
            }
        }

        public static void CopyDirectoryWithFilesRecursively(Dir severalDir, Dir destinationDir, bool recursive)
        {
            Dir rootDir = severalDir;
            while (rootDir.ParentDir != null) rootDir = rootDir.ParentDir;

            Directory.CreateDirectory(destinationDir.Path
                    + severalDir.Path.Substring(rootDir.Path.Length));

            foreach (Dir somedir in severalDir.ChildDirs)
                Directory.CreateDirectory(destinationDir.Path
                    + somedir.Path.Substring(rootDir.Path.Length));

            foreach (JustFile file in severalDir.Files)
                file.Info.CopyTo(destinationDir.Path
                + severalDir.Path.Substring(rootDir.Path.Length) + "\\" + file.Name);

            if (recursive && severalDir.ChildDirs != null)
                foreach (Dir child in severalDir.ChildDirs)
                    CopyDirectoryWithFilesRecursively(child, destinationDir, true);
        }
        public static void DeleteDirByPathRecursive(Dir dir, Dir dependDir, bool recursive)
        {
            Directory.Delete(dir.Path, recursive);
        }
        public static void PrintProblemDirByPath(Dir dir, Dir dependDir, bool recursive)
        {
            Console.WriteLine(dir.Path);
        }
        public static void SyncDirActionDependKey(List<Dir> problemDirs, List<Dir> AllDirs, ConsoleKey key)
        {
            foreach (var dir in problemDirs)
            {
                VoidDirByPath NeedFileVoid = new VoidDirByPath(PrintProblemDirByPath);
                bool recursive = true;

                if (key == ConsoleKey.A)
                    NeedFileVoid += new VoidDirByPath(CopyDirectoryWithFilesRecursively);
                else if (key == ConsoleKey.R)
                    NeedFileVoid += new VoidDirByPath(DeleteDirByPathRecursive);

                NeedFileVoid(dir, AllDirs.Last(), recursive);
            }
        }
        #endregion

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

            List<Dir> AllDirs = BuildDirsClassFromPathList(listOfDir);

            var problemFiles = Dir.CompareFilesInDirs(AllDirs.First(), AllDirs.Last());
            var problemDirs = Dir.CompareDirs(AllDirs.First(), AllDirs.Last());

            SyncDirActionDependKey(problemDirs, AllDirs, ConsoleKey.Enter);
            SyncFileActionDependKey(problemFiles, AllDirs, ConsoleKey.Enter);
            
            Console.WriteLine("Sync? (press \"a\" to add | \"r\" to remove | other key to nothing)");
            ConsoleKey key = Console.ReadKey().Key;

            SyncDirActionDependKey(problemDirs, AllDirs, key);
            SyncFileActionDependKey(problemFiles, AllDirs, key);

            Console.WriteLine("The end");

            Console.ReadKey();
        }
    }
}
