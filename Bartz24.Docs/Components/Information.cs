using HtmlAgilityPack;
using System.Collections.Generic;

namespace Bartz24.Docs;

public class Information : HTMLPage
{
    private List<string> Info { get; }
    public Information(List<string> information, string name, string templateFileName) : base(name, templateFileName)
    {
        Info = information;
    }

    protected override void GenerateContent(HtmlDocument doc)
    {
        base.GenerateContent(doc);

        HtmlNode contentNode = doc.DocumentNode.SelectSingleNode(".//*[contains(concat(\" \",normalize-space(@class),\" \"),\"tm-content\")]");
        foreach (string str in Info)
        {
            HtmlNode node = HtmlNode.CreateNode("<p></p>");
            node.InnerHtml = HtmlDocument.HtmlEncode(str);
            contentNode.AppendChild(node);
        }
    }
}
