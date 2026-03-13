using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for TextBoxDialogue.xaml
    /// </summary>
    public partial class ProfileSaveAsDialog : Window, INotifyPropertyChanged
    {
        private string _textValue;

        public string TextValue
        {
            get => _textValue;
            set
            {
                _textValue = value;
                OnPropertyChanged();
            }
        }        

        public ProfileSaveAsDialog()
        {
            InitializeComponent();            
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
