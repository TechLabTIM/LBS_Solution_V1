using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
namespace AccessingLBS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaunchLBSController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> MakeSimpleRequest()
        {
            string url = "http://your-lbs-equipment-address";  // Replace with your actual URL

            // Construct XML request using the image's information
            var xmlRequest = new StringBuilder();
            xmlRequest.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            xmlRequest.AppendLine("<!DOCTYPE svc_init SYSTEM \"MLP_SVC_INIT_320.DTD\">");
            xmlRequest.AppendLine("<svc_init ver=\"3.2.0\">");
            xmlRequest.AppendLine("    <hdr ver=\"3.2.0\">");
            xmlRequest.AppendLine("        <client>");
            xmlRequest.AppendLine("            <id>user</id>");
            xmlRequest.AppendLine("            <pwd>pass</pwd>");
            xmlRequest.AppendLine("        </client>");
            xmlRequest.AppendLine("    </hdr>");
            xmlRequest.AppendLine("    <slir ver=\"3.2.0\" res_type=\"SYNC\">");
            xmlRequest.AppendLine("        <msids>");
            xmlRequest.AppendLine("            <msid>5521912345678</msid>");
            xmlRequest.AppendLine("        </msids>");
            xmlRequest.AppendLine("        <eqop>");
            xmlRequest.AppendLine("            <hor_acc>100</hor_acc>");
            xmlRequest.AppendLine("        </eqop>");
            xmlRequest.AppendLine("        <loc_type type=\"CURRENT\"/>");
            xmlRequest.AppendLine("    </slir>");
            xmlRequest.AppendLine("</svc_init>");

            // Send the request and get the response
            var responseXml = await SendXmlRequest(url, xmlRequest.ToString());

            // Handle the response
            if (responseXml != null)
            {
                Console.WriteLine("Received response:");
                Console.WriteLine(responseXml);
                // Here you can add code to parse and handle the XML response

                return Ok(responseXml);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<string> SendXmlRequest(string url, string xmlRequest)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine($"Failed to send request: {response.StatusCode}");
                    return null;
                }
            }
        }
    }
}