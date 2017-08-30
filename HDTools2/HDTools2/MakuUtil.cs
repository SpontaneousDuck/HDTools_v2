using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Diagnostics;
using System.Management.Automation;
using System.Windows.Threading;

namespace HDTools2
{
	class MakuUtil
	{
		/*public static Dictionary<string,string> GetUserProperties(PSObject adUser)
		{
			Dictionary<string, string> propDict = new Dictionary<string, string>();
			PSPropertyInfo[] infoArray = adUser.Properties.ToArray();
			PropertyCollection infoProps = adUser.Properties;
			foreach (PSPropertyInfo infoCouple in infoArray)
			{
				string key = infoCouple.Name.ToString();
				string val = "Undefined";
				var itemValue = infoCouple.Value;
				if (itemValue != null)
				{
					if ((itemValue as System.Collections.IEnumerable) != null)
					{
						Debug.WriteLine(itemValue.GetType().ToString());
						var enummed = itemValue as System.Collections.IEnumerable;
						Debug.WriteLine(key + ": " + enummed.ToString());
					}
					//string val = (infoCouple.Value == null) ? "UNDEFINED" : infoCouple.Value.ToString();
				}
				//propDict[key] = val;
				//Debug.WriteLine(key + ": " + val);
			}
			return propDict;
		}*/
		/*public static string GetDeepSearchUser(string data, UserInputWindow window)
		{
			window.LoadPercent = 0;
			data = data.ToLower();
			PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "WIT");
			PrincipalSearcher searcher = new PrincipalSearcher(new UserPrincipal(ctx));
			window.LoadPercent = 10;
			PrincipalSearchResult<Principal> allUsers = searcher.FindAll();
			Dispatcher.Invoke(() => window.LoadPercent = 50);
			double loadChange = 1 / allUsers.Count();
			foreach (Principal user in allUsers)
			{
				DirectoryEntry de = user.GetUnderlyingObject() as DirectoryEntry;
				string firstName = de.Properties["givenName"].Value.ToString().ToLower();
				string lastName = de.Properties["sn"].Value.ToString().ToLower();
				if (data.Contains(firstName) && data.Contains(lastName))
				{
					return user.Name.ToString();
				}
				window.LoadPercent += loadChange;
				Console.WriteLine("First Name: " + de.Properties["givenName"].Value);
				Console.WriteLine("Last Name : " + de.Properties["sn"].Value);
				Console.WriteLine("SAM account name   : " + de.Properties["samAccountName"].Value);
				Console.WriteLine("User principal name: " + de.Properties["userPrincipalName"].Value);
				Console.WriteLine();
			}
			return null;
		}*/
		public static string GetSuggestion(Dictionary<string,string> dict)
		{
			if (dict["enabled"].ToLower() == "false")
			{
				return "User is disabled. Make sure that they are a current student/staff/faculty member, and that they fit the current requirements for gaining account access.";
			}
			else if (dict["passwordExpired"].ToLower() == "false")
			{
				return "The user's password has expired. If they can provide you with their correct Wentworth ID number and all protocols are being followed, you can reset their password. This is almost certainly NOT a NetOps issue.";
			}
			else
			{
				return "The account looks all set!";
			}
		}
		public static bool ConfirmPasswordChange(string username, string password)
		{
			using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "WIT"))
			{
				// validate the credentials
				bool isValid = pc.ValidateCredentials(username, password);
				return isValid;
			}
		}
		public static Dictionary<string, string> GetUserInfoDict(string user)
		{
			//try
			//{
				Dictionary<string, string> props = new Dictionary<string, string>();
				PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "WIT");
				UserPrincipal u = UserPrincipal.FindByIdentity(ctx, user);
				DirectoryEntry underlyingObject = (DirectoryEntry)u.GetUnderlyingObject();

				string[] propsToGet = { "displayname", "created", "emailaddress", "employeeid", "enabled", "lastbadpasswordattempt", "lastlogondate", "lockedout", "logoncount", "modified", "memberof", "passwordexpired", "passwordlastset", "title" };
				
				DirectorySearcher deSearch = new DirectorySearcher(underlyingObject);
				foreach (string prop in propsToGet)
				{
					deSearch.PropertiesToLoad.Add(prop);
				}
				SearchResultCollection results = deSearch.FindAll();
				if (results != null && results.Count > 0)
				{
					ResultPropertyCollection rpc = results[0].Properties;
					//Debug.Write(rpc.PropertyNames.ToString());
					foreach (string rp in rpc.PropertyNames)
					{
						//Debug.WriteLine("RP thing: "+rp);
						if (propsToGet.Contains(rp))
						{
							props[rp] = rpc[rp][0].ToString();
							//Console.WriteLine(rpc[rp][0].ToString());
						}
					}

				}
				return props;
			/*}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			return null;*/
		}
	}
}
