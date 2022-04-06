using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.Docs
{
    public class Button : HTMLElement
    {
        private string OnClick { get; }
        private string Name { get; }
        public Button(string onclick, string id=null, string name="") : base("button", id)
        {
            OnClick = onclick;
            Name = name;
        }

        public override List<HtmlNode> Generate()
        {
            string idInfo = ID == null ? "" : $" id=\"{ID}\"";
            HtmlNode node = HtmlNode.CreateNode($"<{TagType}{idInfo} onclick='{OnClick}'>{Name}</{TagType}");
            GenerateContent(node);
            return new List<HtmlNode>() { node };
        }
    }
}
