using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;
using AccessingLBS.DTO;

namespace AccessingLBS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaunchLBSController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> MakeSimpleRequest()
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

            //xmlRequest.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            //xmlRequest.AppendLine("<!DOCTYPE svc_init SYSTEM \"MLP_SVC_INIT_320.DTD\">");
            //xmlRequest.AppendLine("<svc_init ver=\"3.2.0\">");
            //xmlRequest.AppendLine("    <hdr ver=\"3.2.0\">");
            //xmlRequest.AppendLine("        <client>");
            //xmlRequest.AppendLine("            <id>3</id>");
            //xmlRequest.AppendLine("            <pwd>DevLoc@123</pwd>");
            //xmlRequest.AppendLine("        </client>");
            //xmlRequest.AppendLine("    </hdr>");
            //xmlRequest.AppendLine("    <eme_lir ver=\"3.2.0\">");
            //xmlRequest.AppendLine($"        <msid>{msisdn}</msid>");
            //xmlRequest.AppendLine("        <loc_type type=\"CURRENT\" />");
            //xmlRequest.AppendLine("    </eme_lir>");
            //xmlRequest.AppendLine("</svc_init>");

            xmlRequest.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            xmlRequest.AppendLine("<!DOCTYPE svc_init SYSTEM \"MLP_SVC_INIT_320.DTD\">");
            xmlRequest.AppendLine("<svc_init ver=\"3.2.0\">");
            xmlRequest.AppendLine("   <hdr ver=\"3.2.0\">");
            xmlRequest.AppendLine("       <client>");
            xmlRequest.AppendLine("         <id>3</id>");
            xmlRequest.AppendLine("         <pwd>DevLoc@123</pwd>");
            xmlRequest.AppendLine("       </client>");
            xmlRequest.AppendLine("   </hdr>");
            xmlRequest.AppendLine("   <slir ver=\"3.2.0\" res_type=\"SYNC\">");
            xmlRequest.AppendLine("       <msids>");
            xmlRequest.AppendLine($"          <msid>{msisdn}</msid>");
            xmlRequest.AppendLine("       </msids>");
            xmlRequest.AppendLine("       <eqop>");
            xmlRequest.AppendLine("           <hor_acc>60</hor_acc>");
            xmlRequest.AppendLine("       </eqop>");
            xmlRequest.AppendLine("       <loc_type type=\"CURRENT\"/>");
            xmlRequest.AppendLine("   </slir>");
            xmlRequest.AppendLine("</svc_init>");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var responseXml = await SendXmlRequest(url, xmlRequest.ToString());

            if (responseXml != null)
            {
                Console.WriteLine("Received response:");
                Console.WriteLine(responseXml);

                // Parse the XML
                var xDoc = XDocument.Parse(responseXml);
                var ns = xDoc.Root.GetDefaultNamespace();

                var circularArea = xDoc.Descendants(ns + "CircularArea").FirstOrDefault();
                var circularArcArea = xDoc.Descendants(ns + "CircularArcArea").FirstOrDefault();
                if (circularArea != null)
                {
                    var coord = circularArea.Element(ns + "coord");
                    string xCoord = coord.Element(ns + "X").Value;
                    string yCoord = coord.Element(ns + "Y").Value;

                    var radius = circularArea.Element(ns + "radius").Value;
                    var distanceUnit = circularArea.Element(ns + "distanceUnit").Value;

                    // Convert Coordinates to Decimal
                    var latitude = ConvertCoordinateToDecimal(xCoord);
                    var longitude = ConvertCoordinateToDecimal(yCoord);
                    var radiusValue = double.Parse(radius);

                    // Now you have latitude, longitude, and radius for mapping
                    // ...
                    stopwatch.Stop();
                    TimeSpan requestTime = stopwatch.Elapsed;
                    double requestTimeInSeconds = requestTime.TotalMilliseconds / 1000;

                    return Ok(new { Latitude = latitude, Longitude = longitude, Radius = radiusValue, DistanceUnit = distanceUnit, RequestTime = requestTimeInSeconds });
                } else if(circularArcArea != null)
                {
                    var coord = circularArcArea.Element(ns + "coord");
                    var latitude = ConvertCoordinateToDecimal(coord.Element(ns + "X").Value);
                    var longitude = ConvertCoordinateToDecimal(coord.Element(ns + "Y").Value);
                    var innerRadius = double.Parse(circularArcArea.Element(ns + "inRadius").Value);
                    var outerRadius = double.Parse(circularArcArea.Element(ns + "outRadius").Value);
                    var startAngle = double.Parse(circularArcArea.Element(ns + "startAngle").Value);
                    var stopAngle = double.Parse(circularArcArea.Element(ns + "stopAngle").Value);
                    stopwatch.Stop();
                    TimeSpan requestTime = stopwatch.Elapsed;
                    double requestTimeInSeconds = requestTime.TotalMilliseconds / 1000;
                    var circularArcAreaDto = new CircularArcAreaDto
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        InnerRadius = innerRadius,
                        OuterRadius = outerRadius,
                        StartAngle = startAngle,
                        StopAngle = stopAngle,
                        RequestedTime = requestTimeInSeconds
                    };
                    return Ok(circularArcAreaDto);
                }
                else
                {
                    stopwatch.Stop();
                    TimeSpan requestTime = stopwatch.Elapsed;
                    double requestTimeInSeconds = requestTime.TotalMilliseconds / 1000;
                    return Ok(new { RespostaXML = responseXml, TempoResposta = requestTimeInSeconds });
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private double ConvertCoordinateToDecimal(string coord)
        {
            
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