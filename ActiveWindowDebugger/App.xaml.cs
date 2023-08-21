using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace ActiveWindowDebugger;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string _usage = """
        [基础]
        --poll: <int> 激活频率(ms), 默认值: 1.
        --topmost: <bool> 置顶 AWD 窗口, 默认值: True.
        --single: <bool> 单实例模式, 默认值: False.

        [自动开始条件]
        --auto-start: <bool> 启用/禁用自动开始, 默认值: False.
        --auto-start-state: <bool?> True: 活动状态; False: 非活动状态; null: 任意状态, 默认值: True.
        --auto-start-pid: <int> 进程 ID, -1 表示当前活动进程, 默认值: -1.
        --auto-start-meet: <bool> 满足或不满足正则表达式条件, 默认值: True.
        --auto-start-regex: <string> 正则表达式(匹配进程的主窗口标题), 默认值: ".*".
        --auto-start-match-case: <bool> 正则表达式是否区分大小写, 默认值: True.

        [自动停止条件]
        --auto-stop: <bool> 启用/禁用自动停止, 默认值: False.
        --auto-stop-state: <bool?> True: 活动状态; False: 非活动状态; null: 任意状态, 默认值: True.
        --auto-stop-pid: <int> 进程 ID, -1 表示当前活动进程, 默认值: -1.
        --auto-stop-meet: <bool> 满足或不满足正则表达式, 默认值: True.
        --auto-stop-regex: <string> 正则表达式(匹配进程的主窗口标题), 默认值: ".*".
        --auto-stop-match-case: <bool> 正则表达式是否区分大小写, 默认值: True.
        """;

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            var arguments = new Dictionary<string, string>();

            string prettyArgs = String.Join(' ', e.Args);

            if (e.Args.Length % 2 != 0)
            {
                throw new ArgumentException($"参数缺失或过多: {prettyArgs}");
            }

            for (int index = 0; index < e.Args.Length - 1; index += 2)
            {
                if (!e.Args[index].StartsWith("--"))
                {
                    throw new ArgumentException($"参数 '{e.Args[index]}' 必须使用 '--' 开头。");
                }

                arguments.Add(e.Args[index].TrimStart('-'), value: e.Args[index + 1]);
            }

            bool single = false;

            if (arguments.TryGetValue("single", out string? value))
            {
                single = Convert.ToBoolean(value);
            }

            string activatePoll = "1";
            bool topmost = true;

            bool autoStart = false;
            bool? autoStartState = true;
            int autoStartPid = -1;
            bool autoStartMeet = true;
            string autoStartRegex = ".*";
            bool autoStartMatchCase = true;

            bool autoStop = false;
            bool? autoStopState = true;
            int autoStopPid = -1;
            bool autoStopMeet = true;
            string autoStopRegex = ".*";
            bool autoStopMatchCase = true;

            if (arguments.TryGetValue("poll", out value))
            {
                activatePoll = value;
            }

            if (arguments.TryGetValue("topmost", out value))
            {
                topmost = Convert.ToBoolean(value);
            }

            if (arguments.TryGetValue("auto-start", out value))
            {
                autoStart = Convert.ToBoolean(value);
            }

            if (arguments.TryGetValue("auto-start-state", out value))
            {
                if (value.ToLower() == "null")
                {
                    autoStartState = null;
                }
                else
                {
                    autoStartState = Convert.ToBoolean(value);
                }
            }

            if (arguments.TryGetValue("auto-start-pid", out value))
            {
                autoStartPid = Convert.ToInt32(value);
            }

            if (arguments.TryGetValue("auto-start-meet", out value))
            {
                autoStartMeet = Convert.ToBoolean(value);
            }

            if (arguments.TryGetValue("auto-start-regex", out value))
            {
                autoStartRegex = value;
            }

            if (arguments.TryGetValue("auto-start-match-case", out value))
            {
                autoStartMatchCase = Convert.ToBoolean(value);
            }

            if (arguments.TryGetValue("auto-stop", out value))
            {
                autoStop = Convert.ToBoolean(value);
            }

            if (arguments.TryGetValue("auto-stop-state", out value))
            {
                if (value.ToLower() == "null")
                {
                    autoStopState = null;
                }
                else
                {
                    autoStopState = Convert.ToBoolean(value);
                }
            }

            if (arguments.TryGetValue("auto-stop-pid", out value))
            {
                autoStopPid = Convert.ToInt32(value);
            }

            if (arguments.TryGetValue("auto-stop-meet", out value))
            {
                autoStopMeet = Convert.ToBoolean(value);
            }

            if (arguments.TryGetValue("auto-stop-regex", out value))
            {
                autoStopRegex = value;
            }

            if (arguments.TryGetValue("auto-stop-match-case", out value))
            {
                autoStopMatchCase = Convert.ToBoolean(value);
            }

            if (single)
            {
                CloseOtherInstances();
            }

            var mainWindow = new MainWindow(activatePoll,
                autoStart, autoStartState, autoStartPid, autoStartMeet, autoStartRegex, autoStartMatchCase,
                autoStop, autoStopState, autoStopPid, autoStopMeet, autoStopRegex, autoStopMatchCase) {
                IsTopmost = topmost
            };

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"{ex.Message}\n\n用法:\n{_usage}",
                "ActiveWindowDebugger: 命令行参数错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK);

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

    private static void CloseOtherInstances()
    {
        var curr = Process.GetCurrentProcess();
        var instances = Process.GetProcessesByName(curr.ProcessName)
            .Where(x => {
                try
                {
                    return x?.MainModule?.FileName == curr.MainModule?.FileName;
                }
                catch
                {
                    return false;
                }
            });

        bool exists = instances.Count() > 1;

        if (exists)
        {
            foreach (var p in instances)
            {
                try
                {
                    if (p.Id != curr.Id)
                    {
                        p.Kill();
                    }
                }
                catch
                { }
            }
        }
    }
}
