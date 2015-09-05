# Unreal-Qt-project-generator
This small console utility generates QtCreator projects for Unreal Engine 4 gameplay programming.
It should in theory work with any version of Unreal Engine 4 (not tested)

The tool can generate .pro files with:
<ul>
  <li>Configuration for Unreal Engine development (C++11 support, no Qt)
  <li>All your current source and header files (+ build.cs) included</li>
  <li>All Unreal Engine defines and includes added</li>
  <li>5 different build and launch targets, which are included in the Visual Studio project file (Debug game, Development Editor, Shipping, etc...)</li>
  <li>additional Cook target (allows to cook content for standalone builds)</li>
</ul>

Unreal Engine projects can be debugged with CDB (GDB not supported).

For more information and a tutorial how to setup QtCreator see this post:
https://forums.unrealengine.com/showthread.php?59458-TOOL-Tut-WIN-Unreal-Qt-Creator-Project-Generator-(v0-1-Beta)

<b>Important:</b> I have only tested the tool on my computers with Windows 8.1, QtCreator 3.3.0/3.5.0 and Unreal Engine 4.4/4.5/4.8/4.9. There may be bugs present.

<b>How to build:</b>
Just open the .sln file with Visual Studio 2013/2015 and hit build (.NET Framework 4.0 required, no other dependencies)
The qtBuildPreset.xml file needs to be in the same folder as the executable when you want to run the tool.
