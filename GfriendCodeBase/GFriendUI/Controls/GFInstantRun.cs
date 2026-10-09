using FastColoredTextBoxNS;
using HP.GFriend.Core;
using HP.GFriend.Core.Execution;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace HP.GFriend.UI.Controls
{
    public partial class GFInstantRun : UserControl
    {
        public bool IsConnected { get; private set; } = false;

        private TestDataManager _testDataManager;
        private string _textRunKeywordPattern;
        private string _scriptRoot;
        private DynamicCollection _dynamicCollection;
        
        private DeviceUnderTest _textRunDut;
        private List<DeviceUnderTest> _allDuts;

        private string _textRunOutputDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "output", "textRun");

        private static string PatternLibrary = null;
        private static string PatternKeyword = null;
        private static string PatternVariable = null;
        private static string PatternControl = null;
        private static string PatternXPathArg = null;
        private static string PatternComment = null;

        private Executor _executor;

        public GFInstantRun()
        {
            InitializeComponent();
            checkedListBoxLibraries.Items.AddRange(LibraryUtils.GetAvailbleLibraryNames().ToArray());
        }

        public GFInstantRun(string scriptRoot)
        {
            InitializeComponent();
            _scriptRoot = scriptRoot;
            checkedListBoxLibraries.Items.AddRange(LibraryUtils.GetAvailbleLibraryNames(scriptRoot).ToArray());
        }

        #region PublicInterfaces
        public void SetDeviceList(List<string> dutList)
        {
            comboBoxDeviceList.DataSource = dutList;
        }
        public ComboBox.ObjectCollection GetDeviceList()
        {
            return comboBoxDeviceList.Items;
        }

        public string GetSelectedDevice()
        {
            return comboBoxDeviceList.SelectedValue.ToString();
        }

        public void SelectDevice(string dut)
        {
            comboBoxDeviceList.SelectedItem = dut;
        }

        public void SetAllDuts(List<DeviceUnderTest> deviceUnderTests)
        {
            _allDuts = deviceUnderTests;
        }
        #endregion

        #region StyleAndDesign
        /// <summary>
        /// Set keyword style at text run field 
        /// </summary>
        private void GenerateKeywordPattern()
        {
            PatternKeyword = string.Empty;

            foreach (Library library in _testDataManager.AllUsedLib)
            {
                foreach (Keyword keyword in library.Keywords)
                {
                    PatternKeyword = string.Join("|", PatternKeyword, keyword.KeywordName, keyword.KeywordName);
                }
            }

            PatternKeyword = "@@@ZZZ@@@" + PatternKeyword;
            PatternKeyword = PatternKeyword.Replace("@@@ZZZ@@@|", "");
        }

        /// <summary>
        /// Build auto complete menu for text editor field
        /// It use dynamic collection for separating "library" and "keyword".
        /// </summary>
        private void BuildAutocompelete()
        {
            try
            {
                // Auto Complete Popup Setting 
                AutocompleteMenu autoComplete = new AutocompleteMenu(textBoxRun);

                autoComplete.MinFragmentLength = 1;
                autoComplete.Items.MaximumSize = new System.Drawing.Size(300, 400);
                autoComplete.Items.Width = 300;
                autoComplete.ForeColor = Color.Black;
                autoComplete.BackColor = Color.White;
                autoComplete.SelectedColor = Color.Gray;
                autoComplete.AllowTabKey = true;
                autoComplete.Refresh();

                _dynamicCollection = new DynamicCollection(autoComplete, textBoxRun, _testDataManager);
                autoComplete.Items.SetAutocompleteItems(_dynamicCollection);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Events


        /// <summary>
        /// Click connect button on text run
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextRun_Connect_button_Click(object sender, EventArgs e)
        {
            if (!IsConnected)
            {
                Cursor.Current = Cursors.WaitCursor;
                ConnectTextRun();
                Cursor.Current = Cursors.Default;
            }
            else
            {
                DisconnectTextRun();
            }
        }


        /// <summary>
        /// Click event for textRun_run_button_Click button
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextRun_run_button_Click(object sender, EventArgs e)
        {
            ExecuteTextRun(textBoxRun.Text);
        }

        private void ClearTextRunOutput_button_Click(object sender, EventArgs e)
        {
            DialogResult dr = new DialogResult();
            dr = MessageBox.Show($"Do you want to clear all text on output textbox?", "Clear Output Textbox", MessageBoxButtons.YesNoCancel);

            switch (dr)
            {
                case DialogResult.Yes:
                    gfOutput.Clear();
                    break;
                case DialogResult.Cancel:
                    return;
            }
        }


        private void GFInstantRun_Click(object sender, EventArgs e)
        {
            MessageBox.Show("clicked");
        }

        private void LabelLibrariesSelection_Click(object sender, EventArgs e)
        {
            checkedListBoxLibraries.Visible = true;
            checkedListBoxLibraries.Select();
        }

        private void CheckedListBoxLibraries_Leave(object sender, EventArgs e)
        {
            checkedListBoxLibraries.Visible = false;
        }

        /// <summary>
        /// Text changed event on text run field
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        private void TextRun_fastColoredTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (PatternKeyword != null || PatternLibrary != null)
                {
                    // Apply Text Style
                    e.ChangedRange.ClearStyle(GFStyles.StyleLibrary);
                    e.ChangedRange.ClearStyle(GFStyles.StyleKeyword);
                    e.ChangedRange.ClearStyle(GFStyles.StyleControls);
                    e.ChangedRange.ClearStyle(GFStyles.StyleVariable);
                    e.ChangedRange.ClearStyle(GFStyles.StyleComments);


                    e.ChangedRange.SetStyle(GFStyles.StyleKeyword, PatternXPathArg);
                    e.ChangedRange.SetStyle(GFStyles.StyleComments, PatternComment);
                    e.ChangedRange.SetStyle(GFStyles.StyleKeyword, PatternKeyword, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    e.ChangedRange.SetStyle(GFStyles.StyleLibrary, PatternLibrary, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    e.ChangedRange.SetStyle(GFStyles.StyleControls, PatternControl);
                    e.ChangedRange.SetStyle(GFStyles.StyleVariable, PatternVariable, System.Text.RegularExpressions.RegexOptions.IgnoreCase);



                    // Apply Block
                    e.ChangedRange.ClearFoldingMarkers();
                    e.ChangedRange.SetFoldingMarkers("{", "}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Selected Index Changed event for textRun_DeviceList_comboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextRun_DeviceList_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDeviceList.Items.Count > 0)
            {
                var deviceId = comboBoxDeviceList.SelectedValue.ToString();
            }
        }





        /// <summary>
        /// Text changed event on text run output field
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        private void TextRunOutput_fastColoredTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                gfOutput.SelectionStart = gfOutput.Text.Length;
                gfOutput.GoEnd();

                if (PatternKeyword != null)
                {
                    // Apply Text Style
                    e.ChangedRange.ClearStyle(GFStyles.StylePass);
                    e.ChangedRange.ClearStyle(GFStyles.StyleFail);
                    e.ChangedRange.ClearStyle(GFStyles.StyleHyperLink);

                    e.ChangedRange.SetStyle(GFStyles.StylePass, "Pass", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    e.ChangedRange.SetStyle(GFStyles.StyleFail, "Fail", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    e.ChangedRange.SetStyle(GFStyles.StyleError, "Error", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    e.ChangedRange.SetStyle(GFStyles.StyleHyperLink, "[a-zA-Z]:.*output.xml|[a-zA-Z]:.*report.html|[a-zA-Z]:.*log.txt");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Keydown event for text run field
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextRun_fastColoredTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter && e.Alt))
            {
                ExecuteTextRun(textBoxRun.Text);
            }
        }


        #endregion

        #region Execution


        /// <summary>
        /// Connect for text run test
        /// </summary>
        private void ConnectTextRun()
        {
            try
            {
                string deviceId = comboBoxDeviceList.SelectedValue.ToString();
                _textRunDut = _allDuts.Where(d => d.DeviceId.Equals(deviceId)).FirstOrDefault();

                EnableConnectTextRun();
                GFriendLoggerServices.InitLogger(_textRunOutputDir);
                _testDataManager = new TestDataManager();
                

                foreach(string lib in checkedListBoxLibraries.CheckedItems)
                {
                    string line = $"using {lib}";
                    Parser.ParseUsing(line, _textRunOutputDir, _testDataManager, null);
                }
                _testDataManager.GenerateAllUsedLibs();
                foreach (Library lib in _testDataManager.AllUsedLib)
                {
                    lib.Load(lib.NameAs);
                }
                _testDataManager.ChangeDefaultDUTId(deviceId);
                _executor = new Executor(_allDuts, _testDataManager);
                _executor.InitExecutor(_textRunOutputDir);

                GenerateKeywordPattern();
                BuildAutocompelete();
                Thread textRunThread = new Thread(() => ConnectSafeExecute(() => ThreadConnectTextRun(), ThreadConnectExceptionHandler));
                textRunThread.Start();
            }
            catch (Exception ex)
            {
                gfOutput.AppendText($"\n### Text Run: Connection aborted by exception. ###\n");
                Logger.Trace("### Text Run: Connection aborted by exception ###");
                Logger.Error(ex);

                _executor?.Dispose();
                GFriendLoggerServices.Dispose();

                EnableDisconnectTextRun();
                IsConnected = false;
            }
        }

        /// <summary>
        /// Thread for connect text run
        /// </summary>
        private void ThreadConnectTextRun()
        {

            gfOutput.AppendText($"### Text Run: [{_textRunDut.DeviceAddress}] Connect ###\r\n");
            Logger.Trace($"### Text Run: [{_textRunDut.DeviceAddress}] Connect ###\r\n");

            IsConnected = true;

            EnableAfterConnectTextRun();

            textBoxRun.Select();
        }

        /// <summary>
        /// Get Exception from connect thread exception
        /// </summary>
        private void ConnectSafeExecute(Action connectTextRun, Action<Exception> handler)
        {
            try
            {
                connectTextRun.Invoke();
            }
            catch (Exception ex)
            {
                ThreadConnectExceptionHandler(ex);
            }
        }

        /// <summary>
        /// Exception Handler for connect text run
        /// </summary>
        private void ThreadConnectExceptionHandler(Exception exception)
        {
            gfOutput.AppendText($"\n### Text Run: Connection aborted by exception. ###\n");
            Logger.Trace("### Text Run: Connection aborted by exception ###");
            Logger.Error(exception);

            _executor?.Dispose();
            GFriendLoggerServices.Dispose();

            EnableDisconnectTextRun();

            IsConnected = false;
        }

        /// <summary>
        /// Disconnect for text run test
        /// </summary>
        public void DisconnectTextRun()
        {
            EnableConnectTextRun();
            _executor?.Dispose();
            gfOutput.AppendText($"### Text Run: [{_textRunDut.DeviceAddress}] Disconnect ###\r\n");
            Logger.Trace($"### Text Run: [{_textRunDut.DeviceAddress}] Disconnect ###\r\n");
            buttonConnect.Image = Properties.Resources.Connect_16x;

            EnableDisconnectTextRun();

            IsConnected = false;
        }

        /// <summary>
        /// Execute text run
        /// </summary>        
        /// <param name="text">Run test by text run command</param>        
        private void ExecuteTextRun(string text)
        {
            if (!IsConnected)
            {
                MessageBox.Show("Connect to device before test");
                return;
            }
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please input command for test");
                textBoxRun.Select();
                return;
            }
            textBoxRun.Enabled = false;
            buttonRun.Enabled = false;

            Thread textRunThread = new Thread(() => ExecuteSafeExecute(() => ThreadExcuteTextRun(text), ThreadExecuteExceptionHandler));
            textRunThread.Start();
        }

        /// <summary>
        /// Thread for execute text run
        /// </summary
        private void ThreadExcuteTextRun(string text)
        {
            using (TextBoxStreamWriter streamTextBoxWriter = new TextBoxStreamWriter(gfOutput))
            {
                System.Console.SetOut(streamTextBoxWriter);
                // FIX: Executor.Execute is an instance method, not static, and expects (Statement, int, int, Dictionary<string, string>)
                // Use the instance _executor and pass only the Statement, or use overloads as needed.
                _executor.Execute(Parser.ParseStatemet(text));
            }

            textBoxRun.Enabled = true;
            buttonRun.Enabled = true;
        }

        /// <summary>
        /// Get Exception from execute thread exception
        /// </summary>
        private void ExecuteSafeExecute(Action executeTextRun, Action<Exception> handler)
        {
            try
            {
                executeTextRun.Invoke();
            }
            catch (Exception ex)
            {
                ThreadExecuteExceptionHandler(ex);
            }
        }

        /// <summary>
        /// Exception Handler for Connect text run
        /// </summary>
        private void ThreadExecuteExceptionHandler(Exception exception)
        {
            gfOutput.AppendText("\nText Run: Command is not executed with exception\n" + exception.Message.ToString());
            Logger.Trace("Text Run: Command is not executed with exception");
            Logger.Error(exception);

            textBoxRun.Enabled = true;
            buttonRun.Enabled = true;
        }


        #endregion

        #region EnableDisableControls

        /// <summary>
        /// Update Enabled control for connect text run
        /// </summary>
        private void EnableConnectTextRun()
        {
            buttonConnect.Enabled = false;
            comboBoxDeviceList.Enabled = false;
            labelLibrariesSelection.Enabled = false;
            //this.tcStart_ToolStripButton.Enabled = false;
            //this.tsStart_toolStripButton.Enabled = false;
        }

        /// <summary>
        /// Update Enabled control after connecting text run
        /// </summary>
        private void EnableAfterConnectTextRun()
        {
            buttonConnect.Enabled = true;
            buttonConnect.Image = Properties.Resources.Disconnect_16x;

            textBoxRun.Enabled = true;
            buttonRun.Enabled = true;
            //tcStart_ToolStripButton.Enabled = true;
            //tsStart_toolStripButton.Enabled = true;
        }

        /// <summary>
        /// Update Enabled control for disconnect text run
        /// </summary>
        private void EnableDisconnectTextRun()
        {
            buttonConnect.Enabled = true;
            comboBoxDeviceList.Enabled = true;
            labelLibrariesSelection.Enabled = true;
            textBoxRun.Enabled = false;
            buttonRun.Enabled = false;
            //this.tcStart_ToolStripButton.Enabled = true;
            //this.tsStart_toolStripButton.Enabled = true;
        }


        #endregion

    }
}
