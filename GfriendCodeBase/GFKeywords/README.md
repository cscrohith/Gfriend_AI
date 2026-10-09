# Library Implementation Guide

Project
-------------------------
Project name MUST start with "GFK." such as GFK.Android

Classes which implement IGFLibrary should use same name of the project name without 'GFK.'

Set build output path to following:
- Debug : ..\bin\Debug\GFriendUI\libs\
- Release : ..\bin\Release\GFriendUI\libs\



Library Classes
---------------
Must inherit IGFLibray interface and implement following methods:

- Initialize(DeviceUnderTest, OutputDir) : Make connection to DUT, set output directory which all output will be stored.
- GetName() : Returns library name
- Dispose() : Close all connection from DUT
- GetDependencies() : Returns list of referenced GF Libraries' name as List<string>. Return null if no dependencies.


Keywords
--------
All methods which implement Keywords must return KeywordResult

Keyword can have following attributes:
- KeywordDescription : Description of keyword
- KeywordDisplayName : Keyword name which actually used in GFriend Script
- KeywordParameters : Description of parameters
- GetKeyword : If keyword use variable to store value, use this attribute
