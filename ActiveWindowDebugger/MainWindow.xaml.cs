using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using PropertyChanged;

namespace ActiveWindowDebugger;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class MainWindow : Window
{
    private const int _initActivatePoll = 1;

    private static readonly DispatcherTimer _activateTimer = new();

    private static readonly DispatcherTimer _checkActiveWindowTimer = new();

    private static readonly DispatcherTimer _logTimer = new();

    private static readonly int[] _polls = new[] { 0, 1, 3, 5, 10, 25, 50, 100, 125, 150, 200, 250, 500, 1000 };

    private static Regex? _autoStartRegex = null;

    private static Regex? _autoStopRegex = null;

    private string _activatePollString = "1";

    private Exception? _autoStartPatternException = null;

    private int _autoStartPid = -1;

    private string _autoStartPidString = "-1";

    private Exception? _autoStopPatternException = null;

    private int _autoStopPid = -1;

    private string _autoStopPidString = "-1";

    private DoubleAnimation _scaleX = new() {
        From = 0,
        To = 1,
        Duration = TimeSpan.FromMilliseconds(_initActivatePoll)
    };

    public static Version? Version => Assembly.GetExecutingAssembly().GetName().Version;

    public static string WindowTitle => $"ActiveWindowDebugger v{Version?.Major}.{Version?.Minor}";

    public int ActivatePoll { get; private set; } = _initActivatePoll;

    public string ActivatePollString
    {
        get => _activatePollString;
        set
        {
            value = value.Trim();

            if (value.StartsWith("+") || value.StartsWith("-"))
            {
                value = value[1..];
            }

            if (value.EndsWith("ms"))
            {
                value = value[..^2];
            }

            bool isNumeric = Int32.TryParse(value, out int number) && number >= 0;

            if (isNumeric)
            {
                if (number < 0)
                {
                    return;
                }

                _activatePollString = value;
                ActivatePoll = number;
                ReStart(number);
            }
        }
    }

    public string ActivatePollToolTip => $"激活频率: {ActivatePoll}ms\n\nx1: 滚轮\nx10: Ctrl+滚轮\nx100: Shift+滚轮";

    public string ActiveWindowKeepTimeString
    {
        get
        {
            var recent = ActiveWindowLogs.LastOrDefault()?.Time;
            var now = ActiveWindowLog?.Time;

            if (recent is null || now is null)
            {
                return $"+{TimeSpan.Zero:hh\\:mm\\:ss\\.fff}";
            }

            return $"+{now - recent:hh\\:mm\\:ss\\.fff}";
        }
    }

    public ActiveWindowLog? ActiveWindowLog { get; private set; }

    public ObservableCollection<ActiveWindowLog> ActiveWindowLogs { get; private set; } = new();

    public string? ActiveWindowLogWithoutTime => ActiveWindowLog?.ToStringWithoutTime();

    public string AutoStartIcon => CanAutoStart ? "✔️" : "❌";

    public SolidColorBrush AutoStartIconColor => CanAutoStart ? Brushes.Green : Brushes.Red;

    public bool AutoStartMeet { get; set; } = true;

    public string AutoStartPattern { get; set; } = ".*";

    public bool AutoStartPatternMatchCase { get; set; } = true;

    public string AutoStartPatternToolTip => IsAutoStartPatternValid
        ? $"正则表达式正确:\n{AutoStartPattern}"
        : $"正则表达式错误:\n{AutoStartPattern}\n\n{_autoStartPatternException?.Message}";

    public int AutoStartPid
    {
        get => _autoStartPid;
        private set
        {
            _autoStartPid = value;

            if (value == -1)
            {
                cboAutoStartStates.SelectedIndex = 0;
                cboAutoStartStates.IsEnabled = false;
            }
            else
            {
                cboAutoStartStates.IsEnabled = true;
            }
        }
    }

    public string AutoStartPidString
    {
        get => _autoStartPidString;
        set
        {
            value = value.Trim();

            if (value == "" || value.StartsWith("-"))
            {
                value = "-1";
            }
            else if (value.StartsWith("+"))
            {
                value = value[1..];
            }

            if (Int32.TryParse(value, out int pid))
            {
                _autoStartPidString = value;
                AutoStartPid = pid;
            }
        }
    }

    public string AutoStartPidToolTip => IsAutoStartProcessExists
        ? $"进程正在运行...\n[{AutoStartProcess?.Id}] [{AutoStartProcess?.ProcessName}] {AutoStartProcess?.MainWindowTitle}"
        : $"进程 [{AutoStartPid}] 不存在";

    public Process? AutoStartProcess { get; private set; }

    public bool? AutoStartState { get; set; } = true;

    public string AutoStartStatesTooTip => AutoStartPid == -1 ? "PID 为 -1 时此选项不可用" : "约束指定 PID 进程的窗口活动状态";

    public string AutoStartToolTip => CanAutoStart ? "自动开始触发条件已动作" : "自动开始触发条件未动作";

    public string AutoStopIcon => CanAutoStop ? "✔️" : "❌";

    public SolidColorBrush AutoStopIconColor => CanAutoStop ? Brushes.Green : Brushes.Red;

    public bool AutoStopMeet { get; set; } = true;

    public string AutoStopPattern { get; set; } = ".*";

    public bool AutoStopPatternMatchCase { get; set; } = true;

    public string AutoStopPatternToolTip => IsAutoStopPatternValid
        ? $"正则表达式正确:\n{AutoStopPattern}"
        : $"正则表达式错误:\n{AutoStopPattern}\n\n{_autoStopPatternException?.Message}";

    public int AutoStopPid
    {
        get => _autoStopPid;
        private set
        {
            _autoStopPid = value;

            if (value == -1)
            {
                cboAutoStopStates.SelectedIndex = 0;
                cboAutoStopStates.IsEnabled = false;
            }
            else
            {
                cboAutoStopStates.IsEnabled = true;
            }
        }
    }

    public string AutoStopPidString
    {
        get => _autoStopPidString;
        set
        {
            value = value.Trim();

            if (value == "" || value.StartsWith("-"))
            {
                value = "-1";
            }
            else if (value.StartsWith("+"))
            {
                value = value[1..];
            }

            if (Int32.TryParse(value, out int pid))
            {
                _autoStopPidString = value;
                AutoStopPid = pid;
            }
        }
    }

    public string AutoStopPidToolTip => IsAutoStopProcessExists
        ? $"进程正在运行...\n[{AutoStopProcess?.Id}] [{AutoStopProcess?.ProcessName}] {AutoStopProcess?.MainWindowTitle}"
        : $"进程 [{AutoStopPid}] 不存在";

    public Process? AutoStopProcess { get; private set; }

    public bool? AutoStopState { get; set; } = true;

    public string AutoStopStatesTooTip => AutoStopPid == -1 ? "PID 为 -1 时此选项不可用" : "约束指定 PID 进程的窗口活动状态";

    public string AutoStopToolTip => CanAutoStop ? "自动停止触发条件已动作" : "自动停止触发条件未动作";

    public bool CanAutoScroll { get; set; } = true;

    public bool CanAutoStart { get; private set; }

    public bool CanAutoStop { get; private set; }

    public bool CanClick { get; private set; } = true;

    public bool IsAutoStartEnabled { get; set; }

    public bool IsAutoStartPatternValid
    {
        get
        {
            try
            {
                _autoStartRegex = new Regex(AutoStartPattern);
                _autoStartPatternException = null;

                return true;
            }
            catch (Exception ex)
            {
                _autoStartPatternException = ex;

                return false;
            }
        }
    }

    public bool IsAutoStartProcessExists => AutoStartProcess is not null;

    public bool IsAutoStopEnabled { get; set; }

    public bool IsAutoStopPatternValid
    {
        get
        {
            try
            {
                _autoStopRegex = new Regex(AutoStopPattern);
                _autoStopPatternException = null;

                return true;
            }
            catch (Exception ex)
            {
                _autoStopPatternException = ex;

                return false;
            }
        }
    }

    public bool IsAutoStopProcessExists => AutoStopProcess is not null;

    public bool IsRunning { get; set; }

    public bool IsTopmost { get; set; } = true;

    public int LogPoll { get; set; }

    public double MinWindowHeight { get; private set; }

    public string ReleasedLongTime => ReleasedTime.ToString("yyyy/MM/dd dddd tt HH:mm:ss.fff");

    public DateTime ReleasedTime => File.GetLastWriteTime(GetType().Assembly.Location);

    public string StartStopIcon => IsRunning ? "⬛" : "▶️";

    public SolidColorBrush StartStopIconColor => IsRunning ? Brushes.DarkRed : Brushes.Green;

    public string StartStopString => IsRunning ? "停止" : "开始";

    public MainWindow(string activatePoll = "1",
        bool autoStart = false, bool? autoStartState = true, int autoStartPid = -1, bool autoStartMeet = true, string autoStartRegex = ".*", bool autoStartMatchCase = true,
        bool autoStop = false, bool? autoStopState = true, int autoStopPid = -1, bool autoStopMeet = true, string autoStopRegex = ".*", bool autoStopMatchCase = true)
    {
        InitializeComponent();
        MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        Title = WindowTitle;
        statusBar.ToolTip = $"Created by CodingNinja, Released on {ReleasedLongTime}";

        ActivatePollString = activatePoll;

        IsAutoStartEnabled = autoStart;
        AutoStartState = autoStartState;
        AutoStartPidString = autoStartPid.ToString();
        AutoStartMeet = autoStartMeet;
        AutoStartPattern = autoStartRegex;
        AutoStartPatternMatchCase = autoStartMatchCase;

        IsAutoStopEnabled = autoStop;
        AutoStopState = autoStopState;
        AutoStopPidString = autoStopPid.ToString();
        AutoStopMeet = autoStopMeet;
        AutoStopPattern = autoStopRegex;
        AutoStopPatternMatchCase = autoStopMatchCase;

        _checkActiveWindowTimer.Interval = TimeSpan.FromMilliseconds(1);
        _checkActiveWindowTimer.Tick += CheckActiveWindowTimer_Tick;

        _logTimer.Interval = TimeSpan.FromMilliseconds(1);
        _logTimer.Tick += LogTimer_Tick;

        _activateTimer.Interval = TimeSpan.FromMilliseconds(1);
        _activateTimer.Tick += ActivateTimer_Tick;

        ActiveWindowLogs.CollectionChanged += ActiveWindowLogs_CollectionChanged;
    }

    private static Process? GetActiveProcess()
    {
        try
        {
            IntPtr hWnd = GetForegroundWindow();
            uint threadID = GetWindowThreadProcessId(hWnd, out uint processID);
            var fgProc = Process.GetProcessById(Convert.ToInt32(processID));

            return fgProc;
        }
        catch
        {
            return null;
        }
    }

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetForegroundWindow();

    private static (Process? Process, string? WindowText) GetProcessAndWindowText()
    {
        return default;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    private static void OpenUri(string uri)
    {
        // HACK: 如果不转换为 AbsoluteUri，那么链接中的 “%20” 将以空格形式传入参数。
        string absoluteUri = new Uri(uri, UriKind.RelativeOrAbsolute).AbsoluteUri;

        // HACK: 如果不替换 “&”，那么 “&” 后面的内容将被截断。
        absoluteUri = absoluteUri.Replace("&", "^&");

        var psi = new ProcessStartInfo {
            FileName = "cmd",
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true,
            CreateNoWindow = true,
            Arguments = $"/c start {absoluteUri}"
        };

        Process.Start(psi);
    }

    private void ActivateTimer_Tick(object? sender, EventArgs e)
    {
        _activateTimer.Interval = TimeSpan.FromMilliseconds(ActivatePoll);

        this.GlobalActivate();
        rectScale.BeginAnimation(ScaleTransform.ScaleXProperty, _scaleX);
    }

    private void ActiveWindowLogs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        ScrollToEnd();
    }

    private void BtnAutoStartPatternCaseSensitive_Click(object sender, RoutedEventArgs e)
    {
        AutoStartPatternMatchCase = !AutoStartPatternMatchCase;
    }

    private void BtnAutoStopPatternCaseSensitive_Click(object sender, RoutedEventArgs e)
    {
        AutoStopPatternMatchCase = !AutoStopPatternMatchCase;
    }

    private void BtnDebug_Click(object sender, RoutedEventArgs e)
    {
        Debug(true);
    }

    private void CheckActiveWindowTimer_Tick(object? sender, EventArgs e)
    {
        var p = GetActiveProcess();

        if (p is null || ((p.Id == 0 || (p.ProcessName == "explorer" && String.IsNullOrEmpty(p.MainWindowTitle))) && !IsFocused))
        {
            CanAutoScroll = true;
            ScrollToEnd();
        }

        if (p is not null)
        {
            ActiveWindowLog = new ActiveWindowLog(DateTime.Now, p);
            CanAutoStart = CheckAutoStart();
            CanAutoStop = CheckAutoStop();
        }

        if (CanAutoStart && !IsRunning)
        {
            Debug(false);
        }

        if (CanAutoStop && IsRunning)
        {
            Debug(false);
        }
    }

    private bool CheckAutoStart()
    {
        if (AutoStartPid == -1)
        {
            AutoStartProcess = ActiveWindowLog!.Process;
        }
        else
        {
            try
            {
                AutoStartProcess = Process.GetProcessById(AutoStartPid);
            }
            catch
            {
                AutoStartProcess = null;

                return false;
            }
        }

        if (!IsAutoStartEnabled || !IsAutoStartPatternValid || ActiveWindowLog is null)
        {
            return false;
        }

        try
        {
            if (AutoStartState is true && AutoStartProcess.Id != ActiveWindowLog.PID)
            {
                return false;
            }

            if (AutoStartState is false && AutoStartProcess.Id == ActiveWindowLog.PID)
            {
                return false;
            }

            string autoStartProcessTitle = AutoStartProcess.MainWindowTitle;

            bool b = Regex.IsMatch(autoStartProcessTitle, AutoStartPattern, AutoStartPatternMatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
            b = AutoStartMeet ? b : !b;

            if (!b)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    private bool CheckAutoStop()
    {
        if (AutoStopPid == -1)
        {
            AutoStopProcess = ActiveWindowLog!.Process;
        }
        else
        {
            try
            {
                AutoStopProcess = Process.GetProcessById(AutoStopPid);
            }
            catch
            {
                AutoStopProcess = null;

                return false;
            }
        }

        if (!IsAutoStopEnabled || !IsAutoStopPatternValid || ActiveWindowLog is null)
        {
            return false;
        }

        try
        {
            if (AutoStopState is true && AutoStopProcess.Id != ActiveWindowLog.PID)
            {
                return false;
            }

            if (AutoStopState is false && AutoStopProcess.Id == ActiveWindowLog.PID)
            {
                return false;
            }

            string autoStopProcessTitle = AutoStopProcess.MainWindowTitle;

            bool b = Regex.IsMatch(autoStopProcessTitle, AutoStopPattern, AutoStopPatternMatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
            b = AutoStopMeet ? b : !b;

            if (!b)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    private void Debug(bool click)
    {
        CanClick = false;

        if (click)
        {
            IsAutoStartEnabled = false;
            IsAutoStopEnabled = false;
        }

        if (IsRunning)
        {
            _activateTimer.Stop();
            _activateTimer.Interval = TimeSpan.FromMilliseconds(0);
            rectScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        }
        else
        {
            _activateTimer.Start();
        }

        IsRunning = !IsRunning;
        CanClick = true;
    }

    private void LogPollItem_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioMenuItem rmi)
        {
            int poll = Convert.ToInt32(rmi.Tag);
            _logTimer.Interval = TimeSpan.FromMilliseconds(poll);
        }
    }

    private void LogTimer_Tick(object? sender, EventArgs e)
    {
        var recent = ActiveWindowLogs.LastOrDefault();

        if (ActiveWindowLog is not null && (recent?.PID != ActiveWindowLog.PID || recent?.Title != ActiveWindowLog.Title))
        {
            ActiveWindowLogs.Add(ActiveWindowLog);
        }
    }

    private void LvActiveLogs_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        CanAutoScroll = false;
    }

    private void LvActiveLogs_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
    {
        CanAutoScroll = false;
    }

    private void MiClearLogs_Click(object sender, RoutedEventArgs e)
    {
        ActiveWindowLogs.Clear();
    }

    private void MiOpenGithub_Click(object sender, RoutedEventArgs e)
    {
        OpenUri("https://github.com/CodingOctocat/ActiveWindowDebugger");
    }

    private void ReStart(int poll)
    {
        bool restart = IsRunning;

        Stop();

        _scaleX = new() {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(poll),
        };

        if (restart)
        {
            _activateTimer.Start();
            IsRunning = true;
        }
    }

    private void ScrollToEnd()
    {
        if (ActiveWindowLogs.Count > 0 && CanAutoScroll)
        {
            lvActiveLogs.ScrollIntoView(lvActiveLogs.Items[^1]);
        }
    }

    private void Stop()
    {
        _activateTimer.Stop();
        _activateTimer.Interval = TimeSpan.FromMilliseconds(0);
        rectScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        IsRunning = false;
    }

    private void TbActivatePoll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        int poll = ActivatePoll;
        int delta = e.Delta > 0 ? 1 : -1;

        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            poll = ActivatePoll + (delta * 10);
        }
        else if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            poll = ActivatePoll + (delta * 100);
        }
        else
        {
            poll = ActivatePoll + delta;
        }

        if (poll < 0)
        {
            poll = 0;
        }

        ActivatePollString = poll.ToString();
    }

    private void TbActiveWindowLog_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        MinHeight = 184 + tbActiveWindowLog.ActualHeight;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _checkActiveWindowTimer.Stop();
        _logTimer.Stop();
        _activateTimer.Stop();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Stop();
        IsAutoStartEnabled = false;
        IsAutoStopEnabled = false;

        var result = MessageBox.Show(
            messageBoxText: "确定退出?",
            "ActiveWindowDebugger",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question,
            MessageBoxResult.OK);

        e.Cancel = result != MessageBoxResult.OK;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        foreach (int poll in _polls)
        {
            var cboItem = new ComboBoxItem() {
                Content = $"{poll}ms",
                Tag = poll
            };

            cboActivateWindowPolls.Items.Add(cboItem);

            var logPollItem = new RadioMenuItem() {
                Header = $"{poll}ms",
                Tag = poll
            };

            logPollItem.Checked += LogPollItem_Checked;
            miLogPoll.Items.Add(logPollItem);
        }

        cboActivateWindowPolls.SelectedIndex = 1;

        if (cboActivateWindowPolls.Items[0] is ComboBoxItem cboi0)
        {
            cboi0.Content = $"{cboi0.Content} (高负载)";
            cboi0.ToolTip = "[0ms] 将消耗约 5%~20% CPU 利用率(以实际计算机性能为准)";
        }

        if (miLogPoll.Items[0] is RadioMenuItem rmi0)
        {
            rmi0.Header = $"{rmi0.Header} (高负载)";
            rmi0.ToolTip = "[0ms] 将消耗约 5%~20% CPU 利用率(以实际计算机性能为准)";
        }

        if (miLogPoll.Items[1] is RadioMenuItem rmi1)
        {
            rmi1.Header = $"{rmi1.Header} (推荐)";
            rmi1.IsChecked = true;
        }

        _checkActiveWindowTimer.Start();
        _logTimer.Start();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}
