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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
		public UserInputWindow()
		{
			ui = null;
			ThreadStart preparationThreadStart = new ThreadStart(PreparePowerShellInstance);
			Thread preparationThread = new Thread(preparationThreadStart);
			preparationThread.Start();
			InitializeComponent();
			//string version = My. //((AssemblyVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyVersionAttribute), false)).Version;
			Assembly assem = typeof(UserInputWindow).Assembly;
			string version = assem.GetName().Version.ToString();
			versionLabel.Content = "Version " + version;
			var timer = new System.Threading.Timer(e => SwitchToInterface(),null,TimeSpan.Zero,TimeSpan.FromSeconds(0.2));
			//foreach (string key in Application.Current.Properties.Keys)
			//{
			//	Debug.Print("A key: " + key);
			//}
			//Debug.Print("Keys thing: "+Application.Current.Properties.Keys.ToString());
			//Application.Current.Resources.
			//versionLabel.Content = "Version " + Application.Current.Properties.Keys;
		}
		private void PreparePowerShellInstance()
		{
			PowerShellInstance = PowerShell.Create();
			string importScript = @"$env:ADPS_LoadDefaultDrive = 0; import-module ActiveDirectory";

			PowerShellInstance.AddScript(importScript);
			PowerShellInstance.Invoke();

			string script1 = "Set-ExecutionPolicy -Scope Process -ExecutionPolicy Unrestricted"; // the second command to know the ExecutionPolicy level
			PowerShellInstance.AddScript(script1);
			var someResult = PowerShellInstance.Invoke();

			PowerShellInstance.Commands.Clear();

			string script = System.Text.Encoding.Default.GetString(global::HDTools2.Properties.Resources.ADResetPassword);
			//script = script.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
			PowerShellInstance.AddScript(script, false);
			PowerShellInstance.Invoke();
			PowerShellInstance.Commands.Clear();
			PowerShellReady = true;
			//DebugNote("Powershell preparation complete");
		}
		private void Button_Click(object sender, RoutedEventArgs e){UserEnteredPress();}

		private void UsernameInput_KeyDown(object sender, KeyEventArgs e){if (e.Key == Key.Enter){UserEnteredPress();}}
		private void UserEnteredPress()
		{
			//ThreadStart userEnteredThreadStart = new ThreadStart()
			string usernameInput = UsernameInput.Text;
			Thread userEnteredThread = new Thread(() => UserEntered(usernameInput));
			userEnteredThread.SetApartmentState(ApartmentState.STA);
			userEnteredThread.Start();
		}
		private void DebugNote(string s)
		{
			//debugLabel.Content = s;
			App.Current.Dispatcher.Invoke((Action)delegate
			{
				debugLabel.Content = s;
			});
			//debugLabel.
			//Thread.Sleep(1000);
			//Debug.Write("Debug note should have been added.");
			//Debug.Write("Debug note: " + debugLabel.Content);
		}
		private void SwitchToInterface()
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
			DebugNote("User entered...");
			while (!(PowerShellReady))
			{
				DebugNote("Still loading ActiveDirectory module.");
				Thread.Sleep(100);
			}
			DebugNote("ActiveDirectory module loaded");

			//string usernameInput = Dispatcher.Invoke()
			string getUserCommand = "GetWITUser " + s;
			//PowerShellInstance.AddCommand("GetWITUser").AddArgument(UsernameInput.Text);
			PowerShellInstance.AddScript(getUserCommand);
			// invoke execution on the pipeline (collecting output)
			DebugNote("Searching for user...");
			var PSOutput = PowerShellInstance.Invoke();  //Try making this in a different thread
			DebugNote("User found!");
			PSObject outputItem = PSOutput[0];
			//System.Console.WriteLine(outputItem.ToString());
			// if null object was dumped to the pipeline during the script then a null
			// object may be present here. check for null to prevent potential NRE.
			if (outputItem != null)
			{
				Dispatcher.Invoke(() => ui = new UserInterface(PowerShellInstance, outputItem));
				Dispatcher.Invoke(SwitchToInterface);
				//Dispatcher.Invoke(() => App.Current.MainWindow = newWindow);
				//newWindow.Show();
				DebugNote("New window should now be visible.");
				//Dispatcher.Invoke(this.Hide);
			}
			else
			{
				MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(this, "Invalid user", "Error", System.Windows.MessageBoxButton.OK);
			}
			//}
		}
	}
}
