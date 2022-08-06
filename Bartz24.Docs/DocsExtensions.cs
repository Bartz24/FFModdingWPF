using HtmlAgilityPack;

namespace Bartz24.Docs
{
    public static class DocsExtensions
    {
        public static HtmlNode GetBody(this HtmlDocument doc)
        {
            return doc.DocumentNode.SelectSingleNode("//body");
        }
        public static HtmlNode GetHead(this HtmlDocument doc)
        {
            return doc.DocumentNode.SelectSingleNode("//head");
        }
    }
}
