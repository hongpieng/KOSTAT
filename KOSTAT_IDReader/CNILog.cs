using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

public static class CNILog
{
    public delegate void MessageEventHandle(string str, bool bShow);
    public static event MessageEventHandle Message;

    public delegate void StatusEventHandle(string str);
    public static event StatusEventHandle StatusChange;

    private static readonly SynchronizationContext synchronization = SynchronizationContext.Current;

    static void OnMessage(string str, bool bShow)
    {
        Message?.Invoke(str, bShow);
    }

    static void OnStatus(string str)
    {
        StatusChange?.Invoke(str);
    }

    public static void Write(string sLog, bool bShow)
    {
        try
        {
            string timestampedLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff}]  {sLog}\r\n";
            OnMessage(timestampedLog, bShow);

            string logDir = Path.Combine(Application.StartupPath, "Log");
            string logFile = Path.Combine(logDir, $"{DateTime.Now:yyyy_MM_dd}.txt");

            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            File.AppendAllText(logFile, timestampedLog, Encoding.Default);
        }
        catch (Exception ex)
        {
            OnMessage($"로그 기록 오류: {ex.Message}", true);
        }
    }

    public static void ChangeStatus(string status)
    {
        OnStatus(status);
    }
}

