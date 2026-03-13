using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.ViewModel;
using PNCA_BIM_Suite_Library.ViewModel.PNCAExporter;

namespace PNCA_BIM_Suite_Library.Services
{
    public class RevitCollectorServices

    {
        private readonly Document _document;
        public RevitCollectorServices(Document document)
        {
            _document = document;
        }
        public List<SheetItem> GetSheets()
        {
            var sheets = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Sheets)
            .WhereElementIsNotElementType()
            .Cast<ViewSheet>()
            .Select(sheet =>
            {
                // Get revision information safely
                string revisionNumber = "0"; // Default
                var currentRevision = sheet.GetCurrentRevision();

                if (currentRevision != null)
                {
                    // Get the revision element from the sheet's revision cloud
                    var revision = _document.GetElement(currentRevision.Value.ToString()) as Revision;
                    if (revision != null)
                    {
                        revisionNumber = revision.SequenceNumber.ToString();
                        // OR use RevisionNumber if you want the actual number (like "1", "A", etc.)
                        // revisionNumber = revision.RevisionNumber;
                    }
                }
                return new SheetItem
                    {
                    SheetElement = sheet,
                    ItemElementId =sheet.Id,
                    SheetNumber = sheet.SheetNumber,
                    Name = sheet.Name,
                    RevisionNo = revisionNumber,
                    Size = SheetSizeDetector.GetSheetSize(_document, sheet),
                    Orientation = "Landscape", // Use a method instead of hardcoding
                    FileName = $"{sheet.SheetNumber}_{sheet.Name}"
                    };
            })                
                .OrderBy(a=>a.SheetNumber)
                .ToList();
                return sheets;
        }
        public static EnumItem<T>[] GetEnumItems<T>()
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .Select(v => new EnumItem<T>(v))
                       .ToArray();
        }
        public static List<Model.ViewItem> GetAllViews(Document doc)
        {
            ViewSet m_allViews = new ViewSet();
            var viewItems = new List<Model.ViewItem>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementIterator itor = collector.OfClass(typeof(Autodesk.Revit.DB.View)).GetElementIterator();
            itor.Reset();
            while (itor.MoveNext())
            {
                Autodesk.Revit.DB.View view = itor.Current as Autodesk.Revit.DB.View;
                // skip view templates because they're invisible in project browser
                if (null == view || view.IsTemplate)
                {
                    continue;
                }
                else
                {
                    ElementType objType = doc.GetElement(view.GetTypeId()) as ElementType;
                    if (null == objType || objType.Name.Equals("Schedule")
                        || objType.Name.Equals("Drawing Sheet") || objType.Name.Equals("Sheet"))
                    {
                        continue;
                    }
                    else
                    {
                        var disciplineParam = view.get_Parameter(BuiltInParameter.VIEW_DISCIPLINE);

                        string discipline =disciplineParam != null && disciplineParam.HasValue
                                ? disciplineParam.AsValueString()
                                : "None";

                        viewItems.Add(new Model.ViewItem
                        {
                            ViewElement = view,
                            ItemElementId = view.Id,
                            Name = view.Name,
                            ViewType = objType.Name,
                            ViewScale = view.get_Parameter(BuiltInParameter.VIEW_SCALE_PULLDOWN_IMPERIAL)?.AsValueString().Trim() ?? "N/A",
                            DetailLevel = view.DetailLevel.ToString().Trim(),
                            Discipline = discipline,
                            FileName = $"{view.Name}_{view.ViewType}.pdf"
                        });                        
                    }
                }
            }
            return viewItems;
        }


    }
}
