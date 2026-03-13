using System;
using System.IO;

namespace PNCA_BIM_Suite_Library.Services
{
    public static class RevitAddinPaths
    {
        public static string AppData =>
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static string RevitAddinsRoot =>
            Path.Combine(
                AppData,                
                "Autodesk",
                "Revit",
                "Addins");

        public static string GetAddinRoot(int revitYear) =>
            Path.Combine(
                RevitAddinsRoot,
                revitYear.ToString(),
                "PNCA_BIM_Suite_Application",
                "SheetExporter");

        public static string GetProfilesFolder(int revitYear) =>
            Path.Combine(GetAddinRoot(revitYear), "Profiles");

        public static string GetSettingsFile(int revitYear) =>
            Path.Combine(GetAddinRoot(revitYear), "settings.json");
    }

}