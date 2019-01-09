# Convert your Web Forms ASPX pages to ASP.NET Core Razor Pages

### Problem

You have an ASP.NET Web Forms project. 

Your app needs to be modernized and supported with the cross-platform .NET Core instead of the full Windows-only .NET Framework.

You need to quickly get started in turning these Web Forms pages and code into Razor pages with corresponding C# code files.

### Solution

Run this console app to convert your basic Web Forms (.aspx) pages to Razor Pages (.cshtml).

It will process all .aspx (and .aspx.cs) files in a directory, and create a corresponding .cshtml (and .cshtml.cs) file in the same directory.


### What This App Does

1. Convert your markup - web forms elements to HTML equivalents
2. Ensure your aspx/cs pages get a corresponding cshtml/cs page
3. Ensure that ContentPlaceholders are converted to 



### How To Use

1a. Build and run the console app on the command line

1b. Debug this console app.

Supply it with a directory:

    WebformsToRazorPages "C:\Source\myProject"
    
    WebformsToRazorPages "/Users/me/Source/myProject/"

2. **New** files will be created in the same directory

3. Use these new .cshtml and .cshtml.cs files in your ASP.NET Core web app.

4. .aspx files will NOT be modified in this process.

### What This App Does NOT Do

1. Analyze or modify user controls, either referenced on the page, or the user control files themselves (.ascx).
2. Change any hardcoded HTML or JavaScript strings in your code-behind. If you have `Response.Write` in your code-behind, then you must change that manually.
3. Handle postback events from web forms controls. You must change this manually, usually with new methods for `OnPost` or `OnPostAsync()`.
4. Change any System.Web references (`Request`, `Session`, etc), . Those you will have to change manually.
5. Change any code-behind referencing its page's webforms controls. Those you will have to change manually.
6. Change any master pages.
7. Change anything to do with routing or routevalues. You must do this manually.
