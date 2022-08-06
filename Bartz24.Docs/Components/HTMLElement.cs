using HtmlAgilityPack;
using System.Collections.Generic;

namespace Bartz24.Docs
{
    public class HTMLElement
    {
        public string TagType { get; }
        public string ID { get; }

        public HTMLElement(string tagType, string id)
        {
            TagType = tagType;
            ID = id;
        }

        public virtual List<HtmlNode> Generate()
        {
            string idInfo = ID == null ? "" : $" id=\"{ID}\"";
            HtmlNode node = HtmlNode.CreateNode($"<{TagType}{idInfo}></{TagType}");
            GenerateContent(node);
            return new List<HtmlNode>() { node };
        }

        protected virtual void GenerateContent(HtmlNode node)
        {
        }
    }
}
