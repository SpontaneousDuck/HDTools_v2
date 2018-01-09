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
		private PowerShell PowerShellInstance;
		private PSObject adUser;
		private Dictionary<string, string> mainDetails;

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
			PopulateMainDetails();
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
		private void UserInterface_Closing(object sender, System.ComponentModel.CancelEventArgs e){PowerShellInstance.Dispose();}

		private void ResetButtonClicked(object sender, RoutedEventArgs e)
		{
			string resetMessage = "Reset password for " + NameBlock.Text + "?";
			MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(this, resetMessage, "Reset Confirmation", System.Windows.MessageBoxButton.OKCancel);
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
					System.Windows.MessageBox.Show(this, "Password successfully reset!","Success", System.Windows.MessageBoxButton.OK);
				}
				else
				{
					System.Windows.MessageBox.Show(this, "Password reset was not completed","Failure", System.Windows.MessageBoxButton.OK);
				}
			}
		}

		private void DetailsButtonClicked(object sender, RoutedEventArgs e)
		{
			string[] props = {"DisplayName","Created","EmailAddress","EmployeeID","Enabled","LastBadPasswordAttempt","LastLogonDate","LockedOut","LogonCount","Modified","MemberOf","PasswordExpired","PasswordLastSet","Title"};

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

		private void BackToInput(object sender, RoutedEventArgs e)
		{
			UserInputWindow newWindow = new UserInputWindow();
			App.Current.MainWindow = newWindow;
			this.Close();
			newWindow.Show();
		}

		private void CopyUsername(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(mainDetails["username"]);
		}

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "mailto:pittss@wit.edu?subject=HDT2Bug&body=Please_describe_your_bug";
            proc.Start();
        }
    }
}
