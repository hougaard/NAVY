using NAVYlib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace NAVY
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("NAVY Package Manager\n");
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (!File.Exists(configFile.AppSettings.Settings["Tools"].Value))
            {
                Console.WriteLine("Please update the NAVY.config file with correct information.\n");
                Console.WriteLine("(I tried, but it seems that the information in the NAVY.exe.config file is wrong :)");
                return;
            }
            Operations.NAVTools = String.Format(Operations.NAVTools, configFile.AppSettings.Settings["Tools"].Value,
                                              configFile.AppSettings.Settings["FinSql"].Value);

            if (args.Length == 0)
            {
                Console.WriteLine("NAVY <action> <paramters>\n");
                Console.WriteLine("Action codes: BUILD Name=<name> FOB=<file> TXT=<file> DELTAFILES=<filter>");
                Console.WriteLine("              SEARCH search=<query>");
                Console.WriteLine("              DOWNLOAD name=<packagename> version=<version> buildno=<navbuildno>");
                Console.WriteLine("              INSTALL Name=<navn> DatabaseName=<database>");
                Console.WriteLine("              UNINSTALL Name=<navn> DatabaseName=<database>");
                Console.WriteLine("              SUSPEND Name=<navn> DatabaseName=<database>");
                Console.WriteLine("              RESTORE Name=<navn> DatabaseName=<database>");
                Console.WriteLine("              UPGRADE Name=<navn> DatabaseName=<database>");
            }
            else
            {
                Console.WriteLine("Microsoft Dynamics NAV - Version and build: {0}",Operations.GetBuildNo());
                List<Parameter> parms = GetParameters(args);
                switch (args[0].ToLower())
                {
                    case "upgrade":
                        Operations.Upgrade(parms);
                        break;
                    case "build":
                        Operations.Build(parms);
                        break;
                    case "install":
                        Operations.Install(parms, true);
                        break;
                    case "uninstall":
                        Operations.UnInstall(parms);
                        break;
                    case "suspend":
                        Operations.Suspend(parms);
                        break;
                    case "restore":
                        Operations.Restore(parms);
                        break;
                    case "search":
                        Operations.Search(parms);
                        break;
                    case "download":
                        Operations.Download(parms);
                        break;
                    default:
                        Console.WriteLine("Unknown command, exiting");
                        break;
                }
                Console.WriteLine("Done, press any key");
                Console.ReadKey();
            }
        }
        private static List<Parameter> GetParameters(string[] args)
        {
            List<Parameter> result = new List<Parameter>();
            foreach (var s in args)
            {
                if (s.IndexOf("=") != -1)
                {
                    string[] ss = s.Split('=');
                    result.Add(new Parameter { Type = ss[0], Value = ss[1] });
                }
            }
            return result;
        }
    }
}
