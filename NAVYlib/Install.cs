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
        public static bool Install(List<Parameter> parms, bool InstallObjectPayload)
        {
            FileStream fs = new FileStream(GetParameter("Name", parms) + ".NAVY", FileMode.Open);
            ZipArchive za = new ZipArchive(fs, ZipArchiveMode.Update);
            FileStream fs2 = new FileStream(GetParameter("Name", parms) + ".NAVY.Backup", FileMode.Create);
            ZipArchive zbackup = new ZipArchive(fs2, ZipArchiveMode.Update);
            ZipArchiveEntry manifest = za.GetEntry("manifest.xml");
            Package p = PackageFile.Load(manifest.Open());
            using (PowerShell psi = PowerShell.Create())
            {
                psi.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned");
                psi.AddScript(NAVTools);

                File.Delete("object.txt");
                try { Directory.Delete("ORIGINAL", true); } catch { };
                try { Directory.Delete("DELTA", true); } catch { };
                try { Directory.Delete("RESULT", true); } catch { };
                Directory.CreateDirectory("ORIGINAL");
                Directory.CreateDirectory("DELTA");
                Directory.CreateDirectory("RESULT");
                Console.WriteLine("Exporting original objects from database...");
                foreach (var delta in p.Payload.Deltas)
                {
                    string FileName = delta.DeltaFile.Split('.')[0];

                    // Extract the delta from the ZIP
                    ZipArchiveEntry deltazip = za.GetEntry(delta.DeltaFile);
                    deltazip.ExtractToFile("DELTA\\" + delta.DeltaFile);

                    Console.WriteLine(" - object {0} {1}", delta.Type, delta.ID);
                    psi.AddScript("Export-NAVApplicationObject ORIGINAL\\" + FileName + ".TXT -DatabaseName " +
                                    GetParameter("DatabaseName", parms) +
                                    " -Force -Filter 'Type=" + delta.Type + ";Id=" + delta.ID + "'");
                    ExecutePowerShell(psi, false);
                    zbackup.CreateEntryFromFile("ORIGINAL\\" + FileName + ".TXT", FileName + ".TXT");
                }
                zbackup.Dispose();
                fs2.Close();

                Console.WriteLine("Applying deltas to objects...");
                psi.AddScript(@"Update-NAVApplicationObject -VersionListProperty FromModified -TargetPath ORIGINAL\*.txt -DeltaPath DELTA\*.delta –ResultPath RESULT\");
                ExecutePowerShell(psi, false);
            }

            if (InstallObjectPayload)
            {
                // Importing - First objects FOBs
                p.Payload.Objects.Sort(delegate (NAVObject a, NAVObject b)
                {
                    return a.ImportOrder.CompareTo(b.ImportOrder);
                });
                foreach (var fob in p.Payload.Objects)
                {
                    ZipArchiveEntry fobzip = za.GetEntry(fob.FileName);
                    fobzip.ExtractToFile(fob.FileName, true);

                    using (PowerShell psi2 = PowerShell.Create())
                    {
                        psi2.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned");
                        psi2.AddScript(NAVTools);

                        Console.WriteLine("Importing FOB objects to database ...");

                        Console.WriteLine(" - fob file {0}", fob.FileName);
                        psi2.AddScript(@"Import-NAVApplicationObject -Confirm:$false " +
                                        fob.FileName +
                                        " -ImportAction Overwrite -DatabaseName " +
                                        GetParameter("DatabaseName", parms));
                        ExecutePowerShell(psi2, true);
                    }
                }
            }
            using (PowerShell psi2 = PowerShell.Create())
            {
                psi2.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned");
                psi2.AddScript(NAVTools);

                Console.WriteLine("Importing new objects to database ...");
                foreach (var delta in p.Payload.Deltas)
                {
                    string FileName = delta.DeltaFile.Split('.')[0];
                    Console.WriteLine(" - object {0} {1}", delta.Type, delta.ID);
                    psi2.AddScript(@"Import-NAVApplicationObject -Confirm:$false RESULT\\" +
                                    FileName +
                                    ".TXT -DatabaseName " +
                                    GetParameter("DatabaseName", parms));
                    ExecutePowerShell(psi2, true);
                }
            }

            // Compiling
            using (PowerShell psi2 = PowerShell.Create())
            {
                psi2.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned");
                psi2.AddScript(NAVTools);
                Console.WriteLine("Compiling objects ...");
                psi2.AddScript("Compile-NAVApplicationObject " +
                                GetParameter("DatabaseName", parms) + " -SynchronizeSchemaChanges Force");
                ExecutePowerShell(psi2, true);
            }
            za.Dispose();
            fs.Close();
            return true;
        }
    }
}
