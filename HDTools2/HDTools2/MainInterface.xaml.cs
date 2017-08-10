using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace HDTools2
{
    /// <summary>
    /// Interaction logic for MainInterface.xaml
    /// </summary>
    public partial class MainInterface : Window
    {
        private PowerShell PowerShellInstance;

        public MainInterface()
        {
            InitializeComponent();
        }

        public MainInterface(string uname)
        {
            InitializeComponent();
            PowerShellInstance = PowerShell.Create();
            Closing += MainInterface_Closing;
            
            string script = System.Text.Encoding.Default.GetString(global::HDTools2.Properties.Resources.ADResetPassword);
            PowerShellInstance.AddScript(script);


        }

        private void MainInterface_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PowerShellInstance.Dispose();
        }
    }
}
