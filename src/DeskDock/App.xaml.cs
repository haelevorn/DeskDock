using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace DeskDock;

public partial class App : Application
{
    private Mutex? _instance;
    private bool _ownsMutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (Array.Exists(e.Args, arg => arg == "--layout-test"))
        {
            Shutdown(await LayoutSelfTests.RunAsync());
            return;
        }
        if (Array.Exists(e.Args, arg => arg == "--self-test"))
        {
            Shutdown(SelfTests.Run());
            return;
        }
        _instance = new Mutex(false, @"Local\DeskDock-" + Environment.UserName);
        try { _ownsMutex = _instance.WaitOne(0); }
        catch (AbandonedMutexException) { _ownsMutex = true; }
        if (!_ownsMutex)
        {
            MessageBox.Show("DeskDock ya está abierto. Busca su icono en la bandeja, junto al reloj.", "DeskDock");
            Shutdown();
            return;
        }
        DispatcherUnhandledException += (_, args) =>
        {
            Diagnostics.Record("UI", args.Exception);
            MessageBox.Show("DeskDock debe cerrarse por un error. Consulta diagnostics.log en %LOCALAPPDATA%\\DeskDock.", "DeskDock");
            args.Handled = true;
            Shutdown(1);
        };
        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_ownsMutex) _instance?.ReleaseMutex();
        _instance?.Dispose();
        base.OnExit(e);
    }
}

internal static class Diagnostics
{
    public static string Root => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeskDock");

    // No se escriben mensajes, contactos, títulos de canciones ni URLs de navegación.
    public static void Record(string component, Exception exception)
    {
        try
        {
            Directory.CreateDirectory(Root);
            string path = Path.Combine(Root, "diagnostics.log");
            if (File.Exists(path) && new FileInfo(path).Length > 512_000) File.Delete(path);
            File.AppendAllText(path, $"{DateTimeOffset.Now:O} {component}: {exception.GetType().Name} 0x{exception.HResult:X8}{Environment.NewLine}");
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
