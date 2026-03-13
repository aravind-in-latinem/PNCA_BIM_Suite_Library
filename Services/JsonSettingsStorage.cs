using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.ViewModel;
namespace PNCA_BIM_Suite_Library.Services
{
    public class JsonSettingsStorage
    {        
        public static void SaveProfile(int revitYear, SettingsProfile profile)
        {
            var folder = RevitAddinPaths.GetProfilesFolder(revitYear);
            Directory.CreateDirectory(folder);

            var filePath = Path.Combine(
                folder,
                $"{SanitizeFileName(profile.ProfileName)}.json");

            var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }
        
        public static IEnumerable<SettingsProfile> LoadProfiles(int revitYear)
        {
            var folder = RevitAddinPaths.GetProfilesFolder(revitYear);
            if (!Directory.Exists(folder))
                yield break;

            foreach (var file in Directory.GetFiles(folder, "*.json"))
            {
                SettingsProfile profile;
                try
                {
                    profile = JsonSerializer.Deserialize<SettingsProfile>(
                        File.ReadAllText(file));
                }
                catch
                {
                    continue; // corrupted or incompatible profile
                }

                if (profile != null)
                    yield return profile;
            }
        }
        public class SheetExporterAppSettings
        {
            public string LastUsedProfile { get; set; }
        }
        public static SheetExporterAppSettings LoadSettings(int revitYear)
        {
            var file = RevitAddinPaths.GetSettingsFile(revitYear);
            if (!File.Exists(file))
                return new SheetExporterAppSettings();

            return JsonSerializer.Deserialize<SheetExporterAppSettings>(
                File.ReadAllText(file));
        }
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "Unnamed";

            // Remove invalid file name characters
            var invalidChars = Path.GetInvalidFileNameChars();

            var sanitized = new string(
                fileName
                    .Where(ch => !invalidChars.Contains(ch))
                    .ToArray()
            );

            // Optional: trim and normalize spaces
            sanitized = sanitized.Trim();

            // Windows safety: avoid empty or reserved names
            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "Unnamed";

            return sanitized;
        }      


    }
}