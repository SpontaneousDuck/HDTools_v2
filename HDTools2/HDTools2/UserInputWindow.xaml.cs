using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.DirectoryServices.AccountManagement;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.DirectoryServices;

namespace HDTools2
{
	/// <summary>
	/// Interaction logic for UserInputWindow.xaml
	/// </summary>
	public partial class UserInputWindow : Window
	{
		private PowerShell PowerShellInstance;
		private bool PowerShellReady = false;
		private UserInterface ui = null;
		private string DebugStatus
		{
			get { return debugLabel.Content.ToString(); }
			set { App.Current.Dispatcher.Invoke(delegate { debugLabel.Content = value; }); }
		}
		public UserInputWindow()
		{
			ui = null; //Takes ownership of the ui object from this thread
			(new Thread(new ThreadStart(PreparePowerShellInstance))).Start(); //Starts the preparation for the powershell commands
			InitializeComponent();
			Assembly assem = typeof(UserInputWindow).Assembly; //Gets the current assembly, I think? Helps for getting version label
			versionLabel.Content = "Version " + assem.GetName().Version.ToString();
			var timer = new System.Threading.Timer(e => SwitchToInterface(),null,TimeSpan.Zero,TimeSpan.FromSeconds(0.2)); //Checks every .2 seconds if the user interface is ready (with details about user)
		}
		private void PreparePowerShellInstance()
		{
			PowerShellInstance = PowerShell.Create(); //basically "opens" powershell

			PowerShellInstance.AddScript(@"$env:ADPS_LoadDefaultDrive = 0; import-module ActiveDirectory");//Makes importing AD faster, then imports AD module
			PowerShellInstance.AddScript("Set-ExecutionPolicy -Scope Process -ExecutionPolicy Unrestricted"); //Allows for scripts
			string script = System.Text.Encoding.Default.GetString(Properties.Resources.ADResetPassword); //Access the .ps1 scripts
			PowerShellInstance.AddScript(script, false); //Basically dot-sources the .ps1 scripts
			PowerShellInstance.Invoke();
			PowerShellReady = true;
		}
		private void Button_Click(object sender, RoutedEventArgs e){UserEnteredPress();}
		private void UsernameInput_KeyDown(object sender, KeyEventArgs e){if (e.Key == Key.Enter){UserEnteredPress();}}

		private void UserEnteredPress()
		{
			string username = UsernameInput.Text;
			Thread userEnteredThread = new Thread(() => UserEntered(username));
			userEnteredThread.SetApartmentState(ApartmentState.STA);
			userEnteredThread.Start();
		}
		private void SwitchToInterface() //Switches from window for username input to window with data
		{
			if (! (ui == null))
			{
				App.Current.MainWindow = ui;
				this.Close();
				ui.Show();
			}
		}
		private void UserEntered(string s)
		{
			DebugStatus = "User entered...";
			while (!(PowerShellReady))
			{
				DebugStatus = "Still loading ActiveDirectory module.";
				Thread.Sleep(100);
			}
			DebugStatus = "ActiveDirectory module loaded";



			Dictionary<string,string> props = MakuUtil.GetUserInfoDict(s);
			Debug.Write(props["employeeid"]);


			string getUserCommand = "GetWITUser " + s;
			PowerShellInstance.AddScript(getUserCommand);
			// invoke execution on the pipeline (collecting output)
			DebugStatus = "Searching for user...";
			var PSOutput = PowerShellInstance.Invoke();  //Try making this in a different thread
			DebugStatus = "Search complete";
			PSObject outputItem = PSOutput[0];
			if (outputItem != null)
			{
				DebugStatus = "User found!";
				Dispatcher.Invoke(() => ui = new UserInterface(/*PowerShellInstance, outputItem, */props));
				Dispatcher.Invoke(SwitchToInterface);
				DebugStatus = "New window should now be visible.";
			}
			else
			{
				DebugStatus = "User was not found";
				Dispatcher.Invoke(() => System.Windows.MessageBox.Show(this, "Invalid user", "Error", System.Windows.MessageBoxButton.OK));
			}
		}

		private void VersionEasterEgg(object sender, MouseButtonEventArgs e)
		{
			string[] facts = Properties.Resources.factsFile.Split('\n');
			string fact = facts[new Random().Next(facts.Count())];
			System.Windows.MessageBox.Show(this, fact, "Wow", System.Windows.MessageBoxButton.OK);
		}
	}
}
