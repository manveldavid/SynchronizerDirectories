using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareDir
{
    public class JustFile
    {
        public JustFile(string path)
        {
            Info = new FileInfo(path);
            Path = path;
            Name = Path.Substring(Path.LastIndexOf('\\') + 1);
            Ending = Name.LastIndexOf('.') != -1 ? Name.Substring(Name.LastIndexOf('.') + 1) : null;
        }
        public JustFile(string name, Dir Sourse) : this(name) { SourseDir = Sourse; }

        public Dir SourseDir { get; private set; }
        public string Name { get; private set; }
        public string Ending { get; private set; }
        public FileInfo Info { get; private set; }
        public string Path { get; private set; }
        public new string ToString() => Name;
    }
}
