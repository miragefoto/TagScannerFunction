using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.SqlServer;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using TagScannerFunction.Models;
using TagScannerFunction.Models.data;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using System.Text;

namespace TagScannerFunction
{
    public static class GetScannedTags
    {
        [FunctionName("GetScannedTags")]
        public static async Task<HttpResponseMessage>Run([HttpTrigger(AuthorizationLevel.Anonymous,"get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                List<vw_Scans> scans = new List<vw_Scans>();
                using (var conn= new SqlConnection(Environment.GetEnvironmentVariable("cTagsData")))
                {
                    conn.Open();
                    
                    string sql = "cTag.GetScans";
                    using (SqlDataAdapter da = new SqlDataAdapter())
                    {
                        da.SelectCommand = new SqlCommand(sql, conn);
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da.SelectCommand.Parameters.Add("@cTag", SqlDbType.VarChar).Value = "";
                        

                        DataSet ds = new DataSet();
                        da.Fill(ds, "result_name");

                        DataTable dt = ds.Tables["result_name"];

                        foreach (DataRow row in dt.Rows)
                        {
                            //manipulate your data
                            scans.Add(new vw_Scans
                            {
                                EditBy = row["EditBy"].ToString(),
                                Location = row["Location"].ToString(),
                                Status = Convert.ToInt32(row["Status"].ToString()),
                                TagNo = row["TagNo"].ToString(),
                                ValidFrom = Convert.ToDateTime(row["ValidFrom"]),
                                GlobalValue = row["GlobalValue"].ToString(),
                                GlobalPkey = Convert.ToInt32(row["GlobalPkey"].ToString())
                            });
                        }
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(scans, Formatting.Indented), Encoding.UTF8, "application/json")
                    };
                }
            }
            catch (SqlException sqlex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    $"The following SqlException happened: {sqlex.Message}");
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    $"The following Exception happened: {ex.Message}");
            }
        }

    }
}
