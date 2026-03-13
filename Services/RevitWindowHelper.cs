using System;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Services
{
    public static class RevitWindowHelper
    {
        public static void SetRevitOwner(Window window, UIApplication uiApp)
        {
            var revitHandle = uiApp.MainWindowHandle;
            var helper = new WindowInteropHelper(window)
            {
                Owner = revitHandle
            };
        }
    }
}