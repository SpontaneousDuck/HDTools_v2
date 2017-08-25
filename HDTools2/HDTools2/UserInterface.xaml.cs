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
using System.Diagnostics;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace HDTools2
{
	/// <summary>
	/// Interaction logic for UserInterface.xaml
	/// </summary>
		
	public partial class UserInterface : Window
	{
		//private PowerShell PowerShellInstance;
		//private PSObject adUser;
		private string username;
		Dictionary<string, string> props;

		public UserInterface()
		{
			InitializeComponent();
		}

		public UserInterface(/*PowerShell shell, PSObject usr,*/Dictionary<string,string> propsDict)
		{
			props = propsDict;
			InitializeComponent();
			Closing += UserInterface_Closing;
			username = propsDict["displayname"];
			//PowerShellInstance = shell;
			//adUser = usr;
			NameBlock.Text = props["name"];
			IdBlock.Text = props["employeeid"];
			if ((IdBlock.Text.Length != 9) || (IdBlock.Text[0] != 'W')) { IdBlock.Foreground = Brushes.Red; }
			ExpiredBlock.Text = props["expired"];
			if (ExpiredBlock.Text.ToLower() == "true") { ExpiredBlock.Foreground = Brushes.Red; }
			EnabledBlock.Text = props["enabled"];
			if (EnabledBlock.Text.ToLower() == "false") { EnabledBlock.Foreground = Brushes.Red; }
				


			//PowerShellInstance.Commands.Clear();
			string photoURL = @"http://photoid.wit.edu:8080/" + props["employeeid"] + @".jpg";
			BitmapImage image = new BitmapImage(new Uri(photoURL)) ?? new BitmapImage(new Uri(@"https://pbs.twimg.com/profile_images/580015243780362240/lWe51Oxw.jpg"));
			Picture.Source = image;

			//PowerShellInstance.Commands.Clear();
		}

		private void UserInterface_Closing(object sender, System.ComponentModel.CancelEventArgs e){/*PowerShellInstance.Dispose();*/}

		private void ResetButtonClicked(object sender, RoutedEventArgs e)
		{
			string resetMessage = "Reset password for " + NameBlock.Text + "?";
			MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(this, resetMessage, "Reset Confirmation", System.Windows.MessageBoxButton.OKCancel);
			if (messageBoxResult == MessageBoxResult.OK)
			{
				/*PowerShellInstance.AddCommand("ResetUserPassword").AddArgument(adUser);
				PowerShellInstance.Invoke();
				PowerShellInstance.Commands.Clear();*/
				MakuUtil.ResetUserPassword(username);
			}
		}

		private void DetailsButtonClicked(object sender, RoutedEventArgs e)
		{
			string[] propsToGet = { "name", "displayname", "whencreated", /*"emailaddress",*/ "employeeid", "enabled", "badpasswordtime", "lastlogon", "lockouttime", "logoncount", /*"modified", */"memberof",/* "passwordexpired",*/ "pwdlastset", "title" };
			//string[] props = {"DisplayName","Created","EmailAddress","EmployeeID","Enabled","LastBadPasswordAttempt","LastLogonDate","LockedOut","LogonCount","Modified","MemberOf","PasswordExpired","PasswordLastSet","Title"};
			//List<string> props = new List<string>(); 
			//PowerShellInstance.AddCommand("PropsToGet");
			//var PSOutput = PowerShellInstance.Invoke();
			//foreach (PSObject outputItem in PSOutput)
			//{
			// if null object was dumped to the pipeline during the script then a null
			// object may be present here. check for null to prevent potential NRE.
			//	if (outputItem != null)
			//	{
			//		props.Add(outputItem.BaseObject.ToString());
			//	}
			//}

			//PowerShellInstance.Commands.Clear();
			string output = "";
			foreach (string prop in props.Keys)
			{
				Debug.Print("Prop: " + prop);
				if (propsToGet.Contains(prop))
				{
					string val = props[prop];
					output += prop + ": " + val + "\n";
				}
			}
			//string output = "";
			//var x = adUser.Properties.ToArray();
			//foreach (string prop in props.Keys)
			//{
			//	var values = "";
			//	if (propsToGet.Contains(prop))
			//	{
			//		values = props[prop];
			//		output += prop + ": " + values + "\n";
			//	}
			//}
			MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(this, output, "Detailed Information", System.Windows.MessageBoxButton.OK);
		}
	}
}
