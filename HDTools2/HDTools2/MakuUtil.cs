using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Diagnostics;

namespace HDTools2
{
	class MakuUtil
	{
		public static void ResetUserPassword(string user)
		{
			PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "WIT");
			UserPrincipal u = UserPrincipal.FindByIdentity(ctx, user);
			string WID = u.EmployeeId;
			string lastsix = WID.Substring(WID.Length - 6);
			string newPassword = @"WIT1$" + lastsix;
			Debug.Write(newPassword);
			u.SetPassword(newPassword);
		}
		public static Dictionary<string, string> GetUserInfoDict(string user)
		{
			try
			{
				Dictionary<string, string> props = new Dictionary<string, string>();
				PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "WIT");
				UserPrincipal u = UserPrincipal.FindByIdentity(ctx, user);
				DirectoryEntry underlyingObject = (DirectoryEntry)u.GetUnderlyingObject();
				var propNames = underlyingObject.Properties.PropertyNames;
				foreach (string propName in propNames)
				{
					Debug.WriteLine("YO: " + propName);
				}
				//Debug.Write(propNames);
				string[] propsToGet = { "name", "displayName", "whenCreated", /*"emailaddress",*/ "employeeid", "enabled", "badPasswordTime", "lastLogon", "lockoutTime", "logonCount", /*"modified", */"memberOf",/* "passwordexpired",*/ "pwdLastSet", "title" };

				DirectorySearcher deSearch = new DirectorySearcher(underlyingObject);
				foreach (string prop in propNames)
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
						//if (propsToGet.Contains(rp))
						//{
							props[rp.ToLower()] = rpc[rp][0].ToString();
							//Console.WriteLine(rpc[rp][0].ToString());
						//}
					}

				}
				if (u.AccountExpirationDate.HasValue)
				{
					props["expiration"] = u.AccountExpirationDate.Value.ToLocalTime().ToString();
					DateTime then = u.AccountExpirationDate.Value;
					DateTime now = DateTime.Now;
					int comparison = then.CompareTo(now);
					props["expired"] = (comparison < 0).ToString();
				}
				else
				{
					props["expiration"] = "Never";
					props["expired"] = "False";
				}
				if (underlyingObject.NativeGuid == null)
				{
					props["enabled"] = "False";
				}
				else
				{
					props["enabled"] = (!Convert.ToBoolean((int)underlyingObject.Properties["userAccountControl"].Value & 0x0002)).ToString();
				}
				
				return props;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			return null;
			/*PropertyCollection rawProps = underlyingObject.Properties;
			Dictionary<string, string> props = new Dictionary<string, string>();

			foreach (string prop in propsToGet)
			{
				props[prop] = rawProps[prop].Value.ToString();
			}
			return props;
			//string firstname = u.GivenName;
			//string lastname = u.Surname;
			//string email = u.EmailAddress;
			//string telephone = u.VoiceTelephoneNumber;
			*/
		}
	}
}
