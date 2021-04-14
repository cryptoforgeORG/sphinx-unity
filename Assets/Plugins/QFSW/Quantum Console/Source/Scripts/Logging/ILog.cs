using UnityEngine;

namespace QFSW.QC
{
    public interface ILog
    {
        string Text { get; }
        LogType Type { get; }
    }
}