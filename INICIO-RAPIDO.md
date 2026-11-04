# Usar DeskDock sin instalar herramientas de programación

## Actualizar a la versión 0.2

1. Cierra la versión anterior con la X o desde la bandeja → Salir.
2. Descarga el artefacto de la ejecución más reciente que haya terminado en verde, no el ZIP anterior.
3. Extrae todos los archivos en una carpeta nueva y abre su `DeskDock.exe`. La cabecera debe mostrar **0.2**. El perfil WhatsApp sigue en la misma carpeta local, por lo que no se borra al actualizar.
4. En WhatsApp deja activada **Vista adaptable**. En una ventana estrecha, abre una conversación; **Chats** muestra la lista y **Chat** vuelve a la conversación. Si seleccionar la conversación no cambia la vista automáticamente, pulsa **Chat**. A partir de 720 unidades de ancho disponible vuelve a la vista amplia. En QR/carga o estructuras no reconocidas se usa reducción por zoom.
5. Pulsa **Probar aviso**. Debería aparecer un aviso de DeskDock junto al reloj. Si no aparece, revisa Configuración de Windows → Sistema → Notificaciones, la configuración de notificaciones de DeskDock si figura, y **No molestar**. La aplicación no modifica esas opciones automáticamente.
6. Pulsa **Activar avisos** y activa también notificaciones dentro de los ajustes de WhatsApp. Recibe un mensaje con otra ventana activa y repite con DeskDock en bandeja. La pestaña Spotify indica si llegaron eventos web o si se solicitó mostrar un aviso. El aviso de prueba no comprueba la recepción desde WhatsApp.
7. Los avisos alternativos por contador solo se generan si aumenta el número del título; mensajes sucesivos de un chat ya no leído pueden no generar un nuevo aviso. No hay avisos con DeskDock cerrado.
8. La pestaña Spotify ahora muestra portada, canción, artista y controles; **Abrir aplicación Spotify** sigue abriendo la aplicación instalada solo cuando lo pulsas.

La barra superior es propia de DeskDock. Arrastra la zona del nombre para mover, haz doble clic para maximizar/restaurar y arrastra los bordes para cambiar de tamaño. Las esquinas exteriores redondeadas requieren Windows 11; Windows 10 puede conservar el contorno cuadrado.

No necesitas escribir comandos, instalar Git, Node.js, Visual Studio ni SDKs. El proyecto se compila en una máquina Windows de GitHub Actions.

## 1. Repositorio

El repositorio de este proyecto es https://github.com/haelevorn/DeskDock. Inicia sesión con tu cuenta para ver un repositorio privado. Los archivos de la aplicación deben estar en la raíz: `src`, `docs`, `README.md`, `INICIO-RAPIDO.md` y `.github/workflows/build-windows.yml`.

Si Codex ya ha subido esos archivos, pasa directamente al paso 2. Para una carga manual de respaldo:

1. Extrae `DeskDock-fuentes.zip` con clic derecho → «Extraer todo».
2. Abre el repositorio y pulsa «Add file» → «Upload files».
3. Arrastra el contenido de la carpeta extraída, no la carpeta contenedora ni el ZIP. Confirma que existe `src/DeskDock/DeskDock.csproj`.
4. Pulsa «Commit changes» para guardar en `main`.
5. Si la carpeta `.github` no se subió, pulsa «Add file» → «Create new file». En el nombre escribe `.github/workflows/build-windows.yml`. Abre el archivo del mismo nombre de la carpeta extraída con Bloc de notas, copia su contenido completo y pégalo en GitHub. Guarda con «Commit changes».

## 2. Obtener la compilación

1. Abre https://github.com/haelevorn/DeskDock/actions.
2. Si GitHub pide habilitar Actions, pulsa el botón para permitir los workflows. Si indica restricciones de facturación o minutos disponibles, revisa ese aviso antes de continuar; no es necesario comprar herramientas de desarrollo.
3. Busca el flujo **Build DeskDock Windows x64** a la izquierda.
4. Al guardar cambios en `main` se inicia automáticamente. Si no hay una ejecución, pulsa **Run workflow**, selecciona `main` y confirma **Run workflow**.
5. Entra en la ejecución más reciente y espera la marca verde. El círculo amarillo significa que sigue trabajando.
6. Si aparece una cruz roja, abre el paso rojo y comparte su mensaje con Codex. No descargues una compilación anterior suponiendo que contiene los cambios nuevos.
7. En la parte inferior de la página de la ejecución verde, encuentra **Artifacts** y pulsa **DeskDock-win-x64**. Descargará un ZIP. Debes estar conectado a GitHub. El artefacto se conserva 14 días; después puedes volver a ejecutar el flujo.

## 3. Ejecutar en Windows

1. Usa Windows 11 x64 actualizado. El código apunta a APIs disponibles desde Windows 10 2004; no se ha certificado cada versión antigua de Windows ni equipos ARM64.
2. En Descargas, clic derecho en el ZIP → **Extraer todo**. Elige una carpeta que quieras conservar, por ejemplo `Documentos\DeskDock`.
3. Abre la carpeta extraída y haz doble clic en **DeskDock.exe**. No lo ejecutes dentro del ZIP. Conserva junto al EXE todas las DLL y carpetas.
4. Esta primera versión no tiene firma digital; Windows puede mostrar SmartScreen. Comprueba que el ZIP procede de tu repositorio y de la ejecución que acabas de revisar. No desactives la protección de Windows para usarlo. En un equipo administrado puede ser necesaria una firma o permiso del administrador.
5. No necesitas instalar .NET: va incluido en la carpeta.

### Si falta WebView2

El panel de WhatsApp te lo indicará. Windows 11 normalmente ya incluye el runtime. Si falta:

1. Visita https://developer.microsoft.com/en-us/microsoft-edge/webview2/.
2. En **Download the WebView2 Runtime**, elige **Evergreen Standalone Installer → x64** (o el bootstrapper Evergreen).
3. Descarga el instalador oficial de Microsoft y ejecútalo. Esto instala un componente para ejecutar aplicaciones, no un SDK.
4. Cierra y vuelve a abrir DeskDock. Tener Edge instalado no sustituye necesariamente a WebView2 Runtime.

## 4. WhatsApp

1. Abre la pestaña WhatsApp. Si aparece el QR, abre WhatsApp en tu teléfono → **Dispositivos vinculados** → **Vincular un dispositivo** y escanéalo. Los nombres pueden variar según tu teléfono.
2. Comprueba que ves tus chats. «Página cargada» solo confirma navegación, no un inicio de sesión correcto.
3. Cierra DeskDock con la X y ábrelo de nuevo. Comprueba si conserva la sesión. WhatsApp puede pedir volver a vincularla.
4. Si WhatsApp ofrece activar notificaciones, actívalas y responde «Sí» a la pregunta de DeskDock.
5. Pide que te envíen un mensaje mientras otra ventana está activa. Repite desde la pestaña Spotify y con DeskDock en la bandeja. Registra qué casos muestran un aviso.
6. Si la web indica navegador no compatible, se queda en blanco o no notifica, usa **Abrir en navegador**. La sesión del navegador es independiente y puede pedir otro QR. No se promete soporte oficial de WhatsApp dentro de WebView2.

## 5. Spotify y volumen

1. Abre Spotify desde Inicio, inicia sesión y reproduce una canción. También puedes probar **Abrir Spotify** en DeskDock.
2. Vuelve a DeskDock. En unos segundos deberían aparecer los datos si Spotify publica una sesión reconocida en Windows.
3. Prueba pausar, reproducir, anterior y siguiente, comprobando cada efecto dentro de Spotify. Un botón apagado indica que no hay sesión o que Windows no anuncia esa capacidad.
4. Si hay dos instalaciones de Spotify, cierra una durante la prueba. DeskDock prefiere una sesión reconocida en reproducción.
5. Mueve el volumen poco a poco. Afecta a todo el audio del dispositivo de salida predeterminado. Prueba **Silenciar** y **Activar sonido**.
6. Cambia el volumen desde Windows y comprueba que DeskDock se actualiza. Prueba también conectar auriculares o cambiar la salida predeterminada.

## 6. Ventana y bandeja

- Arrastra la barra de título para mover la ventana y sus bordes para cambiar el tamaño.
- Marca **Siempre encima** para mantenerla sobre ventanas normales. No garantiza superposición sobre pantallas de seguridad o juegos en modo exclusivo.
- El mini reproductor permanece abajo en ambas pestañas; para verlo por encima de otras ventanas activa **Siempre encima**. Al minimizar toda la ventana también se oculta el mini reproductor.
- **Bandeja** oculta la ventana. Para recuperarla, busca el icono junto al reloj, posiblemente dentro de la flecha de iconos ocultos, y haz doble clic. También puedes hacer clic derecho → **Mostrar DeskDock**.
- Si **Minimizar a bandeja** está marcado, el botón de minimizar hace lo mismo. Al desmarcarlo, se minimiza a la barra de tareas.
- La **X** cierra DeskDock completamente. También puedes usar el menú de la bandeja → **Salir**. No habrá avisos de WhatsApp con DeskDock cerrado.

## 7. Informar de un problema

En la pestaña Spotify pulsa **Abrir carpeta de diagnóstico**. El archivo `diagnostics.log` guarda códigos de error. Anota qué hacías y el texto mostrado por la aplicación. No compartas la subcarpeta `WebView2`, códigos QR ni datos de tus chats.

El paquete descargado contiene `self-test-results.txt` y `build-info.json`: sirven para identificar la prueba automática y la versión. Completa `PRUEBAS-MANUALES.md` para verificar las integraciones reales.
