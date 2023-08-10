using System;
using System.IO;

namespace CSharpDemo.Utils
{
    public class DirectoryManager
    {
        private static readonly string BaseDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private static string CreateDir(string s)
        {
            if (!Directory.Exists(s))
            {
                Directory.CreateDirectory(s);
            }

            return s;
        }

        public static string GetAudioDir()
        {
            return CreateDir(BaseDir + "\\CSharpDemo目录\\Audio");
        }
    }
}