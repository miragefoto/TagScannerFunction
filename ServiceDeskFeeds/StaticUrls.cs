using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceDeskFeeds
{
    public static class StaticUrls
    {
        public static Uri GetOpenRequestsForUser = new Uri("http://servicedesktest:8080/sdpapi/request/?OPERATION_NAME=GET_REQUESTS&Content-Type=application/xml&TECHNICIAN_KEY=BDF5ED1B-11D7-4325-A138-79CF2789E2F6&OPERATION_NAME=read&format=json&INPUT_DATA= {\"operation\": {\"details\": {\"from\": \"0\",\"limit\": \"500\",\"filterby\": \"Open_User\"}}}");
        

    }
}