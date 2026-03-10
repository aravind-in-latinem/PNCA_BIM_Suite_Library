using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using PNCA_BIM_Suite_Library.Model;

namespace PNCA_BIM_Suite_Library.Services
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ScheduleDataFromElementsExtractor

    {
        public ViewSchedule ScheduleView { get; set; }
        public List<ScheduledElement> ScheduledElements { get; set; } = new List<ScheduledElement>();

        public Document _document;

        private ILogger _progressLogger;

        List<string> fromRoomParamNames = new List<string>();
        List<string> toRoomParamNames = new List<string>();

        public ScheduleDataFromElementsExtractor(ViewSchedule scheduleView, Document document, ILogger progressLogger)
        {
            _document = document;
            ScheduleView = scheduleView;
            _progressLogger = progressLogger;
        }
        public DataTable CreateScheduleDataTable()
        {

            var dataTableBuilder = new DataTableCreator();
            #region ViewCollector
            var visibleElem = new FilteredElementCollector(_document, ScheduleView.Id).ToElements();
            var scheduleFieldCount = ScheduleView.Definition.GetFieldCount();
            var paramIdFieldIndexPair = new Dictionary<ElementId, int>();


            #endregion
            for (int i = 0; i < scheduleFieldCount; i++)
            {
                var fieldParamElemId = ScheduleView.Definition.GetField(i).ParameterId;
                var fieldIndex = ScheduleView.Definition.GetField(i).FieldIndex;
                // initializing parameter id and field index pair dictionary.
                if (fieldParamElemId != new ElementId(Convert.ToInt64(-1)))
                {
                    if (!paramIdFieldIndexPair.ContainsKey(fieldParamElemId))
                        paramIdFieldIndexPair.Add(fieldParamElemId, fieldIndex);
                }
                else if (ScheduleView.Definition.GetField(i).IsCombinedParameterField)
                {
                    var combinedParameters = ScheduleView.Definition.GetField(i).GetCombinedParameters().ToList().Select(a => a.ParamId);
                    foreach (var paramId in combinedParameters)
                    {
                        if (!paramIdFieldIndexPair.ContainsKey(paramId))
                            paramIdFieldIndexPair.Add(paramId, fieldIndex);
                    }
                }
                if (ScheduleView.Definition.GetField(i).FieldType == ScheduleFieldType.FromRoom)
                {
                    fromRoomParamNames.Add(ScheduleView.Definition.GetField(i).GetName());
                }
                if (ScheduleView.Definition.GetField(i).FieldType == ScheduleFieldType.ToRoom)
                {
                    toRoomParamNames.Add(ScheduleView.Definition.GetField(i).GetName());
                }
            }
            _progressLogger.LogTaskCompleted("Completed Reading all the field header information");
            //iterating through visible elements and sending respective parameters to parameter processors.
            var instanceParameterCount = 0;
            var typeParameterCount = 0;
            var roomParameterCount = 0;
            foreach (var elem in visibleElem)
            {

                var scheduledElement = new ScheduledElement();
                scheduledElement.RowElementId = elem.Id;
                //handle instance parameters
                var instanceParameterSet = ProcessParametersForExport(elem, paramIdFieldIndexPair, "Instance", elem.Parameters.OfType<Parameter>().ToList());
                instanceParameterCount = instanceParameterSet.Count();
                scheduledElement.ScheduledFields.AddRange(instanceParameterSet);
                
                //handle type parameters
                var elemSymbol = _document.GetElement(elem.GetTypeId());
                if(elemSymbol!=null)
                {
                    var typeParameterSet = ProcessParametersForExport(elem, paramIdFieldIndexPair, "Type", elemSymbol.Parameters.OfType<Parameter>().ToList());
                    scheduledElement.ScheduledFields.AddRange(typeParameterSet);
                    typeParameterCount = typeParameterSet.Count();
                }
                
                
                //handle room parameters
                var fromRoomParameterSet = new List<Parameter>();
                var toRoomParameterSet = new List<Parameter>();
                if (fromRoomParamNames.Count != 0)
                {
                    fromRoomParameterSet = (elem as FamilyInstance).FromRoom?.Parameters.OfType<Parameter>().ToList() ?? new List<Parameter>();
                }
                if (toRoomParamNames.Count != 0)
                {
                    toRoomParameterSet = (elem as FamilyInstance).ToRoom?.Parameters.OfType<Parameter>().ToList() ?? new List<Parameter>();
                }
                var roomParameterSet = ProcessParametersForExport(elem, paramIdFieldIndexPair, "Room", fromRoomParameterSet.Concat(toRoomParameterSet).ToList());
                roomParameterCount = roomParameterSet.Count();
                scheduledElement.ScheduledFields.AddRange(roomParameterSet);
                
                ScheduledElements.Add(scheduledElement);

            }
            _progressLogger.LogTaskCompleted($"{instanceParameterCount} Instance Parameters has been processed");
            _progressLogger.LogTaskCompleted($"{typeParameterCount} Type parameters has been processed");
            _progressLogger.LogTaskCompleted($"{roomParameterCount} Room parameters has been processed");
            var dataTable = dataTableBuilder.PrepareTableData(ScheduledElements);
            _progressLogger.LogTaskCompleted("Data Table created for the selected Revit Schedule");
            //TaskDialog.Show("Success", "Read Success");

            return dataTable;
        }


        public List<LookupField> GetScheduledFieldsLookupCollection()
        {

            //var dataTableBuilder = new DataTableCreator();
            #region ViewCollector
            var visibleElem = new FilteredElementCollector(_document, ScheduleView.Id).ToElements();
            var scheduleFieldCount = ScheduleView.Definition.GetFieldCount();
            var paramIdFieldIndexPair = new Dictionary<ElementId, int>();
            HashSet<LookupField> lookupList =
                new HashSet<LookupField>(new LookupFieldComparer());


            #endregion
            for (int i = 0; i < scheduleFieldCount; i++)
            {
                var fieldParamElemId = ScheduleView.Definition.GetField(i).ParameterId;
                var fieldIndex = ScheduleView.Definition.GetField(i).FieldIndex;
                // initializing parameter id and field index pair dictionary.
                if (fieldParamElemId != new ElementId(Convert.ToInt64(-1)))
                {
                    if (!paramIdFieldIndexPair.ContainsKey(fieldParamElemId))
                        paramIdFieldIndexPair.Add(fieldParamElemId, fieldIndex);
                }
                else if (ScheduleView.Definition.GetField(i).IsCombinedParameterField)
                {
                    var combinedParameters = ScheduleView.Definition.GetField(i).GetCombinedParameters().ToList().Select(a => a.ParamId);
                    foreach (var paramId in combinedParameters)
                    {
                        if (!paramIdFieldIndexPair.ContainsKey(paramId))
                            paramIdFieldIndexPair.Add(paramId, fieldIndex);
                    }
                }
                if (ScheduleView.Definition.GetField(i).FieldType == ScheduleFieldType.FromRoom)
                {
                    fromRoomParamNames.Add(ScheduleView.Definition.GetField(i).GetName());
                }
                if (ScheduleView.Definition.GetField(i).FieldType == ScheduleFieldType.ToRoom)
                {
                    toRoomParamNames.Add(ScheduleView.Definition.GetField(i).GetName());
                }
            }
            
            //iterating through visible elements and sending respective parameters to parameter processors.



            
            foreach (var elem in visibleElem)
            {
                //handle instance parameters
                var instanceParameterSet = ProcessParametersForImport(elem, paramIdFieldIndexPair, "Instance",
                    elem.Parameters.OfType<Parameter>().ToList());
                var instanceParameterCount = instanceParameterSet.Count();
                
                lookupList.UnionWith(instanceParameterSet);

                //handle type parameters
                var elemSymbol = _document.GetElement(elem.GetTypeId());
                var typeParameterCount = 0;
                if (elemSymbol != null)
                {
                    var typeParameterSet = ProcessParametersForImport(elem, paramIdFieldIndexPair, "Type",
                        elemSymbol.Parameters.OfType<Parameter>().ToList());
                    lookupList.UnionWith(typeParameterSet);
                    typeParameterCount = typeParameterSet.Count();
                }

                //handle room parameters
                var fromRoomParameterSet = new List<Parameter>();
                var toRoomParameterSet = new List<Parameter>();
                if (fromRoomParamNames.Count != 0)
                {
                    fromRoomParameterSet = (elem as FamilyInstance).FromRoom?.Parameters.OfType<Parameter>().ToList() ??
                                           new List<Parameter>();
                }

                if (toRoomParamNames.Count != 0)
                {
                    toRoomParameterSet = (elem as FamilyInstance).ToRoom?.Parameters.OfType<Parameter>().ToList() ??
                                         new List<Parameter>();
                }

                var roomParameterSet = ProcessParametersForImport(elem, paramIdFieldIndexPair, "Room",
                    fromRoomParameterSet.Concat(toRoomParameterSet).ToList());
                var roomParameterCount = roomParameterSet.Count();
                _progressLogger.LogTaskCompleted($"Lookup data for fields created");
                lookupList.UnionWith(roomParameterSet);
                if(lookupList.Count()==scheduleFieldCount)
                {
                    break;
                }
            }

            //var fieldForDeepSearch = lookupList.Where(a => a.ParameterType)

            return lookupList.ToList();
        }

        


        public List<ScheduledField> ProcessParametersForExport(Element elem, Dictionary<ElementId, int> fieldIds, string parameterType, List<Parameter> parameterCollection)
        {
            List<ScheduledField> scheduledFields = new List<ScheduledField>();

            foreach (var p in parameterCollection)
            {
                if (fieldIds.Keys.Contains(p.Id))
                {
                    ScheduledField field = new ScheduledField();
                    if (p.Definition.Name == "Level")
                        field.ParameterElement = elem.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM);
                    else
                        field.ParameterElement = p;
                    if (!parameterType.Equals("Room"))
                        field.FieldName = p.Definition.Name;
                    if (parameterType.Equals("Room"))
                    {
                        if (fromRoomParamNames.Contains(String.Concat("From Room: ", p.Definition.Name)))
                            field.FieldName = "From Room: " + p.Definition.Name;
                        else if (toRoomParamNames.Contains(String.Concat("To Room: ", p.Definition.Name)))
                            field.FieldName = "To Room: " + p.Definition.Name;
                    }
                    if (!String.IsNullOrEmpty(field.FieldName))
                    {
                        field.FieldValue = p.AsString() ?? p.AsValueString() ?? string.Empty;
                        field.SelectedElementId = elem.Id;
                        field.ParameterType = parameterType;
                        field.UnitType = p.StorageType.ToString();
                        field.ForgeTypeId = p.Definition.GetDataType().TypeId;
                        field.FieldIndex = fieldIds[p.Id];
                        scheduledFields.Add(field);
                    }
                }
            }

            return scheduledFields;
        }

        public List<LookupField> ProcessParametersForImport(Element elem, Dictionary<ElementId, int> fieldIds, string parameterType, List<Parameter> parameterCollection)
        {
            List<LookupField> scheduledFields = new List<LookupField>();
            List<int> fieldIndicesForElementIdDt = new List<int>();
            foreach (var p in parameterCollection)
            {
                if (fieldIds.Keys.Contains(p.Id))
                {
                    LookupField field = new LookupField();
                    if (p.Definition.Name == "Level")
                        field.ParameterElement = elem.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM);
                    else
                        field.ParameterElement = p;
                    if (!parameterType.Equals("Room"))
                        field.FieldName = p.Definition.Name;
                    if (parameterType.Equals("Room"))
                    {
                        if (fromRoomParamNames.Contains(String.Concat("From Room: ", p.Definition.Name)))
                            field.FieldName = "From Room: " + p.Definition.Name;
                        else if (toRoomParamNames.Contains(String.Concat("To Room: ", p.Definition.Name)))
                            field.FieldName = "To Room: " + p.Definition.Name;
                    }
                    if (!String.IsNullOrEmpty(field.FieldName))
                    {
                        //field.FieldValue = p.AsString() ?? p.AsValueString() ?? string.Empty;
                        field.SelectedElementId = elem.Id;
                        field.ParameterType = parameterType;
                        field.UnitType = p.StorageType.ToString();
                        field.ForgeTypeId = p.Definition.GetDataType().TypeId;
                        field.FieldIndex = fieldIds[p.Id];
                        if (p.StorageType == StorageType.ElementId)
                        {
                            PopulateElementLookupForElementIdParameter(_document, p.AsElementId(), field);
                        }
                        if (p.StorageType == StorageType.Integer && p.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
                        {
                            PopulateElementLookupForBooleanParameter(field);
                            field.UnitType = "Boolean";
                        }
                        scheduledFields.Add(field);
                    }
                }
            }

            

            return scheduledFields;
        }

        public static void PopulateElementLookupForElementIdParameter(
        Document doc,
        ElementId referencedId,
        LookupField scheduledField)
        {
            if (doc == null || scheduledField == null || referencedId == null || referencedId == ElementId.InvalidElementId)
                return;

            // Try to resolve the element
            var referencedElement = doc.GetElement(referencedId);
            if (referencedElement == null)
                return;

            Category category = referencedElement.Category;
            if (category == null)
                return;

            // Collect all elements of that category
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elements =
                collector.OfCategoryId(category.Id).WhereElementIsNotElementType().ToElements();

            Dictionary<string, long> values = new Dictionary<string, long>();

            foreach (var element in elements)
            {
                string name = GetElementName(element);

                if (!values.ContainsKey(name))
                {
                    values.Add(name, element.Id.Value);
                }
            }

            scheduledField.ElementElementIdPairs = values;
        }

        private void PopulateElementLookupForElementIdParameterDeepMode(LookupField lookupField, IList<Element> visibleElements)
        {
            foreach (var element in visibleElements)
            {
                var referenceElement = element.get_Parameter(lookupField.ParameterElement.Definition).AsElementId();
                PopulateElementLookupForElementIdParameter(_document, referenceElement, lookupField);
            }
        }



        public static void PopulateElementLookupForBooleanParameter(LookupField scheduledField)
        {
            if (scheduledField == null)
                return;
            Dictionary<string, long> values = new Dictionary<string,long>
            {
                { "Yes", 1 },
                { "No", 0 }
            };
            // Assign to ScheduledField
            scheduledField.ElementElementIdPairs = values;
        }



        


        /// <summary>
        /// Safely get a usable name (fallback to symbol or id if unnamed).
        /// </summary>

        private static string GetElementName(Element e)
        {
            string name = e.Name;

            if (string.IsNullOrWhiteSpace(name) && e is FamilyInstance fi && fi.Symbol != null)
            {
                name = fi.Symbol.Name;
            }

            return string.IsNullOrWhiteSpace(name) ? ("Element " + e.Id.Value) : name;
        }



    }
}
