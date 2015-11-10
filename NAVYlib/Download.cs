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
        public static void Download(List<Parameter> parms)
        {
            WebClient wc = new WebClient();
            string querystring = "?";
            string fileName = "";
            string Name = "";
            string Version = "";
            foreach (var p in parms)
            {
                switch (p.Type.ToLower())
                {
                    case "name":
                        if (querystring.Length > 1)
                            querystring += "&";
                        querystring += "name=" + WebUtility.UrlEncode(p.Value);
                        fileName = p.Value.Replace(' ', '_') + ".NAVY";
                        Name = p.Value;
                        break;
                    case "version":
                        if (querystring.Length > 1)
                            querystring += "&";
                        querystring += "version=" + WebUtility.UrlDecode(p.Value);
                        Version = p.Value;
                        break;
                }
            }
            Console.WriteLine("Downloading {0} as {1}", Name + " " + Version, fileName);
            wc.DownloadFile("http://localhost:1106/download.aspx" + querystring, fileName);
        }
    }
}
