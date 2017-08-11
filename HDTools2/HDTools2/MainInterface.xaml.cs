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
        private PSObject adUser;

        public MainInterface()
        {
            InitializeComponent();
        }

        public MainInterface(string uname)
        {
            InitializeComponent();
            Closing += MainInterface_Closing;
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

            PowerShellInstance.AddCommand("GetWITUser").AddArgument(uname);

            // invoke execution on the pipeline (collecting output)
            var PSOutput = PowerShellInstance.Invoke();

            // loop through each output object item
            foreach (PSObject outputItem in PSOutput)
            {
                // if null object was dumped to the pipeline during the script then a null
                // object may be present here. check for null to prevent potential NRE.
                if (outputItem != null)
                {
                    adUser = outputItem;
                    //dynamic adUser = outputItem.BaseObject;
                    //Console.WriteLine("GivenName: {0}, Surname: {1}", adUser.Enabled.ToString(), adUser.Surname.ToString());
                    //NameBlock.Text = adUser.Name.ToString();
                    try
                    {
                        NameBlock.Text = adUser.Properties["Name"].Value.ToString();
                        IdBlock.Text = adUser.Properties["EmployeeID"].Value.ToString();
                        ExpiredBlock.Text = adUser.Properties["PasswordExpired"].Value.ToString();
                        EnabledBlock.Text = adUser.Properties["Enabled"].Value.ToString();
                    }
                    catch (Exception)
                    {

                    }

                    //dynamic memberof =outputItem.Properties["Eanbled"].Value;
                    //var firstGroup = memberof.ValueList[0].ToString();
                    //var allGroups = memberof.ValueList;
                    //foreach (var item in allGroups)
                    //{
                    //    Console.WriteLine(item.ToString());
                    //}
                }
            }

            PowerShellInstance.Commands.Clear();
            PowerShellInstance.AddCommand("GetPhotoImage").AddArgument(adUser);

            // invoke execution on the pipeline (collecting output)
            var PSOutput1 = PowerShellInstance.Invoke();

            // loop through each output object item
            foreach (PSObject outputItem in PSOutput1)
            {
                // if null object was dumped to the pipeline during the script then a null
                // object may be present here. check for null to prevent potential NRE.
                if (outputItem != null)
                {
                    BitmapImage image = new BitmapImage(new Uri(outputItem.ToString()));
                    Picture.Source = image;
                }
            }

            PowerShellInstance.Commands.Clear();
        }

        private void MainInterface_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PowerShellInstance.Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string resetMessage = "Reset password for " + NameBlock.Text + "?";
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(this, resetMessage, "Reset Confirmation", System.Windows.MessageBoxButton.OKCancel);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                PowerShellInstance.AddCommand("ResetUserPassword").AddArgument(adUser);
                PowerShellInstance.Invoke();
                PowerShellInstance.Commands.Clear();
            }
        }
    }
}
