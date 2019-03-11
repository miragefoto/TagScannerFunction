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
    public static class GetGlobals
    {
        [FunctionName("GetGlobals")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                List<Globals> globals = new List<Globals>();
                using (var conn = new SqlConnection(Environment.GetEnvironmentVariable("cTagsData")))
                {
                    conn.Open();

                    string sql = "cTag.GetGlobals";
                    using (SqlDataAdapter da = new SqlDataAdapter())
                    {
                        da.SelectCommand = new SqlCommand(sql, conn);
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da.SelectCommand.Parameters.Add("@GlobalGroup", SqlDbType.VarChar).Value = "";


                        DataSet ds = new DataSet();
                        da.Fill(ds, "result_name");

                        DataTable dt = ds.Tables["result_name"];

                        foreach (DataRow row in dt.Rows)
                        {

                            //manipulate your data
                            globals.Add(new Globals
                            {
                                Active = (bool)(row["Active"] ?? "false"),
                                GlobalGroup = row["GlobalGroup"].ToString(),
                                GlobalValue = row["GlobalValue"].ToString(),
                                GlobalPkey = Convert.ToInt32(row["GlobalPkey"].ToString())
                            });
                        }
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(globals, Formatting.Indented), Encoding.UTF8, "application/json")
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
    
