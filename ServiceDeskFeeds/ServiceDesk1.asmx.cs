using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Xml;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Configuration;
using System.Text.RegularExpressions;

namespace ServiceDeskFeeds
{
    /// <summary>
    /// Summary description for ServiceDesk1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class ServiceDesk1 : System.Web.Services.WebService
    {
        string baseUrl = ConfigurationManager.AppSettings["BaseUrl"];

        [WebMethod]
        public string GetTagInfo(string tag, string tech)
        {
            string[] item = GetScanType(tag, tech);
           

            string CI_Name = item[0];
            string CI_Type = item[1];

            //Get Details from CI
            string xmlString = System.IO.File.ReadAllText(Server.MapPath("APIFiles/GetCIByTag.xml"));
            xmlString = xmlString.Replace("[citype]", item[1]).Replace("[Asset Tag]", tag);
            string url = baseUrl + "api/cmdb/ci/?OPERATION_NAME=read&TECHNICIAN_KEY=" + tech;

            string retVal = PostXml(xmlString, url);
            TagInfo info = new TagInfo();
            JObject rss = JObject.Parse(retVal);
            List<BaseInfo> baseInfo = new List<BaseInfo>();

            var field = from p in rss["API"]["response"]["operation"]["Details"]["field-names"]["name"]
                        select ((string)p["content"] + "^" + (string)p["type"]);

            int x = 0;
            foreach (var it in field)
            {
                BaseInfo ba = new BaseInfo
                {
                    FieldName = it.Split('^')[0],
                    FieldType = it.Split('^')[1],
                    Value = (string)rss["API"]["response"]["operation"]["Details"]["field-values"]["record"]["value"][x]
                };
                if (ba.FieldType == "Date")
                {
                    try
                    {
                        ba.Value = UnixTimeStampToDateTime(ba.Value);
                    }
                    catch { }
                }
                if (ba.Value != "(null)")
                {
                    baseInfo.Add(ba);
                }

                x++;
            }

            try
            {
                string getUrl = baseUrl + "api/cmdb/cirelationships/" + CI_Name + "/?OPERATION_NAME=read&TECHNICIAN_KEY=" + tech;
                retVal = GetXml(getUrl);

                JObject typeObj = JObject.Parse(retVal);
                string UsedBy = (string)typeObj["API"]["response"]["operation"]["Details"]["relationships"]["relationship"]["ci"]["name"];

                BaseInfo ba = new BaseInfo
                {
                    FieldName = "Used By",
                    FieldType = "string",
                    Value = UsedBy
                };

                baseInfo.Add(ba);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }


            return JsonConvert.SerializeObject(baseInfo);
        }

        [WebMethod]
        public string GetOpenRequests(string tech)
        {
            List<RequestDetail> requests = new List<RequestDetail>();
            string GetUrl = baseUrl + "sdpapi/request/?OPERATION_NAME=GET_REQUESTS&Content-Type=application/xml&TECHNICIAN_KEY=" + tech + "&format=json&INPUT_DATA=";
            string GetParams = "{\"operation\": {\"details\": {\"from\": \"0\",\"limit\": \"500\",\"filterby\": \"Open_User\"}}}";

            string retVal = GetXml(GetUrl + GetParams);
            List<BaseInfo> baseInfo = new List<BaseInfo>();
            //Get the xml without extra data
            JObject typeObj = JObject.Parse(retVal);
            try
            {
                var requestsJson = from p in typeObj["operation"]["details"]
                                   select (p);
                foreach (var it in requestsJson)
                {
                    RequestDetail det = new RequestDetail();
                    det = JsonConvert.DeserializeObject<RequestDetail>(it.ToString().Replace("{{", "{").Replace("}}", "}"));
                    det.CREATEDTIME = UnixTimeStampToDateTime(det.CREATEDTIME);
                    det.DUEBYTIME = UnixTimeStampToDateTime(det.DUEBYTIME);

                    requests.Add(det);


                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return JsonConvert.SerializeObject(requests);

        }

        [WebMethod]
        public string GetOpenRequests2(string tech)
        {
            List<WOList> woList = new List<WOList>();
            List<BaseInfo> baseInfo = new List<BaseInfo>();


            string GetUrl = baseUrl + "sdpapi/request/?OPERATION_NAME=GET_REQUESTS&Content-Type=application/xml&TECHNICIAN_KEY=" + tech + "&format=json&INPUT_DATA=";
            string GetParams = "{\"operation\": {\"details\": {\"from\": \"0\",\"limit\": \"500\",\"filterby\": \"Open_User\"}}}";
            string retVal = GetXml(GetUrl + GetParams);

            //Get the xml without extra data
            JObject typeObj = JObject.Parse(retVal);
            try
            {
                var requestsJson = from p in typeObj["operation"]["details"]
                                   select (p);
                foreach (var it in requestsJson)
                {
                    WOList thisWO = new WOList();
                    thisWO.baseInfo = new List<BaseInfo>();
                    thisWO.DEPARTMENT = "N/A";

                    //WORKORDERID
                    try
                    {
                        //WORKORDERID
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "WORKORDERID",
                            FieldType = "string",
                            Value = (string)it["WORKORDERID"],
                        };
                        thisWO.baseInfo.Add(ba);
                        thisWO.WORKORDERID = ba.Value;

                    }
                    catch { }


                    //SUBJECT
                    try
                    {
                        //SUBJECT
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "SUBJECT",
                            FieldType = "string",
                            Value = (string)it["SUBJECT"],
                        };
                        thisWO.baseInfo.Add(ba);

                    }
                    catch { }
                    //REQUESTER
                    try
                    {
                        //REQUESTER
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "REQUESTER",
                            FieldType = "string",
                            Value = (string)it["REQUESTER"],
                        };
                        thisWO.baseInfo.Add(ba);

                    }
                    catch { }

                    //CREATEDBY
                    try
                    {
                        //CREATEDBY
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "CREATEDBY",
                            FieldType = "string",
                            Value = (string)it["CREATEDBY"],
                        };
                        thisWO.baseInfo.Add(ba);

                    }
                    catch { }

                    // Get Requestor Details
                    thisWO = AddRequesterDetailsByName((string)it["REQUESTER"], thisWO, tech);

                    woList.Add(thisWO);
                }

                woList = woList.OrderBy(si => si.DEPARTMENT).ToList();

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            //return JsonConvert.SerializeObject(requests);
            return JsonConvert.SerializeObject(woList);
        }

        [WebMethod]
        public string GetAllRequesters(string tech)
        {
            
            List<string> names = new List<string>();

            string PostUrl = baseUrl + "api/cmdb/ci/?OPERATION_NAME=read&Content-Type=application/xml&format=json&TECHNICIAN_KEY=" + tech;
            string body = System.IO.File.ReadAllText(Server.MapPath("APIFiles/GetAllRequesters.xml"));
            string retVal = PostXml(body, PostUrl);

            JObject typeObj = JObject.Parse(retVal);
            var requestsJson = from p in typeObj["API"]["response"]["operation"]["Details"]["field-values"]["record"] select (p);
            foreach (var it in requestsJson)
            {
                try
                {
                    names.Add((string)it["value"][0]);
                }
                catch { }
            }

            string GetUrl = baseUrl + "sdpapi/technician/?OPERATION_NAME=GET_ALL&TECHNICIAN_KEY=" + tech + "&INPUT_DATA=<operation><Details><siteName></siteName><groupid></groupid></Details></operation>";
            string retVal1 = GetXml(GetUrl);
            string techs = retVal1.Replace("\r", "").Replace("\n", "");
            names = ParseTechXml(techs, names);

            
            names = names.OrderBy(q => q).ToList();
            List<string> cleanNames= new List<string>();
            cleanNames.AddRange(names.Distinct());
            return JsonConvert.SerializeObject(cleanNames);

        }

        [WebMethod]
        public string GetWODetails(string WO, string tech)
        {
            //st<RequestDetail> requests = new List<RequestDetail>();

            string GetUrl = baseUrl + "api/v3/requests/" + WO + "?&TECHNICIAN_KEY=" + tech + "&OPERATION_NAME=read&format=json";
            List<BaseInfo> baseInfo = new List<BaseInfo>();
            try
            {
                string retVal = GetXml(GetUrl.Replace("{request_id}", WO));
                if (retVal.Contains("not supported"))
                {  //FALL BACK TO V1 API FOR OLD INSTALL
                    retVal = GetXml("http://servicedesk/sdpapi/request/" + WO + "?OPERATION_NAME=GET_REQUEST&TECHNICIAN_KEY=" + tech + "&format=json");

                    JObject rss1 = JObject.Parse(retVal);
                    try
                    {
                        //WO Number
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "WO Number",
                            FieldType = "string",
                            Value = WO,
                        };
                        baseInfo.Add(ba);
                    }
                    catch { }
                    try
                    {
                        //SUBJECT
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "SUBJECT",
                            FieldType = "string",
                            Value = (string)rss1["SUBJECT"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //TECHNICIAN
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "TECHNICIAN",
                            FieldType = "string",
                            Value = (string)rss1["TECHNICIAN"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //STATUS
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "STATUS",
                            FieldType = "string",
                            Value = (string)rss1["STATUS"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //CREATEDTIME
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "CREATED TIME",
                            FieldType = "datetime",
                            Value = UnixTimeStampToDateTime((string)rss1["CREATEDTIME"]),
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //CREATEDBY
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "CREATEDBY",
                            FieldType = "string",
                            Value = (string)rss1["CREATEDBY"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //REQUESTER 
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "REQUESTER",
                            FieldType = "string",
                            Value = (string)rss1["REQUESTER"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //DEPARTMENT
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "DEPARTMENT",
                            FieldType = "string",
                            Value = (string)rss1["DEPARTMENT"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //DESCRIPTION
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "Description",
                            FieldType = "string",
                            Value = (string)rss1["DESCRIPTION"],
                        };

                        if (ba.Value.Contains("<img"))
                        {
                            List<BaseInfo> tmpList = ReplaceImages(ba);
                            foreach (BaseInfo x in tmpList)
                            {
                                baseInfo.Add(x);
                            }
                        }
                        else
                        {
                            baseInfo.Add(ba);
                        }
                    }
                    catch { }
                    try
                    {
                        //DESCRIPTION
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "SHORT DESC",
                            FieldType = "string",
                            Value = (string)rss1["SHORTDESCRIPTION"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }

                    try
                    {
                        //PRIORITY
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "PRIORITY",
                            FieldType = "string",
                            Value = (string)rss1["PRIORITY"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //SUBCATEGORY
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "SUBCATEGORY",
                            FieldType = "string",
                            Value = (string)rss1["SUBCATEGORY"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //CATEGORY
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "CATEGORY",
                            FieldType = "string",
                            Value = (string)rss1["CATEGORY"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }

                    try
                    {
                        //IMPACT
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "IMPACT",
                            FieldType = "string",
                            Value = (string)rss1["IMPACT"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //DUEBYTIME
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "DUE BY",
                            FieldType = "datetime",
                            Value = UnixTimeStampToDateTime((string)rss1["DUEBYTIME"]),
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //Assigned Time
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "CATEGORY",
                            FieldType = "string",
                            Value = (string)rss1["CATEGORY"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }
                    try
                    {
                        //Assigned Time
                        BaseInfo ba = new BaseInfo
                        {
                            FieldName = "CATEGORY",
                            FieldType = "string",
                            Value = (string)rss1["CATEGORY"],
                        };
                        baseInfo.Add(ba);

                    }
                    catch { }



                    //   baseInfo = AddRequesterDetails((string)rss["request"]["requester"]["id"], baseInfo, tech);

                    return JsonConvert.SerializeObject(baseInfo);



                }

                JObject rss = JObject.Parse(retVal);
                try
                {
                    //WO Number
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "WO Number",
                        FieldType = "string",
                        Value = WO,
                    };
                    baseInfo.Add(ba);
                }
                catch { }
                try
                {
                    //Impact
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Impact",
                        FieldType = "string",
                        Value = (string)rss["request"]["impact"]["name"],
                    };
                    baseInfo.Add(ba);

                }
                catch { }
                try
                {
                    //Urgency
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Urgency",
                        FieldType = "string",
                        Value = (string)rss["request"]["urgency"]["name"],
                    };
                    baseInfo.Add(ba);

                }
                catch { }
                try
                {
                    //Description
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Description",
                        FieldType = "string",
                        Value = (string)rss["request"]["description"],
                    };
                    //Get the images
                    if (ba.Value.Contains("<img"))
                    {
                        List<BaseInfo> tmpList = ReplaceImages(ba);
                        foreach (BaseInfo x in tmpList)
                        {
                            baseInfo.Add(x);
                        }
                    }
                    else
                    {
                        baseInfo.Add(ba);
                    }


                }
                catch { }
                try
                {
                    //Priority
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Priority",
                        FieldType = "string",
                        Value = (string)rss["request"]["priority"]["name"],
                    };
                    baseInfo.Add(ba);

                }
                catch { }
                try
                {
                    //Created By
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Creator",
                        FieldType = "string",
                        Value = (string)rss["request"]["created_by"]["name"],
                    };
                    baseInfo.Add(ba);

                }
                catch { }
                try
                {
                    //Created By Email
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Creator Email",
                        FieldType = "string",
                        Value = (string)rss["request"]["created_by"]["email_id"],
                    };
                    baseInfo.Add(ba);

                }
                catch { }
                try
                {
                    //Requester 
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Requester",
                        FieldType = "string",
                        Value = (string)rss["request"]["requester"]["name"],
                    };
                    baseInfo.Add(ba);

                }
                catch { }
                try
                {
                    //Requester Email
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Requester Email",
                        FieldType = "string",
                        Value = (string)rss["request"]["requester"]["email_id"],
                    };
                    baseInfo.Add(ba);

                }
                catch { }
                try
                {
                    //Department
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Department",
                        FieldType = "string",
                        Value = (string)rss["request"]["department"]["name"],
                    };
                    baseInfo.Add(ba);

                }
                catch { }
                try
                {
                    //Subject
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Subject",
                        FieldType = "string",
                        Value = (string)rss["request"]["subject"]["name"],
                    };
                    baseInfo.Add(ba);

                }
                catch { }
                try
                {
                    //Assigned Time
                    BaseInfo ba = new BaseInfo
                    {
                        FieldName = "Assigned",
                        FieldType = "string",
                        Value = (string)rss["request"]["assigned_time"]["display_value"],
                    };
                    baseInfo.Add(ba);

                }
                catch { }

                baseInfo = AddRequesterDetails((string)rss["request"]["requester"]["id"], baseInfo, tech);

                return JsonConvert.SerializeObject(baseInfo);

            }
            catch (Exception ex)
            {
                //return JsonConvert.ToString(baseInfo);
                return "[{\"Error\" : \" Failure to convert JSON:\"" + ex + "}";
            }
        }


        [WebMethod]
        public string AddWorkLog(string Hours, string Minutes, string Description, string Owner, string Request, string tech)
        {
            Hours = (Hours ?? "") == "" ? "0" : Hours;
            Minutes = (Minutes ?? "") == "" ? "0" : Minutes;
            if (Hours == "0" && Minutes == "0") { Minutes = "15"; }

            try
            {
                string Url = baseUrl + "sdpapi/request/" + Request + "/worklogs?OPERATION_NAME=ADD_WORKLOG&TECHNICIAN_KEY=" + tech;
                // string InputData = "INPUT_DATA=<API><Operation><Details><Worklogs><Worklog><description>" + Description + "</description><technician>" + Owner + "</technician><workMinutes>" + Minutes + "</workMinutes><workHours>" + Hours + "</workHours></Worklog></Worklogs></Details></Operation></API>";
                string xmlString = "INPUT_DATA=" + System.IO.File.ReadAllText(Server.MapPath("APIFiles/AddWorkLog.xml"));
                string inputData = string.Format(xmlString, Description, Owner, Minutes, Hours);
                var retVar = PostXmlFormEncoded(inputData, Url);
                return "{\"Status\": \"Success\"}";
            }
            catch (Exception ex)
            {
                return "{\"Status\": \"Error-" + ex.ToString() + "\"}";
            }
        }

        [WebMethod]
        public string AddNote(string isPublic, string noteText, string Request, string tech)
        {
            try
            {
                string Url = baseUrl + "sdpapi/request/" + Request + "/notes?OPERATION_NAME=ADD_NOTE&TECHNICIAN_KEY=" + tech;
                //string InputData = "INPUT_DATA=<Operation><Details><Notes><Note><isPublic>" + isPublic + "</isPublic><notesText>" + noteText + "</notesText></Note></Notes></Details></Operation>";
                string xmlString = "INPUT_DATA=" + System.IO.File.ReadAllText(Server.MapPath("APIFiles/AddNote.xml"));
                string inputData = string.Format(xmlString, isPublic, noteText);
                var retVar = PostXmlFormEncoded(inputData, Url);

                return "{\"Status\": \"Success\"}";
            }
            catch (Exception ex)
            {
                return "{\"Status\": \"" + ex.ToString() + "\"}";
            }
        }

        [WebMethod]
        public string ReassignAsset(string asset, string tech, string newUser)//, string department)
        {
            string result = "{\"Result\":\"Success\"}";
            string url = "api/cmdb/ci/?OPERATION_NAME=update&TECHNICIAN_KEY=" + tech;
            // Is it a workstation
            string[] item = GetScanType(asset, tech);
            string xml = System.IO.File.ReadAllText(Server.MapPath("APIFiles/UpdateOtherAsset.xml"));
            if (item[1] == "Workstation")
            {
                xml = System.IO.File.ReadAllText(Server.MapPath("APIFiles/AssignITAsset.xml"));
            }
            xml = xml.Replace("{0}", item[1]).Replace("{1}", item[0]).Replace("{2}", "In Use").Replace("{3}", DateTime.Now.ToString("yyy-MM-dd")).Replace("{4}",newUser);//.Replace("{5}",department);

            var retVal = PostXml(xml, baseUrl + url);

            if (retVal.Contains("Success"))
            {
                return result;
            }
            return "{\"Result\":\"There was an Error - "+ retVal.ToString()+"\"}"; 
        }

        [WebMethod]
        public string CloseWO(string Request, string tech, string AddTime, string CloseComments, string techName)
        {
            try
            {
                if (AddTime == "true" || AddTime == "True")
                {
                    AddWorkLog("0", "15", "Closing, Completed", techName, Request, tech);
                }

                string Url = baseUrl + String.Format("sdpapi/request/{0}/?OPERATION_NAME=CLOSE_REQUEST&Content-Type=application/xml&format=json&TECHNICIAN_KEY={1}", Request, tech);
                string bd = System.IO.File.ReadAllText(Server.MapPath("APIFiles/CloseWO.txt"));
                string body = bd.Replace("{0}", CloseComments);

                return PostJson(body, Url);
            }
            catch (Exception ex)
            {
                return "{\"Status\": \"" + ex.ToString() + "\"}";
            }
        }

        [WebMethod]
        public string GetTechInfo(string api)
        {
            //First Hit to get a workorder for this user
            List<RequestDetail> requests = new List<RequestDetail>();
            string GetUrl = baseUrl + "sdpapi/request/?OPERATION_NAME=GET_REQUESTS&Content-Type=application/xml&TECHNICIAN_KEY=" + api + "&OPERATION_NAME=read&format=json&INPUT_DATA=";
            string GetParams = "{\"operation\": {\"details\": {\"from\": \"0\",\"limit\": \"1\",\"filterby\": \"All_User\"}}}";
            string retVal1 = GetXml(GetUrl + GetParams);

            if (retVal1.Contains("Authentication failed"))
            {
                return "{\"error\":\"API Key not correct\"}";
            }

            var rss = JObject.Parse(retVal1);
            var tech = (string)rss["operation"]["details"][0]["TECHNICIAN"];

            //Next compare names and get tech Id
            GetUrl = baseUrl + "sdpapi/technician/?OPERATION_NAME=GET_ALL&TECHNICIAN_KEY=" + api + "&INPUT_DATA=<operation><Details><siteName></siteName><groupid></groupid></Details></operation>";
            retVal1 = GetXml(GetUrl);
            string test = retVal1.Replace("\r", "").Replace("\n", "");
            Technician thisTech = ParseXml(test, tech);

            string body = "{'operation':{'details':{'userid':'" + thisTech.parameters[0].value + "'}}}";
            string url = baseUrl + "sdpapi/admin/techician/" + thisTech.parameters[0].value + "/?format=json&OPERATION_NAME=GET_ALL&TECHNICIAN_KEY=" + api + "&INPUT_DATA=";

            return JsonConvert.SerializeObject(thisTech);
        }


        #region "Work Methods"
        /// <summary>
        /// This handles the post operations
        /// </summary>
        /// <param name="body"></param>
        /// <param name="myUrl"></param>
        /// <returns></returns>
        public string PostXml(string body, string myUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(myUrl));
            request.Method = "POST";
            request.ContentType = "application/xml";

            byte[] bytes = Encoding.UTF8.GetBytes(body);

            request.ContentLength = bytes.Length;

            using (Stream putStream = request.GetRequestStream())
            {
                putStream.Write(bytes, 0, bytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        public string PostJson(string body, string url)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            request.Method = "POST";
            request.ContentType = "application/json";

            byte[] bytes = Encoding.UTF8.GetBytes(body);
            request.ContentLength = bytes.Length;

            using (Stream putStream = request.GetRequestStream())
            {
                putStream.Write(bytes, 0, bytes.Length);
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        public string PostXmlFormEncoded(string body, string myUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(myUrl));
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            byte[] bytes = Encoding.UTF8.GetBytes(body);

            request.ContentLength = bytes.Length;

            using (Stream putStream = request.GetRequestStream())
            {
                putStream.Write(bytes, 0, bytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }


        /// <summary>
        /// Handles the Get operations to the ServiceDeskAPI
        /// </summary>
        /// <param name="myUrl"></param>
        /// <returns></returns>
        public string GetXml(string myUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(myUrl));
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }


        public static string UnixTimeStampToDateTime(string unixTimeStamp)
        {
            //Is Date Valid
            double newDouble;
            if (double.TryParse(unixTimeStamp, out newDouble))
            {
                // Unix timestamp is seconds past epoch
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddMilliseconds(newDouble).ToLocalTime();
                try
                {
                    return dtDateTime.ToString("MM/dd/yyyy");
                }
                catch
                {
                    return unixTimeStamp;
                }
            }
            else
            {
                return unixTimeStamp;
            }

        }

        public List<BaseInfo> AddRequesterDetails(String UserId, List<BaseInfo> baseInfo, string tech)
        {
            string url = baseUrl + "sdpapi/requester?format=json&OPERATION_NAME=GET_ALL&TECHNICIAN_KEY=" + tech + "&INPUT_DATA=";
            string body = "{'operation':{'details':{'userid':'" + UserId + "'}}}";

            var retVal = GetXml(url + body);
            JObject typeObj = JObject.Parse(retVal);
            try
            {
                //Created By Email
                BaseInfo ba = new BaseInfo
                {
                    FieldName = "Requester Department",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["department"],
                };
                if (ba.Value != "")
                {
                    baseInfo.Add(ba);
                }

            }
            catch { }
            try
            {
                //AD Name
                BaseInfo ba = new BaseInfo
                {
                    FieldName = "AD Name",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["loginname"],
                };
                if (ba.Value != "")
                {
                    baseInfo.Add(ba);
                }

            }
            catch { }
            try
            {
                //jobtitle
                BaseInfo ba = new BaseInfo
                {
                    FieldName = "Job Title",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["jobtitle"],
                };
                if (ba.Value != "")
                {
                    baseInfo.Add(ba);
                }

            }
            catch { }
            try
            {
                //Phone
                BaseInfo ba = new BaseInfo
                {
                    FieldName = "Requester Phone",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["landline"],
                };
                if (ba.Value != "")
                {
                    baseInfo.Add(ba);
                }

            }
            catch { }
            try
            {
                //Mobile
                BaseInfo ba = new BaseInfo
                {
                    FieldName = "Requester Mobile",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["mobile"],
                };
                if (ba.Value != "")
                {
                    baseInfo.Add(ba);
                }

            }
            catch { }
            return baseInfo;
        }

        public WOList AddRequesterDetailsByName(String name, WOList thisWO, string tech)
        {

            string url = baseUrl + "sdpapi/requester/?OPERATION_NAME=GET_ALL&Content-Type=application/xml&format=json&TECHNICIAN_KEY=" + tech + "&INPUT_DATA=";
            string body = "{'operation':{'details':{'name':'" + name + "'}}}";

            var retVal = GetXml(url + body);
            JObject typeObj = JObject.Parse(retVal);
            try
            {
                //Department
                BaseInfo ba = new BaseInfo()
                {
                    FieldName = "Department",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["department"],
                };
                if (ba.Value != "")
                {
                    thisWO.baseInfo.Add(ba);
                    thisWO.DEPARTMENT = ba.Value;
                }

            }
            catch { }

            try
            {
                //jobtitle
                BaseInfo ba = new BaseInfo()
                {
                    FieldName = "Job Title",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["jobtitle"],
                };
                if (ba.Value != "")
                {
                    thisWO.baseInfo.Add(ba);
                }

            }
            catch { }
            try
            {
                //Phone
                BaseInfo ba = new BaseInfo
                {
                    FieldName = "Requester Phone",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["landline"],
                };
                if (ba.Value != "")
                {
                    thisWO.baseInfo.Add(ba);
                }

            }
            catch { }
            try
            {
                //Mobile
                BaseInfo ba = new BaseInfo
                {
                    FieldName = "Requester Mobile",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["mobile"],
                };
                if (ba.Value != "")
                {
                    thisWO.baseInfo.Add(ba);
                }

            }
            catch { }
            try
            {
                //emailid
                BaseInfo ba = new BaseInfo
                {
                    FieldName = "Requester Email",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["emailid"],
                };
                if (ba.Value != "")
                {
                    thisWO.baseInfo.Add(ba);
                }

            }
            catch { }
            return thisWO;
        }

        public Technician ParseXml(string xml, string name)
        {
            Technician tech = new Technician();
            var doc = XDocument.Parse(xml);
            XNamespace nonamespace = XNamespace.None;
            var xmlTechs = doc.Descendants(nonamespace + "record");
            foreach (var item in xmlTechs)
            {
                Technician aTech = new Technician();
                try
                {
                    aTech.URI = item.Attribute("URI").ToString();

                    var newx = item.Descendants("parameter");
                    aTech.parameters = new List<Parameter>();
                    foreach (var item2 in newx)
                    {
                        Parameter p = new Parameter
                        {
                            name = item2.Element("name").Value,
                            value = item2.Element("value").Value
                        };
                        aTech.parameters.Add(p);
                        if (p.value == name)
                        {
                            return aTech;
                        }
                    }
                }
                catch { }
            }
            return tech;
        }

        public List<string> ParseTechXml(string xml, List<string> names)
        {
            
            var doc = XDocument.Parse(xml);
            XNamespace nonamespace = XNamespace.None;
            var xmlTechs = doc.Descendants(nonamespace + "record");
            foreach (var item in xmlTechs)
            {
                try
                {
                    var newx = item.Descendants("parameter");
                    foreach (var item2 in newx)
                    {
                        if (item2.Element("name").Value == "technicianname")
                        {
                            names.Add(item2.Element("value").Value);
                        }
                    }
                }
                catch { }
            }
            return names;
        }


        public BaseInfo AddRequesterDepartment(String UserId, string tech)
        {
            string url = baseUrl + "sdpapi/requester?format=json&OPERATION_NAME=GET_ALL&TECHNICIAN_KEY=" + tech + "&INPUT_DATA=";
            string body = "{'operation':{'details':{'userid':'" + UserId + "'}}}";

            var retVal = GetXml(url + body);
            JObject typeObj = JObject.Parse(retVal);
            try
            {
                //Created By Email
                BaseInfo ba = new BaseInfo
                {
                    FieldName = "Requester Department",
                    FieldType = "string",
                    Value = (string)typeObj["operation"]["details"][0]["department"],
                };
                if (ba.Value != "")
                {
                    return ba;
                }
                return new BaseInfo();
            }
            catch
            {
                return new BaseInfo();
            }

            #endregion

        }


        public List<BaseInfo> ReplaceImages(BaseInfo baseIn)
        {
            List<BaseInfo> listOut = new List<BaseInfo>();
            //listOut.Add(baseIn);
            try
            {
                string pattern = @"<(img)\b[^>]*>";

                Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                MatchCollection matches = rgx.Matches(baseIn.Value);
                Dictionary<string, string> imgDic = new Dictionary<string, string>();

                for (int i = 0, l = matches.Count; i < l; i++)
                {
                    string imagePath = matches[i].Value;
                    try
                    {
                        string[] paths = imagePath.Split('"');
                        string url = imagePath.Replace("<img src=\"/", baseUrl).Replace("\" />", "");
                        string img = ConvertImageURLToBase64(url);
                        imgDic.Add(imagePath, img);
                    }
                    catch (Exception ex)
                    {
                        string dex = ex.ToString();
                    }
                }
                /*
                for (int x = 0; x < imgDic.Count; x++)
                {

                    BaseInfo ba = new BaseInfo();
                    ba.FieldName = string.Format("Image{0}", x.ToString());
                    ba.FieldType = "image"
                        ba.Value = 
                   //     FieldName = string.Format("Image{0}", x.ToString()), FieldType = "image", Value =  ;

                }
                */
                int x = 0;
                foreach (var item in imgDic)
                {
                    BaseInfo ba = new BaseInfo();
                    ba.FieldName = string.Format("Image{0}", x.ToString());
                    ba.FieldType = "image";
                    ba.Value = string.Format("<img src='{0}' />", item.Value);

                    baseIn.Value = baseIn.Value.Replace(item.Key, ba.FieldName);
                    listOut.Add(ba);

                    ba.FieldName = string.Format("Image{0}", x.ToString());
                    ba.FieldType = "image2";
                    ba.Value = item.Value;
                    x++;
                }
                listOut.Add(baseIn);
            }
            catch { }

            return listOut;
        }


        public String ConvertImageURLToBase64(string url)
        {
            //create an object of StringBuilder type.
            StringBuilder _sb = new StringBuilder();
            //create a byte array that will hold the return value of the getImg method
            Byte[] _byte = this.GetImg(url);
            //appends the argument to the stringbulilder object (_sb)
            _sb.Append(Convert.ToBase64String(_byte, 0, _byte.Length));
            //return the complete and final url in a base64 format.
            string imageData = string.Format(@"data:image/png;base64, {0} ", _sb.ToString());
            return imageData;

        }

        private byte[] GetImg(string url)
        {
            //create a stream object and initialize it to null
            Stream stream = null;
            //create a byte[] object. It serves as a buffer.
            byte[] buf;
            try
            {
                //Create a new WebProxy object.
                WebProxy myProxy = new WebProxy();
                //create a HttpWebRequest object and initialize it by passing the colleague api url to a create method.
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                //Create a HttpWebResponse object and initilize it
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                //get the response stream
                stream = response.GetResponseStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    //get the content length in integer
                    int len = (int)(response.ContentLength);
                    //Read bytes
                    buf = br.ReadBytes(len);
                    //close the binary reader
                    br.Close();
                }
                //close the stream object
                stream.Close();
                //close the response object 
                response.Close();
            }
            catch (Exception exp)
            {
                //set the buffer to null
                buf = null;
            }
            //return the buffer
            return (buf);
        }

        private string[] GetScanType(string tag, string tech)
        {
            //Build the url to get the CI Type from the system.
            string url = baseUrl + "api/cmdb/ci/?OPERATION_NAME=read&TECHNICIAN_KEY=" + tech;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/xml";
            httpWebRequest.Method = "POST";

            //Get the CI Type of the tag
            string xmlString = System.IO.File.ReadAllText(Server.MapPath("APIFiles/GetAllByTag.xml"));
            xmlString = xmlString.Replace("[CI Name]", tag);
            string retVal = PostXml(xmlString, url);
            if (retVal.Contains("No rows found"))
            {
                retVal = PostXml(xmlString.Replace(tag, tag + ".chathamgov.net"), url);
            }
            if (retVal.Contains("No rows found"))
            {
                return new string[] { "Error", "Item Not Found" };
            }


            JObject typeObj = JObject.Parse(retVal);
            string CI_Name = (string)typeObj["API"]["response"]["operation"]["Details"]["field-values"]["record"]["value"][0];
            string CI_Type = (string)typeObj["API"]["response"]["operation"]["Details"]["field-values"]["record"]["value"][1];
            return new string[] { CI_Name, CI_Type };
        }

    }
        [Serializable]
    public class TagInfo
    {
        public string CI_Name { get; set; }
        public string CI_Type { get; set; }
        public string Owned_By { get; set; }
        public string Site { get; set; }
        public string Description { get; set; }
        public string Acquisition_Date { get; set; }
        public string Warranty_Expiry_Date { get; set; }
        public string Expiry_Date { get; set; }
        public string Asset_Tag { get; set; }
        public string Serial_Number { get; set; }
        public string Barcode { get; set; }
        public string Product_Name { get; set; }
        public string Resource_State { get; set; }
        public string Vendor { get; set; }
        public string Location { get; set; }
        public string Mac_Address { get; set; }
        public string WS_UDF_Date { get; set; }
        public string WS_UDF_Num { get; set; }
        public string WS_UDF_Multi { get; set; }
        public string WS_UDF_String { get; set; }
        public string Sites { get; set; }
    }

    public class BaseInfo
    {
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string Value { get; set; }
    }

    public class WOList
    {
        public string WORKORDERID { get; set; }
        public string DEPARTMENT { get; set; }
        public List<BaseInfo> baseInfo{ get; set; }
    }

    [Serializable]
    public class RequestDetail
    {
        public string SUBJECT { get; set; }
        public string CREATEDTIME { get; set; }
        public string CREATEDBY { get; set; }
        public string ISOVERDUE { get; set; }
        public string PRIORITY { get; set; }
        public string REQUESTER { get; set; }
        public string DUEBYTIME { get; set; }
        public string WORKORDERID { get; set; }
        public string STATUS { get; set; }
        public string TECHNICIAN { get; set; }
        //public string DEPARTMENT { get; set; }
    }

  
    [Serializable]
    public class Technician
    {
        public string URI { get; set; }
        public List<Parameter> parameters { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}