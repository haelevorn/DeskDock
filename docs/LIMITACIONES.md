# Compatibilidad, límites y evidencia

## Actualización 0.6

El indicador de Pomodoro representa únicamente un temporizador en ejecución: se oculta estando pausado, reiniciado o esperando inicio manual. Los perfiles conservan duraciones y opciones, no sesiones en curso. Guardar no inicia ni cambia el temporizador; cargar reinicia. Todo se conserva en el archivo local de preferencias.

Glass utiliza `DWMWA_SYSTEMBACKDROP_TYPE = DWMSBT_TRANSIENTWINDOW` (Desktop Acrylic) y extensión de marco DWM en Windows 11 build 22621 o posterior. Las superficies de DeskDock se tintan con alfa; no se usa transparencia por píxel ni se sustituye el WebView2 existente. WhatsApp permanece opaco. Las políticas, efectos de transparencia, composición, acceso remoto y energía pueden limitar el resultado. La autoprueba de Windows Server solo comprueba el comportamiento de alternativa opaca; no demuestra transparencia visual en el equipo del usuario. Fuentes: [materiales DWM](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type), [extensión del marco](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmextendframeintoclientarea).

Referencia carga un archivo elegido por el usuario y conserva únicamente su ruta en preferencias. El contenido se decodifica y queda en memoria, liberando el archivo original. No se incorpora al repositorio, no se copia al paquete y no se sincroniza entre equipos. Límites: 50 MB/100 megapíxeles, lado mayor decodificado hasta 4096; PNG/JPG/BMP/GIF/TIFF, primer fotograma. No admite PSD/HEIC/WebP, documentos multipágina completos, dibujos ni edición. Zoom y desplazamiento son de visualización; al volver a abrir se ajusta de nuevo al panel.

## Actualización 0.5: tamaño, reloj y Pomodoro

El tamaño mínimo es 300 × 340 unidades WPF, no píxeles físicos invariables: escala de Windows y DPI influyen. La interfaz nativa ajusta portada, pestañas y mini reproductor; la configuración de Pomodoro permite desplazamiento vertical. El ajuste de WhatsApp sigue dependiendo de su estructura web. El tamaño se conserva al cerrar; no se conserva posición de ventana.

El reloj utiliza `DateTime.Now` y por tanto la hora/zona local configurada en Windows. No consulta un servidor de hora. «Siempre encima» afecta ventanas normales, no garantiza verse sobre aplicaciones exclusivas o pantallas del sistema. Al minimizar se oculta con el resto de DeskDock.

Pomodoro usa tiempo monotónico y un temporizador de interfaz separado de las consultas WebView2/Spotify. Las comprobaciones de vencimiento se realizan mientras DeskDock está abierto y Windows le permite ejecutar código; no es un servicio de alarma ni despierta el equipo. Tras una suspensión se notifica un vencimiento observado y se prepara/inicia la siguiente etapa desde la reanudación, sin avanzar repetidamente por sesiones que el usuario no realizó. La configuración se conserva, pero la sesión en curso se reinicia al cerrar. Windows puede suprimir avisos o sonido; el estado de etapa también se presenta dentro de la pestaña. No se afirma haber probado estos avisos reales en el equipo del usuario.

## Actualización 0.4: skins, sonido e icono

Skins WPF con recursos dinámicos y preferencias locales. La apariencia está inspirada en las paletas azul/violeta/cian y negro/ámbar proporcionadas por el usuario, sin copiar los mensajes, contactos, portada o elementos de fondo de las imágenes. No implementa transparencia de escritorio/acrílico ni recolorea la interfaz interna de WhatsApp. El skin no modifica la sesión web ni el estado de reproducción.

El icono de marca y el WAV son recursos originales generados para DeskDock. La bandeja se implementa directamente con `Shell_NotifyIconW`, conservando mostrar, salir, doble clic y ciclo de vida de los avisos. Se usa icono personalizado `hBalloonIcon` con `NIIF_USER`, `NIIF_LARGE_ICON`, `NIIF_NOSOUND` y `NIIF_RESPECT_QUIET_TIME`. El soporte y la representación final del aviso/icono dependen de Windows. El tono propio se reproduce con `SoundPlayer` al recibir `NIN_BALLOONSHOW`; un aviso suprimido sin evento de presentación no activa el audio. La prueba automática verifica formato de recursos y estructura/flags del paquete nativo; no demuestra que una persona vea u oiga un aviso. Se necesita «Probar aviso» en el equipo del usuario.

Fuentes: [NOTIFYICONDATAW y flags de avisos](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-notifyicondataw), [recursos dinámicos WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/systems/xaml-resources-overview).

## Actualización 0.2

El usuario informó que WhatsApp carga pero queda recortado en ventanas pequeñas, que la información de Spotify sí aparece en el mini reproductor y que no recibe notificaciones. Esto es evidencia de esos comportamientos reportados, no una validación completa de las integraciones.

Se añade una vista adaptable que identifica `#main` y `#side`/`#pane-side`, ajusta sus contenedores y muestra solo una columna por debajo de 720 unidades disponibles. No usa clases ofuscadas ni lee el texto de conversaciones. Esta adaptación depende de IDs y estructura privados del sitio y puede romperse. Es reversible desde «Vista adaptable»; cuando no reconoce los paneles, usa zoom. No garantiza que todos los paneles auxiliares, diálogos, llamadas, adjuntos o futuros diseños encajen. La prueba automática usa un documento sintético, no la cuenta del usuario.

La presentación de notificaciones ya no usa la UI predeterminada de WebView2. DeskDock maneja el evento y solicita un aviso nativo de `NotifyIcon`, notificando al objeto WebView2 cuando Windows comunica presentación, clic o cierre. El texto del aviso es genérico, sin remitente ni mensaje. `Probar aviso` usa la misma presentación sin necesitar mensajes entrantes. Windows puede suprimir avisos por sus preferencias, No molestar o políticas.

Como alternativa, se observa el contador del título del documento: si aumenta mientras WhatsApp no está en primer plano, se solicita un aviso genérico. No se anuncian los no leídos iniciales; los eventos próximos se agrupan durante ocho segundos. El título no es una API oficial, puede cambiar y suele reflejar chats sin leer, no cada mensaje. No se garantiza detectar mensajes adicionales de un chat que ya estaba sin leer. El permiso del sitio y las preferencias internas de WhatsApp influyen en los eventos web; la alternativa por título no depende de que el sitio cree una notificación.

La pestaña Spotify enlaza su portada, título, artista y capacidades con el mini reproductor. Ambas vistas usan la misma sesión; no se añade Spotify Web API. La barra propia conserva la interacción de ventana con `WindowChrome`; DWM redondea las esquinas exteriores en Windows 11. No se usa transparencia por píxel para conservar compatibilidad con WebView2.

Las secciones siguientes describen la base del MVP; para notificaciones y adaptación visual prevalece esta actualización.

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

Microsoft describe `NotificationReceived` para notificaciones **no persistentes**. DeskDock 0.2 usa `Handled=true` y presenta avisos propios por bandeja. El contador indica que se recibió el evento, no que el usuario haya visto un aviso. Permisos, configuración del sitio, «No molestar», visibilidad, versión del runtime y comportamiento de WhatsApp influyen en el resultado.

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
