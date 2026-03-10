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
using System.Windows.Shapes;
using Autodesk.Revit.UI.Mechanical;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.ViewModel;

namespace PNCA_BIM_Suite_Library.View
{
    /// <summary>
    /// Interaction logic for ProgressLoggerView.xaml
    /// </summary>
    public partial class ProgressLoggerView : Window
    {
        private string uiData = string.Empty;
        public ProgressLoggerView(ILogger progressLoggerViewModel)
        {
            InitializeComponent();
            this.DataContext = progressLoggerViewModel;
            (DataContext as ProgressLoggerViewModel).ProgressUpdated += ProgressLoggerViewModel_updateProgress;
        }

        private void ProgressLoggerViewModel_updateProgress(object sender, EventArgs e)
        {
            uiData = (DataContext as ProgressLoggerViewModel).ExceptionMessageCollection.ToString();
            DataUI.Text = uiData;
            DataUI.Clear();
            string[] lines = uiData.Split(new[] { '\n' }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if(!string.IsNullOrWhiteSpace(line))
                    DataUI.AppendText(line +
                                  "------------------------------------------------------------------------------"
                                  + Environment.NewLine);
            }
        }

    }
}
