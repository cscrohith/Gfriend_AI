using FastColoredTextBoxNS;
using HP.GFriend.Core;
using HP.GFriend.Core.Execution;
using HP.GFriend.GFLogger;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace HP.GFriend.UI.Controls
{
    internal class GFScriptTabPage : TabPage
    {
        public TestDataManager TabTestDataManager;
        public FastColoredTextBox TextEditor { get; set; }
        public bool IsUpdated { get; set; }
        public bool IsAdded { get; set; }

        // Patterns
        private static string PatternLibrary = null;
        private static string PatternKeyword = null;
        private static string PatternVariable = null;
        private static string PatternControl = null;
        private static string PatternXPathArg = null;
        private static string PatternComment = null;
        private static string PatternMetadata = null;

        // Constants
        public static readonly string USING_TEXT = "using ";

        // Event
        public event EventHandler<TextChangedEventArgs> OnTextUpdate;

        // Private Variables
        private DynamicCollection _dynamicCollection;
        private Dictionary<string, string> _controlPatternDic = new Dictionary<string, string>();
        private EventHandler<TextChangedEventArgs> _usingParserHandler = null;
        private static int _usingLineNum = -1;

        

        public new void Dispose()
        {
            base.Dispose();
            base.Dispose(true);

            TabTestDataManager = null;
        }

        public GFScriptTabPage(string text, bool isUpdate) : base(text)
        {
            IsUpdated = isUpdate;
            IsAdded = false;
            TabTestDataManager = new TestDataManager();
            TextEditor = new FastColoredTextBox();
            TextEditor.Dock = DockStyle.Fill;
            TextEditor.DefaultStyle = (TextStyle)GFStyles.StyleDefault;
            TextEditor.ImeMode = ImeMode.Hangul;
            TextEditor.WordWrap = true;
            TextEditor.WordWrapAutoIndent = true;
            SuspendLayout();
            Controls.Add(TextEditor);
            ResumeLayout();
            BuildAutoCompleteMenu();
            TextEditor.TextChanged += new EventHandler<TextChangedEventArgs>(TextChanged_On_Editor);
        }

        #region Initilize
        /// <summary>
        /// Build auto complete menu for text editor field
        /// It use dynamic collection for separating "using XXX", "library" and "keyword".
        /// </summary>
        internal void BuildAutoCompleteMenu()
        {
            try
            {
                // Auto Complete Popup Setting
                AutocompleteMenu autoComplete = new AutocompleteMenu(TextEditor);

                autoComplete.MinFragmentLength = 1;
                autoComplete.Items.MaximumSize = new System.Drawing.Size(300, 400);
                autoComplete.Items.Width = 300;
                autoComplete.ForeColor = Color.Black;
                autoComplete.BackColor = Color.White;
                autoComplete.SelectedColor = Color.Gray;
                autoComplete.AllowTabKey = true;
                _dynamicCollection = new DynamicCollection(autoComplete, TextEditor, TabTestDataManager, _controlPatternDic);
                autoComplete.Items.SetAutocompleteItems(_dynamicCollection);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        internal void InitStyles()
        {
            if (_controlPatternDic != null)
            {
                _controlPatternDic.Clear();
            }
            PatternLibrary = string.Empty;

            foreach (string library in LibraryUtils.GetAvailbleLibraryNames(Path.GetDirectoryName(TabTestDataManager.TestSuitePath)))
            {
                PatternLibrary = string.Join("|", PatternLibrary, $"(?<![\\w\\d]){library}(?![\\w\\d])");

                if (!library.Equals("BuiltIn"))
                {
                    _controlPatternDic.Add($"using {library}^", $"Load the keywords in {library}");
                }

            }

            TabTestDataManager.GenerateAllUsedLibs();
            GenerateKeywordPattern();

            PatternLibrary = "@@@YYY@@@" + PatternLibrary;
            PatternLibrary = PatternLibrary.Replace("@@@YYY@@@|", "");
            PatternVariable = @"\$\{.*\}";
            PatternControl = "Repeat(:[0-9]+)*|using |Using |resource |Resource |If:|(Fail:)|Error:|While:| as | As | with | With |DataSet |dataset |Dataset |For Each Row:|For each row:|for each row:|For:|Remote Run:|Remote run:|remote run|For Selected Row:|For selected row:|for selected row:|Spread Sheet For Each Row:|Spread sheet for each row:|spread sheet for each row:";
            PatternXPathArg = @"\(.{0,}(//.{0,}).{0,}\)";
            PatternComment = @"//.{0,}";
            PatternMetadata = @"///.{0,}";

            _controlPatternDic.Add("Repeat:^\n{\n\n}", $"Repeat:\'#num\': Repeat keywords \'#num\' times");
            _controlPatternDic.Add("For:^\n{\n\n}", $"For:\'#List\'");
            _controlPatternDic.Add("For Each Row:^\n{\n\n}", $"For Each Row:\'DataSet.Sheet.TableName\'");
            _controlPatternDic.Add("Spread Sheet For Each Row:^\n{\n\n}", $"Spread Sheet For Each Row:\'DataSet.Sheet.TableName\'");
            _controlPatternDic.Add("Remote Run:^\n{\n\n}", $"Remote Run:\'Remote Executor ID\'");
            _controlPatternDic.Add("If: ^\n{\n\n}", $"if:\'#keyword\' : if #keyword is pass exeute \'if block\', if fail execute \'fail block' and if error execute \'error block\'");
            _controlPatternDic.Add("Fail:\n{\n^\n}", $"fail: Fail block declaration to run if fail");
            _controlPatternDic.Add("Error:\n{\n^\n}", $"Error: Error block declaration to run if error");
            _controlPatternDic.Add("While:^: {\n\n}", $"While:\'#keyword\':MaxLoop");
            _controlPatternDic.Add("For selected Row:^\n{\n\n}", $"For selected Row:\'DataSet.Sheet.TableName.ColumnValues\'");
        }

        internal void InitStylesForCoDeveloperMode()
        {
            if (_controlPatternDic != null)
            {
                _controlPatternDic.Clear();
            }
            PatternLibrary = string.Empty;

            foreach (string library in LibraryUtils.GetAvailbleLibraryNames(Path.GetDirectoryName(TabTestDataManager.TestSuitePath)))
            {
                PatternLibrary = string.Join("|", PatternLibrary, $"(?<![\\w\\d]){library}(?![\\w\\d])");

                if (!library.Equals("BuiltIn"))
                {
                    _controlPatternDic.Add($"using {library}^", $"Load the keywords in {library}");
                }

            }

            TabTestDataManager.GenerateAllUsedLibs();
            GenerateKeywordPattern();

            PatternLibrary = "@@@YYY@@@" + PatternLibrary;
            PatternLibrary = PatternLibrary.Replace("@@@YYY@@@|", "");
            PatternVariable = @"\$\{.*\}";
            PatternControl = "Repeat(:[0-9]+)*|using |Using |resource |Resource |If:|(Fail:)|Error:|While:| as | As | with | With |DataSet |dataset |Dataset |For Each Row:|For each row:|for each row:|For:|Remote Run:|Remote run:|remote run|For Selected Row:|For selected row:|for selected row:|Spread Sheet For Each Row:|Spread sheet for each row:|spread sheet for each row:";
            PatternXPathArg = @"\(.{0,}(//.{0,}).{0,}\)";
            PatternComment = @"//.{0,}";
            PatternMetadata = @"///.{0,}";

            MainForm form = new MainForm();
            Dictionary<string, string> keywordData = form.GetLoopData("Repeat".ToLower());
            _controlPatternDic.Add("Repeat\n//Please comment above line\n\n" + keywordData["script"].Replace("\r","") + "^", $"Repeat:\'#num\': Repeat keywords \'#num\' times");

            keywordData = form.GetLoopData("For".ToLower());
            _controlPatternDic.Add("For\n//Please comment above line\n\n" + keywordData["script"].Replace("\r", "") + "^", $"For:\'#List\'");

            keywordData = form.GetLoopData("For Each Row".ToLower());
            _controlPatternDic.Add("For Each Row\n//Please comment above line\n\n" + keywordData["script"].Replace("\r", "") + "^", $"For Each Row:\'DataSet.Sheet.TableName\'");

            keywordData = form.GetLoopData("Spread Sheet For Each Row".ToLower());
            _controlPatternDic.Add("Spread Sheet For Each Row\n//Please comment above line\n\n" + keywordData["script"].Replace("\r", "") + "^", $"Spread Sheet For Each Row:\'DataSet.Sheet.TableName\'");

            keywordData = form.GetLoopData("Remote Run".ToLower());
            _controlPatternDic.Add("Remote Run\n//Please comment above line\n\n" + keywordData["script"].Replace("\r", "") + "^", $"Remote Run:\'Remote Executor ID\'");

            keywordData = form.GetLoopData("If".ToLower());
            _controlPatternDic.Add("If\n//Please comment above line\n\n" + keywordData["script"].Replace("\r", "") + "^", $"if:\'#keyword\' : if #keyword is pass exeute \'if block\', if fail execute \'fail block' and if error execute \'error block\'");

            keywordData = form.GetLoopData("Fail".ToLower());
            _controlPatternDic.Add("Fail\n//Please comment above line\n\n" + keywordData["script"].Replace("\r", "") + "^", $"fail: Fail block declaration to run if fail");

            keywordData = form.GetLoopData("Error".ToLower());
            _controlPatternDic.Add("Error\n//Please comment above line\n\n" + keywordData["script"].Replace("\r", "") + "^", $"Error: Error block declaration to run if error");

            keywordData = form.GetLoopData("While".ToLower());
            _controlPatternDic.Add("While\n//Please comment above line\n\n" + keywordData["script"].Replace("\r", "") + "^", $"While:\'#keyword\':MaxLoop");

            keywordData = form.GetLoopData("For selected Row".ToLower());
            _controlPatternDic.Add("For Selected Row\n//Please comment above line\n\n" + keywordData["script"].Replace("\r", "") + "^", $"For selected Row:\'DataSet.Sheet.TableName.ColumnValues\'");
        }

        /// <summary>
        /// Set keyword style at editor text field
        /// </summary>        
        private void GenerateKeywordPattern()
        {
            PatternKeyword = string.Empty;

            foreach (Library library in TabTestDataManager.AllUsedLib)
            {
                library.Load(library.NameAs);
                foreach (Keyword keyword in library.Keywords)
                {
                    PatternKeyword = string.Join("|", PatternKeyword, keyword.KeywordName, keyword.KeywordName);
                }
            }

            PatternKeyword = "@@@ZZZ@@@" + PatternKeyword;
            PatternKeyword = PatternKeyword.Replace("@@@ZZZ@@@|", "");
        }

        /// <summary>
        /// Set keyword style at editor text field for selected library
        /// </summary>        
        /// <param name="library">Selected library</param>
        private void GenerateKeywordPattern(string libraryName)
        {
            PatternKeyword = string.Empty;
            Library library = LibraryUtils.GetAvailableLibraries(Path.GetDirectoryName(TabTestDataManager.TestSuitePath))[libraryName];

            library.Load(library.NameAs);

            foreach (Keyword keyword in library.Keywords)
            {
                PatternKeyword = string.Join("|", PatternKeyword, keyword.KeywordName, keyword.KeywordName);
            }


            PatternKeyword = "@@@ZZZ@@@" + PatternKeyword;
            PatternKeyword = PatternKeyword.Replace("@@@ZZZ@@@|", "");
        }

    

        #endregion


        #region EventHandler



        /// <summary>
        /// Text Changed on text editor
        /// </summary>
        private void TextChanged_On_Editor(object sender, TextChangedEventArgs e)
        {
            //int lineNum = TextEditor.Selection.ToLine;

            for (int lineNum = e.ChangedRange.FromLine; lineNum <= e.ChangedRange.ToLine; lineNum++)
            {
                string lineText = TextEditor.GetLineText(lineNum);

                // In case of TC description
                if (lineText.TrimStart().Equals("/// "))
                {
                    int line = e.ChangedRange.ToLine;
                    int previousLine = line - 1;
                    string previousLineText = previousLine < 0 ? string.Empty : TextEditor.GetLineText(previousLine)?.Trim();
                    if (!previousLineText.StartsWith("/// "))
                    {

                        string toInsert = string.Empty;
                        if (e.ChangedRange.ToLine == 0)
                        {
                            toInsert = MainForm.OverallDescriptionTemplate;
                        }
                        else if (this.Text.Trim().EndsWith(".gflib"))
                        {
                            toInsert = MainForm.LibraryDescriptionTemplate;
                        }
                        else
                        {
                            toInsert = MainForm.ScriptDescriptonTemplate;
                        }
                        TextEditor.InsertText(toInsert, false);
                        lineText = TextEditor.GetLineText(lineNum);
                        int cursorPos = lineText.IndexOf('>') + 1;
                        TextEditor.Selection = new Range(TextEditor, cursorPos, lineNum, cursorPos, lineNum);
                    }
                }

                else if (lineText.ToLower().StartsWith(USING_TEXT))
                {
                    if (_usingParserHandler == null)
                    {
                        _usingLineNum = lineNum;
                        _usingParserHandler = new EventHandler<TextChangedEventArgs>(UsingMonitoring_On_Editor);
                        TextEditor.TextChanged += _usingParserHandler;
                    }
                }

            }

            // Apply Style
            if (IsAdded)
            {
                IsUpdated = true;
                OnTextUpdate?.Invoke(this, e);
            }
            
            try
            {
                // Apply Text Style

                e.ChangedRange.ClearStyle(GFStyles.StyleLibrary);
                e.ChangedRange.ClearStyle(GFStyles.StyleKeyword);
                e.ChangedRange.ClearStyle(GFStyles.StyleControls);
                e.ChangedRange.ClearStyle(GFStyles.StyleVariable);
                e.ChangedRange.ClearStyle(GFStyles.StyleMetadata);
                e.ChangedRange.ClearStyle(GFStyles.StyleComments);


                e.ChangedRange.SetStyle(GFStyles.StyleKeyword, PatternXPathArg);
                e.ChangedRange.SetStyle(GFStyles.StyleMetadata, PatternMetadata);
                e.ChangedRange.SetStyle(GFStyles.StyleComments, PatternComment);
                e.ChangedRange.SetStyle(GFStyles.StyleKeyword, PatternKeyword, RegexOptions.IgnoreCase);
                e.ChangedRange.SetStyle(GFStyles.StyleControls, PatternControl);

                e.ChangedRange.SetStyle(GFStyles.StyleVariable, PatternVariable, RegexOptions.IgnoreCase);
                e.ChangedRange.SetStyle(GFStyles.StyleLibrary, PatternLibrary, RegexOptions.IgnoreCase);

                // Apply Block
                e.ChangedRange.ClearFoldingMarkers();
                e.ChangedRange.SetFoldingMarkers("{", "}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Text Changed on text editor
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        private void UsingMonitoring_On_Editor(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TextEditor.Selection.ToLine != _usingLineNum)
                {
                    string lineText = TextEditor.GetLineText(_usingLineNum);
                    Parser.ParseUsing(lineText, Name, TabTestDataManager);
                    TabTestDataManager.GenerateAllUsedLibs();
                    TextEditor.TextChanged -= _usingParserHandler;
                    _usingParserHandler = null;

                    if (TextEditor.GetLineText(TextEditor.Selection.ToLine).ToLower().StartsWith(USING_TEXT))
                    {
                        _usingLineNum = TextEditor.Selection.ToLine;
                        _usingParserHandler = new EventHandler<TextChangedEventArgs>(UsingMonitoring_On_Editor);
                        TextEditor.TextChanged += _usingParserHandler;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"### Fail to parse using statement: {ex.Message}");
                _usingParserHandler = null;
            }
        }
        #endregion

        #region GeneralMethods
        /// <summary>
        /// Get TC list for TC run. Returns entries in strict script declaration order
        /// (top-to-bottom scan of the file), which is the same order the Parser assigns
        /// stable <see cref="HP.GFriend.Core.Execution.TestCase.Id"/> ordinals in. Callers
        /// must not resort or reorder this list if they intend to correlate it with TestCase.Id.
        /// </summary>        
        internal List<KeyValuePair<int, string>> GetTCList()
        {
            List<KeyValuePair<int, string>> tcList = new List<KeyValuePair<int, string>>();
            int level = 0;
            int lineNum = 1;
            string line;
            bool tcOpened = false;
            string tcName = null;
            int tcLine = -1;

            using (StringReader reader = new StringReader(TextEditor.Text))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrEmpty(line) && !line.StartsWith("$") && !line.StartsWith("{") && !line.StartsWith("}") && !line.StartsWith("//") && level == 0)
                    {
                        tcName = line.Replace("{", "").Replace("}", "");
                        tcName = Regex.Replace(tcName, PatternComment, string.Empty);
                        tcName = tcName.Trim();
                        tcOpened = true;
                        tcLine = lineNum;
                    }
                    if (line.Contains("{"))
                    {
                        level++;
                    }
                    if (line.Contains("}"))
                    {
                        level--;
                        if (level == 0 && tcOpened && tcName != null)
                        {
                            tcOpened = false;

                            // Match Parser.ParseTestSuite's header handling exactly: "using",
                            // "resource" and "dataset" declaration lines are consumed as script
                            // header/metadata and never become an actual TestCase - so they must
                            // never be added here either. Otherwise these header lines would be
                            // counted as extra rows in the UI list, shifting every subsequent
                            // ordinal Id out of sync with the real TestCase.Id the Parser assigns.
                            string tcNameLower = tcName.ToLowerInvariant();
                            if (tcNameLower.StartsWith("using ") ||
                                tcNameLower.StartsWith("resource ") ||
                                tcNameLower.StartsWith("dataset "))
                            { }
                            else
                            {
                                tcList.Add(new KeyValuePair<int, string>(tcLine, tcName));
                                tcName = null;

                            }
                        }
                    }
                    lineNum++;
                }
            }

            return tcList;
        }
        #endregion

        #region LibraryManagement
        /// <summary>
        /// Load library from text editor
        /// </summary>
        internal void LoadLibrary()
        {
            int lineCount = TextEditor.LinesCount;

            for (int line = 0; line < lineCount; line++)
            {
                var lineText = TextEditor.GetLineText(line);

                if (lineText.ToLower().StartsWith(USING_TEXT))
                {
                    var partialText = lineText.Split(' ');

                    if (lineText.Length < 2)
                    {
                        continue;
                    }

                    else if (lineText.Length == 2)
                    {
                        var libraryName = partialText[partialText.Length - 1];

                        if (LibraryUtils.GetAvailbleLibraryNames(Path.GetDirectoryName(TabTestDataManager.TestSuitePath)).Contains(libraryName) && !TabTestDataManager.AllUsedLib.Exists(r => r.Name == libraryName))
                        {
                            LoadLibrary(libraryName);
                        }
                    }

                    try
                    {
                        Parser.ParseUsing(lineText, Name, TabTestDataManager);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"### Fail to load library from {lineText}");
                        continue;
                    }
                }
            }
            TabTestDataManager.GenerateAllUsedLibs();
            GenerateKeywordPattern();
        }

        /// <summary>
        /// Load library from using command
        /// </summary>
        internal void LoadLibrary(string libraryName)
        {
            Library library = LibraryUtils.GetAvailableLibraries(Path.GetDirectoryName(TabTestDataManager.TestSuitePath))[libraryName];
            library.Load(libraryName);
            GenerateKeywordPattern(libraryName);
        }

        internal void ApplyStyles()
        {
            TextEditor.Range.ClearStyle(GFStyles.StyleLibrary);
            TextEditor.Range.ClearStyle(GFStyles.StyleKeyword);
            TextEditor.Range.ClearStyle(GFStyles.StyleControls);
            TextEditor.Range.ClearStyle(GFStyles.StyleVariable);
            TextEditor.Range.ClearStyle(GFStyles.StyleMetadata);
            TextEditor.Range.ClearStyle(GFStyles.StyleComments);

            
            TextEditor.Range.SetStyle(GFStyles.StyleKeyword, PatternXPathArg);
            TextEditor.Range.SetStyle(GFStyles.StyleMetadata, PatternMetadata);
            TextEditor.Range.SetStyle(GFStyles.StyleComments, PatternComment);
            TextEditor.Range.SetStyle(GFStyles.StyleKeyword, PatternKeyword, RegexOptions.IgnoreCase);
            TextEditor.Range.SetStyle(GFStyles.StyleControls, PatternControl);

            TextEditor.Range.SetStyle(GFStyles.StyleVariable, PatternVariable, RegexOptions.IgnoreCase);
            TextEditor.Range.SetStyle(GFStyles.StyleLibrary, PatternLibrary, RegexOptions.IgnoreCase);

            // Apply Block
            TextEditor.Range.ClearFoldingMarkers();
            TextEditor.Range.SetFoldingMarkers("{", "}");
        }
        #endregion
    }
}
