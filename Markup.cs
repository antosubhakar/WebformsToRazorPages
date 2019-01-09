
using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace WebfomsToRazorPages {
    public static class Markup {

        public static void ConvertToRazor(string inputFile, string outputFile) {

            // Known Bug: when writing anything directly to the response (other than for HTML helpers (e.g. Html.RenderPartial or Html.RenderAction)),
            // this Converter will not correctly generate the markup. For example:
            // <% Html.RenderPartial("LogOnUserControl"); %> will properly convert to @{ Html.RenderPartial("LogOnUserControl"); }
            // but
            // <% MyCustom("Foo"); %> will incorrectly convert to @MyCustom("Foo");

            string view;
            string modelName = Path.GetFileNameWithoutExtension(new System.IO.FileInfo(inputFile).Name);
            using (FileStream fs = new FileStream(inputFile, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs)) {
                view = sr.ReadToEnd();
            }

            // Convert Comments
            Regex commentBegin = new Regex("<%--\\s*");
            Regex commentEnd = new Regex("\\s*--%>");
            view = commentBegin.Replace(view, "@*");
            view = commentEnd.Replace(view, "*@");

            // Convert Model
            Regex model = new Regex("(?<=Inherits=\"System.Web.Mvc.ViewPage<|Inherits=\"System.Web.Mvc.ViewUserControl<)(.*?)(?=>\")");
            Regex pageDeclaration = new Regex("(<%@ Page|<%@ Control).*?%>");
            Match modelMatch = model.Match(view);
            if (modelMatch.Success) {
                view = pageDeclaration.Replace(view, "@model " + modelMatch.Value);
            }
            else {
                view = pageDeclaration.Replace(view, String.Empty);
            }

            // TitleContent
            // I'm converting the "TitleContent" ContentPlaceHolder to View.Title because
            // that's what TitleContent was for.  You may want to ommit this.
            Regex titleContent = new Regex("<asp:Content.*?ContentPlaceHolderID=\"TitleContent\"[\\w\\W]*?</asp:Content>");
            Regex title = new Regex("(?<=<%:\\s).*?(?=\\s*%>)");
            var titleContentMatch = titleContent.Match(view);
            if (titleContentMatch.Success) {
                var titleVariable = title.Match(titleContentMatch.Value).Value;
                view = titleContent.Replace(view, "@{" + Environment.NewLine + "    View.Title = " + titleVariable + ";" + Environment.NewLine + "}");
                // find all references to the titleVariable and replace it with View.Title
                Regex titleReferences = new Regex("<%:\\s*" + titleVariable + "\\s*%>");
                view = titleReferences.Replace(view, "@View.Title");
            }

            // MainContent
            // I want the MainContent ContentPlaceholder to be rendered in @RenderBody().
            // If you want another section to be rendered in @RenderBody(), you'll want to modify this
            Regex mainContent = new Regex("<asp:Content.*?ContentPlaceHolderID=\"MainContent\"[\\w\\W]*?</asp:Content>");
            Regex mainContentBegin = new Regex("<asp:Content.*?ContentPlaceHolderID=\"MainContent\".*?\">");
            Regex mainContentEnd = new Regex("</asp:Content>");
            var mainContentMatch = mainContent.Match(view);
            if (mainContentMatch.Success) {
                view = view.Replace(mainContentMatch.Value, mainContentBegin.Replace(mainContentEnd.Replace(mainContentMatch.Value, String.Empty), String.Empty));
            }

            // Match <%= Foo %> (i.e. make sure we're not HTML encoding these)
            Regex replaceWithMvcHtmlString = new Regex("<%=\\s.*?\\s*%>"); // removed * from the first <%=\\s.*?\\s*%> here because I couldn't figure out how to do the equivalent in the positive lookbehind in mvcHtmlStringVariable
            Regex mvcHtmlStringVariable = new Regex("(?<=<%=\\s).*?(?=\\s*%>)");
            // Match <%, <%:
            Regex replaceWithAt = new Regex("<%:*\\s*");
            // Match  %>, <% (but only if there's a proceeding })
            Regex replaceWithEmpty = new Regex("\\s*%>|<%\\s*(?=})");

            var replaceWithMvcHtmlStrings = replaceWithMvcHtmlString.Matches(view);
            foreach (Match mvcString in replaceWithMvcHtmlStrings) {
                view = view.Replace(mvcString.Value, "@MvcHtmlString.Create(" + mvcHtmlStringVariable.Match(mvcString.Value).Value + ")");
            }

            view = replaceWithEmpty.Replace(view, String.Empty);
            view = replaceWithAt.Replace(view, "@");

            Regex contentPlaceholderBegin = new Regex("<asp:Content[\\w\\W]*?>");
            Regex contentPlaceholderId = new Regex("(?<=ContentPlaceHolderID=\").*?(?=\")");
            Regex contentPlaceholderEnd = new Regex("</asp:Content>");
            MatchCollection contentPlaceholders = contentPlaceholderBegin.Matches(view);
            foreach (Match cp in contentPlaceholders) {
                string sectionName = contentPlaceholderId.Match(cp.Value).Value;
                if (sectionName == "pageScripts") { sectionName = "Scripts"; }
                view = view.Replace(cp.Value, "@section " + sectionName + " {");
            }
            view = contentPlaceholderEnd.Replace(view, "}");

            // if we have something like @Html.RenderPartial("LogOnUserControl");, replace it with @{ Html.RenderPartial("LogOnUserControl"); }
            Regex render = new Regex("@Html\\.\\S*\\(.*\\)\\S*?;");
            var renderMatches = render.Matches(view);
            foreach (Match r in renderMatches) {
                view = view.Replace(r.Value, "@{ " + r.Value.Substring(1) + " }");
            }

            view =
            $@"@page
@model " + modelName + @"Model
@{
    ViewData[""TitleHeader""] = ViewData[""Title""];
}
@section Head{

}

" + view;
            view = VariousRazorStringReplacements(view);
            using (FileStream fs = new FileStream(outputFile, FileMode.Create)) {
                byte[] bytes = Encoding.UTF8.GetBytes(view);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static string VariousRazorStringReplacements(string viewHtml) {

            viewHtml = viewHtml.Replace("href=\"@# GetRouteUrl(\"", "asp-page=\"");
            viewHtml = viewHtml.Replace("href=\"@#GetRouteUrl(\"", "asp-page=\"");
            viewHtml = viewHtml.Replace("href=\"@=GetRouteUrl(\"", "asp-page=\"");
            viewHtml = viewHtml.Replace(",null)\"", "");
            viewHtml = viewHtml.Replace("cssclass=", "class=");
            viewHtml = viewHtml.Replace("CssClass=", "class=");
            viewHtml = viewHtml.Replace("runat=\"server\"", "");
            viewHtml = viewHtml.Replace("asp:DropDownList", "select");
            viewHtml = viewHtml.Replace("asp:ListItem", "option");
            viewHtml = viewHtml.Replace("asp:CheckBox", "input type=\"checkbox\"");
            viewHtml = viewHtml.Replace("asp:Literal", "span");
            viewHtml = viewHtml.Replace("asp:TextBox", "input type=\"text\"");
            viewHtml = viewHtml.Replace("asp:Button", "button");
            viewHtml = viewHtml.Replace("asp:Label", "span");
            viewHtml = viewHtml.Replace("asp:Image", "img");
            viewHtml = viewHtml.Replace("asp:FileUpload", "input type=\"file\"");
            viewHtml = viewHtml.Replace("asp:HyperLink", "a");
            viewHtml = viewHtml.Replace("asp:LinkButton", "a");
            viewHtml = viewHtml.Replace("asp:HiddenField", "input type=\"hidden\"");


            return viewHtml;
        }
    }
}