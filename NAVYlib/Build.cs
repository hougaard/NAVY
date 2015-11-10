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
        public static bool Build(List<Parameter> parms)
        {
            Console.WriteLine("Building NAVY Package for {0} Version {1}", GetParameter("Name", parms), GetParameter("Version", parms));
            FileStream fs = new FileStream(GetParameter("Name", parms) + ".NAVY", FileMode.Create);
            ZipArchive za = new ZipArchive(fs, ZipArchiveMode.Create);
            Package pack = new Package();
            pack.App = new App();
            pack.Payload = new Payload();
            pack.App.Id = Guid.NewGuid().ToString();
            pack.App.Name = GetParameter("Name", parms);
            pack.App.Version = GetParameter("Version", parms);
            pack.App.CompatibilityId = "";
            pack.App.Description = "";
            pack.App.Publisher = "";
            pack.Payload.Objects = new List<NAVObject>();
            pack.Payload.Deltas = new List<Delta>();
            int ImportOrder = 1;
            foreach (var p in parms)
            {
                if (p.Type == "FOB")
                {
                    pack.Payload.Objects.Add(new NAVObject
                    {
                        FileName = Path.GetFileName(p.Value),
                        Type = "FOB",
                        ImportOrder = ImportOrder.ToString()
                    });
                    Console.WriteLine("* Adding FOB: {0}", Path.GetFileName(p.Value));
                    za.CreateEntryFromFile(p.Value, Path.GetFileName(p.Value));
                    ImportOrder++;
                }
                if (p.Type == "TXT")
                {
                    pack.Payload.Objects.Add(new NAVObject
                    {
                        FileName = Path.GetFileName(p.Value),
                        Type = "TXT",
                        ImportOrder = ImportOrder.ToString()
                    });
                    Console.WriteLine("* Adding TXT: {0}", Path.GetFileName(p.Value));
                    za.CreateEntryFromFile(p.Value, Path.GetFileName(p.Value));
                    ImportOrder++;
                }
                if (p.Type == "DELTAFILES")
                {
                    foreach (var f in Directory.EnumerateFiles(p.Value, "*.DELTA"))
                    {
                        string Type = "";
                        string DeltaFile = Path.GetFileName(f);
                        switch (DeltaFile.Substring(0, 3))
                        {
                            case "TAB":
                                Type = "Table";
                                break;
                            case "COD":
                                Type = "Codeunit";
                                break;
                            case "PAG":
                                Type = "Page";
                                break;
                            case "REP":
                                Type = "Report";
                                break;
                            case "XML":
                                Type = "XMLport";
                                break;
                            case "QUE":
                                Type = "Query";
                                break;
                            case "MEN":
                                Type = "Menusuite";
                                break;
                        }
                        var id = DeltaFile.Substring(3, DeltaFile.IndexOf('.') - 3);
                        pack.Payload.Deltas.Add(new Delta
                        {
                            DeltaFile = DeltaFile,
                            Type = Type,
                            ID = id
                        });
                        Console.WriteLine("* Adding delta: {0}", DeltaFile);
                        za.CreateEntryFromFile(f, DeltaFile);
                    }
                }
            }
            var manifest = za.CreateEntry("manifest.xml");
            using (StreamWriter writer = new StreamWriter(manifest.Open()))
            {
                Console.WriteLine("* Adding manifest.xml");
                writer.Write(PackageFile.Save(pack));
            }
            za.Dispose();
            fs.Close();
            return true;
        }
    }
}
