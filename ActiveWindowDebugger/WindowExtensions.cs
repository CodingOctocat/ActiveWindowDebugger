using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ActiveWindowDebugger;

public static partial class WindowExtensions
{
    #region Constants

#pragma warning disable IDE1006 // 命名样式

    private const uint SWP_NOMOVE = 0x0002;

    private const uint SWP_NOSIZE = 0x0001;

    private const uint SWP_SHOWWINDOW = 0x0040;

#pragma warning restore IDE1006 // 命名样式

    #endregion Constants

    private enum ShowWindowCommands : int
    {
        SW_SHOWNOACTIVATE = 4
    }

    /// <summary>
    /// Activate a window from anywhere by attaching to the foreground window
    /// </summary>
    public static void GlobalActivate(this Window w)
    {
        //Get the process ID for this window's thread
        var interopHelper = new WindowInteropHelper(w);
        uint thisWindowThreadId = GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);

        //Get the process ID for the foreground window's thread
        nint currentForegroundWindow = GetForegroundWindow();
        uint currentForegroundWindowThreadId = GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);

        //Attach this window's thread to the current window's thread
        AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);

        //Set the window position
        SetWindowPos(interopHelper.Handle, new IntPtr(0), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);

        //Detach this window's thread from the current window's thread
        AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);

        //Show and activate the window
        if (w.WindowState == WindowState.Minimized)
        {
            w.WindowState = WindowState.Normal;
        }

        w.Show();
        w.Activate();
    }

    #region Imports

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AttachThreadInput(uint idAttach, uint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetForegroundWindow();

    [LibraryImport("user32.dll")]
    private static partial uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

    #endregion Imports
}
