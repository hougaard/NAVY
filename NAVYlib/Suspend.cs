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
        public static bool Suspend(List<Parameter> parms)
        {
            FileStream fs = new FileStream(GetParameter("Name", parms) + ".NAVY", FileMode.Open);
            ZipArchive za = new ZipArchive(fs, ZipArchiveMode.Read);
            ZipArchiveEntry manifest = za.GetEntry("manifest.xml");
            Package p = PackageFile.Load(manifest.Open());

            try { Directory.Delete("DELTA", true); } catch { };
            Directory.CreateDirectory("DELTA");

            Console.WriteLine("Suspending {0} in database ...", p.App.Name);


            // Save data in fields
            Codeunit cu = new Codeunit(CodeunitMode.Backup);
            foreach (var delta in p.Payload.Deltas)
            {
                if (delta.Type == "Table")
                {
                    string FileName = delta.DeltaFile.Split('.')[0];

                    // Extract the delta from the ZIP
                    ZipArchiveEntry deltazip = za.GetEntry(delta.DeltaFile);
                    deltazip.ExtractToFile("DELTA\\" + delta.DeltaFile);
                    string DeltaTxt = File.ReadAllText("DELTA\\" + delta.DeltaFile);
                    if (DeltaTxt.IndexOf("ChangedElements=FieldCollection") != -1)
                    {
                        // Fields added, we are interested
                        foreach (var line in File.ReadAllLines("DELTA\\" + delta.DeltaFile))
                        {
                            int x = 0;
                            for (int i = 0; i < line.Length; i++)
                                if (line[i] == ';')
                                    x++;
                            if (line.IndexOf("                           { ") != -1 && x >= 4)
                            {
                                Match m = Regex.Match(line, @"(?<=\{.)\d{1,100}(?=\;)");
                                if (m.Success)
                                    cu.AddOperation(int.Parse(delta.ID), int.Parse(m.Groups[0].Value));
                            }
                        }
                    }
                }
            }
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string code = cu.ExportCode(configFile.AppSettings.Settings["ObjectNoForBackup"].Value);
            File.WriteAllText("BackupCodeunit.txt", code);

            using (PowerShell psi = PowerShell.Create())
            {
                psi.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned");
                psi.AddScript(NAVTools);

                psi.AddScript(@"Import-NAVApplicationObject -Confirm:$false BackupCodeunit.txt" +
                                " -DatabaseName " +
                                GetParameter("DatabaseName", parms));
                ExecutePowerShell(psi, true);
            }
            // Compiling
            using (PowerShell psi2 = PowerShell.Create())
            {
                psi2.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned");
                psi2.AddScript(NAVTools);
                Console.WriteLine("Preparing backup objects ...");
                psi2.AddScript("Compile-NAVApplicationObject " +
                                GetParameter("DatabaseName", parms));
                ExecutePowerShell(psi2, true);
            }
            // - Execute codeunit
            File.Delete("NAVY.success");
            Process.Start(configFile.AppSettings.Settings["ClientInstance"].Value + "/RunCodeunit?Codeunit=50000");

            Console.WriteLine("Awaiting backup codeunit completion...");
            while (!File.Exists("NAVY.success"))
            {
                Thread.Sleep(1000);
                Console.Write(".");
            };
            File.Delete("NAVY.success");
            fs.Close();
            za.Dispose();
            // Uninstall old version
            UnInstall(parms);

            Console.WriteLine("{0} has beeen suspended.", p.App.Name);

            return true;
        }
    }
}
