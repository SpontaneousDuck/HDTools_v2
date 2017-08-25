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
		public static Dictionary<string, string> GetUserInfoDict(string user)
		{
			try
			{
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
