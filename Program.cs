using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace WebfomsToRazorPages {
    class Program
    {
        private readonly static string razorExtension = ".cshtml";

        /// <summary>Usage: RazorConverter.exe  "C:\Files" or RazorConverter.exe  "C:\Files\MyFile.aspx"</summary>
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                throw new ArgumentException("File or folder path is missing.");
            }
            string path = args[0];

            List<string> convertedFiles = new List<string>();
            Regex webFormsExtension = new Regex(".aspx$|.aspx.cs$");
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path, "*.aspx"))
                {
                    var outputFile = webFormsExtension.Replace(file, razorExtension);
                    Markup.ConvertToRazor(file, outputFile);
                    convertedFiles.Add(file);
                }

                foreach (var file in Directory.GetFiles(path, "*.aspx.cs"))
                {
                    var outputFile = webFormsExtension.Replace(file, razorExtension + ".cs");
                    CodeBehind.ConvertToCSharpCodeBehind(file, outputFile);
                    convertedFiles.Add(file);
                }
            }
            else if (File.Exists(path))
            {
                var match = webFormsExtension.Match(path);
                if (match.Success)
                {
                    Markup.ConvertToRazor(path, webFormsExtension.Replace(path, razorExtension));
                    convertedFiles.Add(path);

                    CodeBehind.ConvertToCSharpCodeBehind(path, webFormsExtension.Replace(path, razorExtension + ".cs"));
                    convertedFiles.Add(path);

                }
                else
                {
                    throw new ArgumentException(String.Format("{0} file isn't a WebForms view", path));
                }
            }
            else
            {
                throw new ArgumentException(String.Format("{0} doesn't exist", path));
            }

            Console.WriteLine(String.Format("The following {0} files were converted:", convertedFiles.Count));
            foreach (var file in convertedFiles)
            {
                Console.WriteLine(file);
            }
        }

    }
}