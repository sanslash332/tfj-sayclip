using System;
using logSystem;

namespace sayclip
{
    internal class SayclipLoggerAdapter : ISayclipLogger
    {
        private readonly NLog.Logger _log;

        public SayclipLoggerAdapter()
        {
            _log = LogWriter.getLog();
        }

        public void Debug(string message) => _log.Debug(message);
        public void Info(string message) => _log.Info(message);
        public void Warn(string message) => _log.Warn(message);
        public void Error(string message, Exception ex = null)
        {
            if (ex != null)
                _log.Error(ex, message);
            else
                _log.Error(message);
        }
    }
}
