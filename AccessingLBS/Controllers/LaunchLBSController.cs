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
        private readonly HttpClient _httpClient;    
        public LaunchLBSController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("LBSClient");
        }
        public class Perfil
        {
            public int Id { get; set; }
            public string Password { get; set; }
        }


        [HttpGet]
        public async Task<IActionResult> MakeSimpleRequest()
        {
            //string url = "http://10.221.31.191:2000/le";
            string url = "http://10.221.23.26:2000/le";

            StringBuilder xmlRequest = new StringBuilder();

            xmlRequest.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            xmlRequest.AppendLine("<!DOCTYPE svc_init SYSTEM \"MLP_SVC_INIT_320.DTD\">");
            xmlRequest.AppendLine("<svc_init ver=\"3.2.0\">");
            xmlRequest.AppendLine("    <hdr ver=\"3.2.0\">");
            xmlRequest.AppendLine("        <client>");
            xmlRequest.AppendLine("            <id>5</id>");
            xmlRequest.AppendLine("            <pwd>devloc@4g</pwd>");
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

        [HttpGet("lelist")]
        public async Task<IActionResult> MakeSimpleRequestTeste([FromQuery] List<string> msisdns)
        {
            string url = "http://10.223.23.26:2000/le";
            var results = new List<object>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var msisdn in msisdns)
            {
                StringBuilder xmlRequest = new StringBuilder();

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
                xmlRequest.AppendLine("           <hor_acc>100</hor_acc>");
                xmlRequest.AppendLine("       </eqop>");
                xmlRequest.AppendLine("       <loc_type type=\"CURRENT\"/>");
                xmlRequest.AppendLine("   </slir>");
                xmlRequest.AppendLine("</svc_init>");

                var responseXml = await SendXmlRequest(url, xmlRequest.ToString());

                if (responseXml != null)
                {
                    Console.WriteLine("Received response:");
                    Console.WriteLine(responseXml);

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

                        var latitude = ConvertCoordinateToDecimal(xCoord);
                        var longitude = ConvertCoordinateToDecimal(yCoord);
                        var radiusValue = double.Parse(radius);

                        results.Add(new { Latitude = latitude, Longitude = longitude, Radius = radiusValue, DistanceUnit = distanceUnit, Type = "CircularArea" });
                    }
                    else if (circularArcArea != null)
                    {
                        var coord = circularArcArea.Element(ns + "coord");
                        var latitude = ConvertCoordinateToDecimal(coord.Element(ns + "X").Value);
                        var longitude = ConvertCoordinateToDecimal(coord.Element(ns + "Y").Value);
                        var innerRadius = double.Parse(circularArcArea.Element(ns + "inRadius").Value);
                        var outerRadius = double.Parse(circularArcArea.Element(ns + "outRadius").Value);
                        var startAngle = double.Parse(circularArcArea.Element(ns + "startAngle").Value);
                        var stopAngle = double.Parse(circularArcArea.Element(ns + "stopAngle").Value);

                        results.Add(new CircularArcAreaDto
                        {
                            Latitude = latitude,
                            Longitude = longitude,
                            InnerRadius = innerRadius,
                            OuterRadius = outerRadius,
                            StartAngle = startAngle,
                            StopAngle = stopAngle,
                            Type = "CircularArcArea"
                        });
                    }
                    else
                    {
                        results.Add(new { RespostaXML = responseXml });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }

            stopwatch.Stop();
            TimeSpan requestTime = stopwatch.Elapsed;
            double requestTimeInSeconds = requestTime.TotalMilliseconds / 1000;

            return Ok(new { Results = results, TotalRequestTime = requestTimeInSeconds });
        }

        [HttpGet("lelistTxt")]
        public async Task<IActionResult> MakeSimpleRequestTeste([FromQuery] List<string> msisdns, [FromQuery] int iterations)
        {

            try
            {
                //string url = "http://10.221.31.191:2000/le";
                //string url = "http://10.192.66.29:2000/le";
                string url = "http://10.223.23.26:2000/le";
                var results = new List<object>();
                List<double> times = new List<double>();

                for (int i = 0; i < iterations; i++)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    foreach (var msisdn in msisdns)
                    {
                        StringBuilder xmlRequest = new StringBuilder();

                        xmlRequest.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                        xmlRequest.AppendLine("<!DOCTYPE svc_init SYSTEM \"MLP_SVC_INIT_320.DTD\">");
                        xmlRequest.AppendLine("<svc_init ver=\"3.2.0\">");
                        xmlRequest.AppendLine("   <hdr ver=\"3.2.0\">");
                        xmlRequest.AppendLine("       <client>");
                        xmlRequest.AppendLine("         <id>5</id>");
                        xmlRequest.AppendLine("         <pwd>devloc@4g</pwd>");
                        xmlRequest.AppendLine("       </client>");
                        xmlRequest.AppendLine("   </hdr>");
                        xmlRequest.AppendLine("   <slir ver=\"3.2.0\" res_type=\"SYNC\">");
                        xmlRequest.AppendLine("       <msids>");
                        xmlRequest.AppendLine($"          <msid>{msisdn}</msid>");
                        xmlRequest.AppendLine("       </msids>");
                        xmlRequest.AppendLine("       <eqop>");
                        xmlRequest.AppendLine("           <hor_acc>100</hor_acc>");
                        xmlRequest.AppendLine("       </eqop>");
                        xmlRequest.AppendLine("       <loc_type type=\"CURRENT\"/>");
                        xmlRequest.AppendLine("   </slir>");
                        xmlRequest.AppendLine("</svc_init>");

                        var responseXml = await SendXmlRequest(url, xmlRequest.ToString());

                        if (responseXml != null)
                        {
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

                                var latitude = ConvertCoordinateToDecimal(xCoord);
                                var longitude = ConvertCoordinateToDecimal(yCoord);
                                var radiusValue = double.Parse(radius);

                                results.Add(new { Latitude = latitude, Longitude = longitude, Radius = radiusValue, DistanceUnit = distanceUnit, Type = "CircularArea" });
                            }
                            else if (circularArcArea != null)
                            {
                                var coord = circularArcArea.Element(ns + "coord");
                                var latitude = ConvertCoordinateToDecimal(coord.Element(ns + "X").Value);
                                var longitude = ConvertCoordinateToDecimal(coord.Element(ns + "Y").Value);
                                var innerRadius = double.Parse(circularArcArea.Element(ns + "inRadius").Value);
                                var outerRadius = double.Parse(circularArcArea.Element(ns + "outRadius").Value);
                                var startAngle = double.Parse(circularArcArea.Element(ns + "startAngle").Value);
                                var stopAngle = double.Parse(circularArcArea.Element(ns + "stopAngle").Value);

                                results.Add(new CircularArcAreaDto
                                {
                                    Latitude = latitude,
                                    Longitude = longitude,
                                    InnerRadius = innerRadius,
                                    OuterRadius = outerRadius,
                                    StartAngle = startAngle,
                                    StopAngle = stopAngle,
                                    Type = "CircularArcArea"
                                });
                            }
                            else
                            {
                                results.Add(new { RespostaXML = responseXml });
                            }
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError);
                        }
                    }

                    stopwatch.Stop();
                    TimeSpan requestTime = stopwatch.Elapsed;
                    double requestTimeInSeconds = requestTime.TotalMilliseconds / 1000;
                    times.Add(requestTimeInSeconds);
                }

                double averageTime = times.Average();

                // Save the result in a text file
                string filePath = "request_times.txt";
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Request Times (in seconds):");
                    foreach (var time in times)
                    {
                        writer.WriteLine(time);
                    }
                    writer.WriteLine($"Average Request Time: {averageTime}");
                }

                return Ok(new { Results = results, AverageRequestTime = averageTime });
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            
        }


        [HttpGet("le")]
        public async Task<IActionResult> MakeSimpleRequestTeste(string msisdn)
        {
            //string url = "http://10.221.31.191:2000/le";
            string url = "http://10.223.23.26:2000/le";

            StringBuilder xmlRequest = new StringBuilder();

            xmlRequest.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            xmlRequest.AppendLine("<!DOCTYPE svc_init SYSTEM \"MLP_SVC_INIT_320.DTD\">");
            xmlRequest.AppendLine("<svc_init ver=\"3.2.0\">");
            xmlRequest.AppendLine("   <hdr ver=\"3.2.0\">");
            xmlRequest.AppendLine("       <client>");
            xmlRequest.AppendLine("         <id>6</id>");
            xmlRequest.AppendLine("         <pwd>devloc@ati</pwd>");
            xmlRequest.AppendLine("       </client>");
            xmlRequest.AppendLine("   </hdr>");
            xmlRequest.AppendLine("   <slir ver=\"3.2.0\" res_type=\"SYNC\">");
            xmlRequest.AppendLine("       <msids>");
            xmlRequest.AppendLine($"          <msid>{msisdn}</msid>");
            xmlRequest.AppendLine("       </msids>");
            xmlRequest.AppendLine("       <eqop>");
            xmlRequest.AppendLine("           <hor_acc>100</hor_acc>");
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

                    return Ok(new { Latitude = latitude, Longitude = longitude, Radius = radiusValue, DistanceUnit = distanceUnit, RequestTime = requestTimeInSeconds, Type = "CircularArea" });
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
                        RequestedTime = requestTimeInSeconds,
                        Type = "CircularArcArea"
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

        [HttpGet("lelistnew")]
        public async Task<IActionResult> MakeSimpleRequestTesteComplete([FromQuery] List<string> msisdns, [FromQuery] int iterations, int profileNumber)
        {
            //string url = "http://10.221.31.191:2000/le";
            //string url = "http://10.192.66.29:2000/le";
            string url = "http://10.223.23.26:2000/le";
            var results = new List<object>();
            var timesPerMsisdn = new Dictionary<string, List<double>>();
            var radiiPerMsisdn = new Dictionary<string, List<double>>(); // List to store all radii


          
            var p1 = new Perfil
            {
                Id = 5,
                Password = "devloc@4g"
            };

            var p2 = new Perfil
            {
                Id = 6,
                Password = "devloc@3g"
            };

            var p3 = new Perfil
            {
                Id = 7,
                Password = "devloc@at1"
            };

            var t1 = new Perfil
            {
                Id = 10,
                Password = "Test_3G123"
            };

            var t2 = new Perfil
            {
                Id = 11,
                Password = "Test_4G123"
            };

            var t3 = new Perfil
            {
                Id = 12,
                Password = "Test_ATI123"
            };

            // Initialize time storage for each MSISDN
            foreach (var msisdn in msisdns)
            {
                timesPerMsisdn[msisdn] = new List<double>();
                radiiPerMsisdn[msisdn] = new List<double>();
            }

            try
            {
                
                for (int i = 0; i < iterations; i++)
                {
                    var tasks = msisdns.Select(msisdn => MakeRequestAndMeasureTime(url, msisdn, p1)).ToList();
                    var iterationResults = await Task.WhenAll(tasks);

                    // Aggregate results and times
                    for (int j = 0; j < msisdns.Count; j++)
                    {
                        var msisdn = msisdns[j];
                        var (responseResult, requestTimeInSeconds) = iterationResults[j];

                        results.Add(responseResult);
                        timesPerMsisdn[msisdn].Add(requestTimeInSeconds);

                        if(responseResult is CircularAreaDto area)
                        {
                            radiiPerMsisdn[msisdn].Add(area.Radius);
                        }
                        else if(responseResult is CircularArcAreaDto arcArea)
                        {
                            radiiPerMsisdn[msisdn].Add(arcArea.InnerRadius);
                            radiiPerMsisdn[msisdn].Add(arcArea.OuterRadius);
                        }
                    }
                }

                
                // Compute average times per MSISDN
                var averageTimes = timesPerMsisdn.ToDictionary(pair => pair.Key, pair => pair.Value.Average());

                // Compute average radii per MSISDN safely
                //var averageRadii = radiiPerMsisdn.ToDictionary(pair => pair.Key,
                //    pair => pair.Value.Any() ? pair.Value.Average() : 0); // Use conditional to check for content before averaging

                // Save the result in a text file
                string filePath = "request_times.txt";
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Request Times Per MSISDN (in seconds):");
                    foreach (var entry in averageTimes)
                    {
                        writer.WriteLine($"MSISDN {entry.Key}: Average Time = {entry.Value}");
                    }
                }

                return Ok(new { Results = results, AverageTimesPerMsisdn = averageTimes});
            }
            catch (Exception ex)
            {
                // Log detailed exception information here
                throw new Exception("Error during the request processing: " + ex.Message, ex);
            }
        }


        [HttpGet("lelistdata")]
        public async Task<IActionResult> MakeSimpleRequestTesteCompleted([FromQuery] List<string> msisdns, [FromQuery] int iterations, int profileNumber, string codeRegion)
        {
            string url = "";
            Perfil profile = new Perfil{};

            switch (codeRegion)
            {
                case "RJ-Lab":
                    url = "http://10.221.31.191:2000/le";
                    break;

                case "SPO":
                    url = "http://10.192.66.29:2000/le";
                    break;

                case "RJO":
                    url = "http://10.223.23.26:2000/le";
                    break;

            }

            switch (profileNumber)
            {
                case 1:
                    profile = new Perfil
                    {
                        Id = 5,
                        Password = "devloc@4g"
                    };
                    break;

                case 2:
                    profile = new Perfil
                    {
                        Id = 6,
                        Password = "devloc@3g"
                    };
                    break;

                case 3:
                    profile = new Perfil
                    {
                        Id = 7,
                        Password = "devloc@at1"
                    };
                    break;

                case 4:
                    profile = new Perfil
                    {
                        Id = 10,
                        Password = "Test_3G123"

                    };
                    break;
                case 5:
                    profile = new Perfil
                    {
                        Id = 11,
                        Password = "Test_4G123"
                    };
                    break;

                case 6:
                    profile = new Perfil
                    {
                        Id = 12,
                        Password = "Test_ATI123"
                    };
                    break;
            }
                    //string url = "http://10.221.31.191:2000/le";
                    //string url = "http://10.192.66.29:2000/le";
                    //string url = "http://10.223.23.26:2000/le";
                    var results = new List<object>();
            var timesPerMsisdn = new Dictionary<string, List<double>>();
            var radiiPerMsisdn = new Dictionary<string, List<double>>(); // List to store all radii



            

            // Initialize time storage for each MSISDN
            foreach (var msisdn in msisdns)
            {
                timesPerMsisdn[msisdn] = new List<double>();
                radiiPerMsisdn[msisdn] = new List<double>();
            }

            try
            {

                for (int i = 0; i < iterations; i++)
                {
                    var tasks = msisdns.Select(msisdn => MakeRequestAndMeasureTime(url, msisdn, profile)).ToList();
                    var iterationResults = await Task.WhenAll(tasks);

                    // Aggregate results and times
                    for (int j = 0; j < msisdns.Count; j++)
                    {
                        var msisdn = msisdns[j];
                        var (responseResult, requestTimeInSeconds) = iterationResults[j];

                        results.Add(responseResult);
                        timesPerMsisdn[msisdn].Add(requestTimeInSeconds);

                        if (responseResult is CircularAreaDto area)
                        {
                            radiiPerMsisdn[msisdn].Add(area.Radius);
                        }
                        else if (responseResult is CircularArcAreaDto arcArea)
                        {
                            radiiPerMsisdn[msisdn].Add(arcArea.InnerRadius);
                            radiiPerMsisdn[msisdn].Add(arcArea.OuterRadius);
                        }
                    }
                }


                // Compute average times per MSISDN
                var averageTimes = timesPerMsisdn.ToDictionary(pair => pair.Key, pair => pair.Value.Average());

                // Compute average radii per MSISDN safely
                //var averageRadii = radiiPerMsisdn.ToDictionary(pair => pair.Key,
                //    pair => pair.Value.Any() ? pair.Value.Average() : 0); // Use conditional to check for content before averaging

                // Save the result in a text file
                string filePath = "request_times.txt";
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Request Times Per MSISDN (in seconds):");
                    foreach (var entry in averageTimes)
                    {
                        writer.WriteLine($"MSISDN {entry.Key}: Average Time = {entry.Value}");
                    }
                }

                return Ok(new { Results = results, AverageTimesPerMsisdn = averageTimes });
            }
            catch (Exception ex)
            {
                // Log detailed exception information here
                throw new Exception("Error during the request processing: " + ex.Message, ex);
            }
        }

        private async Task<(object result, double timeInSeconds)> MakeRequestAndMeasureTime(string url, string msisdn, Perfil perfil)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string xmlRequest = CreateXmlRequest(msisdn, perfil); // Function to create XML string
            var responseXml = await SendXmlRequest(url, xmlRequest);
            stopwatch.Stop();

            if (responseXml != null)
            {
                var parsedResult = ParseXmlResponse(responseXml); // Function to parse XML and extract result
                return (parsedResult, stopwatch.Elapsed.TotalSeconds);
            }
            else
            {
                throw new HttpRequestException("Failed to get a response for MSISDN: " + msisdn);
            }
        }

        private object ParseXmlResponse(string responseXml)
        {
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

                var latitude = ConvertCoordinateToDecimal(xCoord);
                var longitude = ConvertCoordinateToDecimal(yCoord);
                var radiusValue = double.Parse(radius);

                return new { Latitude = latitude, Longitude = longitude, Radius = radiusValue, DistanceUnit = distanceUnit, Type = "CircularArea" };
            }
            else if (circularArcArea != null)
            {
                var coord = circularArcArea.Element(ns + "coord");
                var latitude = ConvertCoordinateToDecimal(coord.Element(ns + "X").Value);
                var longitude = ConvertCoordinateToDecimal(coord.Element(ns + "Y").Value);
                var innerRadius = double.Parse(circularArcArea.Element(ns + "inRadius").Value);
                var outerRadius = double.Parse(circularArcArea.Element(ns + "outRadius").Value);
                var startAngle = double.Parse(circularArcArea.Element(ns + "startAngle").Value);
                var stopAngle = double.Parse(circularArcArea.Element(ns + "stopAngle").Value);

                return new CircularArcAreaDto
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    InnerRadius = innerRadius,
                    OuterRadius = outerRadius,
                    StartAngle = startAngle,
                    StopAngle = stopAngle,
                    Type = "CircularArcArea"
                };
            }

            // If neither area is found, return the raw XML for troubleshooting or logging
            return new { ResponseXml = responseXml };
        }



        private string CreateXmlRequest(string msisdn, Perfil perfil)
        {
            StringBuilder xmlRequest = new StringBuilder();

            xmlRequest.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            xmlRequest.AppendLine("<!DOCTYPE svc_init SYSTEM \"MLP_SVC_INIT_320.DTD\">");
            xmlRequest.AppendLine("<svc_init ver=\"3.2.0\">");
            xmlRequest.AppendLine("   <hdr ver=\"3.2.0\">");
            xmlRequest.AppendLine("       <client>");
            xmlRequest.AppendLine($"         <id>{perfil.Id}</id>");
            xmlRequest.AppendLine($"         <pwd>{perfil.Password}</pwd>");
            xmlRequest.AppendLine("       </client>");
            xmlRequest.AppendLine("   </hdr>");
            xmlRequest.AppendLine("   <slir ver=\"3.2.0\" res_type=\"SYNC\">");
            xmlRequest.AppendLine("       <msids>");
            xmlRequest.AppendLine($"          <msid>{msisdn}</msid>");
            xmlRequest.AppendLine("       </msids>");
            xmlRequest.AppendLine("       <eqop>");
            xmlRequest.AppendLine("           <hor_acc>30</hor_acc>");
            xmlRequest.AppendLine("       </eqop>");
            xmlRequest.AppendLine("       <loc_type type=\"CURRENT\"/>");
            xmlRequest.AppendLine("   </slir>");
            xmlRequest.AppendLine("</svc_init>");

            return xmlRequest.ToString();

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

        //private async Task<string> SendXmlRequest(string url, string xmlRequest)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        var content = new StringContent(xmlRequest, Encoding.UTF8, "text/xml");
        //        HttpResponseMessage response;
        //        int maxAttempts = 3;
        //        int attempt = 0;

        //        while (attempt < maxAttempts)
        //        {
        //            attempt++;
        //            try
        //            {
        //                response = await client.PostAsync(url, content);
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    Console.WriteLine($"Success on attempt {attempt}");
        //                    return await response.Content.ReadAsStringAsync();
        //                }
        //                else
        //                {
        //                    Console.WriteLine($"Attempt {attempt}: Failed to send request with status code: {response.StatusCode}");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Attempt {attempt}: Exception occurred - {ex.Message}");
        //                // Optionally, handle specific exceptions differently
        //            }

        //            if (attempt < maxAttempts)
        //            {
        //                await Task.Delay(1000 * attempt); // Increasing delay before retrying
        //            }
        //        }

        //        throw new HttpRequestException($"Failed to receive a successful response after {maxAttempts} attempts.");
        //    }
        //}public class LaunchLBSController : ControllerBase


        private async Task<string> SendXmlRequest(string url, string xmlRequest)
        {
            var content = new StringContent(xmlRequest, Encoding.UTF8, "text/xml");

            HttpResponseMessage response;
            int maxAttempts = 3;
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                attempt++;
                try
                {
                    response = await _httpClient.PostAsync(url, content); // **Use injected HttpClient**

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Success on attempt {attempt}");
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Attempt {attempt}: Failed with status code: {response.StatusCode}");
                    }
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"Attempt {attempt}: Timeout reached.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempt}: Exception - {ex.Message}");
                }

                if (attempt < maxAttempts)
                {
                    await Task.Delay(1000 * attempt);
                }
            }

            throw new HttpRequestException($"Failed after {maxAttempts} attempts.");
        }
    }

}