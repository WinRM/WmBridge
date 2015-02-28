//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Linq;
using System.Web;

namespace HtmlAgilityPack
{
    public static class AgilityExtensions
    {
        private static HtmlNode CreateElement(HtmlNode node, string elementName, params Action<HtmlNode>[] innerNodes)
        {
            var child = node.OwnerDocument.CreateElement(elementName);

            foreach (var action in innerNodes)
            {
                if (action != null)
                    action(child);
            }

            node.AppendChild(child);

            return child;
        }

        public static string HtmlEncode(this string text)
        {
            return HttpUtility.HtmlEncode(text);
        }

        public static HtmlNode AppendHtmlElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "html", innerNodes);
        }

        public static HtmlNode AppendBodyElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "body", innerNodes);
        }

        public static HtmlNode AppendHeadElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "head", innerNodes);
        }

        public static HtmlNode AppendTableElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "table", innerNodes);
        }

        public static HtmlNode AppendTrElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "tr", innerNodes);
        }

        public static HtmlNode AppendTdElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "td", innerNodes);
        }

        public static HtmlNode AppendTdElement(this HtmlNode node, string className, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "td", td => td.SetClassName(className), innerNodes);
        }

        public static HtmlNode AppendParagraphElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "p", innerNodes);
        }

        public static HtmlNode AppendParagraphElement(this HtmlNode node, string className, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "p", p => p.SetClassName(className), innerNodes);
        }

        public static HtmlNode AppendDivElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "div", innerNodes);
        }

        public static HtmlNode AppendDivElement(this HtmlNode node, string id, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "div", div => div.SetAttributeValue("id", id), innerNodes);
        }

        public static HtmlNode AppendDivOfClassElement(this HtmlNode node, string className, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "div", div => div.SetClassName(className), innerNodes);
        }

        public static HtmlNode AppendPreElement(this HtmlNode node, string text, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "pre", h => h.InnerHtml = text.HtmlEncode(), innerNodes);
        }

        public static HtmlNode AppendHeadingElement(this HtmlNode node, int level, string text, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "h" + level, h => h.InnerHtml = text.HtmlEncode(), innerNodes);
        }

        public static HtmlNode AppendHeadingElement(this HtmlNode node, int level, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "h" + level, innerNodes);
        }

        public static HtmlNode AppendTitleElement(this HtmlNode node, string text, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "title", t => t.InnerHtml = text.HtmlEncode(), innerNodes);
        }

        public static HtmlNode AppendSpanElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "span", innerNodes);
        }

        public static HtmlNode AppendSpanElement(this HtmlNode node, string text, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "span", s => { s.InnerHtml = text.HtmlEncode();  }, innerNodes);
        }

        public static HtmlNode AppendSpanElement(this HtmlNode node, string className, string text, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "span", s => s.SetClassName(className).InnerHtml = text.HtmlEncode(), innerNodes);
        }

        public static HtmlNode AppendTextElement(this HtmlNode node, string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            else
                return node.AppendChild(node.OwnerDocument.CreateTextNode(text.HtmlEncode()));
        }

        public static HtmlNode AppendNonBreakingSpace(this HtmlNode node)
        {
            return node.AppendChild(node.OwnerDocument.CreateTextNode("&nbsp;"));
        }

        public static HtmlNode AppendNonBreakingHyphen(this HtmlNode node)
        {
            return node.AppendChild(node.OwnerDocument.CreateTextNode("&#8209;"));
        }

        public static HtmlNode SetClassName(this HtmlNode node, string className)
        {
            node.SetAttributeValue("class", className);
            return node;
        }

        public static HtmlNode AppendUlElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "ul", innerNodes);
        }

        public static HtmlNode AppendLiElement(this HtmlNode node, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "li", innerNodes);
        }

        public static HtmlNode AppendHyperlinkElement(this HtmlNode node, string url, Action<HtmlNode> innerNodes = null)
        {
            return CreateElement(node, "a", a => a.SetAttributeValue("href", url), innerNodes);
        }

    }
}
