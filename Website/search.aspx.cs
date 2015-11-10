using NAVYlib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using WebMatrix.Data;

public partial class search : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected override void Render(System.Web.UI.HtmlTextWriter writer)
    {
        base.Render(writer);

        var db = Database.Open("navygetdb");
        dynamic res = null;
        string selectQueryString;
        object[] parms = null;

        if (Request.QueryString["q"] != null)
        {
            selectQueryString = "SELECT name,description,version,owner FROM navyget_package WHERE name like @0 OR description like @1 ORDER BY Name";
            parms = new object[] { "%" + Request.QueryString["q"] + "%", "%" + Request.QueryString["q"] + "%" };
        }
        else
        {
            selectQueryString = "SELECT name,description,version,owner FROM navyget_package ORDER BY Name";
        }

        res = db.Query(selectQueryString, parms);
        List<OnlinePackage> packages = new List<OnlinePackage>();
        foreach(var r in res)
        {
            packages.Add(new OnlinePackage
            {
                name = r.name,
                description = r.description,
                owner = r.owner,
                version = r.version
            });
        }

        Utf8StringWriter sw = new Utf8StringWriter();
        XmlSerializer x = new XmlSerializer(typeof(List<OnlinePackage>));
        x.Serialize(sw, packages);

        Page.Response.CacheControl = "No-cache";
        Page.Response.AddHeader("Pragma", "no-cache");
        Page.Response.Expires = -1;
        Page.Response.ContentType = "application/xml";
        Page.Response.BinaryWrite(Encoding.UTF8.GetBytes(sw.ToString()));       
    }
}