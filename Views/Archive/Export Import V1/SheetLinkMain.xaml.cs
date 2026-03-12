using System.Windows;
using PNCA_BIM_Suite_Library.ViewModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.View
{
    public partial class SheetLinkMain : Window
    {
        public SheetLinkMain(Document document, UIDocument uiDocument)
        {
            InitializeComponent();
            this.DataContext = new SheetLinkMainViewModel(document, uiDocument,this);
        }
    }
}