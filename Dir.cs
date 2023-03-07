using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                if (childPath != "") ChildDirs.Add(new Dir(childPath, this));
            foreach (var filePath in filesSourse)
                if (filePath != "") Files.Add(new JustFile(filePath, this));
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

        public static List<JustFile> CompareFilesInDirs(Dir masterDir, Dir compareDir)
        {
            List<JustFile> problemFiles = new List<JustFile>();

            bool dirsExist = masterDir != null && compareDir != null;
            if (dirsExist)
            {
                foreach (var fileInMainDir in masterDir.Files)
                {
                    if (!compareDir.Files.Select(o => o.Name).Contains(fileInMainDir.Name))
                        problemFiles.Add(fileInMainDir);
                    else if (compareDir.Files
                      .FirstOrDefault(o => o.Name == fileInMainDir.Name)
                      .Info.Length != fileInMainDir.Info.Length)
                        problemFiles.Add(fileInMainDir);
                }

                if (compareDir.ChildDirs != null)
                    foreach (Dir mainDirChild in masterDir.ChildDirs)
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
}
