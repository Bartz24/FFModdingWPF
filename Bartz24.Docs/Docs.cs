using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.Docs
{
    public class Docs
    {
        public DocsSettings Settings { get; set; } = new DocsSettings();
        private Dictionary<string, HTMLPage> Pages { get; } = new Dictionary<string, HTMLPage>();

        public void AddPage(string path, HTMLPage page)
        {            
            Pages.Add(path, page);
        }

        public void Generate(string mainFolder, string templateFolder)
        {
            if (Directory.Exists(mainFolder))
                Directory.Delete(mainFolder, true);

            CopyFromTemplate(mainFolder, templateFolder);            

            Pages.Add("common/header", new Header(Pages.ToDictionary(p => p.Key, p => p.Value.Name), Settings.Name, "common/header.html"));

            foreach (string path in Pages.Keys)
            {
                Pages[path].Generate(mainFolder + "/" + path + ".html", mainFolder);
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
