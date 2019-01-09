
using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace WebformsToRazorPages {

    public static class CodeBehind {

        const string RazorPageBaseClass = "PageModel"; //if you have a different name to your base page model class, change this value 

        public static void ConvertToCSharpCodeBehind(string inputFile, string outputFile) {

            string codeBehindFileContents = "";
            string modelName = Path.GetFileNameWithoutExtension(new System.IO.FileInfo(inputFile).Name).Replace(".aspx", "");

            using (FileStream fs = new FileStream(inputFile, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs)) {
                codeBehindFileContents = sr.ReadToEnd();
            }

            //literal string replacements
            codeBehindFileContents = VariousStringReplacements(codeBehindFileContents);


            List<string> fileLines = codeBehindFileContents.Split(Environment.NewLine).ToList();

            //rename the class, and add in the configuration into the constructor
            fileLines[fileLines.FindIndex(ind => ind.Contains("public partial class"))] =
            $@"public class {modelName}Model : {RazorPageBaseClass} {{
        public {modelName}Model(IOptions<Config> optionsConfig) : base(optionsConfig) {{}}";

            //replace the page_load method with Razor Pages' OnGetAsync. This is a very rudimentary replacement, you really
            //need to go make OnPostAsync to handle the webforms postback!
            fileLines[fileLines.FindIndex(ind => ind.Contains("Page_Load(object sender"))] =
  $@"public async Task<IActionResult> OnGetAsync() {{
            return Page();";

            //add/remove the page's using declarations
            EnsureCorrectRazorPagesUsingDeclarations(fileLines);

            //write the new file to disk
            WriteNewRazorCodeBehindFile(outputFile, fileLines);
        }

        private static void WriteNewRazorCodeBehindFile(string outputFile, IEnumerable<string> fileLines) {
            using (FileStream fs = new FileStream(outputFile, FileMode.Create)) {
                byte[] bytes = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, fileLines));
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static void EnsureCorrectRazorPagesUsingDeclarations(List<string> fileLines) {
            //we never need these using declarations in Razor pages
            fileLines.Remove("using System.Web;");
            fileLines.Remove("using System.Web.UI;");
            fileLines.Remove("using System.Web.UI.WebControls;");

            //we always want these declaration at the top of the new Razor page .cs file
            fileLines.Insert(0, "using VipClub.AdminApp2;"); //your own custom namespace
            fileLines.Insert(0, "using System.Threading.Tasks;");
            fileLines.Insert(0, "using Microsoft.AspNetCore.Mvc;");
            fileLines.Insert(0, "using Microsoft.AspNetCore.Mvc.RazorPages;");
            fileLines.Insert(0, "using Microsoft.Extensions.Options;");
        }

        /// <summary>
        /// Replace various webforms elements with their html equivalent
        /// </summary>
        /// <returns>The string replacements.</returns>
        /// <param name="codeBehindText">Code behind text.</param>
        public static string VariousStringReplacements(string codeBehindText) {
            codeBehindText = codeBehindText.Replace("asp:Label", "span");
            codeBehindText = codeBehindText.Replace("asp:Image", "img");
            return codeBehindText;
        }



    }
}