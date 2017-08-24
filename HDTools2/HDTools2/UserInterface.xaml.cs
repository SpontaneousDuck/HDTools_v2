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
	/// Interaction logic for UserInterface.xaml
	/// </summary>
		
	public partial class UserInterface : Window
	{
		private PowerShell PowerShellInstance;
		private PSObject adUser;

		public UserInterface()
		{
			InitializeComponent();
		}

		public UserInterface(PowerShell shell, PSObject usr)
		{
			InitializeComponent();
			Closing += UserInterface_Closing;

			PowerShellInstance = shell;
			adUser = usr;

			//PowerShellInstance = PowerShell.Create();

			//string script1 = "Set-ExecutionPolicy -Scope Process -ExecutionPolicy Unrestricted"; // the second command to know the ExecutionPolicy level
			//PowerShellInstance.AddScript(script1);
			//var someResult = PowerShellInstance.Invoke();

			//PowerShellInstance.Commands.Clear();

			//string script = System.Text.Encoding.Default.GetString(global::HDTools2.Properties.Resources.ADResetPassword);
			////script = script.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
			//PowerShellInstance.AddScript(script, false);
			//PowerShellInstance.Invoke();
			//PowerShellInstance.Commands.Clear();

			//PowerShellInstance.AddCommand("GetWITUser").AddArgument(uname);

			//// invoke execution on the pipeline (collecting output)
			//var PSOutput = PowerShellInstance.Invoke();

			//// loop through each output object item
			//foreach (PSObject outputItem in PSOutput)
			//{
				// if null object was dumped to the pipeline during the script then a null
				// object may be present here. check for null to prevent potential NRE.
				if (usr != null)
				{
					adUser = usr;
					//dynamic adUser = outputItem.BaseObject;
					//Console.WriteLine("GivenName: {0}, Surname: {1}", adUser.Enabled.ToString(), adUser.Surname.ToString());
					//NameBlock.Text = adUser.Name.ToString();
					try
					{
						NameBlock.Text = adUser.Properties["Name"].Value.ToString();
						IdBlock.Text = adUser.Properties["EmployeeID"].Value.ToString();
						if ((IdBlock.Text.Length != 9) || (IdBlock.Text[0] != 'W')) { IdBlock.Foreground = Brushes.Red; }
						ExpiredBlock.Text = adUser.Properties["PasswordExpired"].Value.ToString();
						if (ExpiredBlock.Text.ToLower() == "true") { ExpiredBlock.Foreground = Brushes.Red; }
						EnabledBlock.Text = adUser.Properties["Enabled"].Value.ToString();
						if (EnabledBlock.Text.ToLower() == "false") { EnabledBlock.Foreground = Brushes.Red; }

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

		private void UserInterface_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			PowerShellInstance.Dispose();
		}

		private void ResetButtonClicked(object sender, RoutedEventArgs e)
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

		private void DetailsButtonClicked(object sender, RoutedEventArgs e)
		{
			List<string> props = new List<string>(); 
			PowerShellInstance.AddCommand("PropsToGet");
			var PSOutput = PowerShellInstance.Invoke();
			foreach (PSObject outputItem in PSOutput)
			{
				// if null object was dumped to the pipeline during the script then a null
				// object may be present here. check for null to prevent potential NRE.
				if (outputItem != null)
				{
					props.Add(outputItem.BaseObject.ToString());
				}
			}

			PowerShellInstance.Commands.Clear();

			string output = "";
			var x = adUser.Properties.ToArray();
			foreach (var item in x)
			{
				var values = "";
				if (props.Contains(item.Name))
				{
					values = item.Value as string;
					output += item.Name + ": " + item.Value + "\n";
				}
			}
			MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(this, output, "Detailed Information", System.Windows.MessageBoxButton.OK);
		}
	}
}
