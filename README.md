# DeskDock 0.4

Aplicación Windows x64 en C# / WPF / .NET 10 con WhatsApp Web mediante WebView2 y control de Spotify mediante las sesiones multimedia nativas de Windows.

## Cambios de la versión 0.4

- Skins **Aurora** (azul, violeta y cian), **Eclipse** (negro y ámbar) y **Minimal** (original), elegibles en «⋯» y guardados en las preferencias. Cambian marco, superficies, pestañas, menús y reproductor sin reiniciar ni recargar WhatsApp.
- Icono original de onda sonora: ICO multirresolución para el ejecutable, ventana, bandeja y avisos. Windows decide la presentación final del icono en los avisos y puede mantener en caché iconos de accesos anclados antiguos.
- Sonido sintetizado original **bwuooop**, de 0.56 segundos, incluido como WAV. Puede desactivarse en el menú. La opción se conserva al cerrar.
- Bandeja mediante `Shell_NotifyIconW`: avisos con `NIIF_NOSOUND` para evitar el tono estándar simultáneo. El sonido propio se solicita cuando Windows comunica `NIN_BALLOONSHOW`; si Windows suprime el aviso y no emite ese evento, no se fuerza un sonido.
- Los skins son dibujos y degradados WPF. No cambian el fondo de escritorio, no usan desenfoque real del escritorio y no alteran colores internos de WhatsApp Web. Conservan el reproductor de 54 unidades y la vista adaptable.

Las pruebas verifican carga de skins, actualización de recursos, migración de preferencias y presencia/formato del WAV e icono. El sonido audible y los avisos reales deben probarse en Windows con «Probar aviso».

## Cambios de la versión 0.3

Interfaz compacta: cabecera de 36 unidades, pestañas pequeñas, dos iconos para lista/conversación y reproductor inferior de 54 unidades. Las acciones secundarias, preferencias y estado se encuentran en el menú «⋯». El altavoz abre volumen y silencio. Reproducir y pausar ocupan el mismo espacio y alternan según el estado publicado por Spotify. Los iconos tienen descripción al pasar el ratón y nombre accesible. La lógica de WhatsApp adaptable y avisos se conserva.

## Cambios de la versión 0.2

- Barra de ventana propia con arrastre, minimizar, maximizar/restaurar, cerrar y bandeja; esquinas exteriores redondeadas mediante DWM en Windows 11.
- Pestaña Spotify con portada grande, título, artista y controles sincronizados con el mini reproductor. Abrir Spotify es ahora una acción secundaria.
- WhatsApp adaptable: en ventanas estrechas intenta mostrar la conversación a todo el ancho con texto de tamaño normal; «Chats» vuelve a la lista y «Chat» a la conversación. Es una adaptación experimental de los contenedores del sitio, no una interfaz oficial de WhatsApp. Si la estructura no se reconoce se usa zoom para ajustar la web al ancho.
- Avisos propios mediante la bandeja de Windows en lugar de depender de la UI de WebView2. «Probar aviso» permite comprobar la presentación local y «Activar avisos» restablece el permiso del sitio.
- Alternativa de aviso por incremento del contador del título de WhatsApp, con deduplicación de ocho segundos. No lee mensajes ni contactos. No equivale a detectar cada mensaje y puede no avisar si no cambia el número de chats sin leer.
- Prueba adicional con WebView2 real y HTML sintético para comprobar el ancho del chat, caja de escritura, lista, cambio de tamaño y restauración. La compatibilidad con la web real y la visibilidad de avisos siguen requiriendo prueba manual.

**Estado:** primera implementación. La compilación y la autoprueba se verifican mediante GitHub Actions. Ningún resultado de CI demuestra compatibilidad real con WhatsApp, una cuenta de Spotify, notificaciones ni dispositivos de audio. Consulta el resultado de la ejecución en Actions y completa las pruebas manuales.

## Descargar y ejecutar

Lee [INICIO-RAPIDO.md](INICIO-RAPIDO.md). La compilación se realiza en GitHub, sin Node.js, Git, Visual Studio ni SDKs en la computadora del usuario.

El artefacto `DeskDock-win-x64` contiene `DeskDock.exe`, .NET y las bibliotecas necesarias. Hay que extraer **todo** el ZIP y conservar los archivos juntos. No es un instalador ni un EXE independiente. No necesita instalar .NET. WebView2 Runtime es una dependencia de ejecución separada, no una herramienta de desarrollo.

## Funciones implementadas

- Ventana redimensionable, tema oscuro, opción siempre encima persistente.
- Pestañas WhatsApp y Spotify; mini reproductor visible en ambas.
- Perfil WebView2 persistente en `%LOCALAPPDATA%\DeskDock\WebView2`.
- Solicitud de permiso de notificaciones solo para `https://web.whatsapp.com`; presentación propia por bandeja. Compatibilidad real pendiente de prueba.
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
