using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace cTagInventoryDotNet
{
    public partial class cTags : System.Web.UI.Page
    {
        
        GlobalClasses gc;
        string user;
        protected void Page_Load(object sender, EventArgs e)
        {
            user = User.Identity.Name.ToUpper().Replace("CHATHAMGOV\\", "");
            gc = new GlobalClasses();
            if (!IsPostBack)
            {
                string test = Get("https://cc-tagscanner-functionapp20181003103414.azurewebsites.net/api/GetGlobals");
                List<GetGlobalsResult> Status = JsonConvert.DeserializeObject<List<GetGlobalsResult>>(test);

                    ddlStatus.DataSource = Status;
                    ddlStatus.DataBind();
                    ddlStatus.Items.Insert(0, new ListItem { Text = "Please Select", Value = "" });
                
            }
        }


        protected void btnMarkComplete_Click(object sender, EventArgs e)
        {
            try
            {
                using (cTagDataDataContext data = new cTagDataDataContext())
                {
                    data.UpsertScan(hidPkey.Value, hidLocation.Value, gc.IntFromString(ddlStatus.SelectedValue), user);
                }
            }
            catch
            {
                
                string myJson = "{'TagNo': '" + hidPkey.Value + "','Location':'" + hidLocation.Value + "','EditBy':'"+user+"','Status':'" + ddlStatus.SelectedValue + "'}";
                string uRl = "https://cc-tagscanner-functionapp20181003103414.azurewebsites.net/api/UpdateSingleItem?code=Focr4Hc1Z3yWcfU7J/7IUQl5ktomNFFQgQqt2HIzJAiNjTJSs8P5Fw==";

                Post(uRl, myJson);
            }

        }

        public string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }


        public string Post(string url, string json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
        }

    }

   

}
