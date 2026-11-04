using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;

namespace DeskDock;

// Shell nativo: permite usar el icono propio y NIIF_NOSOUND sin modificar ajustes de Windows.
internal sealed class TrayIcon : Forms.NativeWindow, IDisposable
{
    private const uint Callback = 0x8001;
    private readonly uint _taskbarCreated = RegisterWindowMessage("TaskbarCreated");
    private bool _visible, _disposed;
    public string Text { get; set; } = "DeskDock";
    public Icon? Icon { get; set; }
    public Forms.ContextMenuStrip? ContextMenuStrip { get; set; }
    public event EventHandler? DoubleClick;
    public event EventHandler? BalloonTipShown;
    public event EventHandler? BalloonTipClicked;
    public event EventHandler? BalloonTipClosed;
    internal const uint SilentCustomBalloon = 0x04 | 0x10 | 0x20 | 0x80;
    internal static int NativeDataSize => Marshal.SizeOf<NotifyIconData>();

    public TrayIcon() => CreateHandle(new Forms.CreateParams { Caption = "DeskDock.Tray" });

    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value || _disposed) return;
            if (value) AddIcon();
            else { var data = Data(); ShellNotifyIcon(2, ref data); }
            _visible = value;
        }
    }

    private NotifyIconData Data() => new()
    {
        Size = (uint)NativeDataSize, Window = Handle, Id = 1,
        Tip = Text, Info = "", InfoTitle = "", Icon = Icon?.Handle ?? IntPtr.Zero
    };

    private void AddIcon()
    {
        var data = Data();
        data.Flags = 1 | 2 | 4 | 0x80; // NIF_MESSAGE, NIF_ICON, NIF_TIP, NIF_SHOWTIP
        data.CallbackMessage = Callback;
        if (!ShellNotifyIcon(0, ref data)) throw new Win32Exception("No se pudo crear el icono de DeskDock en la bandeja.");
        data.TimeoutOrVersion = 4;
        ShellNotifyIcon(4, ref data); // NOTIFYICON_VERSION_4
    }

    public void ShowBalloonTip(int milliseconds, string title, string text, Forms.ToolTipIcon unused)
    {
        if (!_visible || _disposed) return;
        var data = Data();
        data.Flags = 0x10; // NIF_INFO
        data.InfoTitle = title.Length > 63 ? title[..63] : title;
        data.Info = text.Length > 255 ? text[..255] : text;
        data.TimeoutOrVersion = (uint)milliseconds;
        data.InfoFlags = SilentCustomBalloon;
        data.BalloonIcon = Icon?.Handle ?? IntPtr.Zero;
        if (!ShellNotifyIcon(1, ref data)) throw new Win32Exception("Windows no aceptó el aviso de DeskDock.");
    }

    protected override void WndProc(ref Forms.Message message)
    {
        if (message.Msg == _taskbarCreated && _visible)
        {
            try { AddIcon(); } catch (Exception ex) { Diagnostics.Record("TrayRestore", ex); }
        }
        else if (message.Msg == Callback)
        {
            uint action = (uint)(message.LParam.ToInt64() & 0xffff);
            switch (action)
            {
                case 0x203: // WM_LBUTTONDBLCLK
                case 0x401: // NIN_KEYSELECT
                    DoubleClick?.Invoke(this, EventArgs.Empty); break;
                case 0x205: // WM_RBUTTONUP (versiones antiguas)
                case 0x7b:  // WM_CONTEXTMENU
                    SetForegroundWindow(Handle);
                    ContextMenuStrip?.Show(Forms.Cursor.Position);
                    break;
                case 0x402: BalloonTipShown?.Invoke(this, EventArgs.Empty); break;
                case 0x405: BalloonTipClicked?.Invoke(this, EventArgs.Empty); break;
                case 0x403:
                case 0x404: BalloonTipClosed?.Invoke(this, EventArgs.Empty); break;
            }
        }
        base.WndProc(ref message);
    }

    public void Dispose()
    {
        if (_disposed) return;
        Visible = false;
        _disposed = true;
        DestroyHandle();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NotifyIconData
    {
        public uint Size;
        public IntPtr Window;
        public uint Id, Flags, CallbackMessage;
        public IntPtr Icon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Tip;
        public uint State, StateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Info;
        public uint TimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string InfoTitle;
        public uint InfoFlags;
        public Guid Guid;
        public IntPtr BalloonIcon;
    }

    [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIconW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShellNotifyIcon(uint operation, ref NotifyIconData data);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint RegisterWindowMessage(string text);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr window);
}
