using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bartz24.Docs
{
    public class HTMLPage
    {
        public string Name { get; }
        public string TemplateFileName { get; }

        public List<HTMLElement> HTMLElements { get; set; } = new List<HTMLElement>();

        public HTMLPage(string name, string templateFileName)
        {
            Name = name;
            TemplateFileName = templateFileName;
        }

        protected HtmlDocument GetMainDocument(string mainFolder)
        {
            HtmlDocument doc = new HtmlDocument();
            if (!String.IsNullOrEmpty(TemplateFileName))
            {
                doc.LoadHtml(File.ReadAllText(mainFolder + "/" + TemplateFileName));
            }

            return doc;
        }

        public void Generate(string fileName, string mainFolder, Docs docs)
        {
            HtmlDocument doc = GetMainDocument(mainFolder);

            new Header(docs.Pages.ToDictionary(p => p.Key, p => p.Value.Name), docs.Settings.Name, "common/header.html", mainFolder).Generate().ForEach(n => doc.GetBody().AppendChild(n));
            GenerateContent(doc);
            doc.Save(fileName);
        }

        protected virtual void GenerateContent(HtmlDocument doc)
        {
            HtmlNode title = doc.GetHead().SelectSingleNode("//title");
            title.InnerHtml = HtmlDocument.HtmlEncode(Name);

            HtmlNode contentNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'container')]");
            HTMLElements.ForEach(e => e.Generate().ForEach(n => contentNode.AppendChild(n)));
        }
    }
}
