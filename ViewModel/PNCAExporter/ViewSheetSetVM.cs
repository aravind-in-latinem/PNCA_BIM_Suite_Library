using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.Views.PNCAExporter;

namespace PNCA_BIM_Suite_Library.ViewModel.PNCAExporter
{
    internal class ViewSheetSetVM : ObservableObject
    {
        public ObservableCollection<ViewSheetSetItem> ViewSheetSetCollection { get; set; }
        public ViewSheetSetItem SelectedViewSheetSet { get; set; }
        private Document _doc { get; set; }

        public PrintManager PrintManager { get; set; }
        public ViewSheetSetting ViewSheetSetting { get; set; }
        public ViewSheetSetVM(Document doc)
        {
            _doc = doc;

            ViewSheetSetCollection = new ObservableCollection<ViewSheetSetItem>();
            
            LoadVSSets();
        }

        
        private void LoadVSSets()
        {
            PrintManager = _doc.PrintManager;
            PrintManager.PrintRange = PrintRange.Select;
            ViewSheetSetting = PrintManager.ViewSheetSetting;

            var viewSheetSets = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewSheetSet))
                .Cast<ViewSheetSet>()
                .ToList();

            ViewSheetSetCollection.Clear();

            ViewSheetSetCollection.Add(new ViewSheetSetItem("<In Session>"));
            SelectedViewSheetSet = ViewSheetSetCollection.Where(a => a.Name == "<In Session>").First();

            foreach (var set in viewSheetSets)
                ViewSheetSetCollection.Add(new ViewSheetSetItem(set));


            if (viewSheetSets.Any())
            {
                using (var t = new Transaction(_doc))
                {
                    t.Start("Set ViewSet");
                    ViewSheetSetting.CurrentViewSheetSet = viewSheetSets.First();
                    t.Commit();
                }
            }
            else
            {
                ViewSheetSetting.CurrentViewSheetSet = ViewSheetSetting.InSession;
            }

        }

        public void SaveVSSet(ViewSet views)
        {
            if (SelectedViewSheetSet.Name == "<In Session>" || SelectedViewSheetSet == null)
                SaveAsVSSet(views);
            else
            {
                SelectedViewSheetSet.ViewSheetSet.Views = views;
                ViewSheetSetting.CurrentViewSheetSet = SelectedViewSheetSet.ViewSheetSet;
                ViewSheetSetting.Save();
            }

        }

        public void SaveAsVSSet(ViewSet views)
        {
            var dialog = new TextBoxDialogue
            {
                TitleText = "New View/Sheet Set Creation",
                Message = "View/Sheet Set Name:"
            };
            bool? result = dialog.ShowDialog();
            if (result != true || string.IsNullOrWhiteSpace(dialog.TextValue))
                throw new InvalidOperationException("A name is required for the View/Sheet Set Creation.");
            string newName = dialog.TextValue.Trim();
            SaveAsVSSetRevitDB(newName, views);
        }

        public void SaveAsVSSetRevitDB(string newName, ViewSet views)
        {
            using (var t = new Transaction(_doc, "New V/S Set"))
            {
                t.Start();
                ViewSheetSetting.CurrentViewSheetSet.Views = views;
                ViewSheetSetting.SaveAs(newName);
                t.Commit();

                // Update UI collection
                var wrapper = new ViewSheetSetItem(ViewSheetSetting.CurrentViewSheetSet as ViewSheetSet);
                ViewSheetSetCollection.Add(wrapper);
                SelectedViewSheetSet = wrapper;
                OnPropertyChanged(nameof(SelectedViewSheetSet));
            }
        }
        public void RenameVSSet()
        {
            if (SelectedViewSheetSet == null || SelectedViewSheetSet.Name == "<In Session>")
                throw new InvalidOperationException("Please select a valid View/Sheet Set to rename.");
            var dialog = new TextBoxDialogue
            {
                TitleText = "Rename View/Sheet Set",
                Message = "New View/Sheet Set Name:"
            };
            bool? result = dialog.ShowDialog();
            if (result != true || string.IsNullOrWhiteSpace(dialog.TextValue))
                throw new InvalidOperationException("A name is required for the View/Sheet Set Renaming.");
            var viewSet = SelectedViewSheetSet.ViewSheetSet.Views;
            string oldName = SelectedViewSheetSet.Name;
            string newName = dialog.TextValue.Trim();
            using (var t = new Transaction(_doc, "Rename V/S Set"))
            {
                t.Start();
                ViewSheetSetting.CurrentViewSheetSet = SelectedViewSheetSet.ViewSheetSet;
                ViewSheetSetting.Delete();
                ViewSheetSetting.CurrentViewSheetSet = ViewSheetSetting.InSession;
                ViewSheetSetting.SaveAs(newName);
                ViewSheetSetting.CurrentViewSheetSet.Views = viewSet;
                
                t.Commit();
                // Update UI collection
                var wrapper = new ViewSheetSetItem(ViewSheetSetting.CurrentViewSheetSet as ViewSheetSet);
                ViewSheetSetCollection.Add(wrapper);
                ViewSheetSetCollection.Remove(ViewSheetSetCollection.Where(a => a.Name == oldName).First());
                SelectedViewSheetSet = ViewSheetSetCollection.Where(a=>a.Name==newName).FirstOrDefault();
            }

        }
    
    public void DeleteVSSet()
        {
            using (var t = new Transaction(_doc, "Rename V/S Set"))
            {
                t.Start();
                ViewSheetSetting.CurrentViewSheetSet = SelectedViewSheetSet.ViewSheetSet;
                ViewSheetSetting.Delete();
                ViewSheetSetCollection.Remove(SelectedViewSheetSet);
                SelectedViewSheetSet = ViewSheetSetCollection.Last();
                ViewSheetSetting.CurrentViewSheetSet = SelectedViewSheetSet.ViewSheetSet;
                t.Commit();
            }
        }
    }
}

