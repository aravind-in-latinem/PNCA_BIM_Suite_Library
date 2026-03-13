using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace PNCA_BIM_Suite_Library.Services
{

    public class SheetSizeDetector
    {
        private static readonly Dictionary<string, (double width, double height)> StandardSizes = new Dictionary<string, (double, double)>
    {
        // Metric sizes (in mm converted to feet)
        {"A4", (0.279, 0.197)},     // 297 x 210 mm
        {"A3", (0.394, 0.279)},     // 420 x 297 mm
        {"A2", (0.558, 0.394)},     // 594 x 420 mm
        {"A1", (0.791, 0.558)},     // 841 x 594 mm
        {"A0", (1.118, 0.791)},     // 1189 x 841 mm
        {"A5", (0.689, 0.486)},                // 210 x 148 mm
        {"LETTER", (0.708, 0.917)},            // 8.5 x 11 in
        {"LETTER SMALL", (0.625, 0.833)},      // 7.5 x 10 in
        
        // Imperial sizes (in inches converted to feet)
        {"ANSI A", (0.917, 0.708)}, // 11 x 8.5 inches
        {"ANSI B", (1.167, 0.917)}, // 17 x 11 inches
        {"ANSI C", (1.667, 1.167)}, // 22 x 17 inches
        {"ANSI D", (2.167, 1.667)}, // 34 x 22 inches
        {"ANSI E", (2.833, 2.167)}, // 44 x 34 inches
        
        // Architectural sizes
        {"ARCH A", (0.917, 0.708)}, // 12 x 9 inches
        {"ARCH B", (1.250, 0.917)}, // 18 x 12 inches
        {"ARCH C", (1.667, 1.250)}, // 24 x 18 inches
        {"ARCH D", (2.333, 1.667)}, // 36 x 24 inches
        {"ARCH E", (3.000, 2.333)}, // 48 x 36 inches
        {"ARCH E1", (2.500, 3.500)}, // 30 x 42 in
        {"ARCH E2", (2.167, 3.167)}, // 26 x 38 in
        {"ARCH E3", (2.250, 3.250)}, // 27 x 39 in
    };

        public static string GetSheetSize(Document document, ViewSheet sheetView)
        {
            // category and are family instances.
            ElementClassFilter familyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));

            // Create a category filter for Doors
            ElementCategoryFilter titleBlockCategoryfilter = new ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks);
            LogicalAndFilter titleblockInstancesFilter = new LogicalAndFilter(familyInstanceFilter, titleBlockCategoryfilter);
            var titleblockId = sheetView.GetDependentElements(titleblockInstancesFilter).FirstOrDefault();
            if (titleblockId == null)
            {
                return "N/A";
            }
            var sheet = document.GetElement(titleblockId);

            // If not found, calculate from width and height


            double widthInFeet = sheet.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble();
            double heightInFeet = sheet.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble();


            // Round to reasonable precision
            widthInFeet = Math.Round(widthInFeet, 4);
            heightInFeet = Math.Round(heightInFeet, 4);

            // Try to match with standard sizes
            string matchedSize = MatchStandardSize(widthInFeet, heightInFeet);

            if (!string.IsNullOrEmpty(matchedSize))
            {
                return matchedSize;
            }

            // Return custom size
            return FormatCustomSize(widthInFeet, heightInFeet);
        }

        private static string MatchStandardSize(double width, double height)
        {
            // Allow small tolerance for floating point comparison
            const double tolerance = 0.001;

            foreach (var size in StandardSizes)
            {
                // Check both orientations
                if ((Math.Abs(width - size.Value.width) < tolerance &&
                     Math.Abs(height - size.Value.height) < tolerance) ||
                    (Math.Abs(width - size.Value.height) < tolerance &&
                     Math.Abs(height - size.Value.width) < tolerance))
                {
                    return size.Key;
                }
            }

            return null;
        }

        private static string FormatCustomSize(double width, double height)
        {
            // Convert to inches for display (more readable for imperial)
            double widthInInches = width * 12;
            double heightInInches = height * 12;

            // Round to nearest 1/16 inch
            widthInInches = Math.Round(widthInInches * 16) / 16;
            heightInInches = Math.Round(heightInInches * 16) / 16;

            // Format as fraction if needed
            string widthString = FormatFraction(widthInInches);
            string heightString = FormatFraction(heightInInches);

            return $"Custom ({widthString} × {heightString} in)";
        }

        private static string FormatFraction(double inches)
        {
            int whole = (int)inches;
            double fraction = inches - whole;

            if (Math.Abs(fraction) < 0.001)
                return whole.ToString();

            // Convert decimal to fraction
            int numerator = (int)(fraction * 16);
            if (numerator == 0)
                return whole.ToString();

            // Simplify fraction
            if (numerator % 8 == 0) return $"{whole} 1/2";
            if (numerator % 4 == 0) return $"{whole} 1/4";
            if (numerator % 2 == 0) return $"{whole} 1/8";

            return $"{whole} {numerator}/16";
        }
    }
}