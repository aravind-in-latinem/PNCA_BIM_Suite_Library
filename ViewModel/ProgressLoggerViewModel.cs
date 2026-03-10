using System;
using System.Text;
using PNCA_BIM_Suite_Library.Services;

namespace PNCA_BIM_Suite_Library.ViewModel
{
    public class ProgressLoggerViewModel : ILogger
    {
        public StringBuilder ExceptionMessageCollection { get; set; }
        
        public ProgressLoggerViewModel() 
        {
            ExceptionMessageCollection = new StringBuilder();
        }
        public event EventHandler ProgressUpdated;
        public void LogException(Exception ex , string task)
        {
            ExceptionMessageCollection.AppendLine($"Exception occurred during {task}:");
            ExceptionMessageCollection.AppendLine(ex.Message);
            if (ex.InnerException != null)
            {
                ExceptionMessageCollection.AppendLine("Inner Exception: " + ex.InnerException.Message);
            }
            ExceptionMessageCollection.AppendLine("Stack Trace: " + ex.StackTrace);
            UpdateUIText();
        }

        public void LogTaskCompleted(string task)
        {
            ExceptionMessageCollection.AppendLine($"{task} ✓");
            UpdateUIText();
        }

        private void UpdateUIText()
        {
            ProgressUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
