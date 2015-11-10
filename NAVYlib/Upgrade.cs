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
        public static void Upgrade(List<Parameter> parms)
        {
            // Open beer
            // Download new version of package
            // Suspend existing
            // Install new version
            // Restore data back to existing fields
            // Drink beer
        }
    }
}
