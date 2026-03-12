using System.Windows;
using PNCA_BIM_Suite_Library.ViewModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Input;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;

namespace PNCA_BIM_Suite_Library.Views
{
    public partial class SheetLinkImport : Window
    {
        
        public SheetLinkImport(Document document, UIDocument uiDocument , ILogger progressLogger)
        {
            InitializeComponent();
            this.DataContext = new SheetLinkImportViewModel(document, uiDocument,this,progressLogger);
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}