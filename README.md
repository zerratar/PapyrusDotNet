PapyrusDotNet 1.0 Technical Preview 2
=============

PapyrusDotNet is a Papyrus compiler, difference between this one and Bethesda's is that this one takes<br/>
.NET binaries and compiles it into Papyrus Binaries (.PEX files).<br/>
This makes it possible for you to write your Papyrus scripts using C# or VB.


This project is built using Visual Studio 2015, .NET Framework 4.5.2, and is licensed under GPL v3

Please see http://www.gnu.org/licenses/gpl.txt

Copyright © Karl Patrik Johansson 2014-2015

### Readme Overview
* [What is Papyrus?](#what-is-papyrus)
* [Why use PapyrusDotNet?](#why-use-papyrusdotnet)
* [Using PapyrusDotNet](#using-papyrusdotnet)
* [Technical Preview](#technical-preview)
* [Limitations of PapyrusDotNet](#limitations-of-papyrusdotnet)
* [FAQ](#faq)
* [Examples](#examples)
* [Changelog](#changelog)


What is Papyrus?
=========
Papyrus is a scripting language created by Bethesda for use in their Elder Scrolls Games as well for Fallout 3,<br/>
Fallout New Vegas and Fallout 4. Bethesda used it for creating their ingame logics such as Quests and Character behaviours while.
They let us modders use it as well for creating mods.<br/>
You can take a more deeper look at Papyrus over here<br/>
http://www.creationkit.com/Category:Papyrus

Why use PapyrusDotNet?
=========
#### Savvy?
PapyrusDotNet delivers a possibility rather than just the solution. Opening up for more people to start<br/>
writing their own mods for both Fallout 4 and Skyrim. (Only Fallout 4 and Skyrim is supported)

A lot of people, (Mostly talking about myself) does not like to jump in to new languages with "small"<br/>
amount of support and tutorials. There are people that want to be able to use the language they use daily
for everything they do. C# is a good example for me. I use it at work, I use it at home and I wanna use it for modding. There are great IDE's for C# (Just take a look at Visual Studio, its awesome). Whilst Papyrus does not have its own IDE, only syntax highlightning for different text-editors.

#### Papyrus ain't got it!
C# also delivers a few interesting things that you can do which also works in PapyrusDotNet that would normally not work with plain
Papyrus. Such as using `Delegates`, `Generics`, `Foreach`, `Switch`, `Enums`, `var`. None of wich is available in Papyrus :)

#### No dependency on Creation Kit
Now if this doesn't make you wanna try it out then, how about:
You can use Visual Studio, compile your C# or VB library project and have it automatically build into a .pex file.
And yep, `it works without using the Creation Kit`. So you don't have to wait for the Creation Kit to be available before you can start writing your nifty little/or big Fallout 4 mods.


Using PapyrusDotNet
==========
First of all, the solution contains a bunch of project but only one project is important.<br/>
**PapyrusDotNet**. The rest you can ignore for now, unless you want to use them for your own projects or what not.

### PapyrusDotNet
Main Application for Compiling .NET into Papyrus and Papyrus into .NET
This is the main Project and tool used for generating the Fallout 4 and Skyrim scripts.

**Usage**

    PapyrusDotNet.exe <input> <output> [option] [<target papyrus version (-fo4 | -skyrim)>] [<compiler settings (-strict|-easy)>]
        Options:
            -papyrus :: [Default] Converts a .NET .dll into .pex files. Each class will be a separate .pex file.
			    * <input> :: file (.dll)
			    * <output> :: folder
			    * <target version> :: [Fallout 4 is default] -fo4 or -skyrim
			    * <compiler options> :: [Strict is default] -strict or -easy determines how the compiler will react
			      on features known to not work in papyrus. -strict will throw a build exception while -easy may let
                  it slide and just remove the usage but may cause problems with the final script. 
            -clr :: Converts a .pex or folder containg .pex files into a .NET library usable when modding.
                * <input> :: .pex file or folder
                * <output> :: folder (File will be named PapyrusDotNet.Core.dll)


<br/>
**Example usage** (Compiling into Papyrus, Fallout 4, Strict)

    PapyrusDotNet.exe  "MyFallout4Mod.dll" "c:\fallout 4\data\scripts\"
    
> Note: This will generate a .pex file for each class defined inside the MyFallout4Mod.dll
    
**Example usage** (Compiling into Papyrus, Skyrim, Strict)

    PapyrusDotNet.exe  "MyFallout4Mod.dll" "c:\fallout 4\data\scripts\" -skyrim

> Note: This will generate a .pex file for each class defined inside the MyFallout4Mod.dll

**Example usage** (Creating a .NET utility library from Papyrus)

    PapyrusDotNet.exe  "c:\fallout 4\data\scripts\" "c:\PapyrusDotNet\" -clr

> Note: This will generate a utility library called PapyrusDotNet.Core.dll, and it can (should) be used as reference for any new mod-project you create. A core.dll already exists in the binaries folder but it is compiled for Fallout 4. 
> So the Core.dll may be renamed in the future to: PapyrusDotNet.Core.Fallout4.dll and PapyrusDotNet.Core.Skyrim.dll

### Warning
Keep in mind that the generated PapyrusDotNet.Core.dll does __not__ contain any logic.<br/>
Which means you can't (absolutely should not) use PapyrusDotNet.Core.dll with PapyrusDotNet.exe to re-generate .pex files. As these .pex files will completely break your scripts and you WILL have to reinstall skyrim or fallout 4 completely unless you took backups...

## Technical Preview

The second technical preview (TP2) does **not** have support for `Generics`

This feature will be added again when the base functionality is stable enough. To use it, you will have to checkout an earlier version of PapyrusDotNet but unfortunately it only works for Skyrim.

Short: Generics will be added very soon but is not implemented in TP2.

## Limitations of PapyrusDotNet
#### .NET Framework is NOT supported!
For people that are unaware, .NET Framework is not a language, it is the actual framework/utilities/classes that are used in .NET, when you're using C# you're most likely using the `System` namespace and that alone is using the .NET Framework, however skipping to use the System library, or any existing .NET Framework Libraries and you should be good to go!

This is due to the fact that .NET Framework doesnt exist in Papyrus, which makes it impossible to use those functions.
Instead, you should rely on the PapyrusDotNet.Core.dll and any Extension libraries available for PapyrusDotNet as those will help you out tremendously!


#### The sad parts
Now, not using the .NET Framework means we are missing out on a lot of nice features, biggest ones are probably `Linq`.
Yep, you heard me.. No Linq. 

##### Limitations of Papyrus itself
Not only are we limited to not use .NET Framework we are also limited to a bunch of other stuff as.
<br/>`No object instantiation.` So you can't use `new object()`
<br/>`No base object type.` So you can't use the `object` type.
<br/>`No dynamic types.` So you can't use the `dynamic` keyword.

##### Skyrim Vs Fallout 4
With Skyrim there are even limitations on Array Sizes, for instance an array can not exceed 128 items and it cannot be created dynamically. So for Skyrim, you can't do `int[] myAwesomeArray = new int[sizeVariable];` and instead you would have to specifiy the size with a constant value.

Fallout 4 has lifted this curse and does now `allow you to create dynamically sized arrays`.<br/>
Fallout 4 has also introduced a few new feature such as:

`Remove Last Element from array`, `Remove Elements from Array`, `Add Elements to an Array`, `Find Element in Array`, `Clear Array`,

And also introduced `struct` types. Yupsies! These however DOES allow being created as a new instance. The limitations are though that the struct can only contain fields and not properties.

### List of what cans and cans-not


**Things that do not work**

1. The .NET framework is **not** supported.
This means you can only rely on the actual C# language itself and not on any of the existing libraries.
2. Linq does **not** work just yet.
2. The data type **object** is **not** supported.
3. *Boxing and Unboxing* is **not** supported. (Due to the fact that there are no Object type)
4. *Extension methods* is **not** supported.
5. Overloading your own operators is **not** supported.
6. Creating new instances of objects is **not** supported.
7. Base class methods is **not** supported,
such as .ToString(), int.Parse("42"), bool.Parse(..), etc.
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
4. Delegates works.
4. Following operators: +,++,-,--,%,%=,=,==,!=,<,>,<=,>=,\*,/,\*=,/=,&&,||
5. Explicit casts works. Ex: 
    **var x = (Actor)myObjectReference;** and **var y = myObjectReference as Actor;**
6. Static methods works. Normal methods works.
7. Properties works, ex: public int helloThere { get; set; }
8. Constructor works but will be translated into a OnInit function if one does not already exist.
or if OnInit already exists, the Constructor will be renamed into __ctor and called by the OnInit.
9. Following primitive types works: **byte, short, int long, float, double, bool, char, string**
10. ~~Generics works. Ex: public class< T > ScriptName~~

This should give you a strict overview of what you can and cannot do.
I may have missed out a lot of things from both lists. Mostly because it would be impossible to mention them all. Just make sure you remember to not use anything part of the .NET Framework. So if you skip out the default **using System;** etc. You should most likely be fine.

Just don't get freightened by the long list of unsupported things. The list may grow by time but it may also (and hopefully) shrink!


See Examples folder for usable examples.

If you're not familiar with Papyrus, i highly recommend you read some 
of their basic tutorials.<br/>
See [http://www.creationkit.com/Category:Papyrus](http://www.creationkit.com/Category:Papyrus)


# Examples
This section will be updated very soon!

For now, you can check my article about PapyrusDotNet, where I include a few examples.

[http://www.codeandux.com/writing-your-skyrim-mods-using-c/](http://www.codeandux.com/writing-your-skyrim-mods-using-c/)


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
    
    [Auto]
    public Actor MyPlayerRef { get; set; }

Attributes Example:

    [Hidden, Conditional]
    public class MyScriptName : ObjectReference


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
2. StringCat has a bug causing the string to append to itself (This was previously intended but was never really thought-through).
3. Generics are not supported in TP2


Changelog
========
### v1.0.0f2 TP2
 * Delegate support added. Be aware: Delegates cannot be used as Properties, Parameters or as Return Type of a method. You can check the DelegateTests.cs inside the fallout4example project for different examples of what is currently working.

### v1.0.0 TP1
* Completely rewrote the whole project from scratch.
* Papyrus Assembly Reader/Writer (.pex files) - This can be used in your projects if you want to.
* Papyrus Decompiler (Right now, it is a C#/Papyrus Hybrid output without any control flow analysis)
* All actions such as Compile into Papyrus, creating core library and more are all directly from PapyrusDotNet.exe instead of separate binaries.
* Supports both Fallout 4 and Skyrim
* In general the compiler is extremely more stable than the previous versions.
* New Console UI 


### v0.1.6
* Delegates now works, still needs to be properly tested. Check above examples to see the difference scenarios that works.

### v0.1.5f3
* Parameterless delegates works. (As long as you are not providing a locally instanced variable to be used inside the delegate. See example above.)

### v0.1.5f2
* Added support for Enums! :-)
* Fixed a few bugs that would cause the project not to build. Stupid code refactoring. My bad..

### v0.1.5f1
* Tons of code refactoring, still working on cleaning up the solution more to make it easier to manage and add new features.

### v0.1.5
* CoreBuilder can now parse psc files
* CoreBuilder will now handle arrays better´
* Some general bug fixes to PapyrusDotNet

### v0.1.4f4
* Cleaned up the solution, removed old and unusable projects.
* Added a new Project: PapyrusDotNet.System, a helper library for your papyrus scripts to add some feeling of the .NET framework.
* Fixed a bug causing some static functions being called as normal functions.
* Fixed a bug that would cause incorrect class to be used when some static method calls where made.
* Started working on enums and boxing (these are not complete yet)

### v0.1.4f3
+ Minor bug fixes

### v0.1.4f2
+ Improved support for generics
+ Added support for generating Papyrus from referenced assemblies, this excludes assemblies that is part of the .NET framework and papyrusdotnet.core.dll, extended libraries for papyrusdotnet.core is included though.
+ Added a WIP class: List<T> that can now be used, see 'ListExample.cs' for usage. This generic class can hold up to 16384 items. (128*128)

### v0.1.4f1
+ Fixed a bug with Generic Fields not being properly resolved.

### v0.1.4
+ Added support for generic types. This is still very experimental, so don't expect it to be bugfree.
+ Added attributes: GenericType, GenericMember, GenericIgnore. They are currently unnecessary, the idea behind them are still undocumented. I will explain more when they do as expected.
+ Added a new example file, GenericTest.cs, see how generics can be used. (Extremely simple)

### v0.1.3f2
+ Improved support for type casting
+ Fixed a type casting bug
+ Fixed a bug with Conditional Jumps making 'switch' select wrong cases.
+ Fixed a bug with Initial Values

### v0.1.3f1
+ Added support for foreach statements

### v0.1.3
+ Added support for switch cases.

### v0.1.2
+ Added support for Properties and Atttributes.
+ Added support for InitialValue for fields.
+ Fixed a bug when concating strings
+ Added new example on how to use Attributes and Properties.<br/>

### v0.1
+ Initial Release to github<br/>


## Contact
If you got any questions, please don't hesitate to e-mail me at zerratar@gmail.com or adding me on skype: zerratar