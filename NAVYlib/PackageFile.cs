using NAVYlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NAVYlib
{
    public static class PackageFile
    {
        public static Package Load(string FileName)
        {
            var x = XDocument.Load(FileName);
            return ReadContent(x);
        }
        public static Package Load(Stream s)
        {
            var x = XDocument.Load(s);
            return ReadContent(x);
        }
        private static Package ReadContent(XDocument x)
        { 
            Package p = (from _package in x.Elements("Package")
                         select new Package
                         {
                             App = (from _app in _package.Elements("App")
                                    select new App
                                    {
                                        CompatibilityId = _app.Attribute("CompatibilityId").Value,
                                        Description = _app.Attribute("Description").Value,
                                        Id = _app.Attribute("Id").Value,
                                        Name = _app.Attribute("Name").Value,
                                        Publisher = _app.Attribute("Publisher").Value,
                                        Version = _app.Attribute("Version").Value
                                    }).First(),
                             Payload = (from _payload in _package.Elements("Payload")
                                        let _objects = _payload.Element("Objects")
                                        let _deltas = _payload.Elements("Deltas")
                                        select new Payload
                                        {
                                            Objects = (from _object in _objects.Elements("NAVObject")
                                                       select new NAVObject
                                                       {
                                                           FileName = _object.Attribute("FileName").Value,
                                                           ImportOrder = _object.Attribute("ImportOrder").Value,
                                                           Type = _object.Attribute("Type").Value
                                                       }).ToList(),
                                            Deltas = (from _delta in _deltas.Elements("Delta")
                                                      select new Delta
                                                      {
                                                          DeltaFile = _delta.Attribute("DeltaFile").Value,
                                                          ID = _delta.Attribute("ID").Value,
                                                          Type = _delta.Attribute("Type").Value
                                                      }).ToList()
                                        }).First()
                         }).First();
            return p;    
        }
        public static string Save(Package p)
        {
            Utf8StringWriter sw = new Utf8StringWriter();
            XmlSerializer x = new XmlSerializer(typeof(Package));
            x.Serialize(sw, p);
            return sw.ToString();
        }
    }
    [Serializable]
    public class Package
    {
        public App App { get; set; }
        public Payload Payload { get; set; }
    }
    [Serializable]
    public class Payload
    {
        public List<NAVObject> Objects { get; set; }
        public List<Delta> Deltas { get; set; }
    }
    [Serializable]
    public class NAVObject
    {
        [XmlAttribute]
        public string FileName { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string ImportOrder { get; set; }
    }
    [Serializable]
    public class Delta
    {
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public string DeltaFile { get; set; }
    }
    [Serializable]
    public class App
    {
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Publisher { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlAttribute]
        public string Version { get; set; }
        [XmlAttribute]
        public string CompatibilityId { get; set; }
    }
}
