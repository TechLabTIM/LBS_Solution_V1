using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AccessingLBS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaunchLBSController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> MakeSimpleRequest()
        {
            string url = "http://10.221.31.191:2000/le";  // Replace with your actual URL

            // Construct XML request using the image's information
            //var xmlRequest = new StringBuilder();
            //xmlRequest.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            //xmlRequest.AppendLine("<!DOCTYPE svc_init SYSTEM \"MLP_SVC_INIT_320.DTD\">");
            //xmlRequest.AppendLine("<svc_init ver=\"3.2.0\">");
            //xmlRequest.AppendLine("    <hdr ver=\"3.2.0\">");
            //xmlRequest.AppendLine("        <client>");
            //xmlRequest.AppendLine("            <id>user</id>");
            //xmlRequest.AppendLine("            <pwd>pass</pwd>");
            //xmlRequest.AppendLine("        </client>");
            //xmlRequest.AppendLine("    </hdr>");
            //xmlRequest.AppendLine("    <slir ver=\"3.2.0\" res_type=\"SYNC\">");
            //xmlRequest.AppendLine("        <msids>");
            //xmlRequest.AppendLine("            <msid>5521912345678</msid>");
            //xmlRequest.AppendLine("        </msids>");
            //xmlRequest.AppendLine("        <eqop>");
            //xmlRequest.AppendLine("            <hor_acc>100</hor_acc>");
            //xmlRequest.AppendLine("        </eqop>");
            //xmlRequest.AppendLine("        <loc_type type=\"CURRENT\"/>");
            //xmlRequest.AppendLine("    </slir>");
            //xmlRequest.AppendLine("</svc_init>");

            StringBuilder xmlRequest = new StringBuilder();

            xmlRequest.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            xmlRequest.AppendLine("<!DOCTYPE svc_init SYSTEM \"MLP_SVC_INIT_320.DTD\">");
            xmlRequest.AppendLine("<svc_init ver=\"3.2.0\">");
            xmlRequest.AppendLine("    <hdr ver=\"3.2.0\">");
            xmlRequest.AppendLine("        <client>");
            xmlRequest.AppendLine("            <id>5</id>");
            xmlRequest.AppendLine("            <pwd>Lab@1234</pwd>");
            xmlRequest.AppendLine("        </client>");
            xmlRequest.AppendLine("        <requestor>");
            xmlRequest.AppendLine("            <id>13300250000</id>");
            xmlRequest.AppendLine("        </requestor>");
            xmlRequest.AppendLine("    </hdr>");
            xmlRequest.AppendLine("    <eme_lir ver=\"3.2.0\" res_type=\"sync\">");
            xmlRequest.AppendLine("        <msid type=\"EME_MSID\">034120220916105721000026-normal-5511983137566-724031900000066-</msid>");
            xmlRequest.AppendLine("        <gsm_net_param>");
            xmlRequest.AppendLine("            <neid>");
            xmlRequest.AppendLine("                <vlrid>");
            xmlRequest.AppendLine("                    <vlrno>5511983137373</vlrno>");
            xmlRequest.AppendLine("                </vlrid>");
            xmlRequest.AppendLine("            </neid>");
            xmlRequest.AppendLine("        </gsm_net_param>");
            xmlRequest.AppendLine("        <eqop>");
            xmlRequest.AppendLine("            <resp_req type=\"LOW_DELAY\">");
            xmlRequest.AppendLine("            </resp_req>");
            xmlRequest.AppendLine("            <hor_acc>30</hor_acc>");
            xmlRequest.AppendLine("        </eqop>");
            xmlRequest.AppendLine("        <geo_info>");
            xmlRequest.AppendLine("            <CoordinateReferenceSystem>");
            xmlRequest.AppendLine("                <Identifier>");
            xmlRequest.AppendLine("                    <code>4326</code>");
            xmlRequest.AppendLine("                    <codeSpace>EPSG</codeSpace>");
            xmlRequest.AppendLine("                    <edition>6.1</edition>");
            xmlRequest.AppendLine("                </Identifier>");
            xmlRequest.AppendLine("            </CoordinateReferenceSystem>");
            xmlRequest.AppendLine("        </geo_info>");
            xmlRequest.AppendLine("        <loc_type type=\"CURRENT\">");
            xmlRequest.AppendLine("        </loc_type>");
            xmlRequest.AppendLine("    </eme_lir>");
            xmlRequest.AppendLine("</svc_init>");

            // To use the built XML string
            //string finalXml = xmlRequest.ToString();



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

        [HttpGet("le")]
        public async Task<IActionResult> MakeSimpleRequestTeste(string msisdn)
        {
            string url = "http://10.221.31.191:2000/le";

            StringBuilder xmlRequest = new StringBuilder();

            xmlRequest.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            xmlRequest.AppendLine("<!DOCTYPE svc_init SYSTEM \"MLP_SVC_INIT_320.DTD\">");
            xmlRequest.AppendLine("<svc_init ver=\"3.2.0\">");
            xmlRequest.AppendLine("    <hdr ver=\"3.2.0\">");
            xmlRequest.AppendLine("        <client>");
            xmlRequest.AppendLine("            <id>5</id>");
            xmlRequest.AppendLine("            <pwd>Lab@1234</pwd>");
            xmlRequest.AppendLine("        </client>");
            xmlRequest.AppendLine("    </hdr>");
            xmlRequest.AppendLine("    <eme_lir ver=\"3.2.0\">");
            xmlRequest.AppendLine($"        <msid>{msisdn}</msid>");
            xmlRequest.AppendLine("        <loc_type type=\"CURRENT\" />");
            xmlRequest.AppendLine("    </eme_lir>");
            xmlRequest.AppendLine("</svc_init>");

            var responseXml = await SendXmlRequest(url, xmlRequest.ToString());

            if (responseXml != null)
            {
                Console.WriteLine("Received response:");
                Console.WriteLine(responseXml);

                // Parse the XML
                var xDoc = XDocument.Parse(responseXml);
                var ns = xDoc.Root.GetDefaultNamespace();
                var coord = xDoc.Descendants(ns + "coord").FirstOrDefault();
                if (coord != null)
                {
                    string xCoord = coord.Element(ns + "X").Value;
                    string yCoord = coord.Element(ns + "Y").Value;

                    // Convert Coordinates to Decimal
                    var latitude = ConvertCoordinateToDecimal(xCoord);
                    var longitude = ConvertCoordinateToDecimal(yCoord);

                    // Now you can use latitude and longitude for mapping
                    // ...

                    return Ok(new { Latitude = latitude, Longitude = longitude });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Coordinates not found in response");
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private double ConvertCoordinateToDecimal(string coord)
        {
            // Assuming coord format is like "S22.984182" or "W43.359089"
            var direction = coord[0];
            var value = double.Parse(coord.Substring(1));
            if (direction == 'S' || direction == 'W')
            {
                value *= -1;
            }
            return value;
        }

        private async Task<string> SendXmlRequest(string url, string xmlRequest)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(xmlRequest, Encoding.UTF8, "text/xml");
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