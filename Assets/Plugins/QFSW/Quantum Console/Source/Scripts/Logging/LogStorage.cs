using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace QFSW.QC
{
    public class LogStorage : ILogStorage
    {
        private readonly List<string> _consoleLogs = new List<string>(10);
        private readonly StringBuilder _logTraceBuilder = new StringBuilder(2048);

        public int MaxStoredLogs { get; set; }

        public LogStorage(int maxStoredLogs = -1)
        {
            MaxStoredLogs = maxStoredLogs;
        }

        public void AddLog(ILog log, bool newLine = true)
        {
            string logText = log.Text;
            if (_logTraceBuilder.Length > 0 && newLine)
            {
                logText = $"{Environment.NewLine}{logText}";
            }

            _consoleLogs.Add(logText);
            
            int logLength = _logTraceBuilder.Length + logText.Length;
            if (MaxStoredLogs > 0)
            {
                while (_consoleLogs.Count > MaxStoredLogs)
                {
                    int junkLength = Mathf.Min(_consoleLogs[0].Length, _logTraceBuilder.Length);
                    logLength -= junkLength;
                    
                    _logTraceBuilder.Remove(0, junkLength);
                    _consoleLogs.RemoveAt(0);
                }
            }
        
            int capacity = _logTraceBuilder.Capacity;
            while (capacity < logLength)
            {
                capacity *= 2;
            }
            
            _logTraceBuilder.EnsureCapacity(capacity);
            _logTraceBuilder.Append(logText);
        }

        public void RemoveLog()
        {
            if (_consoleLogs.Count > 0)
            {
                string log = _consoleLogs[_consoleLogs.Count - 1];
                _consoleLogs.RemoveAt(_consoleLogs.Count - 1);
                _logTraceBuilder.Remove(_logTraceBuilder.Length - log.Length, log.Length);
            }
        }

        public void Clear()
        {
            _consoleLogs.Clear();
            _logTraceBuilder.Length = 0;
        }

        public string GetLogString()
        {
            return _logTraceBuilder.ToString().TrimStart('\n', '\r');
        }
    }
}