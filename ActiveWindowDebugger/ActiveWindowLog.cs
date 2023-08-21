using System;
using System.Diagnostics;

using PropertyChanged;

namespace ActiveWindowDebugger;

[AddINotifyPropertyChangedInterface]
public class ActiveWindowLog
{
    public string? Name => Process?.ProcessName;

    public int? PID => Process?.Id;

    public Process Process { get; init; }

    public DateTime Time { get; init; }

    public string? Title => Process?.MainWindowTitle;

    public ActiveWindowLog(DateTime time, Process process)
    {
        Time = time;
        Process = process;
    }

    public override string ToString()
    {
        return $"[{Time:HH:mm:ss.fff}] [{PID,-5}] [{Name}] {Title}";
    }

    public string ToStringWithoutTime()
    {
        return $"[{PID,-5}] [{Name}] {Title}";
    }
}
