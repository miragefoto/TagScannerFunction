using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace OutsideServiceDeskFeed_WebApi.Controllers
{
    public class ValuesController : ApiController
    {

        //// GET api/values/5
        public string GetScanDetails(string scan, string api)
        {
            if (scan != "")
            {
                using (servicedeskfeeds.ServiceDesk1 feed = new servicedeskfeeds.ServiceDesk1())
                {
                    string ret = feed.GetTagInfo (scan, api );
                    ret = ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", "");
                    return ret;
                }
            }
            else
            {
                string ret = "Opps, Travis broke it";
                return ret.Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", "");
            }
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
