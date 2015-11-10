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
        public static bool UnInstall(List<Parameter> parms)
        {
            FileStream fs = new FileStream(GetParameter("Name", parms) + ".NAVY", FileMode.Open);
            ZipArchive za = new ZipArchive(fs, ZipArchiveMode.Read);
            FileStream fs2 = new FileStream(GetParameter("Name", parms) + ".NAVY.Backup", FileMode.Open);
            ZipArchive zbackup = new ZipArchive(fs2, ZipArchiveMode.Read);
            ZipArchiveEntry manifest = za.GetEntry("manifest.xml");
            Package p = PackageFile.Load(manifest.Open());

            Console.WriteLine("Uninstalling {0} ...", p.App.Name);

            try { Directory.Delete("ORIGINAL", true); } catch { };
            Directory.CreateDirectory("ORIGINAL");

            using (PowerShell psi = PowerShell.Create())
            {
                psi.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned");
                psi.AddScript(NAVTools);

                foreach (var delta in p.Payload.Deltas)
                {
                    string FileName = delta.DeltaFile.Split('.')[0];

                    // Extract the original from the ZIP backup
                    ZipArchiveEntry zip = zbackup.GetEntry(FileName + ".TXT");
                    zip.ExtractToFile("ORIGINAL\\" + FileName + ".TXT");

                    Console.WriteLine(" - object {0} {1}", delta.Type, delta.ID);
                    psi.AddScript(@"Import-NAVApplicationObject -Confirm:$false ORIGINAL\\" +
                                    FileName +
                                    ".TXT -DatabaseName " +
                                    GetParameter("DatabaseName", parms));
                    ExecutePowerShell(psi, true);
                }
            }
            // Compiling
            using (PowerShell psi2 = PowerShell.Create())
            {
                psi2.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned");
                psi2.AddScript(NAVTools);
                Console.WriteLine("Compiling objects ...");
                psi2.AddScript("Compile-NAVApplicationObject " +
                                GetParameter("DatabaseName", parms) +
                                " -SynchronizeSchemaChanges Force");
                ExecutePowerShell(psi2, true);
            }
            return true;
        }
    }
}
