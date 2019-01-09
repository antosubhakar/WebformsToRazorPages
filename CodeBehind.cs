
using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace WebfomsToRazorPages {
    public static class CodeBehind {
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
            //rename the class
            fileLines[fileLines.FindIndex(ind => ind.Contains("public partial class"))] =
            $@"public class {modelName}Model : VipPageModel {{
        public {modelName}Model(IOptions<Config> optionsConfig) : base(optionsConfig) {{}}";

            fileLines[fileLines.FindIndex(ind => ind.Contains("Page_Load(object sender"))] =
  $@"public async Task<IActionResult> OnGetAsync() {{
            return Page();";

            fileLines.Remove("using System.Web;");
            fileLines.Remove("using System.Web.UI;");
            fileLines.Remove("using System.Web.UI.WebControls;");

            fileLines.Insert(0, "using VipClub.AdminApp2;");
            fileLines.Insert(0, "using System.Threading.Tasks;");
            fileLines.Insert(0, "using Microsoft.AspNetCore.Mvc;");
            fileLines.Insert(0, "using Microsoft.AspNetCore.Mvc.RazorPages;");
            fileLines.Insert(0, "using Microsoft.Extensions.Options;");



            using (FileStream fs = new FileStream(outputFile, FileMode.Create)) {
                byte[] bytes = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, fileLines));
                fs.Write(bytes, 0, bytes.Length);
            }

        }
        public static string VariousStringReplacements(string codeBehindText) {
            codeBehindText = codeBehindText.Replace("asp:Label", "span");
            codeBehindText = codeBehindText.Replace("asp:Image", "img");
            return codeBehindText;
        }
    }
}