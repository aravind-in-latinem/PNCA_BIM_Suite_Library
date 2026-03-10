using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Model;

namespace PNCA_BIM_Suite_Library.Services
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RevitDBUpdater

    {
        private Document _document;
        private UIDocument _uiDocument;
        
        
        public RevitDBUpdater(Document document, UIDocument uiDocument)
        {
            _document = document;
            _uiDocument = uiDocument;            
        }

        public List<LookupField> LookupFields { get; set; } = new List<LookupField>();
        public void UpdateRevitDB(DataTable dataTable, ScheduleDataFromElementsExtractor scheduledElementsExtractor)
        {
            List<Exception> errorCollection = new List<Exception>();
            var existingParamIdValuePair = new Dictionary<Parameter, string>();
            var lookupFields = scheduledElementsExtractor.GetScheduledFieldsLookupCollection();
            using (var t = new Transaction(_document, "Import Excel Data"))
            {
                t.Start();
                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {
                        var elemId = new ElementId(Convert.ToInt64(row["ElementId"]));
                        var paramName = row["ParameterName"].ToString();

                        var param = GetParameterFromSchedule(
                            scheduledElementsExtractor, elemId, paramName);

                        if (param == null)
                            continue;

                        if (param.StorageType != StorageType.String)
                            continue;

                        if (!IsUniqueConstrained(param))
                            continue;

                        // Temporary unique value
                        switch (row["UnitType"].ToString())
                        {
                            case "String":                                
                                    string tempValue = $"__TMP__{Guid.NewGuid():N}";
                                    param.Set(tempValue);
                                    break;
                            case "Integer":
                            int tempIntValue = (int)DateTime.Now.Ticks;
                            param.Set(tempIntValue);
                            break;
                            case "Double":
                            double tempDoubleValue = DateTime.Now.Ticks;
                            param.Set(tempDoubleValue);
                            break;
                            case "None":
                                break;
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        errorCollection.Add(ex);
                    }
                }

                _document.Regenerate();

                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {

                        var elemId = new ElementId(Convert.ToInt64(row["ElementId"]));
                        var paramName = row["ParameterName"].ToString();
                        var param = scheduledElementsExtractor.ScheduledElements
                        .FirstOrDefault(a => a.RowElementId?.Value == Convert.ToInt64(row["ElementId"]))
                        ?.ScheduledFields?
                        .FirstOrDefault(f => f.FieldName == paramName)?
                        .ParameterElement;
                        if (param == null)
                            continue;
                        switch (row["UnitType"].ToString())
                        {
                            case "String":
                                if (param.StorageType == StorageType.String)
                                {
                                    var success = param.Set(row["ValueInTable1"].ToString());
                                }
                                break;
                            case "Integer":
                                if (param.StorageType == StorageType.Integer && UnitFormatUtils.TryParse(_document.GetUnits(), SpecTypeId.Number,
                                        row["ValueInTable1"].ToString(), out double intValue))
                                {
                                    var success=param.Set(intValue);
                                    _document.Regenerate();
                                }
                                break;
                            case "Double":
                            {
                                if (param.StorageType != StorageType.Double)
                                    break;

                                var scheduledField = scheduledElementsExtractor.ScheduledElements
                                    .FirstOrDefault(a => a.RowElementId?.Value == Convert.ToInt64(row["ElementId"]))
                                    ?.ScheduledFields?
                                    .FirstOrDefault(f => f.FieldName == paramName);

                                if (scheduledField == null)
                                    break;

                                ForgeTypeId specTypeId = new ForgeTypeId(scheduledField.ForgeTypeId);

                                
                                if (!UnitUtils.IsMeasurableSpec(specTypeId))
                                {
                                    break;
                                }

                                string uiValueString = row["ValueInTable1"].ToString();

                                if (UnitFormatUtils.TryParse(
                                        _document.GetUnits(),
                                        specTypeId,
                                        uiValueString,
                                        out double internalValue))
                                {
                                    param.Set(internalValue);
                                }


                                break;
                            }

                            case "ElementId":
                                if (param.StorageType == StorageType.ElementId)
                                {
                                    var selectedElemId = FindElementIdByFieldAndName(paramName, row["ValueInTable1"].ToString(), lookupFields);
                                    var success = param.Set(selectedElemId);
                                    _document.Regenerate();
                                }
                                break;
                            case "Boolean":
                                if (param.StorageType == StorageType.Integer && param.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
                                {
                                    bool boolValue = false;
                                    if (bool.TryParse(row["ValueInTable1"].ToString(), out bool parsedBool))
                                    {
                                        boolValue = parsedBool;
                                    }
                                    else if (row["ValueInTable1"].ToString() == "1" || row["ValueInTable1"].ToString().Equals("Yes", StringComparison.OrdinalIgnoreCase))
                                    {
                                        boolValue = true;
                                    }
                                    param.Set(boolValue ? 1 : 0);
                                    _document.Regenerate();
                                }
                                break;
                            case "None":
                                break;

                        }


                    }
                    catch (Exception ex)
                    {
                        errorCollection.Add(ex);
                    }                    
                }
                if (errorCollection.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    var groupedErrors = errorCollection
                    .GroupBy(e => e.Message)
                    .Select(g => new
                    {
                        Message = g.Key,
                        Count = g.Count()
                    })
                    .ToList();
                    sb.AppendLine("The following errors occurred during the update process:");
                    foreach (var error in groupedErrors)
                    {
                        sb.AppendLine(error.Message);
                    }
                    TaskDialog.Show("Errors Occurred", sb.ToString());
                    throw (new InvalidOperationException());
                }
                    _document.Regenerate();
                t.Commit();
            }


        }

        public bool IsUniqueConstrained(Parameter param)
        {
            return param.Id == new ElementId(BuiltInParameter.SHEET_NUMBER)
                || param.Id == new ElementId(BuiltInParameter.VIEW_NAME)               
                || param.Id == new ElementId(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);              
        }
        private Parameter GetParameterFromSchedule(
    ScheduleDataFromElementsExtractor scheduledElementsExtractor,
    ElementId elemId,
    string paramName)
        {
            return scheduledElementsExtractor.ScheduledElements
                .FirstOrDefault(e => e.RowElementId?.Value == elemId.Value)?
                .ScheduledFields?
                .FirstOrDefault(f => f.FieldName == paramName)?
                .ParameterElement;
        }


        public static ElementId FindElementIdByFieldAndName(string fieldName, string elementName, List<LookupField> lookupFields)
        {
            if (lookupFields == null || string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(elementName))
                return ElementId.InvalidElementId;

            // Get the matching field
                var field = lookupFields.FirstOrDefault(f => f.FieldName == fieldName);

                if (field == null)
                    return ElementId.InvalidElementId; 

                if (field.ElementElementIdPairs == null || field.ElementElementIdPairs.Count == 0)
                    return ElementId.InvalidElementId; 

                // Look for the element name inside ElementElementIdPairs
                if (field.ElementElementIdPairs.ContainsKey(elementName))
                {
                    long idValue = field.ElementElementIdPairs[elementName];
                    return new ElementId(idValue);
                }
            

            // If nothing is found
            throw new InvalidOperationException(
            $"No element found matching name '{elementName}' under field '{fieldName}'.");
        }

    }
}
