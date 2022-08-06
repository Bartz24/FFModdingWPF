using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.Docs
{
    public class Table : HTMLElement
    {
        private string HeaderName { get; }
        private List<string> ColumnNames { get; }
        private List<int> ColumnWidths { get; }
        private List<List<string>> TableContents { get; }
        private bool EncodeStrings { get; }
        public Table(string header, List<string> columns, List<int> colWidths, List<List<string>> contents, string id = null, bool encode = true) : base("table", id == null ? header.Replace(" ", "").ToLower() : id)
        {
            HeaderName = header;
            ColumnNames = columns;
            ColumnWidths = colWidths;
            TableContents = contents;
            EncodeStrings = encode;
        }

        public override List<HtmlNode> Generate()
        {
            List<HtmlNode> list = new List<HtmlNode>();
            if (!String.IsNullOrEmpty(HeaderName))
            {
                HtmlNode header = HtmlNode.CreateNode("<h1></h1>");
                header.InnerHtml = HtmlDocument.HtmlEncode(HeaderName);
                list.Add(header);
            }

            list.AddRange(base.Generate());
            return list;
        }

        protected override void GenerateContent(HtmlNode node)
        {
            "table table-sm table-dark table-hover align-middle".Split(" ").ToList().ForEach(c => node.AddClass(c));
            node.SetAttributeValue("data-toggle", "table");
            node.SetAttributeValue("data-search", "true");
            node.SetAttributeValue("data-search-highlight", "true");

            HtmlNode colgroup = HtmlNode.CreateNode("<colgroup></colgroup>");
            foreach (int width in ColumnWidths)
            {
                HtmlNode colNode = HtmlNode.CreateNode($"<col style=\"width: {width}%\">");
                colgroup.AppendChild(colNode);
            }
            node.AppendChild(colgroup);

            HtmlNode header = HtmlNode.CreateNode("<thead></thead>");

            HtmlNode columns = HtmlNode.CreateNode("<tr></tr>");
            foreach (string name in ColumnNames)
            {
                HtmlNode thNode = HtmlNode.CreateNode($"<th data-sortable=\"true\">{HtmlDocument.HtmlEncode(name)}</th>");
                columns.AppendChild(thNode);
            }
            header.AppendChild(columns);
            node.AppendChild(header);


            HtmlNode body = HtmlNode.CreateNode("<tbody></tbody>");

            foreach (List<string> row in TableContents)
            {
                HtmlNode rowNode = HtmlNode.CreateNode("<tr></tr>");
                foreach (string text in row)
                {
                    HtmlNode thNode = HtmlNode.CreateNode($"<td style=\"white-space: pre-line\">{(EncodeStrings ? HtmlDocument.HtmlEncode(text) : text)}</td>");
                    rowNode.AppendChild(thNode);
                }
                body.AppendChild(rowNode);
            }
            node.AppendChild(body);
        }
    }
}
