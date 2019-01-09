using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebformsToRazorPages {
    public class Converter {

        internal List<string> ConvertedFiles = new List<string>();
        internal Regex WebFormsExtension = new Regex(".aspx$|.aspx.cs$");
        private readonly static string RazorExtension = ".cshtml";

        public Converter() {
        }

        /// <summary>
        /// Converts all webforms aspx pages and their corresponding cs pages in a directory. Subdirectories are NOT processed.
        /// </summary>
        /// <param name="path">Path.</param>
        public void ConvertProjectInDirectory(string path) {
            foreach (var file in Directory.GetFiles(path, "*.aspx")) {
                var outputFile = WebFormsExtension.Replace(file, RazorExtension);
                Markup.ConvertToRazor(file, outputFile);
                ConvertedFiles.Add(file);
            }

            foreach (var file in Directory.GetFiles(path, "*.aspx.cs")) {
                var outputFile = WebFormsExtension.Replace(file, RazorExtension + ".cs");
                CodeBehind.ConvertToCSharpCodeBehind(file, outputFile);
                ConvertedFiles.Add(file);
            }
        }

        /// <summary>
        /// Process a single webforms aspx page and its .cs pair
        /// </summary>
        /// <param name="path">the full file path to the aspx page</param>
        public void ConvertFile(string path) {
            var match = WebFormsExtension.Match(path);
            if (match.Success) {

                Markup.ConvertToRazor(path, WebFormsExtension.Replace(path, RazorExtension));
                ConvertedFiles.Add(path);

                CodeBehind.ConvertToCSharpCodeBehind(path, WebFormsExtension.Replace(path, RazorExtension + ".cs"));
                ConvertedFiles.Add(path);

            }
            else {
                throw new ArgumentException($"{path} file isn't a WebForms view page");
            }
        }

        public IEnumerable<string> ListConvertedFiles() {
            yield return $"The following {ConvertedFiles.Count} files were converted:";
            foreach (var file in ConvertedFiles.OrderBy(x=>x)) {
                yield return file;
            }
        }

    }
}
