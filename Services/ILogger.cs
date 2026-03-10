using System;

namespace PNCA_BIM_Suite_Library.Services
{
    public interface ILogger
    {
        void LogException(Exception ex, string task);
        void LogTaskCompleted(string task);
    }
}
