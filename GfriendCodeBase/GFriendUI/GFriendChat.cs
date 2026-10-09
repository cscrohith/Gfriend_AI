using HP.GFriend.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace HP.GFriend.UI
{
    public partial class GFriendChat : Form
    {
        private const string MenuMessage = "Menu";
        private const string MoreMessage = "More";
        private const string PlaceholderText = "   Type your message here. eg: Menu | LibraryName | Library.KeywordName | Manual";
        private static int ReqResSpace = 15;
        private string _section;
        private TestDataManager _testDataManager = new TestDataManager();
        private Dictionary<string, Library> _availableLibs;
        private readonly List<System.Windows.Forms.Button> _menuButtonList = new List<System.Windows.Forms.Button>();
        private string _keywordDocument = Path.Combine(Directory.GetCurrentDirectory(), "KeywordPaths.html");
        private readonly string _manualPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "manual\\README.html");
        public System.Windows.Forms.Button _menuButton { get; private set; }
        public bool _isPlaceholderVisible { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GFriendChat"/> class.
        /// Sets up the UI components, displays default messages, loads available libraries, and creates the keyword document.
        /// </summary>
        public GFriendChat()
        {
            InitializeComponent();
            DisplayDefaultMessages();
            _availableLibs = LibraryUtils.GetAvailableLibraries();
            CreateKeywordDocument();
        }
        /// <summary>
        /// Handles the load event of the GFriendChat form.
        /// Resets the vertical scroll position of the message panel to the top.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments associated with the load event.</param>
        private void GFriendChat_load(object sender, EventArgs e)
        {
            messagePanel.VerticalScroll.Value = 0;
        }
        /// <summary>
        /// Handles the user input textbox click event.
        /// Clears the text and changes the text color to black.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        #region userInput

        private void userTextBoxInput_Click(object sender, EventArgs e)
        {
            userInputTextBox.Clear();
            userInputTextBox.ForeColor = Color.Black;
            _isPlaceholderVisible = false;
        }
        /// <summary>
        /// Handles the KeyDown event for the user input text box.
        /// When the Enter key is pressed, it clears the text.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void userInputTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Prevent newline in the TextBox
                e.SuppressKeyPress = true;
                sendButton_Click(sender, e);
            }
        }
        /// <summary>
        /// Handles the Enter event for the user input text box.
        /// If the input is empty or matches the placeholder text, the method exits.
        /// Otherwise, it displays the user message and generates a response, then clears the text box and resets the placeholder.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void userInputTextBox_Enter(object sender, EventArgs e)
        {
            userInputTextBox.BackColor = Color.White;
            userInputTextBox.BorderStyle = BorderStyle.FixedSingle;
            string userInput = userInputTextBox.Text.Trim();
            if (userInput == PlaceholderText.Trim() || string.IsNullOrEmpty(userInput))
            {
                return;
            }
            DisplayUserMessage(userInput);
            DisplayGFriendResponse(userInput);
            userInputTextBox.Clear();
            SetPlaceholder();
        }
        /// <summary>
        /// Handles the LinkClicked event for the text box.
        /// Opens the clicked hyperlink in the default web browser.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments containing the clicked link.</param>
        private void textBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
        /// <summary>
        /// Sets the placeholder text in the user input text box.
        /// Updates the text color, font style, and marks the placeholder as visible.
        /// </summary>
        private void SetPlaceholder()
        {
            userInputTextBox.Text = PlaceholderText;
            userInputTextBox.ForeColor = Color.Gray;
            _isPlaceholderVisible = true;
            userInputTextBox.Font = new System.Drawing.Font("Arial", 9, FontStyle.Regular);
        }
        #endregion
        /// <summary>
        /// Displays default messages like welcome message, GFriend description, and assistance prompt.Also prompts a menu button
        /// </summary>
        private void DisplayDefaultMessages()
        {
            string welcomeMessage = " Hello and welcome to Gfriend!\n";
            string descriptionMessage = "\r\nGFriend is an automated testing tool based on user scripts, supporting web, Windows, android platforms and devices/solutions like JediOmni, Dune, OXPD solutions and more.";
            string assistMessage = " Hello, How can I assist you today?\r\n Please type your query or type “menu” for suggestions.";

            System.Windows.Forms.Panel welcomeMessagePanel = new System.Windows.Forms.Panel
            {
                Location = new Point(20, 20),
                Width = messagePanel.Width,
                AutoSize = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(193, 233, 245),
            };
            var defaultWelcomeTextBox = CreateTextBox(welcomeMessage, new Point(5, 5), 16);

            PictureBox gFriendWelcomeImage = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new System.Drawing.Size(welcomeMessagePanel.ClientSize.Width - 30, 230),
                Location = new Point(20, defaultWelcomeTextBox.Bottom + 10),
            };
            gFriendWelcomeImage.Image = Properties.Resources.Gfriend_Default_Image;

            var descriptionMessageTextbox = CreateTextBox(descriptionMessage, new Point(10, gFriendWelcomeImage.Bottom - 18), 80);
            welcomeMessagePanel.Controls.Add(defaultWelcomeTextBox);
            welcomeMessagePanel.Controls.Add(gFriendWelcomeImage);
            welcomeMessagePanel.Controls.Add(descriptionMessageTextbox);
            messagePanel.Controls.Add(welcomeMessagePanel);

            var assistMessagetextbox = CreateTextBoxAndAddToPanel(assistMessage, "Left", 68, false);
            messagePanel.Controls.Add(assistMessagetextbox);

            CreateButton(MenuMessage, 58, new Point(10, 10), Color.White);
            _menuButton.Click += (s, e) => MenuTextBox_Click(s, e);
            messagePanel.Controls.Add(_menuButton);
            SetPlaceholder();
        }
        /// <summary>
        /// Handles the click event for the menu button.
        /// Removes the menu button from the message panel and displays menu items horizontally.
        /// </summary>
        /// <param name="sender">The source of the event (menu button).</param>
        /// <param name="e">Event arguments.</param>
        #region menu
        private void MenuTextBox_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Button menuButton)
            {
                messagePanel.Controls.Remove(menuButton);
            }
            DisplayMenuItemsHorizontally();
        }
        /// <summary>
        /// Displays menu items in a horizontal layout within a panel.
        /// Initially shows a subset of menu items with an option to view all.
        /// </summary>
        private void DisplayMenuItemsHorizontally()
        {
            string initialMessage = "Here are the libraries for the Gfriend, to click on the particular libraries for opening the document, if click on more will get all the list of libraraies. ";
            string allItemsMessage = "Here are all the list libraries for the Gfriend, to click on the particular libraries for opening the document:";
            int menupanelHeight = 380;
            int menuYPosition = 10;
            int menuXPositionLeft = 5;
            System.Windows.Forms.Panel menuPanel = new System.Windows.Forms.Panel
            {
                Location = new Point(10, menupanelHeight),
                BorderStyle = BorderStyle.None,
                AutoSize = true,
                BackColor = Color.FromArgb(193, 233, 245),
            };
            var intialMessageTextbox = CreateTextBox(initialMessage, new Point(10, 10), 38);
            menuPanel.Controls.Add(intialMessageTextbox);

            // Define menu items           
            var menuItems = new List<string>(new[]
            {
                "Manual", "BuiltIn", "Web", "Windows", "Dune", "JediOmni", "Android", "Vision",
                "Info", "Oxpd", "Telnet", "IOS", "Mac", "LP", "Preparation", "Regex", "Hallasan",
                "Fleet", "BadgeBox", "Rest", "Sirius", "LogInfo", "Json", "Ssh", "MSOffice", "XML"
            });

            // Loop through each menu item and create a new TextBox for each
            var itemsToDisplay = menuItems.Take(10).ToList();
            menuYPosition = menuYPosition + intialMessageTextbox.Height;
            AddMenuItems(itemsToDisplay, menuPanel, ref menuYPosition, menuXPositionLeft);

            // Add the "More" button if there are more items to display       
            var morebutton = CreateButton(MoreMessage, Width = messagePanel.Width + 18, new Point(450, menuYPosition + (3 * ReqResSpace)), Color.FromArgb(193, 233, 245));
            _menuButton.TabStop = false;
            _menuButton.FlatAppearance.BorderSize = 0;
            morebutton.FlatStyle = FlatStyle.Flat;
            morebutton.ForeColor = Color.FromArgb(0, 99, 177);
            morebutton.BackColor = Color.FromArgb(193, 233, 245);

            _menuButton.Click += (s, args) =>
            {
                menuPanel.Controls.Clear();
                int morepanelHeight = 580;
                menuYPosition = 15;
                var moremessagetextbox = CreateTextBox(allItemsMessage, new Point(20, menuYPosition), 35);
                menuPanel.Controls.Add(moremessagetextbox);
                menuPanel.Height = morepanelHeight;
                menuYPosition = menuYPosition + 30;
                AddMenuItems(menuItems, menuPanel, ref menuYPosition, menuXPositionLeft);
                messagePanel.Controls.Add(menuPanel);
                if (menuPanel.Controls.Count > 0)
                {
                    System.Windows.Forms.Control lastControl = menuPanel.Controls[menuPanel.Controls.Count - 1];
                    messagePanel.ScrollControlIntoView(lastControl);
                }
            };
            menuPanel.Controls.Add(_menuButton);
            messagePanel.Controls.Add(menuPanel);
            messagePanel.ScrollControlIntoView(morebutton);
        }
        /// <summary>
        /// Adds menu items as buttons to the specified panel and arranges them in a horizontal layout.
        /// Adjusts button positioning dynamically based on panel width.
        /// </summary>
        /// <param name="menuItems">List of menu item names to be displayed as buttons.</param>
        /// <param name="menuPanel">The panel where the menu buttons will be added.</param>
        /// <param name="menuYPosition">Reference to the Y position to track vertical spacing.</param>
        /// <param name="menuXPositionLeft">The starting X position for placing menu buttons.</param>
        private void AddMenuItems(List<string> menuItems, System.Windows.Forms.Panel menuPanel, ref int menuYPosition, int menuXPositionLeft)
        {
            int menuXposition = menuXPositionLeft;
            int itemcount = 0;
            menuYPosition = menuYPosition + ReqResSpace;
            foreach (var item in menuItems)
            {
                itemcount++;

                // Create a new button for each menu item
                var responseButton = CreateButton(item, 135, new Point(menuXposition, menuYPosition + ReqResSpace), Color.White);
                responseButton.FlatAppearance.BorderColor = Color.DarkBlue; // Set the desired border color
                responseButton.FlatAppearance.BorderSize = 2;
                responseButton.FlatStyle = FlatStyle.Flat;
                responseButton.ForeColor = Color.FromArgb(0, 99, 177);
                responseButton.BackColor = Color.White;
                responseButton.Font = new System.Drawing.Font("Arial", 11, FontStyle.Regular);

                if (itemcount == menuItems.Count)
                {
                    menuXposition = menuXPositionLeft;
                }
                _menuButtonList.Add(responseButton);
                responseButton.Click += (s, args) =>
                {
                    messagePanel.Controls.Remove(menuPanel);
                    // Handle the menu item click event
                    HandleMenuItemClick(item, responseButton);
                };
                menuXposition = menuXposition + 130;
                if (menuXposition > messagePanel.Width - 200)
                {
                    menuXposition = menuXPositionLeft;
                    menuYPosition = menuYPosition + 50;
                }
                menuPanel.Controls.Add(responseButton);
            }
        }
        /// <summary>
        /// Handles the click event for a menu item button. Clears existing menu items
        /// </summary>
        /// <param name="item">The name of the clicked menu item.</param>
        /// <param name="clickedButton">The button that was clicked.</param>
        private void HandleMenuItemClick(string item, System.Windows.Forms.Button clickedButton)
        {
            ClearMenuItems();
            if (item.ToLower().Equals("manual"))
            {
                if (!String.IsNullOrEmpty(_manualPath))
                {
                    string informationText = $"Information for {clickedButton.Text} is here, click on this to open the document.\n";
                    DisplayGFriendResponse(item);
                }
                else
                {
                    MessageBox.Show($"Document for {item} not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                string libraryPath = GetLibraryPath(item);
                if (!String.IsNullOrEmpty(libraryPath))
                {
                    string informationText = $"Information for {clickedButton.Text} is here, click on this to open the document.\n";
                    DisplayGFriendResponse(item);
                }
                else
                {
                    MessageBox.Show($"Document for {item} not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        /// <summary>
        /// Removes all menu item buttons from the message panel and clears the button list.
        /// </summary>
        private void ClearMenuItems()
        {
            // Iterate over each TextBox in the list and remove it from the Controls collection
            foreach (var button in _menuButtonList)
            {
                messagePanel.Controls.Remove(button);
            }
            _menuButtonList.Clear();
        }
        #endregion
        /// <summary>
        /// Retrieves the file path for the specified library document.
        /// </summary>
        /// <param name="userInput">The name of the library for which to get the document path.</param>
        #region ReqResMessages
        private string GetLibraryPath(string userInput)
        {
            string libraryPath = null;
            libraryPath = Path.Combine(Directory.GetCurrentDirectory(), "manual\\KeywordDoc", $"GF_Keywords_{userInput}.html");
            if (File.Exists(libraryPath))
            {
                return libraryPath;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Displays the user's input message in the panel.
        /// </summary>
        /// <param name="userInput">The message entered by the user.</param>
        private void DisplayUserMessage(string userInput)
        {
            userInputTextBox.Clear();
            _isPlaceholderVisible = false;
            CreateTextBoxAndAddToPanel(userInput, "Right", 38, true);
        }
        /// <summary>
        /// Processes the user's input and displays the corresponding GFriend response.
        /// </summary>
        /// <param name="userInput">The user's input string.</param>
        private void DisplayGFriendResponse(string userInput)
        {
            try
            {
                if (userInput.ToLower().Equals("menu"))
                {
                    DisplayMenuItemsHorizontally();
                    return;
                }
                _section = null;
                //to create the html doc for all lib and keywords , if user enters input as lib.keyword
                if (userInput.Contains("."))
                {
                    string[] libraryAndKeyword = userInput.Split(' ');
                    for (int i = 0; i < libraryAndKeyword.Length; i++)
                    {
                        if (libraryAndKeyword[i].Contains('.'))
                        {
                            GetSection(userInput, libraryAndKeyword[i].Split('.')[0]);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(_section))
                {
                    OpenDocumentInBrowser(userInput, null, true);
                    _section = "";
                    return;
                }
                else if (userInput.ToLower().Contains("manual"))
                {
                    if (!String.IsNullOrEmpty(_manualPath))
                    {
                        CreateTextBoxAndAddToPanel($"Here is the documentation for manual\n", "Left", 65, false);
                        OpenDocumentInBrowser(userInput, _manualPath, false);
                        return;
                    }
                }
                else
                {
                    string[] libraryAndKeyword = userInput.Split(' ');
                    for (int i = 0; i < libraryAndKeyword.Length; i++)
                    {
                        string libraryPath = GetLibraryPath(libraryAndKeyword[i]);
                        if (!String.IsNullOrEmpty(libraryPath))
                        {
                            CreateTextBoxAndAddToPanel($"Here is the documentation for {libraryAndKeyword[i]}\n", "Left", 65, false);
                            OpenDocumentInBrowser(userInput, libraryPath, false);
                            return;
                        }
                    }
                }
                string fileUrl = "file:///" + _manualPath;
                CreateTextBoxAndAddToPanel($"Sorry, I couldn't find information on '{userInput}' please type valid input (libraryname | library.keywordname).\r\n {fileUrl}", "Left", 65, false);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        /// <summary>
        /// Generates an HTML document containing all the details of GFriend libraries and keywords 
        /// By formatting keyword sections with unique IDs for reference and saves all the HTML content to a file .
        /// </summary>
        private void CreateKeywordDocument()
        {
            // Generate HTML content
            var htmlContent = new StringBuilder();
            htmlContent.AppendLine("<!DOCTYPE html>");
            htmlContent.AppendLine("<html lang='en'>");
            htmlContent.AppendLine("<head>");
            htmlContent.AppendLine("<meta charset='UTF-8'>");
            htmlContent.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            htmlContent.AppendLine("<title text-decoration: none;'> GFriend Library and Keyword Documentation</title>");
            htmlContent.AppendLine("<style>");
            htmlContent.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.5; margin: 20px; color: #337ab7; }");
            htmlContent.AppendLine(".section { margin-top: 20px; padding: 10px; border: 1px solid #ccc; border-radius: 5px; }");
            htmlContent.AppendLine("#mainContent { display: none; margin-top: 20px; }");
            htmlContent.AppendLine(".library-list { list-style-type: none; padding: 0; }");
            htmlContent.AppendLine(".library-list li { margin: 10px 0; }");
            htmlContent.AppendLine(".keyword-list { list-style-type: none; padding: 0; margin-left: 20px; }");
            htmlContent.AppendLine(".keyword-list li { margin: 5px 0; }");
            htmlContent.AppendLine("</style>");
            htmlContent.AppendLine("</head>");
            htmlContent.AppendLine("<body>");
            htmlContent.AppendLine("<h1 style='color: #20B2AA; font-size: 22px;'>GFriend Library and Keyword Documentation</h1>");
            foreach (var libraryName in _availableLibs.Keys)
            {
                string dutID = "";
                if (string.IsNullOrEmpty(dutID))
                {
                    dutID = _testDataManager.DEFAULT_DUT;
                }
                if (!_testDataManager.DeviceLibraryMapping.ContainsKey(dutID))
                {
                    _testDataManager.DeviceLibraryMapping[dutID] = new List<Library>();
                }

                bool existing = false;
                if (_testDataManager.DeviceLibraryMapping[dutID].Where(s => s.NameAs.Equals(libraryName, StringComparison.CurrentCultureIgnoreCase)).Any())
                {
                    existing = true;
                }
                if (!existing && _availableLibs.ContainsKey(libraryName))
                {
                    Library toAdd = _availableLibs[libraryName].Clone();
                    _testDataManager.DeviceLibraryMapping[dutID].Add(toAdd);
                }
                _testDataManager.GenerateAllUsedLibs();
                Library libraryDetails = _testDataManager.GetLibrary(libraryName.Trim());
                libraryDetails.Load(libraryDetails.NameAs.Trim());

                if (libraryDetails.Keywords != null && libraryDetails.Keywords.Count > 0)
                {
                    foreach (var keyword in libraryDetails.Keywords)
                    {
                        string currentKeyword = libraryName + ".";
                        string keywordId = currentKeyword;
                        string[] splittedKeyword = keyword.KeywordName.Split(' ');
                        //getting the keyword without space and adding this keywordId as id of the section
                        for (int i = 0; i < splittedKeyword.Length; i++)
                        {
                            keywordId = keywordId + splittedKeyword[i];
                        }

                        string args = keyword.Args?.Replace("\r\n", " ").Replace("'", "&#39;") ?? "N/A";
                        string description = keyword.Description?.Replace("\r\n", " ").Replace("'", "&#39;") ?? "N/A";
                        string sampleScript = keyword.SampleScript?.Replace("\r\n", " ").Replace("'", "&#39;") ?? "N/A";

                        string additionalKeywordLink = Path.Combine("additional_keywords", $"{keyword.KeywordName}.html");
                        htmlContent.AppendLine($"<div id={keywordId.ToUpper()} class=\"section\">");
                        htmlContent.AppendLine($"<h3>{libraryName}.{keyword.KeywordName}</h3>");
                        htmlContent.AppendLine($"<p>Description: {description}</p>");
                        htmlContent.AppendLine($"<p>Arguments: {args}</p>");
                        htmlContent.AppendLine($"<p>Sample Script: {sampleScript}</p>");
                        htmlContent.AppendLine(" </div>");
                    }
                }
            }
            htmlContent.AppendLine("</body>");
            htmlContent.AppendLine("</html>");
            File.WriteAllText(_keywordDocument, htmlContent.ToString());
        }
        /// <summary>
        /// Retrieves and sets the section identifier for a given user input and library name.
        /// </summary>
        /// <param name="userInput">The user-provided input string representing a keyword.</param>
        /// <param name="libraryName">The name of the library containing the keyword.</param>
        private void GetSection(string userInput, string libraryName)
        {
            Library libraryDetails = _testDataManager.GetLibrary(libraryName.Trim());
            libraryDetails.Load(libraryDetails.NameAs.Trim());

            if (libraryDetails.Keywords != null && libraryDetails.Keywords.Count > 0)
            {
                foreach (var keyword in libraryDetails.Keywords)
                {
                    string currentKeyword = libraryName + ".";
                    string keywordId = currentKeyword;
                    string[] splittedKeyword = keyword.KeywordName.Split(' ');

                    //getting the keyword without space and adding this keywordId as id of the section
                    for (int i = 0; i < splittedKeyword.Length; i++)
                    {
                        keywordId = keywordId + splittedKeyword[i];
                    }
                    if (userInput.Split('.')[1].Equals(keyword.KeywordName))
                    {
                        currentKeyword = currentKeyword + keyword.KeywordName;
                    }
                    else
                    {
                        currentKeyword = keywordId;
                    }
                    if (userInput.ToLower().Equals(currentKeyword.ToLower()))
                    {
                        _section = keywordId.ToUpper();
                    }
                }
            }
        }
        /// <summary>
        /// Opens the specified documentation file or section in a web browser.
        /// </summary>
        /// <param name="userInput">The user-provided input string representing a keyword or library.</param>
        /// <param name="libraryPath">The file path to the library documentation.</param>
        /// <param name="isSection">Indicates whether to open a specific section within the documentation.</param>
        private void OpenDocumentInBrowser(string userInput, string libraryPath, bool isSection)
        {
            if (isSection)
            {
                string keywordDocumentFilePath = Path.GetFullPath(_keywordDocument);
                // Create an instance of HtmlDocument
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                // Load the HTML content from the file
                htmlDoc.Load(keywordDocumentFilePath);
                string fileUrl = "file:///" + keywordDocumentFilePath.Replace("\\", "/") + "#" + _section;
                string sectionData = _section;

                var sectionNode = htmlDoc.DocumentNode.SelectSingleNode($"//*[@id='{sectionData}']");
                var innerText = sectionNode.InnerText.Trim();
                CreateTextBoxAndAddToPanel($"Here is the documentation for {userInput}.\r\n" + innerText, "Left", 85, false);
            }
            //opens a web link for library documentation
            else
            {
                if (!string.IsNullOrEmpty(libraryPath) && File.Exists(libraryPath))
                {
                    try
                    {
                        // Construct the file URL
                        string fileUrl = "file:///" + libraryPath.Replace("\\", "/");
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = fileUrl,
                            // Opens in the default browser
                            UseShellExecute = true
                        });
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error opening file: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid file path or file does not exist.");
                }
            }
        }
        #endregion

        /// <summary>
        /// Creates a read-only RichTextBox with the specified message, location, and height.
        /// </summary>
        /// <param name="message">The text to display inside the RichTextBox.</param>
        /// <param name="Location">The position where the RichTextBox should be placed.</param>
        /// <param name="Height">The height of the RichTextBox.</param>
        #region Controls
        private System.Windows.Forms.RichTextBox CreateTextBox(string message, Point Location, int Height)
        {
            System.Windows.Forms.RichTextBox welcomeTextBox = new System.Windows.Forms.RichTextBox
            {
                Text = message,
                Location = Location,
                Width = messagePanel.Width,
                Height = Height,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.None,
                BackColor = Color.FromArgb(193, 233, 245),
                BorderStyle = BorderStyle.None,
                Font = new System.Drawing.Font("Arial", 11, FontStyle.Regular)
            };
            return welcomeTextBox;
        }
        /// <summary>
        /// Creates a RichTextBox with the specified message, alignment, and minimum height, and adds it to the message panel.
        /// </summary>
        /// <param name="message">The text to display inside the RichTextBox.</param>
        /// <param name="selectionAlignment">Determines the text alignment ("Left" for responses, otherwise right-aligned).</param>
        /// <param name="minHeight">The minimum height of the RichTextBox.</param>
        /// <param name="isCentered">Specifies whether the text should be center-aligned.</param>
        private System.Windows.Forms.RichTextBox CreateTextBoxAndAddToPanel(string message, string selectionAlignment, int minHeight, bool isCentered)
        {
            // Determine if it's a response
            bool isResponse = selectionAlignment == "Left";
            // Calculate the width dynamically based on text length
            int calculatedHeight;
            using (Graphics g = messagePanel.CreateGraphics())
            {
                System.Drawing.Size textSize = TextRenderer.MeasureText(message, new System.Drawing.Font("Arial", 11), new System.Drawing.Size(messagePanel.Width - 50, int.MaxValue), TextFormatFlags.WordBreak);
                calculatedHeight = (int)textSize.Height + 10;
            }
            minHeight = Math.Max(minHeight, calculatedHeight);
            int calculatedWidth = TextRenderer.MeasureText(message, new System.Drawing.Font("Arial", 11)).Width + 20;
            // Set max width to avoid overflow
            int maxWidth = messagePanel.Width - 50;
            int finalWidth = Math.Min(calculatedWidth, maxWidth);

            string[] messageList = message.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            System.Windows.Forms.RichTextBox textBox = new System.Windows.Forms.RichTextBox
            {
                Text = messageList[0] + Environment.NewLine,
                Width = finalWidth,
                WordWrap = true,
                Height = minHeight,
                ReadOnly = true,
                Multiline = true,
                Padding = new Padding(5, (minHeight - calculatedHeight) / 2, 5, 5), // Adjust top padding
                Font = new System.Drawing.Font("Arial", 11, FontStyle.Regular),
                DetectUrls = true,
            };
            for (int i = 1; i < messageList.Length; i++)
            {
                textBox.AppendText(messageList[i] + Environment.NewLine);
            }
            textBox.SelectionStart = 0;
            textBox.SelectionLength = message.Length;
            if (isCentered)
            {
                textBox.Rtf = @"{\rtf1\ansi\deff0{\pard\qc " + message + @"\par}";
            }

            // Subscribe to the LinkClicked event
            textBox.LinkClicked += new LinkClickedEventHandler(textBox_LinkClicked);
            if (isResponse)
            {
                textBox.BackColor = Color.FromArgb(193, 233, 245);
                textBox.ScrollBars = RichTextBoxScrollBars.None;
                CreateImage(isResponse);
                messagePanel.Controls.Add(textBox);
                messagePanel.SetFlowBreak(textBox, true);
                messagePanel.ScrollControlIntoView(textBox);
            }
            else
            {
                textBox.BackColor = Color.FromArgb(220, 248, 235);
                textBox.Margin = new Padding(messagePanel.Width - finalWidth - 50, 5, 5, 5);
                textBox.Multiline = false;
                textBox.SelectionAlignment = HorizontalAlignment.Right;
                messagePanel.Controls.Add(textBox);
                CreateImage(isResponse);
                messagePanel.ScrollControlIntoView(textBox);
            }
            messagePanel.PerformLayout();
            return textBox;
        }
        /// <summary>
        /// Creates and adds an image icon (PictureBox) to the message panel, representing either a response or a request.
        /// </summary>
        /// <param name="isResponse">Determines whether the image represents a response (true) or a request (false).</param>
        private void CreateImage(bool isResponse)
        {
            PictureBox ImageIcon = new PictureBox
            {
                Image = isResponse ? Properties.Resources.Response_Icon : Properties.Resources.Request_Icon, // Choose image
                Location = new Point(0),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new System.Drawing.Size(30, 30)
            };
            messagePanel.Controls.Add(ImageIcon);
            int xOffset = ImageIcon.Right + 1;
            messagePanel.SetFlowBreak(ImageIcon, false);
        }
        /// <summary>
        /// Creates and returns a button with specified text, width, location, and background color.
        /// </summary>
        /// <param name="message">The text to display on the button.</param>
        /// <param name="width">The width of the button (currently not applied in the method).</param>
        /// <param name="Location">The position where the button will be placed.</param>
        /// <param name="backColor">The background color of the button.</param>
        private System.Windows.Forms.Button CreateButton(string message, int width, Point Location, Color backColor)
        {
            _menuButton = new System.Windows.Forms.Button
            {
                Text = message,
                Location = Location,
                ForeColor = Color.DarkBlue,
                BackColor = backColor,
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 11, FontStyle.Bold),
            };
            return _menuButton;
        }
        #endregion
        /// <summary>
        /// Handles the click event for the send button. It processes the user's input, displays the user's message,
        /// shows the corresponding response, and clears the input field with placeholder reapplication.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the send button).</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void sendButton_Click(object sender, EventArgs e)
        {
            if (messagePanel.Contains(_menuButton))
            {
                messagePanel.Controls.Remove(_menuButton);
            }
            string userInput = userInputTextBox.Text.Trim();
            // Avoid sending placeholder text
            if (userInput == PlaceholderText || string.IsNullOrEmpty(userInput))
            {
                return;
            }
            DisplayUserMessage(userInput);
            DisplayGFriendResponse(userInput);
            userInputTextBox.Clear();
            // Reapply placeholder after clearing the text
            SetPlaceholder();
            userInputTextBox.SelectionStart = userInputTextBox.Text.Length;
            userInputTextBox.TextAlign = HorizontalAlignment.Left;
        }
    }
}
