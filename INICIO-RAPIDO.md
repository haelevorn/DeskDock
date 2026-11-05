# Usar DeskDock sin instalar herramientas de programación

## Ultramini, referencia y ChatGPT — versión 0.8

Actualiza cerrando DeskDock, extrayendo el nuevo ZIP completo y abriendo su EXE. La cabecera muestra **0.8**.

### Ultramini

Abre **⋯ → Modo ultramini**. Se oculta la ventana completa y aparece una de aproximadamente 300 × 98 unidades con hora y música. Si corre un pomodoro, también muestra tiempo/etapa; pulsarlo vuelve a la pestaña Pomodoro. El icono de esquinas arriba a la derecha vuelve a la ventana completa. Cerrar ultramini con Alt+F4 también vuelve a la ventana completa; para salir del programa usa la X de la ventana completa o Salir desde la bandeja.

La opción Siempre encima se conserva. El reloj, pomodoro y avisos siguen funcionando mientras DeskDock está abierto. El modo no se inicia automáticamente al reabrir el programa. No es un modo de ahorro de memoria: WhatsApp continúa abierto para intentar recibir avisos.

### Referencia mejorada y Recientes

Se mantiene **una imagen abierta**, elegida desde tu computadora. La carpeta abre el selector; el icono de reloj abre **Recientes**, con las últimas 10 imágenes usadas. Un archivo que ya no existe aparece deshabilitado. **Limpiar recientes** limpia el historial sin borrar los archivos.

Los nuevos iconos giran **90°** y voltean horizontalmente. Al hacerlo, la imagen vuelve a ajustarse al panel. Zoom y arrastre siguen disponibles después del giro/volteo. Los cambios son solo de visualización: no alteran la imagen original. Al abrir otra imagen, se reinician el giro y volteo. No hay galería ni colección de varias imágenes cargadas.

### ChatGPT web experimental

1. Abre **ChatGPT**. Solo entonces se crea su navegador incrustado y se visita `https://chatgpt.com/`.
2. Inicia sesión tú mismo en el sitio si lo solicita. DeskDock no captura contraseñas ni lee tus conversaciones. Su perfil es independiente del navegador normal y puede pedir un nuevo inicio de sesión.
3. Comprueba que puedes escribir una consulta y recibir una respuesta. «Página cargada» no demuestra que hayas iniciado sesión ni que todas las funciones funcionen.
4. Si hay un bloqueo, una verificación que no avanza o un proveedor de acceso incompatible, pulsa **Abrir navegador** e inicia sesión allí. No se intenta eludir verificaciones ni se importan cookies.
5. **Cerrar web** elimina el control de esa pestaña para liberar recursos, conservando el perfil. **Recargar** lo vuelve a abrir. Si no usas esta pestaña, no añade un navegador al iniciar DeskDock.
6. Micrófono, cámara y permisos privilegiados están desactivados en este primer contenedor. No se garantiza voz, ventanas auxiliares de login, pagos, cargas/descargas o todas las herramientas web. Los límites y funciones disponibles son los de tu cuenta de ChatGPT.
7. No se usa una clave API. La foto de Referencia y los datos de DeskDock no se envían automáticamente a ChatGPT; cualquier consulta/archivo lo seleccionas tú dentro de la web.

El paquete incluye `chatgpt-web-probe.txt`: registra un intento de navegación pública desde el servidor de compilación, sin cuenta. No valida tu sesión ni la compatibilidad completa. Con cinco pestañas, las etiquetas pueden pasar a dos filas en el ancho mínimo para seguir siendo accesibles.

## Correcciones — versión 0.7

1. Cierra la versión anterior, extrae el ZIP nuevo completo en otra carpeta y abre el EXE. Debe mostrar **0.7**.
2. En WhatsApp, usa los botones pequeños **Chats** y **Chat**, otra vez visibles arriba del contenido, para alternar lista y conversación. También siguen en «⋯».
3. Pulsa **Avisos / Activar avisos** en esa barra para conceder el permiso del sitio. Revisa también que las notificaciones estén activas dentro de WhatsApp.
4. Recibe un mensaje con otra ventana activa y repite con DeskDock en bandeja. En **⋯ → Ver estado y avisos…** se muestran el permiso del sitio, eventos web recibidos y contador detectado. La prueba de aviso solo comprueba la presentación de Windows, no la recepción desde WhatsApp.
5. Si la prueba funciona y el mensaje real no notifica, anota esos tres datos de estado después de recibirlo. La alternativa por título solo detecta aumentos del contador; puede no avisar de otro mensaje de un chat que ya estaba sin leer. No es una garantía de push con la app cerrada.
6. Glass está eliminado. No necesitas borrar preferencias ni cambiar ninguna configuración de Windows. Las instrucciones de versiones antiguas que lo mencionan ya no se aplican.
7. En Referencia, de izquierda a derecha: **carpeta = abrir**, **esquinas/rectángulo = ajustar**, **lupa − = alejar**, **lupa + = acercar**, **X = quitar la referencia sin borrar el archivo**. Cada botón incluye descripción al pasar el ratón.

## Pomodoro visible, perfiles, Glass y Referencia — versión 0.6

Actualiza cerrando la aplicación anterior, extrayendo el nuevo ZIP completo en otra carpeta y abriendo su EXE. La cabecera debe mostrar **0.6**.

### Pomodoro activo y perfiles

Cuando el temporizador corre, aparece una franja pequeña sobre el reproductor con tiempo y etapa, incluso al ver WhatsApp, Spotify o Referencia. Pulsa esa franja para ir a Pomodoro. Al pausar o reiniciar se oculta.

Para guardar distintas configuraciones:

1. En Pomodoro, despliega **Configurar etapas** y escribe los minutos, sesiones y avance automático que quieras guardar.
2. Despliega **Mis pomodoros**, escribe un nombre (por ejemplo «Dibujo» o «Lectura») y pulsa **Guardar**. No cambia el temporizador que ya corre. Guardar el mismo nombre, aunque cambien mayúsculas, actualiza sus valores.
3. Para usar una configuración, selecciónala en la lista y pulsa **Cargar y reiniciar**; después pulsa **Iniciar**. Esto reinicia el ciclo actual.
4. **Eliminar** retira el perfil seleccionado, sin modificar el temporizador actual. Los perfiles se guardan localmente y siguen disponibles al reabrir.

### Glass opcional

Abre **⋯ → Glass translúcido (Windows 11)**. En Windows 11 22H2 o posterior se solicita el fondo acrílico de Windows para el marco y paneles nativos de DeskDock. Texto, foto de referencia y contenido web siguen siendo legibles; WhatsApp conserva su fondo. Desmarca la opción para recuperar el skin opaco. En sistemas no compatibles la opción aparece deshabilitada. Su apariencia depende también de la configuración de transparencia, composición, energía y sesión remota de Windows; no se modifica ninguna de esas preferencias automáticamente.

### Referencia de dibujo

1. Abre la pestaña **Referencia** y pulsa el icono de carpeta, o arrastra una imagen desde el Explorador al panel.
2. Usa **Ajustar** para verla completa. Al cambiar el tamaño de la ventana se mantiene ajustada hasta que hagas zoom o la muevas manualmente.
3. La rueda y los iconos +/− cambian el zoom. Arrastra con el botón izquierdo para desplazar la imagen. **Ajustar** vuelve al encuadre completo.
4. El icono X quita la referencia de DeskDock; no elimina el archivo original.
5. La imagen es local: no se envía a GitHub ni a un servicio. Se recuerda su ruta. Si cambias de carpeta, eliminas el archivo o envías el programa a otra computadora, habrá que abrirla de nuevo allí.
6. Admite PNG, JPG, BMP, GIF y TIFF. GIF/TIFF muestran solo el primer fotograma/página. Límite 50 MB y 100 megapíxeles; imágenes grandes se reducen en memoria a 4096 píxeles en el lado mayor. No hay edición, pintura, exportación ni gestión de varias referencias simultáneas.

Para usarla como referencia flotante, activa **⋯ → Siempre encima** y ajusta el tamaño. **Ver lista de chats** y **Ver conversación** de WhatsApp están ahora en el menú «⋯».

## Ventana pequeña, reloj y Pomodoro — versión 0.5

1. Cierra DeskDock, descarga el nuevo artefacto y extrae todos sus archivos en otra carpeta. Abre el EXE; la cabecera debe mostrar **0.5**.
2. Arrastra los bordes para reducir la ventana. Ahora permite bajar hasta **300 × 340**. También puedes usar **⋯ → Tamaño compacto** para 320 × 420. Se recordará el tamaño al cerrar. En pantallas con otra escala de Windows el tamaño físico puede variar.
3. La portada Spotify se hace más pequeña y el mini reproductor deja más espacio al texto. Las opciones largas de Pomodoro se pueden desplazar verticalmente si no caben. WhatsApp usa la vista adaptable que ya conoces; el resultado real depende del diseño de su web.
4. La esquina superior muestra la **hora actual de Windows** en formato 24 horas. Pasa el ratón para consultar la fecha. Si quieres verla al trabajar con otras ventanas, activa **⋯ → Siempre encima**. No garantiza superposición sobre juegos exclusivos o pantallas de seguridad; en bandeja toda la ventana queda oculta.
5. Abre **Pomodoro**. Por defecto ofrece **25 min de concentración → 5 min de descanso**, con **15 min de descanso largo cada 4 sesiones**.
6. En **Configurar etapas**, escribe minutos enteros para cada etapa y el número de sesiones. Elige si quieres pasar automáticamente de etapa y pulsa **Aplicar y reiniciar**. Eso guarda las duraciones y reinicia el ciclo; no cambia el temporizador mientras solo editas los campos.
7. Pulsa **Iniciar**. El botón cambia a **Pausar**; al reanudar conserva el tiempo restante. El icono de reinicio vuelve al comienzo de concentración y pone a cero las sesiones.
8. Al terminar una etapa, verás el mensaje en Pomodoro y se solicitará un aviso de Windows con el sonido configurado. Al pulsar el aviso se abre Pomodoro. Si desactivaste avance automático, pulsa Iniciar para comenzar la siguiente etapa.
9. Puedes cambiar de pestaña o minimizar a bandeja: el contador continúa. **Cerrar DeskDock detiene la sesión**; al abrirlo conserva configuración pero comienza un ciclo nuevo. Con Windows suspendido no se presentan avisos; al volver se comprueba el vencimiento y se inicia o prepara la siguiente etapa sin sumar sesiones ficticias.
10. Si los avisos no aparecen, comprueba **⋯ → Probar aviso** y las preferencias de Windows. El mensaje dentro de Pomodoro permanece disponible aunque Windows suprima el aviso.

## Skins, icono y sonido — versión 0.4

1. Cierra DeskDock por completo, descarga el nuevo artefacto, extrae todos sus archivos en otra carpeta y abre `DeskDock.exe`. La cabecera debe mostrar **0.4**.
2. Abre **⋯**. En **SKINS**, elige **Aurora · azul y violeta** o **Eclipse · negro y ámbar**. **Minimal · original** recupera el diseño anterior. El cambio es inmediato y queda guardado.
3. Los skins colorean la ventana y sus controles; WhatsApp conserva su interfaz web y el fondo de tu escritorio permanece igual.
4. En el menú, deja marcado **Sonido de aviso · bwuooop** y pulsa **Probar aviso**. Cuando Windows presente el aviso, debería oírse el nuevo sonido, a un nivel moderado, según el volumen y mezclador de Windows.
5. Desmarca esa opción para que los avisos sean silenciosos. No se modifica el sonido global de Windows.
6. El icono de la onda aparece en el ejecutable, barra de tareas y bandeja. Si habías anclado una versión vieja, desancla ese acceso y ancla el nuevo ejecutable. Windows puede mantener en caché el icono anterior durante un tiempo.
7. Si no ves ni oyes el aviso de prueba, consulta **⋯ → Ver estado y avisos…**, las notificaciones de Windows y No molestar. El aviso de prueba no confirma recepción desde WhatsApp.

## Interfaz minimalista 0.3

Para actualizar, cierra la versión anterior, descarga el nuevo artefacto, extrae todos sus archivos en otra carpeta y abre DeskDock.exe. La cabecera muestra **0.3**.

- **⋯**, arriba a la derecha: recargar WhatsApp, abrir navegador o Spotify, vista adaptable, siempre encima, bandeja, activar/probar avisos, ver estado y diagnóstico.
- Los dos iconos a la derecha de las pestañas muestran la **lista de chats** y la **conversación**. Pasa el ratón para ver su descripción. Solo aparecen en WhatsApp.
- La franja inferior muestra portada pequeña, título/artista y anterior, reproducir o pausar, siguiente y altavoz. El altavoz abre el volumen maestro y silencio.
- La pestaña Spotify conserva portada grande y controles. Los mensajes de diagnóstico se consultan en **⋯ → Ver estado y avisos…** y ya no ocupan espacio permanente.
- La zona del nombre permite arrastrar la ventana. Los tres iconos siguientes al menú minimizan, maximizan/restauran y cierran.

Las instrucciones históricas de 0.2 que mencionan botones grandes siguen siendo válidas para las funciones; en 0.3 las acciones se encuentran en el menú indicado.

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
