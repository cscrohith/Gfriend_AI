using HP.GFriend.Utils.Web;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace HP.GFriend.Tool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            WebDriverHelper.WebDriverPath = Path.Combine(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName, "libs");
            foreach (DriverInfo d in WebDriverHelper.GetCurrentStatus())
            {
                WebDriverInstallControl wic = new WebDriverInstallControl(d);
                flowLayoutPanelDrivers.Controls.Add(wic);
                flowLayoutPanelDrivers.Height += wic.Height + 10;
                Height += wic.Height + 10;
            }
            
            
        }
    }
}
