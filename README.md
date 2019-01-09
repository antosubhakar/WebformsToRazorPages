# Convert your Web Forms ASPX pages to ASP.NET Core Razor Pages

### Problem

You have an ASP.NET Web Forms project. 

Your app needs to be modernized and supported with the cross-platform .NET Core instead of the full Windows-only .NET Framework.

You need to quickly get started in turning these Web Forms pages and code into Razor pages with corresponding C# code files.

### Solution

Run this console app to convert your basic Web Forms (.aspx) pages to Razor Pages (.cshtml).

It will process all .aspx (and .aspx.cs) files in a directory, and create a corresponding .cshtml (and .cshtml.cs) file in the same directory.

### How To Use

1a. Build and run the console app on the command line
1b. Debug this console app.

Supply it with a directory:

    WebformsToRazorPages "C:\MyProject"

or

    WebformsToRazorPages "/Users/me/Source/myProject/"

2. **New** files will be created in the same directory

3. Use these new .cshtml and .cshtml.cs files in your ASP.NET Core web app.

4. .aspx files will NOT be modified in this process.

### What This App Does NOT Do

1. Analyze or modify user controls
2. Change any hardcoded HTML or JavaScript strings in your code-behind. If you have `Response.Write` in your code-behind, then you must change that manually.
