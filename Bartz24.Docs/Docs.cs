using System.Collections.Generic;
using System.IO;

namespace Bartz24.Docs
{
    public class Docs
    {
        public DocsSettings Settings { get; set; } = new DocsSettings();
        public Dictionary<string, HTMLPage> Pages { get; } = new Dictionary<string, HTMLPage>();

        public void AddPage(string path, HTMLPage page)
        {
            Pages.Add(path, page);
        }

        public void Generate(string mainFolder, string templateFolder)
        {
            if (Directory.Exists(mainFolder))
                Directory.Delete(mainFolder, true);

            CopyFromTemplate(mainFolder, templateFolder);

            foreach (string path in Pages.Keys)
            {
                Pages[path].Generate(mainFolder + "/" + path + ".html", mainFolder, this);
            }

            DeleteTemplateFiles(mainFolder);
        }

        private void CopyFromTemplate(string mainFolder, string templateFolder)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(templateFolder, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(templateFolder, mainFolder));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(templateFolder, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(templateFolder, mainFolder), true);
        }
        private void ClearAttributes(string currentDir)
        {
            if (Directory.Exists(currentDir))
            {
                File.SetAttributes(currentDir, FileAttributes.Normal);

                string[] subDirs = Directory.GetDirectories(currentDir);
                foreach (string dir in subDirs)
                {
                    ClearAttributes(dir);
                }

                string[] files = files = Directory.GetFiles(currentDir);
                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                }
            }
        }

        private void DeleteTemplateFiles(string mainFolder)
        {
            ClearAttributes(mainFolder + "/template");
            Directory.Delete(mainFolder + "/template", true);
        }
    }
}
