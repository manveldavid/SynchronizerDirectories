using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CompareDir
{
    public delegate void VoidFileByPath(JustFile file, string path);
    public delegate void VoidDirByPath(Dir dir, Dir dependDir, bool recursive);

    internal class ActionsOnDIrAndFiles
    {
        public static void MainCompareAction(Dir masterDir, Dir compareDir)
        {
            var problemFiles = Dir.CompareFilesInDirs(masterDir, compareDir);
            var problemDirs = Dir.CompareDirs(masterDir, compareDir);

            
            if (problemDirs.Count > 0 || problemFiles.Count > 0)
            {
                ActionsOnDIrAndFiles.SyncDirActionDependKey(problemDirs, masterDir, compareDir, ConsoleKey.Enter);
                ActionsOnDIrAndFiles.SyncFileActionDependKey(problemFiles, masterDir, compareDir, ConsoleKey.Enter);
                Console.WriteLine("Sync? (press \"a\" to send | \"r\" to remove | {_other key_} to nothing)");
                ConsoleKey key = Console.ReadKey().Key; Console.WriteLine("\n");
                if (key == ConsoleKey.A || key == ConsoleKey.R)
                {
                    ActionsOnDIrAndFiles.SyncDirActionDependKey(problemDirs, masterDir, compareDir, key);
                    ActionsOnDIrAndFiles.SyncFileActionDependKey(problemFiles, masterDir, compareDir, key);
                }
            }
        }
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
        public static void SyncFileActionDependKey(List<JustFile> problemFiles, Dir masterDir, Dir compareDir, ConsoleKey key)
        {

            foreach (var file in problemFiles)
            {
                string PathNew = file.Path;
                VoidFileByPath NeedFileVoid = new VoidFileByPath(PrintProblemFileByPath);

                if (key == ConsoleKey.A)
                {
                    PathNew = compareDir.Path + "\\" +
                        file.Path.Substring(masterDir.Path.Length);
                    NeedFileVoid += new VoidFileByPath(ReplaceOrCopyFileByPath);
                }
                else if (key == ConsoleKey.R)
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
        public static void SyncDirActionDependKey(List<Dir> problemDirs, Dir masterDir, Dir compareDir, ConsoleKey key)
        {
            foreach (var dir in problemDirs)
            {
                VoidDirByPath NeedFileVoid = new VoidDirByPath(PrintProblemDirByPath);
                bool recursive = true;

                if (key == ConsoleKey.A)
                    NeedFileVoid += new VoidDirByPath(CopyDirectoryWithFilesRecursively);
                else if (key == ConsoleKey.R)
                    NeedFileVoid += new VoidDirByPath(DeleteDirByPathRecursive);

                NeedFileVoid(dir, compareDir, recursive);
            }
        }
        #endregion
    }
}
