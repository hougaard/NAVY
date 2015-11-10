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
        public static void Search(List<Parameter> parms)
        {
            WebClient wc = new WebClient();
            var xmlstr = wc.DownloadString("http://localhost:1106/search.aspx?q=" + GetParameter("search", parms));
            XmlSerializer xml = new XmlSerializer(typeof(List<OnlinePackage>));
            var s = new MemoryStream(Encoding.UTF8.GetBytes(xmlstr));
            List<OnlinePackage> PackageList = (List<OnlinePackage>)xml.Deserialize(s);
            Console.WriteLine("navyget.org - Search Result");
            Console.WriteLine("---------------------------");
            foreach (var p in PackageList)
            {
                Console.WriteLine("{0,-30} {1,-30} {2,-8} {3}", p.name, p.description?.Substring(0, p.description.Length < 30 ? p.description.Length : 30), p.version, p.owner);
            }
        }
    }
}
