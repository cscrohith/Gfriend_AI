"""
GFriend Documentation Generation Script
Need to install Pandoc first.
"""

# Imports
import sys
import os
import shutil
import fileinput
import re
from subprocess import call
from pathlib import Path

# Global variables
repoRoot = "..\\"
outputDir = "..\\bin\\Release\\GFriendUI\\manual"
#outputDir = "..\\bin\\Debug\\GFriendUI\\manual"
# Class declarations

# Function declarations
def mkdir_p(directory):
    if not os.path.isdir(directory):
        os.makedirs(directory)

def generate_documents():
    global outputDir
    global repoRoot
    mkdir_p(outputDir)
    for root, directories, filenames in os.walk(repoRoot):
        for directory in directories:
            src = os.path.join(root,directory)
            if (".images" in src or ".resources" in src) and outputDir not in src:
                relativePath = src.replace(repoRoot, '')
                des = outputDir + relativePath
                print ("FOUND::" + src)
                print ("COPY TO :" + des)
                mkdir_p(des)
                
                for fileName in os.listdir(src):
                    fullName = os.path.join(src, fileName)
                    if(os.path.isfile(fullName)):
                        shutil.copy (fullName, des)
                
        shutil.copy(os.path.dirname(os.path.abspath(__file__))+"\\gfmanual.css", outputDir)
        for filename in filenames:
            if filename == "README.md" and outputDir not in root:
                src = root
                relativePath = src.replace(repoRoot, '')
                depth = relativePath.count('\\')
                src = os.path.join(root, filename)
                des = outputDir + relativePath
                print ("FOUND::" + src)
                print ("COPY TO :" + des)
                mkdir_p(des)
                if(os.path.isfile(src)):
                    shutil.copy (src, des)
                    mdFile = os.path.join(des, "README.md")
                    htmlFile = os.path.join(des, "README.html")
                    print(repoRoot)
                    cssFile = os.path.join("", "gfmanual.css")
                    cssFilePath = os.path.join("../"*depth, "gfmanual.css")
                    
                    if(os.path.exists(cssFile)):
                        call(["pandoc", "-s", "-c", cssFile,"--metadata", "title=GFriend Manual", "-f","gfm","-t","html",mdFile,"-o",htmlFile])
                    else:
                        call(["pandoc", "-s", "-c", "../"*depth +"gfmanual.css","--metadata", "title=GFriend Manual", "-f","gfm","-t","html",mdFile,"-o",htmlFile])
                    
                    os.remove(mdFile)
                    with fileinput.FileInput(htmlFile, inplace=True) as file:
                        for line in file:
                            print(line.replace("README.md", "README.html"), end='')
                #print (os.path.join(root, filename))

def add_keyword_documentation():
    print ('')
    print(':::: Adding keyword documantaion link to manual ::::')
    # Generate Keyword documetation
    gfRunner = os.path.join(Path(outputDir).parent, "GF_Runner.exe")
    call([gfRunner, "-k"])
    
    # Copy keyword document to manual and add to link to maual main page
    destination = os.path.join(outputDir, "KeywordDoc")
    mkdir_p(destination)

    regex = re.compile('GF_Keywords_.*\.html')
    for root, directories, filenames in os.walk(os.getcwd()):
        with open(os.path.join(outputDir, "README.html"), "a") as mainPage:
            mainPage.write("<h2>Keyword Documents</h2>\n")
            for filename in filenames:
                if re.match(regex, filename):
                    src = os.path.join(root, filename)
                    des = os.path.join(destination, filename)
                    realative = des.replace(outputDir, '')[1:].replace('\\', '/')
                    displayName = filename.replace('GF_Keywords_','').replace('.html','')
                    shutil.move(src, des)
                    mainPage.write("<p><a href=\"" + realative +"\">" + displayName + "</a></p>\n")

        

def main():
    args = sys.argv[1:]
    global repoRoot
    global outputDir
    if not args:
        print('usage: python GenerateManual.py [RepositoryRoot] [OutputDir]')
        print('use default setting!')
        
    else:
        repoRoot = args[0]
        outputDir = args[1]
    
    repoRoot = os.path.abspath(repoRoot)
    outputDir = os.path.abspath(outputDir)
    
    generate_documents()
    add_keyword_documentation()
# Main body
if __name__ == '__main__':
    main()

