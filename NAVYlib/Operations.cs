using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Net;
using System.Xml.Serialization;
using NAVYlib;

namespace NAVYlib
{
    public static partial class Operations
    {
        public static string NAVTools = @"Import-Module '{0}' -ArgumentList '{1}' -DisableNameChecking -PassThru";
        private static int BuildNo = 0;
        public static int GetBuildNo()
        {
            if (BuildNo != 0)
                return BuildNo;

            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var Lines = File.ReadAllLines(configFile.AppSettings.Settings["Tools"].Value);
            // Search for:
            // ModuleVersion = '9.0.42815.0'
            foreach(var l in Lines)
            {
                if (l.IndexOf("ModuleVersion") != -1)
                {
                    Match m = Regex.Match(l, @"(?<=\')(\d{1,3})\.(\d{1,3})\.(\d{5,5})(?=\.\d{1}\')");
                    BuildNo = Int32.Parse(m.Groups[1].Value + m.Groups[2].Value + m.Groups[3].Value);
                    return BuildNo;
                }
            }
            return 0;
        }
        static void ExecutePowerShell(PowerShell psi, bool verbose)
        {
            Collection<PSObject> powershelloutput = psi.Invoke();

            foreach (PSObject outputItem in powershelloutput)
            {
                if (outputItem != null)
                {
                    if (verbose)
                    {
                        Console.WriteLine(outputItem.BaseObject.GetType().FullName);
                        Console.WriteLine(outputItem.BaseObject.ToString() + "\n");
                    }
                }
            }
            if (psi.Streams.Error.Count > 0)
            {
                foreach (var e in psi.Streams.Error)
                {
                    Console.WriteLine(e.Exception.Message);
                }
                Console.ReadKey();
            }
        }
        private static string GetParameter(string v, List<Parameter> parms)
        {
            foreach (var p in parms)
                if (p.Type.ToLower() == v.ToLower())
                    return p.Value;
            return "";
        }        
    }
    public class Parameter
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

}
