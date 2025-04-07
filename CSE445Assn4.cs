using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;

namespace ConsoleApp1
{
    public class Program
    {
        public static string xmlURL = "https://arcturus987.github.io/CSE445_A4/Hotels.xml";
        public static string xmlErrorURL = "https://arcturus987.github.io/CSE445_A4/HotelsErrors.xml";
        public static string xsdURL = "https://arcturus987.github.io/CSE445_A4/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                // Set up validation settings
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                
                // Get and apply the schema
                using (WebClient client = new WebClient())
                {
                    string xsdContent = client.DownloadString(xsdUrl);
                    using (StringReader stringReader = new StringReader(xsdContent))
                    {
                        using (XmlReader schemaReader = XmlReader.Create(stringReader))
                        {
                            XmlSchema schema = XmlSchema.Read(schemaReader, null);
                            settings.Schemas.Add(schema);
                        }
                    }
                }

                // Track validation errors
                StringBuilder errorMessage = new StringBuilder();
                settings.ValidationEventHandler += (sender, e) =>
                {
                    if (errorMessage.Length > 0)
                        errorMessage.Append(Environment.NewLine);
                    
                    errorMessage.Append($"Line {e.Exception.LineNumber}, Position {e.Exception.LinePosition}: {e.Message}");
                };

                // Validate XML against schema
                using (WebClient client = new WebClient())
                {
                    string xmlContent = client.DownloadString(xmlUrl);
                    using (StringReader stringReader = new StringReader(xmlContent))
                    {
                        using (XmlReader reader = XmlReader.Create(stringReader, settings))
                        {
                            while (reader.Read()) { }
                        }
                    }
                }

                return errorMessage.Length == 0 ? "No Error" : errorMessage.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Q2.2
        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                // Get XML content
                string xmlContent;
                using (WebClient client = new WebClient())
                {
                    xmlContent = client.DownloadString(xmlUrl);
                }

                // Parse XML
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                // Convert to JSON (third parameter makes attributes prefixed with "_")
                string jsonText = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented, true);
                
                return jsonText;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}