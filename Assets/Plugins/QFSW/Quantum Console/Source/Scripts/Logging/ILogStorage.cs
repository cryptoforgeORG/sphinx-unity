namespace QFSW.QC
{
    public interface ILogStorage
    {
        int MaxStoredLogs { get; set; }

        void AddLog(ILog log, bool newLine = true);
        void RemoveLog();
        void Clear();

        string GetLogString();
    }

}