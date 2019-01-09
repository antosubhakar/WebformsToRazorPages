using System;
using System.IO;

namespace WebformsToRazorPages {
    class Program {

        /// <summary>Usage: WebformsToRazorPages.exe  "C:\Files" or 
        ///                 WebformsToRazorPages.exe  "C:\Files\MyFile.aspx"
        /// </summary>
        static void Main(string[] args) {
            if (args.Length < 1) {
                throw new ArgumentException("File or folder path is missing.");
            }

            string path = args[0];

            var converter = new Converter();

            if (Directory.Exists(path)) {
                converter.ConvertProjectInDirectory(path);
            }
            else if (File.Exists(path)) {
                converter.ConvertFile(path);
            }
            else {
                throw new ArgumentException($"'{path}' doesn't exist");
            }

            foreach (var item in converter.ListConvertedFiles()) {
                Console.WriteLine(item);
            }
        }
    }
}