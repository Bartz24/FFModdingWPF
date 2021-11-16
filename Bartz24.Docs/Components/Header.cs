using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.Docs
{
    public class Header : HTMLPage
    {
        private Dictionary<string, string> Links { get; }
        private string DocsName { get; }
        public Header(Dictionary<string, string> links, string docsName, string templateFileName) : base("Header", templateFileName)
        {
            Links = links;
            DocsName = docsName;
        }

        protected override void GenerateContent(HtmlDocument doc)
        {
            HtmlNode navUlNode = doc.DocumentNode.SelectSingleNode(".//*[contains(concat(\" \",normalize-space(@class),\" \"),\"tm-nav\")]/ul");
            foreach (string key in Links.Keys)
            {
                HtmlNode node = HtmlNode.CreateNode($"<li><a href = \"../{key + ".html"}\", target = \"_top\">{Links[key]}</a></li>");
                navUlNode.AppendChild(node);
            }
            HtmlNode nameNode = doc.DocumentNode.SelectSingleNode(".//*[contains(concat(\" \",normalize-space(@class),\" \"),\"tm-name\")]");
            nameNode.InnerHtml = HtmlDocument.HtmlEncode(DocsName);

        }
    }
}
