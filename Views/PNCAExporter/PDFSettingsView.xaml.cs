using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PNCA_BIM_Suite_Library.Views.PNCAExporter
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class PDFSettingsView : UserControl
    {
        public PDFSettingsView()
        {
            InitializeComponent();
        }
        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }
    
        
    }
  
    
}
