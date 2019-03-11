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
    public static class UpdateSingleItem
    {
        [FunctionName("UpdateSingleItem")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            dynamic body = await req.Content.ReadAsStringAsync();

            try
            {

               
                vw_Scans sc = JsonConvert.DeserializeObject<vw_Scans>(body as string);

                using (var conn = new SqlConnection(Environment.GetEnvironmentVariable("cTagsData")))
                {
                    conn.Open();
                    string sql = "cTag.UpsertScan";

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
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    //Content = new StringContent(JsonConvert.SerializeObject(scansList, Formatting.Indented), Encoding.UTF8, "application/json")
                    Content = new StringContent(body as string)
                };
            
            }
            catch (SqlException sqlex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    $"The following SqlException happened: {sqlex.Message} ^^^^^ {body as string}");
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    $"The following Exception happened: {ex.Message} ^^^^^ {body as string}");
            }

        }
    }
}
