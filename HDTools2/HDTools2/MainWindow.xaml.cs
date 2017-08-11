using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HDTools2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PowerShell PowerShellInstance;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UserEntered();
        }

        private void UsernameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UserEntered();
            }
        }

        private void UserEntered()
        {
            
            PowerShellInstance = PowerShell.Create();

            string script1 = "Set-ExecutionPolicy -Scope Process -ExecutionPolicy Unrestricted"; // the second command to know the ExecutionPolicy level
            PowerShellInstance.AddScript(script1);
            var someResult = PowerShellInstance.Invoke();

            PowerShellInstance.Commands.Clear();

            string script = System.Text.Encoding.Default.GetString(global::HDTools2.Properties.Resources.ADResetPassword);
            //script = script.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
            PowerShellInstance.AddScript(script, false);
            PowerShellInstance.Invoke();
            PowerShellInstance.Commands.Clear();

            PowerShellInstance.AddCommand("GetWITUser").AddArgument(UsernameInput.Text);

            // invoke execution on the pipeline (collecting output)
            var PSOutput = PowerShellInstance.Invoke();

            // loop through each output object item
            foreach (PSObject outputItem in PSOutput)
            {
                // if null object was dumped to the pipeline during the script then a null
                // object may be present here. check for null to prevent potential NRE.
                if (outputItem != null)
                {
                    var newWindow = new MainInterface(PowerShellInstance, outputItem);
                    newWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(this, "Invalid user", "Error", System.Windows.MessageBoxButton.OK);
                }
            }
        }
    }
}
