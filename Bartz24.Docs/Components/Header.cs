using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.Docs
{
    public class Header : HTMLElement
    {
        private Dictionary<string, string> Links { get; }
        private string DocsName { get; }
        private string TemplateFileName { get; }
        private string MainFolder { get; }
        public Header(Dictionary<string, string> links, string docsName, string templateFileName, string mainFolder) : base("Header", null)
        {
            Links = links;
            DocsName = docsName;
            TemplateFileName = templateFileName;
            MainFolder = mainFolder;
        }

        public override List<HtmlNode> Generate()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(File.ReadAllText(MainFolder + "/" + TemplateFileName));
            HtmlNode node = doc.DocumentNode.FirstChild;
            GenerateContent(node);
            return new List<HtmlNode>() { node };
        }

        protected override void GenerateContent(HtmlNode node)
        {
            HtmlNode navUlNode = node.OwnerDocument.DocumentNode.SelectSingleNode(".//*[contains(concat(\" \",normalize-space(@class),\" \"),\"navbar-nav\")]");
            foreach (string key in Links.Keys)
            {
                HtmlNode nav = HtmlNode.CreateNode($"<a class=\"nav-link\" href = \"{key + ".html"}\">{Links[key]}</a>");
                navUlNode.AppendChild(nav);
            }
            HtmlNode nameNode = node.OwnerDocument.DocumentNode.SelectSingleNode(".//*[contains(concat(\" \",normalize-space(@class),\" \"),\"navbar-brand\")]");
            nameNode.InnerHtml = HtmlDocument.HtmlEncode(DocsName);

        }
    }
}
