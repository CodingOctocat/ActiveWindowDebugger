# ActiveWindowDebugger

[中文](/README.md) | English

> Translate by [DeepL](https://www.deepl.com/translator)

[![Target framework](https://img.shields.io/badge/support-.NET_7.0--Windows-blue)](https://github.com/CodingOctocat/ActiveWindowDebugger)
[![GitHub issues](https://img.shields.io/github/issues/CodingOctocat/ActiveWindowDebugger)](https://github.com/CodingOctocat/ActiveWindowDebugger/issues)
[![GitHub stars](https://img.shields.io/github/stars/CodingOctocat/ActiveWindowDebugger)](https://github.com/CodingOctocat/ActiveWindowDebugger/stargazers)
[![GitHub license](https://img.shields.io/github/license/CodingOctocat/ActiveWindowDebugger)](https://github.com/CodingOctocat/ActiveWindowDebugger/blob/master/LICENSE)
[![CodeFactor](https://www.codefactor.io/repository/github/codingoctocat/ActiveWindowDebugger/badge)](https://www.codefactor.io/repository/github/codingoctocat/ActiveWindowDebugger)

ActiveWindowDebugger: Active window debugger.

<a href="https://github.com/CodingOctocat/ActiveWindowDebugger">
    <img src="/ActiveWindowDebugger/awd_logo_1024x1024.png" alt="Logo" width="128">
</a>

---

#### Introduction

AWD was originally a collaborative debugger for one of my `Selenium` projects. During `Selenium` development, there are some elements that require the browser window to be in the active window state in order to locate them successfully. In order to find potential such elements, you need to make the browser window lose the active window to locate the element, and if it fails to locate the element, you need to make the browser window regain the active state to locate the element again, which is not good for debugging, so AWD provides a smooth solution. AWD can make itself get the active window state, with the addition of `autostart` and `autostop` trigger conditions, AWD can automatically seize the active window state at the right time.

<img src="/ActiveWindowDebugger/screenshot.png" alt="screenshot" width="628">

#### Software Architecture
Powered by [C# 10](https://docs.microsoft.com/dotnet/csharp/)/[.NET 7](https://docs.microsoft.com/dotnet/), Proudly Built by [WPF](https://docs.microsoft.com/dotnet/desktop/wpf/).


#### Command Line Support

 **[Base]** 

- `--poll`: `<int>` Activation frequency (ms), *default: 1*.
- `-topmost`: `<bool>` Top AWD window, *default: True*.
- `-single`: `<bool>` Single instance mode, *default: False*.


 **[Auto-start condition]**. 

- `--auto-start`: `<bool>` Enable/disable auto-start, *default: False*.
- `--auto-start-state`: `<bool?>` True: active; False: inactive; null: any state, *default: True*.
- `--auto-start-pid`: `<int>` Process ID, -1 means currently active, *default: -1*.
- `--auto-start-meet`: `<bool>` Regular expression condition satisfied or not, *default: True*.
- `--auto-start-regex`: `<string>` Regular expression (matches the process's main window title), *default: ".*"*.
- `--auto-start-match-case`: `<bool>` Whether the regular expression is case sensitive, *default: True*.


 **[Auto-stop-case]**. 

- `--auto-stop`: `<bool>` Enable/disable auto-stop, *default: False*.
- `--auto-stop-state`: `<bool?>` True: active; False: inactive; null: arbitrary, *default: True*.
- `--auto-stop-pid`: `<int>` Process ID, -1 means currently active, *default: -1*.
- `--auto-stop-meet`: `<bool>` Regular expression to be met or not met, *default: True*.
- `--auto-stop-regex`: `<string>` Regular expression (matches the process's main window title), *default: ".*"*.
- `--auto-stop-match-case`: `<bool>` Whether the regular expression is case sensitive, *default: True*.
