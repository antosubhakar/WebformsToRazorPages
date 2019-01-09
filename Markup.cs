using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace WebformsToRazorPages {

    public static class Markup {

        public static void ConvertToRazor(string inputFile, string outputFileName) {

            string viewHtml = ReadAspxFileContents(inputFile);

            string modelName = Path.GetFileNameWithoutExtension(new FileInfo(inputFile).Name);

            //replace comment syntax
            viewHtml = ConvertComments(viewHtml);

            //build the page model
            viewHtml = ConvertModel(viewHtml);

            // TitleContent
            viewHtml = ConvertTitle(viewHtml);

            // MainContent
            viewHtml = ConvertMainContent(viewHtml);

            // Match <%= Foo %> (i.e. make sure we're not HTML encoding these)
            viewHtml = ReplaceLiteralOutputs(viewHtml);

            //any master page content placeholders
            viewHtml = ReplaceContentPlaceholders(viewHtml);

            // if we have something like @Html.RenderPartial("LogOnUserControl");, replace it with @{ Html.RenderPartial("LogOnUserControl"); }
            viewHtml = ReplacePartials(viewHtml);

            //ensure the file always has the correct Razor declarations and code blocks (including Head and Script code blocks)
            string razorPageTop = $@"@page
@model " + modelName + @"Model
@{
    ViewData[""TitleHeader""] = ViewData[""Title""];
}
@section Head{

}

@section Scripts{

}
";
            viewHtml = razorPageTop + viewHtml;

            viewHtml = ReplaceVariousWebformsElementsWithHtmlEquivalents(viewHtml);

            using (FileStream fs = new FileStream(outputFileName, FileMode.Create)) {
                byte[] bytes = Encoding.UTF8.GetBytes(viewHtml);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        private static string ReplaceLiteralOutputs(string viewHtml) {

            Regex replaceWithMvcHtmlString = new Regex("<%=\\s.*?\\s*%>"); // removed * from the first <%=\\s.*?\\s*%> here
            Regex mvcHtmlStringVariable = new Regex("(?<=<%=\\s).*?(?=\\s*%>)");
            // Match <%, <%:
            Regex replaceWithAt = new Regex("<%:*\\s*");
            // Match  %>, <% (but only if there's a preceeding })
            Regex replaceWithEmpty = new Regex("\\s*%>|<%\\s*(?=})");

            var replaceWithMvcHtmlStrings = replaceWithMvcHtmlString.Matches(viewHtml);
            foreach (Match mvcString in replaceWithMvcHtmlStrings) {
                viewHtml = viewHtml.Replace(mvcString.Value, "@MvcHtmlString.Create(" + mvcHtmlStringVariable.Match(mvcString.Value).Value + ")");
            }
            viewHtml = replaceWithEmpty.Replace(viewHtml, "");
            viewHtml = replaceWithAt.Replace(viewHtml, "@");
            return viewHtml;
        }

        private static string ReplacePartials(string viewHtml) {
            Regex render = new Regex("@Html\\.\\S*\\(.*\\)\\S*?;");
            var renderMatches = render.Matches(viewHtml);
            foreach (Match r in renderMatches) {
                viewHtml = viewHtml.Replace(r.Value, "@{ " + r.Value.Substring(1) + " }");
            }
            return viewHtml;
        }

        private static string ConvertTitle(string viewHtml) {
            Regex titleContent = new Regex("<asp:Content.*?ContentPlaceHolderID=\"TitleContent\"[\\w\\W]*?</asp:Content>");
            Regex title = new Regex("(?<=<%:\\s).*?(?=\\s*%>)");
            var titleContentMatch = titleContent.Match(viewHtml);
            if (titleContentMatch.Success) {
                var pageTitle = title.Match(titleContentMatch.Value).Value;
                viewHtml = titleContent.Replace(viewHtml, "@{" + Environment.NewLine + "    View.Title = " + pageTitle + ";" + Environment.NewLine + "}");
                // find all references to the title and replace it with View.Title
                Regex titleReferences = new Regex("<%:\\s*" + pageTitle + "\\s*%>");
                viewHtml = titleReferences.Replace(viewHtml, "@View.Title");
            }
            return viewHtml;
        }

        private static string ConvertMainContent(string viewHtml) {
            Regex mainContent = new Regex("<asp:Content.*?ContentPlaceHolderID=\"MainContent\"[\\w\\W]*?</asp:Content>");
            Regex mainContentBegin = new Regex("<asp:Content.*?ContentPlaceHolderID=\"MainContent\".*?\">");
            Regex mainContentEnd = new Regex("</asp:Content>");
            var mainContentMatch = mainContent.Match(viewHtml);
            if (mainContentMatch.Success) {
                viewHtml = viewHtml.Replace(mainContentMatch.Value, mainContentBegin.Replace(mainContentEnd.Replace(mainContentMatch.Value, ""), ""));
            }
            return viewHtml;
        }

        private static string ConvertModel(string viewHtml) {
            Regex model = new Regex("(?<=Inherits=\"System.Web.Mvc.ViewPage<|Inherits=\"System.Web.Mvc.ViewUserControl<)(.*?)(?=>\")");
            Regex pageDeclaration = new Regex("(<%@ Page|<%@ Control).*?%>");
            Match modelMatch = model.Match(viewHtml);
            if (modelMatch.Success) {
                viewHtml = pageDeclaration.Replace(viewHtml, "@model " + modelMatch.Value);
            }
            else {
                viewHtml = pageDeclaration.Replace(viewHtml, "");
            }
            return viewHtml;
        }

        private static string ConvertComments(string viewHtml) {
            // Convert Comments
            Regex commentBegin = new Regex("<%--\\s*");
            Regex commentEnd = new Regex("\\s*--%>");
            viewHtml = commentBegin.Replace(viewHtml, "@*");
            viewHtml = commentEnd.Replace(viewHtml, "*@");
            return viewHtml;
        }

        private static string ReplaceContentPlaceholders(string viewHtml) {
            Regex contentPlaceholderBegin = new Regex("<asp:Content[\\w\\W]*?>");
            Regex contentPlaceholderId = new Regex("(?<=ContentPlaceHolderID=\").*?(?=\")");
            Regex contentPlaceholderEnd = new Regex("</asp:Content>");
            MatchCollection contentPlaceholders = contentPlaceholderBegin.Matches(viewHtml);
            foreach (Match cp in contentPlaceholders) {
                string sectionName = contentPlaceholderId.Match(cp.Value).Value;
                if (sectionName == "pageScripts") { sectionName = "Scripts"; }
                viewHtml = viewHtml.Replace(cp.Value, "@section " + sectionName + " {");
            }
            viewHtml = contentPlaceholderEnd.Replace(viewHtml, "}");
            return viewHtml;
        }

        private static string ReadAspxFileContents(string inputFile) {
            string fileContents = "";
            using (FileStream fs = new FileStream(inputFile, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs)) {
                fileContents = sr.ReadToEnd();
            }
            return fileContents;
        }

        /// <summary>
        /// Replace webforms page elements with HTML equivalents
        /// </summary>
        /// <returns>The razor string replacements.</returns>
        public static string ReplaceVariousWebformsElementsWithHtmlEquivalents(string pageHtml) {

            Dictionary<string, string> replacements = new Dictionary<string, string> {
                {"href=\"@# GetRouteUrl(\"", "asp-page=\"" },
                {"href=\"@#GetRouteUrl(\"", "asp-page=\""},
                {"href=\"@=GetRouteUrl(\"", "asp-page=\""},
                {",null)\"", ""},
                {"cssclass=", "class="},
                {"CssClass=", "class="},
                {"runat=\"server\"", ""},
                {"asp:DropDownList", "select"},
                {"asp:ListItem", "option"},
                {"asp:CheckBox", "input type=\"checkbox\""},
                {"asp:Literal", "span"},
                {"asp:TextBox", "input type=\"text\""},
                {"asp:Button", "button"},
                {"asp:Label", "span"},
                {"asp:Image", "img"},
                {"asp:FileUpload", "input type=\"file\""},
                {"asp:HyperLink", "a"},
                {"asp:LinkButton", "a"},
                {"asp:HiddenField", "input type=\"hidden\""}
            };

            foreach (var item in replacements) {
                pageHtml = pageHtml.Replace(item.Key, item.Value);
            }

            return pageHtml;
        }
    }
}