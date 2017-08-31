using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace HDTools2
{
    /// <summary>
    /// Interaction logic for UserInterfacePage.xaml
    /// </summary>
    public partial class UserInterfacePage : Page
	{
		private PowerShell PowerShellInstance;
		private PSObject adUser;
		private Dictionary<string, string> mainDetails;
		private MainWindow master;
		//private Dictionary<string, string> fullProps;

		public UserInterfacePage()
		{
			InitializeComponent();
		}

		public UserInterfacePage(PowerShell shell, PSObject usr, MainWindow master)
		{
			InitializeComponent();
			//Closing += UserInterface_Closing;
			this.master = master;
			PowerShellInstance = shell;
			adUser = usr;
			PopulateMainDetails();
			//fullProps = MakuUtil.GetUserProperties(adUser);
			NameBlock.Text = mainDetails["name"];
			IdBlock.Text = mainDetails["employeeID"];
			if ((IdBlock.Text.Length != 9) || (IdBlock.Text[0] != 'W')) { IdBlock.Foreground = Brushes.Red; }
			ExpiredBlock.Text = mainDetails["passwordExpired"];
			if (ExpiredBlock.Text.ToLower() == "true") { ExpiredBlock.Foreground = Brushes.Red; }
			EnabledBlock.Text = mainDetails["enabled"];
			if (EnabledBlock.Text.ToLower() == "false") { EnabledBlock.Foreground = Brushes.Red; }



			PowerShellInstance.Commands.Clear();
			string photoURL = @"http://photoid.wit.edu:8080/" + adUser.Properties["EmployeeID"].Value.ToString() + @".jpg";
			BitmapImage image = new BitmapImage(new Uri(photoURL)) ?? new BitmapImage(new Uri(@"https://pbs.twimg.com/profile_images/580015243780362240/lWe51Oxw.jpg"));
			Picture.Source = image;
			PowerShellInstance.Commands.Clear();
			
		}

		private void PopulateMainDetails()
		{
			mainDetails = new Dictionary<string, string>()
			{
				{ "name",adUser.Properties["Name"].Value.ToString()},
				{"employeeID",adUser.Properties["EmployeeID"].Value.ToString() },
				{"username",adUser.Properties["EmailAddress"].Value.ToString().Substring(0,adUser.Properties["EmailAddress"].Value.ToString().IndexOf('@')) },
				{"passwordExpired",adUser.Properties["PasswordExpired"].Value.ToString() },
				{"enabled",adUser.Properties["Enabled"].Value.ToString() }
			};

		}
		private void UserInterface_Closing(object sender, System.ComponentModel.CancelEventArgs e) { PowerShellInstance.Dispose(); }

		private void ResetButtonClicked(object sender, RoutedEventArgs e)
		{
			string resetMessage = "Reset password for " + NameBlock.Text + "?";
			MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(master, resetMessage, "Reset Confirmation", System.Windows.MessageBoxButton.OKCancel);
			if (messageBoxResult == MessageBoxResult.OK)
			{
				PowerShellInstance.AddCommand("ResetUserPassword").AddArgument(adUser);
				PowerShellInstance.Invoke();
				PowerShellInstance.Commands.Clear();
				string WID = adUser.Properties["EmployeeID"].Value.ToString();
				string newPassword = @"WIT1$" + WID.Substring(WID.Length - 6);
				bool resetSuccessfully = MakuUtil.ConfirmPasswordChange(mainDetails["username"], newPassword);
				if (resetSuccessfully)
				{
					System.Windows.MessageBox.Show(master, "Password successfully reset!", "Success", System.Windows.MessageBoxButton.OK);
				}
				else
				{
					System.Windows.MessageBox.Show(master, "Password reset was not completed", "Failure", System.Windows.MessageBoxButton.OK);
				}
			}
		}

		private void DetailsButtonClicked(object sender, RoutedEventArgs e)
		{
			string[] props = { "DisplayName", "Created", "EmailAddress", "EmployeeID", "Enabled", "LastBadPasswordAttempt", "LastLogonDate", "LockedOut", "LogonCount", "Modified", "MemberOf", "PasswordExpired", "PasswordLastSet", "Title" };

			string output = "";
			var x = adUser.Properties.ToArray();
			foreach (var item in x)
			{
				string values = "";
				if (props.Contains(item.Name))
				{
					values = item.Value as string;
					output += item.Name + ": " + item.Value + "\n";
				}
			}
			MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(master, output, "Detailed Information", System.Windows.MessageBoxButton.OK);
		}

		private void OpenMemberOf(object sender, RoutedEventArgs e)
		{
			PSPropertyInfo[] thing1 = adUser.Properties.ToArray();
			foreach (PSPropertyInfo thing2 in thing1)
			{
				if (thing2.Value != null)
				{
					Debug.WriteLine(thing2.Name.ToString() + " " + thing2.Value.ToString());
				}
			}
		}
	}
}
