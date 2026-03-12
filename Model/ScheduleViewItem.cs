using Autodesk.Revit.DB;

namespace PNCA_BIM_Suite_Library.Model
{
    // Helper class for ComboBox items
    public class ScheduleViewItem
    {
        public ElementId ViewId { get; set; }
        public string Name { get; set; }
        public ViewSchedule Schedule { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}