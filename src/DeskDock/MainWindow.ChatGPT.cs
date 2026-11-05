using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace DeskDock;

public partial class MainWindow
{
    private WebView2? _chatWeb;
    private bool _chatStarting;

    private async void MainTabChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_testMode && ReferenceEquals(e.Source, MainTabs) && MainTabs.SelectedIndex == 4)
            await StartChatGptAsync();
    }

    private async Task StartChatGptAsync()
    {
        if (_closing || _chatStarting || _chatWeb is not null) return;
        _chatStarting = true;
        WebView2? view = null;
        try
        {
            ChatGptStatus.Text = "Iniciando ChatGPT web… Compatibilidad de inicio de sesión pendiente de comprobar.";
            view = new WebView2 { DefaultBackgroundColor = System.Drawing.Color.FromArgb(25, 34, 40) };
            _chatWeb = view;
            ChatGptHost.Children.Add(view);
            var environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Diagnostics.Root, "WebView2-ChatGPT"));
            if (_closing || view != _chatWeb) return;
            await view.EnsureCoreWebView2Async(environment);
            if (_closing || view != _chatWeb) return;
            var core = view.CoreWebView2!;
            core.Settings.AreHostObjectsAllowed = false;
            core.Settings.IsWebMessageEnabled = false;
            core.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Dark;
            core.PermissionRequested += (_, args) => args.State = CoreWebView2PermissionState.Deny;
            core.NavigationStarting += (_, args) =>
            {
                if (NavigationPolicy.IsChatGptNavigation(args.Uri)) return;
                args.Cancel = true;
                if (args.IsUserInitiated) OpenChatExternal(args.Uri);
                else ChatGptStatus.Text = "Se detuvo una redirección fuera de los dominios previstos. Usa Abrir navegador si el acceso no continúa.";
            };
            core.NewWindowRequested += (_, args) =>
            {
                args.Handled = true;
                if (NavigationPolicy.IsChatGptNavigation(args.Uri)) core.Navigate(args.Uri);
                else OpenChatExternal(args.Uri);
            };
            core.NavigationCompleted += (_, args) =>
            {
                if (_closing || view != _chatWeb) return;
                ChatGptStatus.Text = args.IsSuccess
                    ? "Página cargada; comprueba el acceso con tu cuenta. Si hay bloqueo o verificación que no avanza, usa Abrir navegador."
                    : $"No se pudo cargar ChatGPT ({args.WebErrorStatus}). Puedes usar Abrir navegador.";
            };
            core.ProcessFailed += (_, _) =>
            {
                if (!_closing) ChatGptStatus.Text = "Un proceso de ChatGPT web falló. Pulsa Cerrar web y vuelve a abrir esta pestaña.";
            };
            core.Navigate("https://chatgpt.com/");
        }
        catch (Exception ex)
        {
            Diagnostics.Record("ChatGPTWeb", ex);
            if (view == _chatWeb) CloseChatGpt();
            if (!_closing) ChatGptStatus.Text = "No se pudo iniciar ChatGPT web. Usa Abrir navegador.";
        }
        finally { _chatStarting = false; }
    }

    private void OpenChatExternal(string url)
    {
        if (!NavigationPolicy.IsExternalWeb(url)) return;
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Exception ex) { Diagnostics.Record("ChatGPTBrowser", ex); ChatGptStatus.Text = "No se pudo abrir el navegador predeterminado."; }
    }
    private void OpenChatGptBrowserClick(object sender, RoutedEventArgs e) => OpenChatExternal("https://chatgpt.com/");
    private async void ReloadChatGptClick(object sender, RoutedEventArgs e)
    {
        if (_chatWeb?.CoreWebView2 is not null) _chatWeb.Reload();
        else { CloseChatGpt(); await StartChatGptAsync(); }
    }
    private void CloseChatGptClick(object sender, RoutedEventArgs e)
    {
        CloseChatGpt();
        ChatGptStatus.Text = "Web cerrada para liberar recursos. Pulsa Recargar para abrirla de nuevo. La sesión guardada se conserva.";
    }
    private void CloseChatGpt()
    {
        _chatWeb?.Dispose();
        _chatWeb = null;
        ChatGptHost.Children.Clear();
    }
}
