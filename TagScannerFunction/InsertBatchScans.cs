using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using TagScannerFunction.Models;

namespace TagScannerFunction
{
    public static class InsertBatchScans
    {
        [FunctionName("InsertBatchScans")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string newBody = "";
            try
            {
                dynamic body = await req.Content.ReadAsStringAsync();
                newBody = (body as string).Replace("[[","[");
                if (!newBody.StartsWith("["))
                {
                    newBody = "[" + newBody + "]";
                }


                

                List<vw_Scans> scansIn = JsonConvert.DeserializeObject<List<vw_Scans>>(body as string);


                List<vw_Scans> scansList = new List<vw_Scans>();
                using (var conn = new SqlConnection(Environment.GetEnvironmentVariable("cTagsData")))
                {
                    conn.Open();
                    string sql = "cTag.UpsertScan";



                    foreach (vw_Scans sc in scansIn) {

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@TagNo", SqlDbType.VarChar).Value = sc.TagNo;
                            cmd.Parameters.Add("@Location", SqlDbType.VarChar).Value = sc.Location;
                            cmd.Parameters.Add("@Status", SqlDbType.Int).Value = sc.Status;
                            cmd.Parameters.Add("@EditBy", SqlDbType.VarChar).Value = sc.EditBy;
                            var res = await cmd.ExecuteNonQueryAsync();

                        }
                    }

                    sql = "cTag.GetScans";
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
                            scansList.Add(new vw_Scans
                            {
                                EditBy = row["EditBy"].ToString(),
                                Location = row["Location"].ToString(),
                                Status = Convert.ToInt32(row["Status"].ToString()),
                                TagNo = row["TagNo"].ToString(),
                                GlobalValue = row["GlobalValue"].ToString(),
                                GlobalPkey = Convert.ToInt32(row["GlobalPkey"].ToString())
                            });
                        }
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        //Content = new StringContent(JsonConvert.SerializeObject(scansList, Formatting.Indented), Encoding.UTF8, "application/json")
                        Content = new StringContent(newBody)
                    };
                }
            }
            catch (SqlException sqlex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    $"The following SqlException happened: {sqlex.Message} ^^^^^ {newBody}");
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    $"The following Exception happened: {ex.Message} ^^^^^ {newBody}");
            }
        }
    }
}
