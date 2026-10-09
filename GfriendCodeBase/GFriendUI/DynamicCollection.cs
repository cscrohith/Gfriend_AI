using FastColoredTextBoxNS;
using HP.GFriend.Core;
using HP.GFriend.Core.Execution;
using HP.GFriend.Keywords;
using System.Collections.Generic;
using System.Linq;

namespace HP.GFriend.UI
{
    /// <summary>
    /// Builds list of methods and properties for current class name was typed in the textbox
    /// </summary>
    internal class DynamicCollection : IEnumerable<AutocompleteItem>
    {
        public bool usingCheck;
        private Dictionary<string, string> _controlPatternDic;
        private AutocompleteMenu _autoCompleteMenu;
        private FastColoredTextBox _textEditor;
        private TestDataManager _testDataManager;
        BuiltInLibrary builtIn;

        public DynamicCollection(AutocompleteMenu menu, FastColoredTextBox tb, TestDataManager testDataManager)
        {
            _autoCompleteMenu = menu;
            _textEditor = tb;
            _testDataManager = testDataManager;
            builtIn = new BuiltInLibrary();
            usingCheck = true;
        }

        public DynamicCollection(AutocompleteMenu menu, FastColoredTextBox tb, TestDataManager testDataManager, Dictionary<string, string> controlPatternDic)
        {
            _autoCompleteMenu = menu;
            _textEditor = tb;
            _testDataManager = testDataManager;
            builtIn = new BuiltInLibrary();
            usingCheck = true;
            _controlPatternDic = controlPatternDic;
        }

        public IEnumerator<AutocompleteItem> GetEnumerator()
        {
            //get current fragment of the text
            var text = _autoCompleteMenu.Fragment.Text;

            if (_controlPatternDic != null)
            {
                foreach (KeyValuePair<string, string> controlName in _controlPatternDic.OrderBy(i => i.Key))
                {
                    SnippetAutocompleteItem controlAutoCompleteItem = new SnippetAutocompleteItem(controlName.Key);
                    controlAutoCompleteItem.ToolTipTitle = controlName.Key.Replace("^", "").Replace("\n", "");
                    controlAutoCompleteItem.ToolTipText = controlName.Value;
                    yield return controlAutoCompleteItem;
                }
            }

            if (_testDataManager.AllUsedLib != null)
            {
                Library builtInlibrary = _testDataManager.GetLibrary(builtIn.GetName());

                if(string.Equals(HP.GFriend.Core.CommonExecutionInfo.GetVariable(SystemVariables.CO_DEVELOPER_MODE),"true"))
                {
                    foreach (KeyValuePair<string, string> keywordName in builtInlibrary.GetKeywordInfoForAutoCompleteForCoDeveloperMode().OrderBy(i => i.Key))
                    {
                        SnippetAutocompleteItem builtInAutoCompleteItem = new SnippetAutocompleteItem(keywordName.Key);
                        builtInAutoCompleteItem.ToolTipTitle = keywordName.Key.Replace("^", "");
                        builtInAutoCompleteItem.ToolTipText = keywordName.Value;
                        yield return builtInAutoCompleteItem;
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, string> keywordName in builtInlibrary.GetKeywordInfoForAutoComplete().OrderBy(i => i.Key))
                    {
                        SnippetAutocompleteItem builtInAutoCompleteItem = new SnippetAutocompleteItem(keywordName.Key);
                        builtInAutoCompleteItem.ToolTipTitle = keywordName.Key.Replace("^", "");
                        builtInAutoCompleteItem.ToolTipText = keywordName.Value;
                        yield return builtInAutoCompleteItem;
                    }
                }


                List<string> usedLibraryNames = _testDataManager.AllUsedLib.Select(e => e.NameAs).ToList();
                usedLibraryNames = usedLibraryNames.Distinct().ToList();


                if (text.Contains('.'))
                {
                    var parts = text.Split('.');
                    var libraryName = parts[parts.Length - 2];

                    if (usedLibraryNames.Contains(libraryName))
                    {
                        Library library = _testDataManager.GetLibrary(libraryName);
                        library.Load(libraryName);

                        if (string.Equals(HP.GFriend.Core.CommonExecutionInfo.GetVariable(SystemVariables.CO_DEVELOPER_MODE), "true"))
                        {
                            foreach (KeyValuePair<string, string> keywordName in library.GetKeywordInfoForAutoCompleteForCoDeveloperMode().OrderBy(i => i.Key))
                            {
                                SnippetMethodAutocompleteItem keywordAutoCompleteItem = new SnippetMethodAutocompleteItem(keywordName.Key);
                                keywordAutoCompleteItem.ToolTipTitle = keywordName.Key.Replace("^", "");
                                keywordAutoCompleteItem.ToolTipText = keywordName.Value;
                                yield return keywordAutoCompleteItem;
                            }
                        }
                        else
                        {
                            foreach (KeyValuePair<string, string> keywordName in library.GetKeywordInfoForAutoComplete().OrderBy(i => i.Key))
                            {
                                SnippetMethodAutocompleteItem keywordAutoCompleteItem = new SnippetMethodAutocompleteItem(keywordName.Key);
                                keywordAutoCompleteItem.ToolTipTitle = keywordName.Key.Replace("^", "");
                                keywordAutoCompleteItem.ToolTipText = keywordName.Value;
                                yield return keywordAutoCompleteItem;
                            }
                        }
                    }
                }

                foreach (string libraryName in usedLibraryNames)
                {
                    AutocompleteItem libraryAutoCompleteItem = new AutocompleteItem(libraryName);
                    libraryAutoCompleteItem.ToolTipTitle = libraryName;
                    libraryAutoCompleteItem.ToolTipText = libraryName;
                    yield return libraryAutoCompleteItem;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}