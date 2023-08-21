# ActiveWindowDebugger

中文 | [English](/README.en.md)

[![Target framework](https://img.shields.io/badge/support-.NET_7.0--Windows-blue)](https://github.com/CodingOctocat/ActiveWindowDebugger)
[![GitHub issues](https://img.shields.io/github/issues/CodingOctocat/ActiveWindowDebugger)](https://github.com/CodingOctocat/ActiveWindowDebugger/issues)
[![GitHub stars](https://img.shields.io/github/stars/CodingOctocat/ActiveWindowDebugger)](https://github.com/CodingOctocat/ActiveWindowDebugger/stargazers)
[![GitHub license](https://img.shields.io/github/license/CodingOctocat/ActiveWindowDebugger)](https://github.com/CodingOctocat/ActiveWindowDebugger/blob/master/LICENSE)
[![CodeFactor](https://www.codefactor.io/repository/github/codingoctocat/ActiveWindowDebugger/badge)](https://www.codefactor.io/repository/github/codingoctocat/ActiveWindowDebugger)

ActiveWindowDebugger：活动窗口调试器。

<a href="https://github.com/CodingOctocat/ActiveWindowDebugger">
    <img src="/ActiveWindowDebugger/awd_logo_1024x1024.png" alt="Logo" width="128">
</a>

---

#### 介绍

AWD 原本是我的某个 `Selenium` 项目的协同调试器。在进行 `Selenium` 开发时，有些元素需要浏览器窗口处于活动窗口状态才能定位成功，要找到潜在的此类元素，需要让浏览器窗口失去活动窗口进行元素定位，如果定位失败，使其重新获得活动状态再次定位元素，这对调试是不利的，AWD 提供了一种流畅的解决方案，AWD 可以使自身获取活动窗口状态，配合添加 `自动启动`、`自动停止` 触发条件，AWD 可以自动的在合适的时机夺取活动窗口状态。

<img src="/ActiveWindowDebugger/screenshot.png" alt="screenshot" width="628">

#### 软件架构
Powered by [C# 10](https://docs.microsoft.com/dotnet/csharp/)/[.NET 7](https://docs.microsoft.com/dotnet/)，Proudly Built by [WPF](https://docs.microsoft.com/dotnet/desktop/wpf/)。


#### 命令行支持

 **[基础]** 

- `--poll`: `<int>` 激活频率(ms), *默认值: 1*.
- `--topmost`: `<bool>` 置顶 AWD 窗口, *默认值: True*.
- `--single`: `<bool>` 单实例模式, *默认值: False*.


 **[自动开始条件]** 

- `--auto-start`: `<bool>` 启用/禁用自动开始, *默认值: False*.
- `--auto-start-state`: `<bool?>` True: 活动状态; False: 非活动状态; null: 任意状态, *默认值: True*.
- `--auto-start-pid`: `<int>` 进程 ID, -1 表示当前活动进程, *默认值: -1*.
- `--auto-start-meet`: `<bool>` 满足或不满足正则表达式条件, *默认值: True*.
- `--auto-start-regex`: `<string>` 正则表达式(匹配进程的主窗口标题), *默认值: ".*"*.
- `--auto-start-match-case`: `<bool>` 正则表达式是否区分大小写, *默认值: True*.


 **[自动停止条件]** 

- `--auto-stop`: `<bool>` 启用/禁用自动停止, *默认值: False*.
- `--auto-stop-state`: `<bool?>` True: 活动状态; False: 非活动状态; null: 任意状态, *默认值: True*.
- `--auto-stop-pid`: `<int>` 进程 ID, -1 表示当前活动进程, *默认值: -1*.
- `--auto-stop-meet`: `<bool>` 满足或不满足正则表达式, *默认值: True*.
- `--auto-stop-regex`: `<string>` 正则表达式(匹配进程的主窗口标题), *默认值: ".*"*.
- `--auto-stop-match-case`: `<bool>` 正则表达式是否区分大小写, *默认值: True*.
