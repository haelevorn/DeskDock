using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace DeskDock;

internal static class LayoutSelfTests
{
    // Documento sintético; no inicia sesión ni visita WhatsApp.
    private const string Fixture = """
    <!doctype html><html><head><meta charset="utf-8"><style>
    *{box-sizing:border-box}html,body{margin:0;width:100%;height:100%;overflow:hidden}
    #app{display:flex;min-width:1100px;width:1100px;height:100%}
    #left{width:320px;display:flex;flex:none}#side{width:320px;min-width:320px;display:flex;flex-direction:column}
    #right{display:flex;flex:1;min-width:700px}#main{display:flex;flex-direction:column;min-width:700px;width:100%;height:100%}
    header{height:50px;flex:none}.messages{flex:1;min-height:0;overflow:auto}footer{height:50px;flex:none}input{width:100%;min-width:0}
    </style></head><body><div id="app"><div id="left"><div id="side"><div role="row" id="row">Conversación de prueba</div></div></div>
    <div id="right"><div id="main"><header>Cabecera</header><div class="messages">Contenido de prueba</div><footer id="composer"><input aria-label="Escribir"></footer></div></div></div></body></html>
    """;

    public static async Task<int> RunAsync()
    {
        var results = new List<string>();
        var web = new WebView2();
        var window = new Window { Width = 460, Height = 480, Content = web, ShowInTaskbar = false, Opacity = 0, WindowStyle = WindowStyle.None };
        try
        {
            window.Show();
            var environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Path.GetTempPath(), "DeskDock-layout-" + Guid.NewGuid().ToString("N")));
            await web.EnsureCoreWebView2Async(environment);
            var core = web.CoreWebView2!;
            var loaded = new TaskCompletionSource<bool>();
            core.NavigationCompleted += (_, e) => loaded.TrySetResult(e.IsSuccess);
            core.NavigateToString(Fixture);
            Check(await loaded.Task.WaitAsync(TimeSpan.FromSeconds(20)), "Carga del documento sintético en WebView2", results);
            await core.ExecuteScriptAsync(WhatsAppLayout.InstallScript);
            Check(await Evaluate(core, "window.__deskDockLayout.configure(true, 'auto', 360).state === 'chat'"), "Modo chat estrecho", results);
            Check(await Evaluate(core, "document.getElementById('main').getBoundingClientRect().right <= innerWidth + 1 && document.getElementById('main').getBoundingClientRect().left >= -1"), "Chat completo dentro del ancho disponible", results);
            Check(await Evaluate(core, "document.getElementById('composer').getBoundingClientRect().bottom <= innerHeight + 1"), "Caja de escritura dentro del alto disponible", results);
            Check(await Evaluate(core, "window.__deskDockLayout.configure(true, 'list', 360).state === 'list' && getComputedStyle(document.getElementById('right')).display === 'none'"), "Volver a lista sin chat superpuesto", results);
            await core.ExecuteScriptAsync("document.getElementById('row').click()");
            Check(await Evaluate(core, "window.__deskDockLayout.update().state === 'chat'"), "Selección de conversación vuelve al chat", results);
            Check(await Evaluate(core, "window.__deskDockLayout.configure(true, '', 900).state === 'wide' && !document.documentElement.hasAttribute('data-dd-view')"), "Ampliar restaura dos columnas", results);
            Check(await Evaluate(core, "window.__deskDockLayout.configure(false, '', 360).state === 'off' && !document.querySelector('[data-dd-chat]')"), "Desactivar elimina los ajustes de contenedores", results);
            await core.ExecuteScriptAsync("document.getElementById('main').remove()");
            Check(await Evaluate(core, "window.__deskDockLayout.configure(true, '', 360).state === 'unsupported'"), "Estructura desconocida pide alternativa por zoom", results);
            results.Add("PASS: WebView2 real con HTML sintético. NO verifica la estructura actual de WhatsApp ni avisos reales de Windows.");
            return 0;
        }
        catch (Exception ex) { results.Add("FAIL: " + ex); return 1; }
        finally
        {
            File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "layout-test-results.txt"), results);
            web.Dispose();
            window.Close();
        }
    }

    private static async Task<bool> Evaluate(CoreWebView2 core, string expression) =>
        JsonSerializer.Deserialize<bool>(await core.ExecuteScriptAsync("(() => (" + expression + "))()"));

    private static void Check(bool condition, string description, List<string> results)
    {
        if (!condition) throw new InvalidOperationException(description);
        results.Add("PASS: " + description);
    }
}
