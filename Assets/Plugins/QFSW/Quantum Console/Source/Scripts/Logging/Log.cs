using UnityEngine;

namespace QFSW.QC
{
    public readonly struct Log : ILog
    {
        public string Text { get; }
        public LogType Type { get; }

        public Log(string text, LogType type = LogType.Log)
        {
            Text = text;
            Type = type;
        }
    }
}
