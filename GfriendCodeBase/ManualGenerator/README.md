# Documentation Guide

## Documentation Format

GFriend Documentation need to be written with GFM(Github Flavored Markdown.)

Refer following link for GFM Spec:

https://github.github.com/gfm/


## Naming conventions and Path of documentations
- README.md file

  All GFM file for used for manual must have name of "README.md"

  It is recommended to store root directory of project (not the solution)
  
- Images
 
  All images which will be used within documentation, must be placed under ".images" folder.
  
  In the README.md, image link should be used as relative path as example below:
  
  ```
  ![GFriend::Main](.images/GFriend_Main.png)
  ```
  
## Document Generation

In the build step, GenerateManual.py file will copy all related files (README.md and images) to manual location

and convert GFM to HTML format and change links.

To run this script, following tools should be installed :

Python 3.7.0 (or latest) : https://www.python.org/downloads/

Pandoc 2.9.2.1 (or latest) : https://github.com/jgm/pandoc/releases/tag/2.9.2.1

