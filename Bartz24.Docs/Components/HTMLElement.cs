using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Bartz24.Docs
{
    public class HTMLElement
    {
        public string TagType { get; }

        public HTMLElement(string tagType)
        {
            TagType = tagType;
        }

        public virtual List<HtmlNode> Generate()
        {
            HtmlNode node = HtmlNode.CreateNode($"<{TagType}></{TagType}");
            GenerateContent(node);
            return new List<HtmlNode>() { node };
        }

        protected virtual void GenerateContent(HtmlNode node)
        {
        }
    }
}
