using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading;
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
    /// Interaction logic for UserInputPage.xaml
    /// </summary>
    public partial class UserInputPage : Page
	{
		private PowerShell PowerShellInstance;
		private bool PowerShellReady = false;
		//private UserInterface ui = null;
		private MainWindow master;
		private string DebugStatus
		{
			get { string labelContent = null; Dispatcher.Invoke(() => labelContent = debugLabel.Content.ToString()); return labelContent; }
			set { App.Current.Dispatcher.Invoke(delegate { debugLabel.Content = value; }); /*Debug.WriteLine(value);*/ }
		}
		public double LoadPercent
		{
			get { double loadingValue = Double.NaN; Dispatcher.Invoke(() => loadingValue = loadingBar.Value); return loadingValue; }
			set
			{
				App.Current.Dispatcher.Invoke(delegate
				{
					loadingBar.Value = value;
					if (value == 0)
					{
						loadingBar.Visibility = Visibility.Collapsed;
					}
					else { loadingBar.Visibility = Visibility.Visible; }
				});
			}
		}
		public UserInputPage(MainWindow master)
		{
			this.master = master;
			//ui = null; //Takes ownership of the ui object from this thread
			(new Thread(new ThreadStart(PreparePowerShellInstance))).Start(); //Starts the preparation for the powershell commands
			InitializeComponent();
			LoadPercent = 0;
			DebugStatus = "Please enter a username.";
			Assembly assem = typeof(UserInputWindow).Assembly; //Gets the current assembly, I think? Helps for getting version label
			versionLabel.Content = "Version " + assem.GetName().Version.ToString();
			//var timer = new System.Threading.Timer(e => SwitchToInterface(), null, TimeSpan.Zero, TimeSpan.FromSeconds(0.2)); //Checks every .2 seconds if the user interface is ready (with details about user)
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
		private void Button_Click(object sender, RoutedEventArgs e) { UserEnteredPress(); }
		private void UsernameInput_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { UserEnteredPress(); } }

		private void UserEnteredPress()
		{
			loadingBar.Visibility = Visibility.Visible;
			LoadPercent = 20;
			string username = UsernameInput.Text;
			Thread userEnteredThread = new Thread(() => UserEntered(username));
			userEnteredThread.SetApartmentState(ApartmentState.STA);
			userEnteredThread.Start();
		}
		//private void SwitchToInterface() //Switches from window for username input to window with data
		//{
			//if (!(ui == null))
			//{
				//App.Current.MainWindow = ui;
				//this.Close();
				//ui.Show();
				//master.ChangePage(
			//}
		//}
		private void UserEntered(string s)
		{
			DebugStatus = "User entered...";
			while (!(PowerShellReady))
			{
				DebugStatus = "Still loading ActiveDirectory module.";
				Thread.Sleep(100);
			}
			LoadPercent = 30;
			DebugStatus = "ActiveDirectory module loaded";

			string getUserCommand = "GetWITUser " + s;
			PowerShellInstance.AddScript(getUserCommand);
			// invoke execution on the pipeline (collecting output)
			LoadPercent = 40;
			DebugStatus = "Searching for user...";
			var PSOutput = PowerShellInstance.Invoke();
			LoadPercent = 90;
			PSObject outputItem = PSOutput[0];
			DebugStatus = "Search complete";
			if (outputItem != null)
			{
				DebugStatus = "User found!";
				//Dispatcher.Invoke(() => ui = new UserInterface(PowerShellInstance, outputItem));
				LoadPercent = 100;
				//Dispatcher.Invoke(SwitchToInterface);
				master.ChangePage(new UserInterfacePage(PowerShellInstance, outputItem, master));
				DebugStatus = "New window should now be visible.";
			}
			else
			{

				/*MessageBoxResult messageBoxResult = MessageBoxResult.None;
				Dispatcher.Invoke(() => messageBoxResult = System.Windows.MessageBox.Show(this, "User \""+s+"\" was not found. Perform a deep search?", "Deep search?", System.Windows.MessageBoxButton.OKCancel));
				if (messageBoxResult == MessageBoxResult.OK)
				{
					ThreadStart deepThreadStart = (() => DeepSearchStart(s));
					Thread deepThread = new Thread(deepThreadStart);
					deepThread.Start();
				}
				else
				{*/
				DebugStatus = "User \"" + s + "\" was not found";
				LoadPercent = 0;
				Dispatcher.Invoke(() => UsernameInput.Text = "");
				//}
			}
		}
		private void DeepSearchStart(string s)
		{
			throw new NotImplementedException();
#pragma warning disable CS0162 // Unreachable code detected
			DebugStatus = "Starting deep search";
#pragma warning restore CS0162 // Unreachable code detected
			LoadPercent = 0;
			s = s.ToLower();
			PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "WIT");
			PrincipalSearcher searcher = new PrincipalSearcher(new UserPrincipal(ctx));
			LoadPercent = 10;
			PrincipalSearchResult<Principal> allUsers = searcher.FindAll();
			DebugStatus = "Users acquired, optimizing for loop...";
			LoadPercent = 20;
			//IEnumerator<Principal> allUsersEnum = allUsers.GetEnumerator();
			int totalEntries = allUsers.Count();
			List<DirectoryEntry> allDEs = new List<DirectoryEntry>(totalEntries);
			//List<Principal> allUsersList = allUsers.ToList<Principal>();
			for (int i = 0; i < totalEntries; i++)
			{
				Principal user = allUsers.ElementAt(i);
				DirectoryEntry de = user.GetUnderlyingObject() as DirectoryEntry;
				if ((de.Properties["givenName"].Value != null) && (de.Properties["sn"].Value != null))
				{
					allDEs.Add(de);
				}
				LoadPercent = 20 + ((30 * i) / totalEntries);
			}
			allUsers = null;
			DebugStatus = "Optimization complete, preparing for loop...";
			LoadPercent = 50;
			int numUsers = allDEs.Count();
			double loadChange = 1 / numUsers;
			for (int i = 0; i < numUsers; i++)
			{
				DirectoryEntry de = allDEs[i];
				DebugStatus = "Now searching user " + i.ToString();
				//DirectoryEntry de = user.GetUnderlyingObject() as DirectoryEntry;
				//if ((de.Properties["givenName"].Value != null) && (de.Properties["sn"].Value != null))
				//{
				string firstName = de.Properties["givenName"].Value.ToString().ToLower();
				string lastName = de.Properties["sn"].Value.ToString().ToLower();
				if (s.Contains(firstName) && s.Contains(lastName))
				{
					UserEntered(de.Name.ToString());
					return;
				}
				//}
				LoadPercent = 50 + (50 * i / numUsers);
			}
			LoadPercent = 0;
			DebugStatus = "Deep search completed, no results";
		}
		private void VersionEasterEgg(object sender, MouseButtonEventArgs e)
		{
			string[] facts = Properties.Resources.factsFile.Split('\n');
			string fact = facts[new Random().Next(facts.Count())];
			System.Windows.MessageBox.Show(master, fact, "Wow", System.Windows.MessageBoxButton.OK);
		}

		private void ShowAboutInfo(object sender, RoutedEventArgs e)
		{
			string aboutString = Properties.Resources.aboutHDT2;
			Dispatcher.Invoke(() => System.Windows.MessageBox.Show(master, aboutString, "About", System.Windows.MessageBoxButton.OK));
		}

		private void OpenAD(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"C:\Windows\system32\dsa.msc");
			//this.Close();
		}

		private void ChangeTheme(object sender, RoutedEventArgs e)
		{
			Dispatcher.Invoke(() => System.Windows.MessageBox.Show(master, "This button is just to make Tristan think we have Theme customization", "NO", System.Windows.MessageBoxButton.OK));
		}

		private void OpenServiceNow(object sender, RoutedEventArgs e)
		{
			Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", "wit.service-now.com");
		}
	}
}
