# Unreal Qt project generator (uProGen)
<h3>Overview</h3>
This small console utility generates QtCreator (qmake) projects for Unreal Engine 4 game development.
It should work with any version of Unreal Engine 4.

The tool can generate .pro files with:
<ul>
  <li>Configuration for Unreal Engine development (C++11 support, no Qt)</li>
  <li>All your current source and header files included</li>
  <li>All Unreal Engine defines and includes added</li>
  <li>5 different build and launch targets (the same which are included in the Visual Studio project: Debug game, Development Editor, Shipping, etc...)</li>
  <li>Additional Cooking target (allows to cook content for standalone builds directly from within QtCreator)</li>
</ul>

<h3>Usage information</h3>
Before you can use the tool you should configure your QtCreator installation for Unreal Engine development.
Just follow the tutorial in my [forum post][ue post].

<b>Installation:</b>
<ol>
  <li>Download the latest version from [here][latest version]</li>
  <li>Extract the contents of the zip file to your preferred location</li>
  <li>First make sure that .pro files are associated with QtCreator, then execute uProGen for the initial configuration</li>
  <li>(Optional) Add the uProGen folder to your PATH variable, so you can launch uProGen from any location</li>
</ol>

<b>Usage:</b>
<ol>
  <li>Open a cmd window and navigate to your project folder (where .uproject file is located)</li>
  <li>Run uProGen (the resulting .pro file is stored in the Intermediate\ProjectFiles subdirectory)
</ol>

<h3>Additional information</h3>

Unreal Engine projects can only be debugged with CDB on Windows (GDB not supported).

Since QtCreator 4.7, [Clang code model](https://blog.qt.io/blog/2018/06/05/qt-creators-clang-code-model/) is enabled by default. If you experience slow or unreliable auto-completion with UE projects, I recommend switching to the built-in code model by disabling "ClangCodeModel" under Help -> About Plugins -> C++.

<b>Important:</b> I have tested the tool on my computers with Windows 8.1 64bit, Qt 5.4/5.5/5.7, QtCreator 3.3.0/3.5.0/4.1.0 and Unreal Engine 4.4/4.5/4.8/4.9/4.13.

<h3>How to build</h3>
Just open the .sln file with Visual Studio 2013/2015 and hit build (.NET Framework 4.0 required, no other dependencies).<br>
The qtBuildPreset.xml file needs to be in the same folder as the executable when you want to run the tool.

[ue post]: https://forums.unrealengine.com/development-discussion/c-gameplay-programming/30348-tool-tut-win-unreal-qt-creator-project-generator-v0-3
[latest version]: https://github.com/nibau/Unreal-Qt-project-generator/releases/latest
