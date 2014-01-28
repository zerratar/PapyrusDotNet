PapyrusDotNet
=============

Papyrus.NET, enables you to write C# or any CLR enabled languages to be used in Skyrim.



This project is built using Visual Studio 2012, .NET Framework 4.0, and is licensed with GPL v3

Please see http://www.gnu.org/licenses/gpl.txt

Copyright © Karl Patrik Johansson 2014


There are currently no 'How To Use' as its just a first initial upload to github and its still pretty much
hardcoded to work on my own system. I'm working on making it more configurable for everyone to use a lot easier.


##NOTE:

__ACTUAL .NET FRAMEWORK IS NOT SUPPORTED AT ALL! THIS IS DUE TO THE LIMITATIONS IN PAPYRUS ITSELF. THEREFOR
YOU CANNOT USE ANY .NET SPECIFIC CODE IN YOUR SCRIPTS. FUTURE UPDATES MAY ENABLE SOME OR A FEW OF THE .NET FUNCTIONS
BUT RIGHT NOW NONE IS WORKING.__

__You CANNOT use any of the types, methods, functions from any existing .NET library as it wont work in Papyrus.
Even if it builds, and are able to generate some sort of Papyrus Assembly Code, the papyrus code wont compile
as it does not have these types, methods, functions defined.__

__So Generics does NOT work, dynamics does not work, linq does not work, extensions does not work, not even the System.Object
ValueType does not work.__

However:

string, bool, int, byte, short, long, float, double: works.
Extensions or functions of those TypeValues does NOT work so:

Ex: int.Parse(val) __does not work.__

    int x = 0; 

    // Does not work, use: hello =  x + ""; Instead.    
    string hello = x.ToString(); 


So for now, if you want to make a C# Skyrim Script, you will have to follow the original limitations of Papyrus
and only use classes from the PapyrusDotNet.Core.dll and/or any PapyrusDotNet.Core extended libraries, e.g. 
PapyrusDotNet.Core.Collections

See TestDLL for example of usage.


If you're not familiar with Papyrus, i highly recommend you read some<br/>
of their basic tutorials.<br/>
See http://www.creationkit.com/Category:Papyrus


### What is Papyrus.NET (PapyrusDotNet)?

Papyrus.NET is a library/tool that I created to translate (mainly C#) CIL into Papyrus Assembly Code.

The project currently contains:

### PapyrusDotNet

A .NET -> Papyrus Assembly Code Generator.
This is the main Project and tool used for generating the Skyrim scripts.

Example usage: 

PapyrusDotNet.exe  -i "MyDotNetSkyrimScripts.dll" -o "c:\skyrim\data\scripts\asm\"

    -i : Input DLL file, containing your .NET built skyrim scripts
    -o : output directory, where your generated .pas files will be put.

> Note: This will generate a .pas file for each class defined inside the .dll <br/>
> And does not currently compile it into a .pex for you. You will have to do this  <br/>
> yourself. I'm currently working on getting this to work automatically for you.

### PapyrusDotNet.CoreBuilder

A Papyrus Assembly Code -> .NET Library Generator.
It is used for generating a core.dll that can be used
to reference to your already existing Papyrus scripts in your C# Skyrim Script.

Example usage:

PapyrusDotNet.CoreBuilder.exe -i "c:\skyrim\data\scripts\asm\"

    -i : Input directory containing .pas or .psc files
    -t : (Optional) Input type, 'pas' or 'psc' can be used. if none are defined, 'pas' will be used.
         (don't include the single quoutes)


This will generate PapyrusDotNet.Core.dll in the same folder as PapyrusDotNet.CoreBuilder.exe relies,
this is a dll that can be used in your projects, enabling references to previously created scripts.

> Note: Only .pas files are supported as input ( disassembled .pex files ) <br/>
> I'm still working on parsing .psc (script source files).


Other Projects
==============

Most of the other projects are just different tests or planned features.

### PapyrusDotNet.Launcher

A test to see if it is possible in the future to inject PapyrusDotNet.Bridge
directly into skyrim to get a better control over the scripting, a possibility for opening up more functionality
of the .NET framework.


### TestDll

Contains a few test C# Skyrim Scripts
This .dll can be used directly with PapyrusDotNet

### PapyrusDotNet.Tester

Just a test project to see if the extended library works.


### PapyrusDotNet.Core.Collections

Extending the PapyrusDotNet.Core.dll Framework with different Skyrim 'optimized' / 'working' scripts.
Not yet complete, and should not be used just yet.






## Contact
If you got any questions, please don't hesitate to e-mail me at zerratar@gmail.com !
