
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using Newtonsoft.Json.Linq;


namespace OutsideServiceDeskFeed
{
    /// <summary>
    /// Summary description for ServiceDeskOutsideFeed
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]

    public class ServiceDeskOutsideFeed : System.Web.Services.WebService
    {

        /*
        [WebMethod]
        public void GetScanDetails(string scan)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.GetTagInfoJsonString(scan);

               HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">","").Replace("</string>",""));
            }
        }
        */
        
        [WebMethod]
        public void GetScanDetails(string scan, string api)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.GetTagInfo(scan,api);

                HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", ""));
            }
        }

        [WebMethod]
        public void GetOpenWorkorders(string api)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.GetOpenRequests(api);

                HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", ""));
            }
        }

        [WebMethod]
        public void GetOpenWorkorders2(string api)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.GetOpenRequests2(api);

                HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", ""));
            }
        }

        [WebMethod]
        public void GetWorkorderDetails(string WO,string api)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.GetWODetails(WO, api);

                HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", ""));
            }
        }

        [WebMethod]
        public void AddWorkLog(string Hours, string Minutes, string Description, string Owner, string Request, string api)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.AddWorkLog(Hours, Minutes, Description, Owner, Request, api);
                HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", ""));
            }
        }

        [WebMethod]
        public void AddNote(string isPublic, string noteText, string Request, string api)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.AddNote(isPublic, noteText, Request, api);
                HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", ""));
            }
        }
 
        [WebMethod]
        [ScriptMethod]
        public string AddNoteJson()
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                try
                {
                    StreamReader reader = new StreamReader(Context.Request.InputStream);
                    Context.Request.InputStream.Position = 0;
                    String Body = reader.ReadToEnd().ToString();
                    string isPublic = "", noteText = "", Request = "", api = "";
                    JObject json = JObject.Parse(Body);
                    isPublic = (string)json["isPublic"];
                    noteText = (string)json["Note"];
                    Request = (string)json["Request"];
                    api = (string)json["api"];


                    feed.AddNote(isPublic, noteText, Request, api);
                    return " Success";
                }
                catch(Exception ex)
                {
                    return "Failed: "+ ex;
                }
            }

        }

        [WebMethod]
        public void GetTechInfo(string api)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.GetTechInfo(api);

                HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", ""));
            }
        }

        [WebMethod]
        public void CloseWO(string Request, string tech, string AddTime, string CloseComments, string techName)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.CloseWO( Request,  tech,  AddTime,  CloseComments,  techName);
                HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", ""));
            }
        }


        [WebMethod]
        public void Reassign(string asset, string tech, string newUser)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.ReassignAsset(asset, tech, newUser);
                HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", ""));
            }
        }

        [WebMethod]
        public void GetAllRequesters(string api)
        {
            using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
            {
                string ret = feed.GetAllRequesters(api);
                HttpContext.Current.Response.Write(ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", ""));
            }
        }


    }
}
