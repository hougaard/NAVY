using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebMatrix.Data;

public partial class Download : System.Web.UI.Page
{
    protected void Parse_Page(object sender, EventArgs e)
    {
    }
    protected override void Render(System.Web.UI.HtmlTextWriter writer)
    {
        base.Render(writer);

        var db = Database.Open("navygetdb");
        dynamic res = null;
        if (Request.QueryString["name"] != null &&
            Request.QueryString["version"] != null)
        {
            // We got name and version number, easy :)
            res = db.QuerySingle("SELECT name,version,package FROM navyget_version WHERE name=@0 AND version = @1 and ispublic='true'",
                                 new object[] { Request.QueryString["name"], Request.QueryString["version"] });
        }
        else
        {
            if (Request.QueryString["name"] != null &&
                Request.QueryString["buildno"] != null)
            {
                // Only name and build no - lets find the best version for this buildno
                var res2 = db.Query("SELECT version FROM navyget_version WHERE name=@0 and ispublic='true' AND min_nav_buildno  < @1 AND max_nav_buildno > @2",
                                     new object[] { Request.QueryString["name"],
                                                    Request.QueryString["buildno"],
                                                    Request.QueryString["buildno"]});
                string BestVersion = "0";
                foreach(var c in res2)
                {
                    if (NAVYlib.Version.Compare(c.version, BestVersion) == 1)
                        BestVersion = c.version;
                }
                res = db.QuerySingle("SELECT name,version,package FROM navyget_version WHERE name=@0 AND version = @1 and ispublic='true'",
                                     new object[] { Request.QueryString["name"], BestVersion});

            }
            else
            {
                if (Request.QueryString["name"] != null)
                {
                    var res2 = db.Query("SELECT version FROM navyget_version WHERE name=@0 and ispublic='true'",
                                     new object[] { Request.QueryString["name"]});
                    string BestVersion = "0";
                    foreach (var c in res2)
                    {
                        if (NAVYlib.Version.Compare(c.version, BestVersion) == 1)
                            BestVersion = c.version;
                    }
                    res = db.QuerySingle("SELECT name,version,package FROM navyget_version WHERE name=@0 AND version = @1 and ispublic='true'",
                                         new object[] { Request.QueryString["name"], BestVersion });

                }
                else
                    throw new Exception("No search parameters supplied");
            }
        }
        if (res != null)
        {
            Page.Response.CacheControl = "No-cache";
            Page.Response.AddHeader("Pragma", "no-cache");
            Page.Response.Expires = -1;
            Page.Response.ContentType = "application/octet-stream";
            Page.Response.AddHeader("Content-Disposition", "inline; filename=" + res.name.Replace(' ','_') + "." + res.version + ".NAVY");
            Page.Response.BinaryWrite(res.package);
        }
    }
}