using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace HP.GFriend.Client.Forms
{
    public partial class GFServerViewerForm : Form
    {
        private GFServerConnector GFServer;
        private List<TestProject> TestProjectList = new List<TestProject>();
        private string _serverAddress = null;

        private int CurrentPage = 1;
        private int PagesCount = 1;
        private int PageRows = 10;

        private BindingList<PartialTestProject> FullTestSuitesList = null;
        private BindingList<PartialTestProject> PartialTestSuitesList = null;

        private GFClientUtil _gFClientUtil = new GFClientUtil();


        public GFServerViewerForm(Icon icon)
        {
            Icon = icon;
            InitializeComponent();

            _serverAddress = _gFClientUtil.LoadScriptServerInfoINI();
            GFServer = new GFServerConnector(_serverAddress);
            gfServerAddr_textBox.Text = _serverAddress;
            
            SetTestProjectsGridViewHeader();
            InitServerForm(GFServer);
        }

        public GFServerViewerForm(Icon icon, TestProject testProject)
        {
            Icon = icon;
            InitializeComponent();

            _serverAddress = _gFClientUtil.LoadScriptServerInfoINI();
            GFServer = new GFServerConnector(_serverAddress);
            gfServerAddr_textBox.Text = _serverAddress;
            
            SetTestProjectsGridViewHeader();
            InitServerForm(GFServer);
            SelectRowbyExecId(testProject.ExecutionID);

            MessageBox.Show($"{testProject.ExecutionID} is started");
        }
        
        private void GFServerViewerForm_Load(object sender, EventArgs e)
        {
            RefreshGridViewByPageChange();
            RefreshPageCount();
            SetButtonStatus();
        }

        #region button Control
        private void refresh_button_Click(object sender, EventArgs e)
        {
            WaitingForm waitingForm = new WaitingForm();
            waitingForm.Show();

            InitServerForm(GFServer);

            waitingForm.Close();
            this.Focus();
        }

        private void Connect_button_Click(object sender, EventArgs e)
        {
            WaitingForm waitingForm = new WaitingForm();
            waitingForm.Show();

            if (string.IsNullOrEmpty(gfServerAddr_textBox.Text))
            {
                MessageBox.Show("Server address field should have a value for connecting");
                gfServerAddr_textBox.Focus();
                return;
            }

            GFServerConnector gFServer = new GFServerConnector(gfServerAddr_textBox.Text);            
            GFServerResult serverResult = InitServerForm(gFServer);

            if (serverResult.Status == RequestStatus.Success)
            {
                _serverAddress = gfServerAddr_textBox.Text;
                _gFClientUtil.SaveScriptServerInfoINI(_serverAddress);                
            }   

            waitingForm.Close();
            this.Focus();
        }

        private void ToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                ToolStripButton toolStripButton = ((ToolStripButton)sender);

                if (toolStripButton == back_toolStripButton)
                {
                    CurrentPage--;
                }
                else if (toolStripButton == forward_toolStripButton)
                {
                    CurrentPage++;
                }
                else if (toolStripButton == gotoFirst_toolStripButton)
                {
                    CurrentPage = 1;
                }
                else if (toolStripButton == gotoLast_toolStripButton)
                {
                    CurrentPage = PagesCount;
                }
                else
                {
                    CurrentPage = Convert.ToInt32(toolStripButton.Text);
                }

                if (CurrentPage < 1)
                {
                    CurrentPage = 1;
                }
                else if (CurrentPage > PagesCount)
                {
                    CurrentPage = PagesCount;
                }

                RefreshGridViewByPageChange();
                RefreshPageCount();
            }
            catch (Exception)
            {
                //Loading pages is affected by Server status, and it can make exception. It would be ignored.
            }
        }
        #endregion button Control
        
        #region Job Cancel Control
        private void SetButtonStatus()
        {
            foreach (DataGridViewRow row in testProjects_dataGridView.Rows)
            {
                if((GFServer.IsSuperUser(Environment.UserName) || Environment.UserName.Equals(row.Cells["Author"].Value.ToString())) && "Running".Equals(row.Cells["Status"].Value))
                {
                    DataGridViewDisableButtonCell buttonCell = (DataGridViewDisableButtonCell)row.Cells["Cancel"];
                    buttonCell.Enabled = true;                    
                }
            }

            testProjects_dataGridView.Refresh();
        }

        private void testProjects_dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (testProjects_dataGridView.Columns[e.ColumnIndex] is DataGridViewLinkColumn)
            {
                string link = testProjects_dataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
                System.Diagnostics.Process.Start(link);
            }

            if (testProjects_dataGridView.Columns[e.ColumnIndex].Name == "Cancel")
            {
                DataGridViewDisableButtonCell buttonCell =
                (DataGridViewDisableButtonCell)testProjects_dataGridView.
                Rows[e.RowIndex].Cells["Cancel"];
                
                if (buttonCell.Enabled)
                {
                    string projectId = testProjects_dataGridView.Rows[e.RowIndex].Cells["ExecutionID"].Value.ToString();

                    if (!string.IsNullOrEmpty(projectId))
                    {
                        if (CancelTestProject(projectId))
                        {
                            MessageBox.Show($"{projectId} job canceled");
                        }
                        InitServerForm(GFServer);
                    }
                    else
                    {
                        MessageBox.Show($"Fail to cancel job");
                    }
                }
            }
        }

        private bool CancelTestProject(string projectId)
        {
            TestProject testProject = (TestProject)GFServer.GetTestProject(projectId).Data;
            GFServerResult serverResult = GFServer.CancelTestProject(testProject);

            if (serverResult.Status != RequestStatus.Success)
            {
                MessageBox.Show(serverResult.Message, serverResult.GetStatusDescription());
                return false;
            }

            return true;
        }
        #endregion Job Cancel Control



        #region Test Projects GridView Control

        private void SelectRowbyExecId(string execID)
        {
            foreach (DataGridViewRow row in testProjects_dataGridView.Rows)
            {
                if (execID.Equals(row.Cells[0].Value))
                {
                    row.Selected = true;
                    testProjects_dataGridView.CurrentCell = testProjects_dataGridView[0, row.Index];
                    break;
                }
            }
        }

        private GFServerResult InitServerForm(GFServerConnector gFServer)
        {
            GFServerResult serverResult = gFServer.GetTestProjects();

            if (serverResult.Status != RequestStatus.Success)
            {
                MessageBox.Show(serverResult.Message, serverResult.GetStatusDescription());
                return serverResult;
            }

            GFServer = gFServer;
            TestProjectList = serverResult.DataList.ConvertAll(o => (TestProject)o);

            TestProjectList = TestProjectList.OrderByDescending(c => c.StartTime).ToList();
            
            FullTestSuitesList = GetTestProjectsList();
            PagesCount = Convert.ToInt32(Math.Ceiling(FullTestSuitesList.Count * 1.0 / PageRows));

            serverResult = RefreshGridViewByPageChange();
            RefreshPageCount();

            return serverResult;
        }

        private BindingList<PartialTestProject> GetTestProjectsList()
        {
            BindingList<PartialTestProject> testProjects = new BindingList<PartialTestProject>();

            if (TestProjectList.Count > 0)
            {
                foreach (TestProject tp in TestProjectList)
                {
                    testProjects.Add(new PartialTestProject()
                    {
                        ExecutionID = tp.ExecutionID,
                        DeviceAddress = tp.DUT.DeviceAddress,
                        Author = tp.Author,
                        StartTime = tp.StartTime,
                        EndTime = tp.EndTime,
                        Status = Enum.GetName(typeof(ExecutionStatus), tp.Status),
                        Report = tp.TestReportURL,
                        Jenkins = string.Join("", tp.JenkinsJobURL, "console")
                    });
                }
            }

            return testProjects;
        }
        
        private GFServerResult RefreshGridViewByPageChange()
        {
            int startIndex = (CurrentPage - 1) * PageRows;
            GFServerResult serverResult = null;

            PartialTestSuitesList = new BindingList<PartialTestProject>();

            for (int i = startIndex; i < startIndex + PageRows; i++)
            {
                if (i >= FullTestSuitesList.Count)
                {
                    break;
                }
                serverResult = GFServer.GetTestProjectResult(FullTestSuitesList[i].ExecutionID);
                if(serverResult.Status != RequestStatus.Success)
                {
                    MessageBox.Show(serverResult.Message, serverResult.GetStatusDescription());
                    return serverResult;
                }
                TestProjectResult testResult = (TestProjectResult)serverResult.Data;
                FullTestSuitesList[i].Pass = testResult.Pass;
                FullTestSuitesList[i].Fail = testResult.Fail;
                FullTestSuitesList[i].Error = testResult.Error;

                PartialTestSuitesList.Add(FullTestSuitesList[i]);
            }

            testProjects_dataGridView.DataSource = PartialTestSuitesList;
            SetButtonStatus();

            return serverResult;
        }

        private void RefreshPageCount()
        {
            ToolStripButton[] items = new ToolStripButton[] { toolStripButton1, toolStripButton2, toolStripButton3, toolStripButton4, toolStripButton5 };

            int startPageIndex = 1;

            if (PagesCount > 5 && CurrentPage > 2)
            {
                startPageIndex = CurrentPage - 2;
            }
            if (PagesCount > 5 && CurrentPage > PagesCount - 2)
            {
                startPageIndex = PagesCount - 4;
            }

            for (int i = startPageIndex; i < startPageIndex + 5; i++)
            {
                if(i > PagesCount)
                {
                    items[i - startPageIndex].Visible = false;
                }
                else
                {
                    items[i - startPageIndex].Visible = true;
                    items[i - startPageIndex].Text = i.ToString();

                    if (i == CurrentPage)
                    {
                        items[i - startPageIndex].BackColor = Color.Black;
                        items[i - startPageIndex].ForeColor = Color.FromArgb(240,238,233);
                    }
                    else
                    {
                        items[i - startPageIndex].BackColor = Color.FromArgb(240, 238, 233);
                        items[i - startPageIndex].ForeColor = Color.Black;
                    }
                }                
            }

            if (CurrentPage == 1)
            {
                back_toolStripButton.Enabled = false;
                gotoFirst_toolStripButton.Enabled = false;
            }
            else
            {
                back_toolStripButton.Enabled = true;
                gotoFirst_toolStripButton.Enabled = true;
            }

            if (CurrentPage == PagesCount)
            {
                forward_toolStripButton.Enabled = false;
                gotoLast_toolStripButton.Enabled = false;
            }
            else
            {
                forward_toolStripButton.Enabled = true;
                gotoLast_toolStripButton.Enabled = true;
            }
        }

        private void SetTestProjectsGridViewHeader()
        {
            testProjects_dataGridView.AutoGenerateColumns = false;

            DataGridViewTextBoxColumn execIdColumn = new DataGridViewTextBoxColumn();
            execIdColumn.DataPropertyName = "ExecutionID";
            execIdColumn.HeaderText = "Execution ID";
            execIdColumn.Name = "ExecutionID";
            execIdColumn.FillWeight = 120;

            DataGridViewTextBoxColumn AddressColumn = new DataGridViewTextBoxColumn();
            AddressColumn.DataPropertyName = "DeviceAddress";
            AddressColumn.HeaderText = "Device";
            AddressColumn.FillWeight = 80;

            DataGridViewTextBoxColumn authorColumn = new DataGridViewTextBoxColumn();
            authorColumn.DataPropertyName = "Author";
            authorColumn.HeaderText = "Author";
            authorColumn.Name = "Author";
            authorColumn.FillWeight = 50;

            DataGridViewTextBoxColumn startTimeColumn = new DataGridViewTextBoxColumn();
            startTimeColumn.DataPropertyName = "StartTime";
            startTimeColumn.HeaderText = "Start Time";
            startTimeColumn.FillWeight = 100;

            DataGridViewTextBoxColumn endTimeColumn = new DataGridViewTextBoxColumn();
            endTimeColumn.DataPropertyName = "EndTime";
            endTimeColumn.HeaderText = "End Time";
            endTimeColumn.FillWeight = 100;

            DataGridViewTextBoxColumn passCountColumn = new DataGridViewTextBoxColumn();
            passCountColumn.DataPropertyName = "Pass";
            passCountColumn.HeaderText = "Pass";
            passCountColumn.FillWeight = 30;
            passCountColumn.DefaultCellStyle.ForeColor = Color.Blue;
            passCountColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;            

            DataGridViewTextBoxColumn failCountColumn = new DataGridViewTextBoxColumn();
            failCountColumn.DataPropertyName = "Fail";
            failCountColumn.HeaderText = "Fail";
            failCountColumn.FillWeight = 30;
            failCountColumn.DefaultCellStyle.ForeColor = Color.Red;
            failCountColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewTextBoxColumn errorCountColumn = new DataGridViewTextBoxColumn();
            errorCountColumn.DataPropertyName = "Error";
            errorCountColumn.HeaderText = "Error";
            errorCountColumn.FillWeight = 30;
            errorCountColumn.DefaultCellStyle.ForeColor = Color.Orange;
            errorCountColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewTextBoxColumn statusColumn = new DataGridViewTextBoxColumn();
            statusColumn.DataPropertyName = "Status";
            statusColumn.HeaderText = "Status";
            statusColumn.Name = "Status";
            statusColumn.FillWeight = 70;

            DataGridViewDisableButtonColumn buttonColumn = new DataGridViewDisableButtonColumn();
            buttonColumn.DataPropertyName = "Cancel";
            buttonColumn.HeaderText = "Cancel";
            buttonColumn.FillWeight = 60;
            buttonColumn.UseColumnTextForButtonValue = true;
            buttonColumn.Text = "Cancel";
            buttonColumn.Name = "Cancel";


            DataGridViewLinkColumn reportColumn = new DataGridViewLinkColumn();
            reportColumn.DataPropertyName = "Report";
            reportColumn.HeaderText = "Report";
            reportColumn.LinkBehavior = LinkBehavior.SystemDefault;
            reportColumn.ActiveLinkColor = Color.White;
            reportColumn.LinkColor = Color.Blue;
            reportColumn.VisitedLinkColor = Color.YellowGreen;
            reportColumn.TrackVisitedState = true;
            reportColumn.UseColumnTextForLinkValue = false;
            reportColumn.FillWeight = 100;

            DataGridViewLinkColumn jenkinsColumn = new DataGridViewLinkColumn();
            jenkinsColumn.DataPropertyName = "Jenkins";
            jenkinsColumn.HeaderText = "Jenkins";
            jenkinsColumn.LinkBehavior = LinkBehavior.SystemDefault;
            jenkinsColumn.ActiveLinkColor = Color.White;
            jenkinsColumn.LinkColor = Color.Blue;
            jenkinsColumn.VisitedLinkColor = Color.YellowGreen;
            jenkinsColumn.TrackVisitedState = true;
            jenkinsColumn.UseColumnTextForLinkValue = false;
            jenkinsColumn.FillWeight = 100;

            testProjects_dataGridView.Columns.Add(execIdColumn);
            testProjects_dataGridView.Columns.Add(AddressColumn);
            testProjects_dataGridView.Columns.Add(authorColumn);
            testProjects_dataGridView.Columns.Add(startTimeColumn);
            testProjects_dataGridView.Columns.Add(endTimeColumn);
            testProjects_dataGridView.Columns.Add(passCountColumn);
            testProjects_dataGridView.Columns.Add(failCountColumn);
            testProjects_dataGridView.Columns.Add(errorCountColumn);
            testProjects_dataGridView.Columns.Add(statusColumn);
            testProjects_dataGridView.Columns.Add(buttonColumn);            
            testProjects_dataGridView.Columns.Add(reportColumn);
            testProjects_dataGridView.Columns.Add(jenkinsColumn);

            testProjects_dataGridView.EnableHeadersVisualStyles = false;

        }
        #endregion Test Projects GridView Control
    }

    internal class PartialTestProject
    {
        public string ExecutionID { get; set; }

        public string DeviceAddress { get; set; }

        public string Author { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int Pass { get; set; }

        public int Fail { get; set; }

        public int Error { get; set; }

        public string Status { get; set; }
        
        public string Report { get; set; }

        public string Jenkins { get; set; }
    }

    public class DataGridViewDisableButtonColumn : DataGridViewButtonColumn
    {
        public DataGridViewDisableButtonColumn()
        {
            this.CellTemplate = new DataGridViewDisableButtonCell();
        }
    }

    /// <summary>
    /// Adapted from https://msdn.microsoft.com/en-us/library/ms171619.aspx. Double-buffering was added to remove flicker on re-paints.
    /// </summary>
    public class DataGridViewDisableButtonCell : DataGridViewButtonCell
    {
        private bool enabledValue;

        public bool Enabled
        {
            get { return enabledValue; }
            set
            {
                if (enabledValue == value) return;
                enabledValue = value;
                // force the cell to be re-painted
                if (DataGridView != null) DataGridView.InvalidateCell(this);
            }
        }

        // Override the Clone method so that the Enabled property is copied. 
        public override object Clone()
        {
            var cell = (DataGridViewDisableButtonCell)base.Clone();
            cell.Enabled = Enabled;
            return cell;
        }

        // By default, enable the button cell. 
        public DataGridViewDisableButtonCell()
        {
            enabledValue = false;
        }

        protected override void Paint(
            Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates elementState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            // The button cell is disabled, so paint the border, background, and disabled button for the cell. 
            if (!enabledValue)
            {
                var currentContext = BufferedGraphicsManager.Current;

                using (var myBuffer = currentContext.Allocate(graphics, cellBounds))
                {
                    // Draw the cell background, if specified. 
                    if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
                    {
                        using (var cellBackground = new SolidBrush(cellStyle.BackColor))
                        {
                            myBuffer.Graphics.FillRectangle(cellBackground, cellBounds);
                        }
                    }

                    // Draw the cell borders, if specified. 
                    if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
                    {
                        PaintBorder(myBuffer.Graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
                    }

                    // Calculate the area in which to draw the button.
                    var buttonArea = cellBounds;
                    var buttonAdjustment = BorderWidths(advancedBorderStyle);
                    buttonArea.X += buttonAdjustment.X;
                    buttonArea.Y += buttonAdjustment.Y;
                    buttonArea.Height -= buttonAdjustment.Height;
                    buttonArea.Width -= buttonAdjustment.Width;

                    // Draw the disabled button.                
                    ButtonRenderer.DrawButton(myBuffer.Graphics, buttonArea, PushButtonState.Disabled);

                    // Draw the disabled button text.  
                    var formattedValueString = FormattedValue as string;
                    if (formattedValueString != null)
                    {
                        TextRenderer.DrawText(myBuffer.Graphics, formattedValueString, DataGridView.Font, buttonArea, SystemColors.GrayText, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }

                    myBuffer.Render();
                }
            }
            else
            {
                // The button cell is enabled, so let the base class handle the painting. 
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
            }
        }
    }
}
