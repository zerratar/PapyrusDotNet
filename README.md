PapyrusDotNet
=============

PapyrusDotNet is a Papyrus Compiler. It will parse .NET binaries and compile it into Papyrus.<br/>
This makes it possible for you to write your Papyrus scripts using C#.


This project is built using Visual Studio 2012, .NET Framework 4.5, and is licensed with GPL v3

Please see http://www.gnu.org/licenses/gpl.txt

Copyright © Karl Patrik Johansson 2014-2015

###Readme Overview
* [What is Papyrus.NET (PapyrusDotNet)?](#what-is-papyrusnet-papyrusdotnet)
* [Limitations of PapyrusDotNet](#limitations-of-papyrusdotnet)
* [FAQ](#faq)
* [Ready for Test](#ready-for-test)
* [Changelog](#changelog)



What is Papyrus.NET (PapyrusDotNet)?
=========
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


### Warning
Keep in mind that the generated PapyrusDotNet.Core.dll does __not__ contain any logic.<br/>
Which means you can't (absolutely should not) use PapyrusDotNet.Core.dll with PapyrusDotNet.exe to re-generate .pas files. As these .pas files can completely break your scripts, worst case scenario you WILL have to reinstall skyrim completely.

### PapyrusDotNet.Common
A shared library between PapyrusDotNet and PapyrusDotNet.CoreBuilder<br/>
just to make sure I don't have to reinvent the wheel too many times over and over again.

### PapyrusDotNet.System - WIP
Extending the PapyrusDotNet.Core.dll Framework with some classes that you would normally find under .NET Framework System namespace. This reference can be used with your scripts.


## Limitations of PapyrusDotNet
**THE .NET FRAMEWORK IS NOT SUPPORTED!** This is because .NET Framework does not exist within Papyrus. Using PapyrusDotNet we can only translate functions, classes, objects, etc. That are already exposed to Papyrus.

__Unfortunately this means:__<br/>Dynamics does not work, linq does not work, extensions does not work, not even the System.Object
ValueType works. No functions of any Value/Base type works.

To give you an idea. Take a look at the following code


    // Following code does not work.
    string val = "0";
    var i = int.Parse(val);

    int x = 0;

    // This does not work    
    string hello = x.ToString();
    
    // This works, so use this instead
    string hello = x + "";  




**Things that do not work**

1. The .NET framework is **not** supported.
This means you can only rely on the actual C# language itself and not on any of the existing libraries. So **LINQ is not currently supported**.
2. The data type **object** is **not** supported.
3. *Boxing and Unboxing* is **not** supported.
4. *Extension methods* is **not** supported.
5. Overloading your own operators is **not** supported.
6. Creating new instances of objects is **not** supported.
7. Base class methods is **not** supported,
such as .ToString(), int.Parse("42"), bool.Parse(..), etc.
8. *Delegates* are **partial** supported. This is still being worked on.
9. *Destructors* does **not** work.
10. *Interfaces* does not work as intended. They are translated into classes currently.
11. Keywords such as abstract, virtual, protected, internal, private, public does **not** make any difference. Recommended is to only use public right now.
12. *Native* functionality is **not** supported. This includes:
unsafe, extern, DllImport, etc.
13. The keyword **using** is **not** supported.
14. Events are **not** supported.
15. Static fields or properties are **not** supported. Only static methods.
16. Bitwise operations are **not** supported.

**Things that do work**

1. The keyword **var** works.
2. Conditional statements, function return points and loops including:
for, foreach, while, switch, do, if, else, break, return
3. Enum works.
4. Following operators: +,++,-,--,%,%=,=,==,!=,<,>,<=,>=,\*,/,\*=,/=,&&,||
5. Explicit casts works. Ex: 
    **var x = (Actor)myObjectReference;** and **var y = myObjectReference as Actor;**
6. Static methods works. Normal methods works.
7. Properties works, ex: public int helloThere { get; set; }
8. Constructor works but will be translated into a OnInit function if one does not already exist.
or if OnInit already exists, the Constructor will be renamed into __ctor and called by the OnInit.
9. Following primitive types works: **byte, short, int long, float, double, bool, char, string**
10. Generics works. Ex: public class< T > ScriptName
11. Simple delegates works but are still in heavy development. I expect delegates to work fully in just a matter of weeks!

This should give you a strict overview of what you can and cannot do.
I may have missed out a lot of things from both lists. Mostly because it would be impossible to mention them all. Just make sure you remember to not use anything part of the .NET Framework. So if you skip out the default **using System;** etc. You should most likely be fine.

Just don't get freightened by the long list of unsupported things. The list may grow by time but it may also (and hopefully) shrink!


See Examples folder for usable examples.

If you're not familiar with Papyrus, i highly recommend you read some 
of their basic tutorials.<br/>
See [http://www.creationkit.com/Category:Papyrus](http://www.creationkit.com/Category:Papyrus)


## Examples
This section will be updated very soon!

For now, you can check my article about PapyrusDotNet, where I include a few examples.

[http://www.codeandux.com/writing-your-skyrim-mods-using-c/](http://www.codeandux.com/writing-your-skyrim-mods-using-c/)


FAQ
======
This section was just added, dont be sad if you don't find a solution for your problem here.

__Q1. Can I use SKSE with the scripts I generate here?__<br/>
A1. Yes you can! Just make sure that when you're generating a PapyrusDotNet.Core.dll you will need to have dissassembled</br>
all of SKSE's scripts into .pas files, then they will be included automatically.
<br/><br/>
__Q2. The solution does not seem to build. I'm getting a Metadata file 'xxx' could not be found.__<br/>
A2. Try building one project at a time, always start with PapyrusDotNet.Common, then PapyrusDotNet, then the rest should work just fine!
<br/><br/>
__Q3. How do I know that my code will work in Skyrim?__<br/>
A3.1. First off, make sure that you are using the same functions and/or types that are available in Papyrus. Using anything else just doesnt work. 
(Excluding your own already written scripts, be it Papyrus or C#)<br/><br/>
A3.2. Check http://www.creationkit.com/Category:Papyrus so that you're following their coding behaviour.<br/><br/>
A3.3. If you are still not sure, you can e-mail me your code, see contact at bottom of this page.

Known Issues
======
1. Script States are not yet supported.<br/>


Ready for Test
======
1. Delegates. See example below.
1. Enums :-)
1. Generic objects.
2. Papyrus Attributes: Auto, AutoReadOnly, Conditional, Hidden, InitialValue, Property

Basic delegates are now done. It may still have some untested bugs or so, but at least we have something working here! :-)

Following code now works

    public delegate void HelloThereDelegate();
    public void UtilizeDelegate()
    {
        HelloThereDelegate awesome = () =>
        {
            PapyrusDotNet.Core.Debug.Trace("Awesome was used!", 0);
        };

        HelloThereDelegate secondAwesome = () =>
        {
            PapyrusDotNet.Core.Debug.Trace("Second awesome was used!", 0);
        };

        awesome();

        secondAwesome();
    }

    public delegate void SecondDelegate();
    public void UtilizeDelegate2()
    {
        string whatHorrorLiesHere = "test123";

        SecondDelegate arrr = () =>
        {
            PapyrusDotNet.Core.Debug.Trace("UtilizeDelegate2 was used!" + whatHorrorLiesHere, 0);
        };

        arrr();
    }

    public delegate void AnotherDelegate(string input);
    public void UtilizeDelegate3()
    {
        string horror = "test";

        AnotherDelegate awesome = (s) =>
        {
            PapyrusDotNet.Core.Debug.Trace("UtilizeDelegate3 was used!" + s, 0);
        };

        awesome(horror);
    }

    public delegate void HorribleDelegate();
    public void UtilizeDelegate4()
    {
        string magic = "helloo";
        HorribleDelegate awesome = () =>
        {
            AnotherDelegate awe2 = (s) =>
            {
                PapyrusDotNet.Core.Debug.Trace("UtilizeDelegate4 was used!" + s, 0);
            };

            awe2(magic);

        };
        awesome();
    }

Initial Value Example:

    // When setting a initial value directly on the field
    // That value will be set in the constructor (Constructors are a function that OnInit will call first) 
    public int myStartingVal = 0;
    
    // When using the InitialValue attribute, the value will be set directly in the output assembly code 
    [InitialValue("hello world!")]
	public string dummy;
	
Properties Example:

    [Property, Auto]
    public Actor MyPlayerRef;

Attributes Example:

    [Hidden, Conditional]
    public class MyScriptName : ObjectReference
	
Changelog
========

###v0.1.6
* Delegates now works, still needs to be properly tested. Check above examples to see the difference scenarios that works.

###v0.1.5f3
* Parameterless delegates works. (As long as you are not providing a locally instanced variable to be used inside the delegate. See example above.)

###v0.1.5f2
* Added support for Enums! :-)
* Fixed a few bugs that would cause the project not to build. Stupid code refactoring. My bad..

###v0.1.5f1
* Tons of code refactoring, still working on cleaning up the solution more to make it easier to manage and add new features.

###v0.1.5
* CoreBuilder can now parse psc files
* CoreBuilder will now handle arrays better´
* Some general bug fixes to PapyrusDotNet

###v0.1.4f4
* Cleaned up the solution, removed old and unusable projects.
* Added a new Project: PapyrusDotNet.System, a helper library for your papyrus scripts to add some feeling of the .NET framework.
* Fixed a bug causing some static functions being called as normal functions.
* Fixed a bug that would cause incorrect class to be used when some static method calls where made.
* Started working on enums and boxing (these are not complete yet)

###v0.1.4f3
+ Minor bug fixes

###v0.1.4f2
+ Improved support for generics
+ Added support for generating Papyrus from referenced assemblies, this excludes assemblies that is part of the .NET framework and papyrusdotnet.core.dll, extended libraries for papyrusdotnet.core is included though.
+ Added a WIP class: List<T> that can now be used, see 'ListExample.cs' for usage. This generic class can hold up to 16384 items. (128*128)

###v0.1.4f1
+ Fixed a bug with Generic Fields not being properly resolved.

###v0.1.4
+ Added support for generic types. This is still very experimental, so don't expect it to be bugfree.
+ Added attributes: GenericType, GenericMember, GenericIgnore. They are currently unnecessary, the idea behind them are still undocumented. I will explain more when they do as expected.
+ Added a new example file, GenericTest.cs, see how generics can be used. (Extremely simple)

###v0.1.3f2
+ Improved support for type casting
+ Fixed a type casting bug
+ Fixed a bug with Conditional Jumps making 'switch' select wrong cases.
+ Fixed a bug with Initial Values

###v0.1.3f1
+ Added support for foreach statements

###v0.1.3
+ Added support for switch cases.

###v0.1.2
+ Added support for Properties and Atttributes.
+ Added support for InitialValue for fields.
+ Fixed a bug when concating strings
+ Added new example on how to use Attributes and Properties.<br/>

###v0.1
+ Initial Release to github<br/>


## Contact
If you got any questions, please don't hesitate to e-mail me at zerratar@gmail.com !