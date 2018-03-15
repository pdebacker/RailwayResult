using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Railway.Result;

namespace RailwayResultTests.Examples
{
    public class SimpleLogger : IResultFailureLogger
    {
        private static object _lock = new object();
        private readonly string _fileName;

        public SimpleLogger(string logFileName)
        {
            _fileName = logFileName;
        }

        public void ClearLog()
        {
            if (System.IO.File.Exists(_fileName))
                System.IO.File.Delete(_fileName);
        }
        public void LogFailure(ResultFailure failureInfo)
        {
            lock (_lock)
            {
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                System.IO.File.AppendAllText(_fileName, timeStamp + failureInfo.ToString());
            }
        }
    }
}
