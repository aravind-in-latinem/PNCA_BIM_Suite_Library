using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DocumentFormat.OpenXml.Spreadsheet;
using PNCA_BIM_Suite_Library.Model;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PNCA_BIM_Suite_Library.Services
{
    internal static class UserLogRecorder
    {
        public static async Task SendLog(string projectName, string addinName, string status, string message, string timeStart, string timeStop)
        {
            string url = "https://script.google.com/macros/s/AKfycbwlbiwtdfj_xsdvjjn5N1tS5UqZzvxheWxmMghQuk9VRL4Nwh3Uzj5Bq5hI4oAZa6mq/exec";

            DateTime now = DateTime.Now;

            var json = @"{
            ""date"": """ + now.ToString("yyyy-MM-dd") + @""",
            ""time"": """ + now.ToString("HH:mm:ss") + @""",
            ""username"": """ + Environment.UserName + @""",
            ""addin"": """ + addinName + @""",
            ""project"": """ + projectName + @""",
            ""timestart"": """ + now.ToString("HH:mm:ss") + @""",
            ""timestop"": """ + now.ToString("HH:mm:ss") + @""",
            ""status"": """ + status + @""",
            ""message"": """ + message + @"""
        }";

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync(url, content);
            }
        }

        public static async Task SendLog(UserLogData userLogData, Document doc)
        {
            // web api url
            string url = "https://script.google.com/macros/s/AKfycbyS2i2hRFrl6BOKG4gjWYo2MshFiuWUlRiB7tfKlpAyT8aWmXf_u1LcpIYMH8EeWXoH/exec";

            // data collection
            DateTime now = DateTime.Now;
            string projectName = doc.Title;
            string projectNumber = doc.ProjectInformation.Number;

            var json = @"{
            ""date"": """ + now.ToString("yyyy-MM-dd") + @""",
            ""time"": """ + now.ToString("HH:mm:ss") + @""",
            ""username"": """ + Environment.UserName + @""",
            ""addin"": """ + userLogData.AddinName + @""",
            ""project"": """ + projectName + @""",
            ""timestart"": """ + userLogData.StartTime + @""",
            ""timestop"": """ + userLogData.StopTime + @""",
            ""status"": """ + userLogData.Status + @""",
            ""message"": """ + userLogData.Message + @""",
            ""fullusername"": """ + Environment.UserDomainName + @""",
            ""projectnumber"": """ + projectNumber + @"""

        }";

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync(url, content);
            }
        }


    }

}
