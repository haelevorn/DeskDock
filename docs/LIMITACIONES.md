# Compatibilidad, límites y evidencia

Revisión documental: 4 de noviembre de 2026. Consultar los resultados concretos de Actions para saber si una versión compiló. El código implementado y una revisión documental no equivalen a una integración probada.

| Área | Implementación | Qué debe probarse en el equipo del usuario |
| --- | --- | --- |
| Ventana | WPF, tamaño mínimo, Topmost, preferencias | Movimiento, escalado DPI, tamaño y superposición |
| WhatsApp | WebView2 con perfil local permanente | Compatibilidad del sitio, QR, mensajes y sesión tras reiniciar |
| Notificaciones | Permiso por origen y UI predeterminada del runtime | Que WhatsApp emita una notificación y Windows la presente en cada estado |
| Spotify | Windows.Media.Control; solo IDs reconocidos | Instalación Win32/Store, metadatos y aceptación/efecto de cada comando |
| Portada | Thumbnail que publica la sesión, hasta 8 MB | Que Spotify la publique y cambie al cambiar canción |
| Volumen | Core Audio vía NAudio; salida predeterminada multimedia | Lectura, cambio, silencio y cambio de dispositivo |
| Distribución | .NET self-contained, carpeta portable x64 | Arranque del artefacto en un equipo sin SDK ni runtime .NET instalado |

## WhatsApp Web y WebView2

La documentación de WhatsApp sobre navegadores no establece aquí una garantía para este contenedor WebView2. No se afirma que una cuenta se haya vinculado ni que el sitio funcione solo porque termine una navegación. El sitio puede cambiar, rechazar el navegador embebido o revocar una sesión.

WebView2 guarda cookies, permisos y almacenamiento en su carpeta de datos. DeskDock usa `%LOCALAPPDATA%\DeskDock\WebView2`, fuera del directorio de descarga; cambiar el EXE no borra el perfil. Esto permite intentar persistencia, pero no impide que WhatsApp cierre sesión. No se importa la sesión de Edge/Chrome.

Microsoft describe `NotificationReceived` para notificaciones **no persistentes**. DeskDock mantiene `Handled=false` y deja que el runtime presente su UI predeterminada. El contador indica que se recibió el evento, no que Windows haya mostrado el aviso. Permisos, configuración del sitio, «No molestar», visibilidad, versión del runtime y comportamiento de WhatsApp influyen en el resultado.

No se implementa push con la aplicación cerrada ni se garantiza recepción en segundo plano. Se mantiene WebView2 vivo mientras DeskDock está en bandeja; esto no demuestra que WhatsApp siga notificando. No hay bridge de JavaScript para leer chats ni scraping de elementos privados. Cámara y micrófono se deniegan en este MVP: llamadas y mensajes de voz grabados no forman parte de la validación. Descargas y adjuntos dependen de las funciones predeterminadas de WebView2 y no están certificados.

Si falla, el botón para abrir el navegador ofrece una alternativa. No se falsifica el user-agent ni se intenta eludir restricciones.

Fuentes: [WhatsApp: navegadores compatibles](https://faq.whatsapp.com/803865374003522/), [carpetas de datos WebView2](https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/user-data-folder), [NotificationReceived](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.notificationreceived), [Handled y UI predeterminada](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2notificationreceivedeventargs.handled).

## Spotify y sesiones multimedia

`GlobalSystemMediaTransportControlsSessionManager` enumera sesiones que otras aplicaciones publican en Windows. No descubre música por sí mismo ni proporciona una API privada de Spotify. La sesión puede no existir o desaparecer al cerrar Spotify.

La documentación de Windows menciona la capacidad `globalMediaControl`; este proyecto es una aplicación de escritorio WPF de confianza completa, sin paquete MSIX. Un manifiesto Win32 no declara capacidades UWP. El acceso real mediante `RequestAsync` debe probarse en Windows; se informa del fallo si el sistema o sus políticas lo deniegan. Un futuro empaquetado MSIX requerirá revisar sus capacidades.

Se reconocen `Spotify.exe` y el prefijo de aplicación Store `SpotifyAB.SpotifyMusic_`. Estos identificadores son una regla del MVP que debe comprobarse con las instalaciones reales; pueden cambiar. Una variante no reconocida quedará sin controles. No se usa `GetCurrentSession()` como sustituto: así se evita controlar accidentalmente Chrome, Edge, un vídeo o un audio de WhatsApp.

Cada botón se habilita según las capacidades publicadas. Los métodos `TryPlayAsync`, `TryPauseAsync` y `TrySkip…Async` devuelven un resultado que se comprueba. Que devuelvan `true` no demuestra que se haya oído la reproducción: se necesita confirmar el efecto en Spotify. En anuncios, ciertas cuentas, pistas o estados, las acciones pueden ser limitadas. «Anterior» puede volver al comienzo de la canción según Spotify.

Los metadatos y la portada pueden faltar o estar retrasados. La consulta se hace cada dos segundos, también al estar en bandeja; no se garantiza actualización instantánea. Solo se intenta abrir la aplicación mediante el esquema `spotify:`; si Windows no lo tiene registrado, el usuario debe abrir Spotify desde Inicio.

Fuentes: [sesión multimedia global](https://learn.microsoft.com/en-us/uwp/api/windows.media.control.globalsystemmediatransportcontrolssession), [RequestAsync y requisitos](https://learn.microsoft.com/en-us/uwp/api/windows.media.control.globalsystemmediatransportcontrolssessionmanager.requestasync), [ejemplo de Microsoft para aplicaciones de escritorio](https://devblogs.microsoft.com/oldnewthing/20231108-00/?p=108980).

## Volumen

Se usa `IAudioEndpointVolume` a través de NAudio. Se controla el dispositivo predeterminado con rol multimedia, no todas las salidas ni una sesión individual. Aplicaciones en modo exclusivo o con salida seleccionada manualmente pueden comportarse de otra manera. Se vuelve a consultar el dispositivo en cada operación. Sin dispositivo o ante un error se apagan los controles y se reintenta.

Fuentes: [IAudioEndpointVolume](https://learn.microsoft.com/en-us/windows/win32/api/endpointvolume/nn-endpointvolume-iaudioendpointvolume), [enumeración NAudio](https://github.com/naudio/NAudio/blob/main/Docs/EnumerateOutputDevices.md).

## Ejecutable y nube

Self-contained incluye .NET, pero no WebView2 Runtime. El artefacto es una carpeta con un ejecutable y sus bibliotecas; mover únicamente el EXE romperá la aplicación. No se usa trimming, AOT ni un empaquetado de un solo archivo en esta versión para conservar la compatibilidad de WPF/WinRT/WebView2.

GitHub Actions compila y ejecuta comprobaciones sin instalar SDKs en el equipo del usuario. Su Windows Server no sustituye una prueba interactiva en Windows 11 con cuentas reales y dispositivos de audio. No se transfieren credenciales WhatsApp/Spotify al CI. Los binarios no están firmados y no se configura actualización automática de DeskDock.

Fuentes: [distribución WebView2 Runtime](https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution), [publicación .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/), [artefactos GitHub Actions](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/download-workflow-artifacts).
