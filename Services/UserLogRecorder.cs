using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
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
        public static async Task SendLog(UserLogData userLogData, Document doc)
        {
            // web api url
            string url = "https://script.google.com/macros/s/AKfycbx_HHweKkC7PoOC3eOj5bOU30s90LalCVFNJ4nMajEjxxG91bmldP85q8n1j-nPUlyJ/exec";

            // data collection
            DateTime now = DateTime.Now;
            string projectName = doc.Title;
            string projectNumber = doc.ProjectInformation.Number;
            string calculatedTimeSaving = "on test";


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
            ""projectnumber"": """ + projectNumber + @""",
            ""calculatedtimesaving"": """ + calculatedTimeSaving + @"""

        }";

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync(url, content);
            }
        }
    }
}
