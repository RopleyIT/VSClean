# VSClean - Visual Studio source folder cleaner and zipper
This application cleans reproducible files from the source folders of 
an application. By reproducible, we mean files that the
compiler either generates during the build/testing phases,
or that are downloaded from other locations, like NuGet
packages for example.

The application comes in two executable forms. The `vsclean`
application is a command line tool suitable for inclusion
in scripts. The `WinVSClean` application is a windows
application that does the same job driven from a window.

### The command line application `vsclean`
The syntax for the command line is as follows:
``` javascript
vsclean folderToBeCleaned [-z zipFilePath] [-v] [-d]
```
The first argument is mandatory, and identifies the root
folder for the visual studio solution to be cleaned.

The optional argument `[-z zipPath]` allows you to zip
up the files that would have remained after cleaning, and
puts them into the zip file whose path you provide. If
the zip file already existed, it is summarily replaced.

The optional argument `[-v]` causes the version control
files to be included in the zip file. For example, if the
folder to be cleaned contains a `.git` sub folder, representing
a local git repository, the contents of this repository
are also zipped up into the zip file. Omission of this argument
causes the source files without the repository to be zipped.

The final optional argument `[-d]` causes a verbose listing
of every subfolder visited, together with the decision
on each file/folder that is kept or skipped/deleted reported
in the console window. Omission of this flag causes the
tool to operate silently.

### The Windows-based application WinVSClean
The application drives all its operations from a main menu
at the top of the window. These menu items behave as follows:

| Menu selection | Behaviour |
|---|---|
| `File>ZIP Source...` | Creates a zip file containing the source files without the version control files. Note that the zip file has the same name as the folder, and is placed in the root of the folder being zipped. |
| `File>ZIP Source with VC...` | As above, but include the version control files in the zip file. |
| `File>Clean folder...` | Clean unnecessary files from the folder. No zip file is created. |
| `File>Exit` | End the program |
| `View log` | Causes the empty text panel to be filled with the details of files and folders, and how each was processed. |

### Configuration
By default, the application uses an inbuilt list of file and folder
filters to determine which files or folders should be
excluded or included in the folder/zip file after cleaning.
It is possible however to include your own custom filters
thereby replacing the inbuilt defaults.

To do this, you will need to place a file named `.vsclean`
in the folder that is passed to the tool to be cleaned.
This file should contain a list of filters using the
same filter syntax as used by git's `.gitignore` files.
Details of the syntax for these files will be found in the
Pattern Format section of the
[Git Documentation](https://git-scm.com/docs/gitignore).
Do remember though that these filters identify the files
that are to be _excluded_ from the result set, not those
that are to be kept.

The inbuilt default filters for this application are as specified below:
``` javascript
            **/bin/
            **/obj/
            **/TestResults/
            **/debug/
            **/debugpublic/
            **/release/
            **/releases/
            **/x64/
            **/x86/
            **/build/
            **/bld/
            **/.vs/
            **/_upgradereport_files/
            **/backup*/
            **/packages/
            *.suo
            *.user
            *.userosscache
            *.sln.docstates
            *.userprefs
            *.pdb
            *.vsp
            *.vspx
            *.vspscc
            *.vssscc
            *.vsmdi
            *.psess
```

### The code libraries
The functional heart of these applications lies in the `vscleanlib`
project. 

The `Cleaner` class implements the folder cleaning
and/or zipping functionality, itself containing the
inbuilt default set of filters.

The `PathFilterParser` and its contained list of
`PathFilter` objects reads the gitignore-formatted
filter source and converts it into an active filter.
File paths can then each be offered to its methods for
then to return a boolean result, telling you whether
the file should be included in the output file set.

If nothing else, these two classes provide a useful
independent implementation of a `.gitignore` file 
parser for use in the .NET world.

#### Key methods and properties of `PathFilterParser`

| Method/property | Description |
|---|---|
|`PathFilterParser(string)`|Constructor. Argument is the multi-line string containing filters using the `gitignore` syntax|
|`PathFilterParser(TextReader)`|Constructor. Reads the filters from the input text stream passed as the argument. |
|`string RootFolder`|Property where the full path to the folder being parsed is placed. If left empty, paths will need to be relative to that folder, rather than full.|
|`bool Accepts(string, bool)`|Returns true if the file path assed as the first argument should be included in the output. Note that the boolean argument should be set true, if the path is a folder, false if it is a file.|
|`bool Denies(string, bool)`|Inverse of the `Accepts` method.|
|`bool DeniesDirectory(string)`|Returns true if there exists an exact pattern match that explicitly denies this path if it is a folder not a file. The pattern must end with a '/' for this to be true. This is used when we want to make the PathFilterParser behave like Git's own gitignore logic, where excluded folders cannot reinclude child files or folders.|

#### Key methods of the `Cleaner` class
Note that this class is a static class, and therefore does not need to be constructed for its methods to be used.

| Method | Description |
|---|---|
|`public static async Task SourceBackup(string folderPath, bool excludeVC, string zipPath = null)`|The method that creates a zip file and populates it with the contents of the included files from the folder `folderPath`. The `excludeVC` argument excludes version control folders and files. Omission of the `zipPath` argument causes the zip file to be created in the same `folderPath` with the name of the folder and a `.zip` extension.|
|`public static async Task SourceClean(string folderPath)`|Rids the specified folder of any files or folders exccluded by the filter list. Include a file named `.vsclean` in that folder, populated with filters in the `.gitignore` format to create custom exclusion rules.|
|`public static Action<string, double> ProgressNotify`|Delegate to a function to be invoked when progress is to be reported. The first argument is a message string. The second argument if multiplied by 100 would be the percentage complete the folder processing has reached.|

Other properties exist that allow you to obtain a count 
of how many files or folders have been included or 
excluded from the result set. Explore the source code if you
think these might be useful.

### Licensing
This product is published under the [standard MIT License](https://opensource.org/licenses/MIT). The specific wording for this license is as follows:

Copyright 2018 [Ropley Information Technology Ltd.](http://www.ropley.com)
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.