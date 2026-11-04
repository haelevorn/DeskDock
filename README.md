# DeskDock 0.1

Aplicación Windows x64 en C# / WPF / .NET 10 con WhatsApp Web mediante WebView2 y control de Spotify mediante las sesiones multimedia nativas de Windows.

**Estado:** primera implementación. La compilación y la autoprueba se verifican mediante GitHub Actions. Ningún resultado de CI demuestra compatibilidad real con WhatsApp, una cuenta de Spotify, notificaciones ni dispositivos de audio. Consulta el resultado de la ejecución en Actions y completa las pruebas manuales.

## Descargar y ejecutar

Lee [INICIO-RAPIDO.md](INICIO-RAPIDO.md). La compilación se realiza en GitHub, sin Node.js, Git, Visual Studio ni SDKs en la computadora del usuario.

El artefacto `DeskDock-win-x64` contiene `DeskDock.exe`, .NET y las bibliotecas necesarias. Hay que extraer **todo** el ZIP y conservar los archivos juntos. No es un instalador ni un EXE independiente. No necesita instalar .NET. WebView2 Runtime es una dependencia de ejecución separada, no una herramienta de desarrollo.

## Funciones implementadas

- Ventana redimensionable, tema oscuro, opción siempre encima persistente.
- Pestañas WhatsApp y Spotify; mini reproductor visible en ambas.
- Perfil WebView2 persistente en `%LOCALAPPDATA%\DeskDock\WebView2`.
- Solicitud de permiso de notificaciones solo para `https://web.whatsapp.com`; UI predeterminada de WebView2. Compatibilidad real pendiente de prueba.
- Búsqueda exclusiva de sesiones con ID de Spotify Win32 o Microsoft Store; sin fallback hacia otras aplicaciones.
- Título, artista y portada que Spotify exponga a Windows; anterior, reproducir, pausar y siguiente según capacidades anunciadas. Se comprueba el resultado booleano de cada comando.
- Volumen y silencio del dispositivo de salida predeterminado de Windows mediante Core Audio, usando NAudio como adaptador.
- Minimizar a bandeja, restaurar con doble clic o menú, salir con X o «Salir».
- Avisos cuando faltan runtime, sesión multimedia o dispositivo de audio.

## Límites del MVP

No usa Spotify Web API ni permite guardar canciones. No incluye llamadas de WhatsApp, permisos de cámara/micrófono, push con DeskDock cerrado, instalación automática de runtime ni firma digital. No modifica el user-agent para eludir restricciones de WhatsApp.

Lee [limitaciones y fuentes](docs/LIMITACIONES.md) y [pruebas manuales](docs/PRUEBAS-MANUALES.md).

## Compilación en la nube

El flujo [.github/workflows/build-windows.yml](.github/workflows/build-windows.yml) usa un servidor `windows-2022`, instala .NET allí, restaura dependencias, publica `win-x64 --self-contained true`, ejecuta `DeskDock.exe --self-test` y sube el artefacto solo si esas comprobaciones pasan.

`--self-test` carga el XAML real y verifica filtros de origen, rechazo de esquemas peligrosos, reconocimiento de IDs Spotify y serialización de opciones. No envía comandos multimedia, no modifica volumen, no abre una cuenta ni inicia WebView2. El artefacto incluye el informe y `build-info.json` con el commit exacto.

Fuentes en `src/DeskDock`. No hace falta una solución `.sln`: Actions compila directamente el `.csproj`.

## Privacidad local

DeskDock no implementa servidor, telemetría propia ni almacenamiento de contraseñas. WhatsApp y Spotify mantienen sus propios servicios y políticas. El perfil local contiene datos sensibles de la sesión WhatsApp: no lo subas a GitHub ni lo compartas. El registro de diagnóstico guarda componente, tipo de excepción y código de error, sin mensajes ni títulos de canciones.

Para cerrar tu sesión usa el menú de WhatsApp o «Dispositivos vinculados» en el teléfono. Borrar el programa no elimina el perfil local.
