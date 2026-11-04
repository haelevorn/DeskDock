# Pruebas del MVP en Windows

## Cambios 0.6 — comprobaciones manuales pendientes

- Iniciar Pomodoro y pasar por las cuatro pestañas: indicador con tiempo/etapa siempre presente. Clic vuelve a Pomodoro. Pausar/reiniciar lo oculta.
- Cambiar cada skin con configuración expandida: campos, casillas, lista y progreso con contraste legible, sin controles blancos o azules del tema de Windows.
- Guardar «Dibujo» y «Lectura» con tiempos diferentes; cargar cada uno, cerrar/reabrir, actualizar mismo nombre y eliminar otro. La configuración en marcha solo se reinicia al Aplicar o Cargar y reiniciar.
- Windows 11 compatible: activar/desactivar Glass sobre otra ventana y con cada pestaña/skin. Texto y WebView2 utilizables. Windows no compatible: opción deshabilitada. Probar también con efectos de transparencia de Windows desactivados, si esa es ya la configuración del equipo.
- Referencia: abrir/arrastrar PNG y JPG, ajustar, zoom, desplazar y recuperar ajuste. Cambiar tamaño hasta 300 × 340. Imagen dentro de límites, sin superponer controles.
- Cerrar/reabrir conservando la ruta de referencia; mover el archivo y reabrir: aviso claro, posibilidad de elegir otra imagen. Quitar referencia no debe borrar el original.
- Archivo inválido o demasiado grande: no cerrar la aplicación ni reemplazar la referencia válida actual.
- Glass y avisos reales no se verifican en el Windows Server del CI. Las pruebas de imagen usan una imagen sintética, no una foto del usuario.

## Ventana y concentración 0.5 — prueba manual pendiente

- Reducir a 300 × 340 y ampliar; reloj, tres pestañas y controles básicos sin superposición. Comprobar en WhatsApp que caben cabecera y caja de escritura; probar con 125%/150% de escala.
- Usar Tamaño compacto y cerrar/reabrir: conservar tamaño, skin, sonido y duraciones.
- Comparar HH:mm con el reloj de Windows, cambiar de pestaña y mantener la ventana encima de una aplicación normal. Verificar fecha al pasar el ratón.
- Configurar concentración/corto/largo a 1 minuto y 2 sesiones, aplicar, iniciar y comprobar secuencia concentración → corto → concentración → largo → concentración, un aviso por etapa.
- Pausar, esperar y reanudar: conservar tiempo. Reiniciar: cero sesiones y concentración detenida. Probar avance automático desactivado.
- Editar campos inválidos (cero, decimales, letras): mensaje claro y ninguna modificación de la sesión. Corregir y aplicar.
- Iniciar Pomodoro, pasar a WhatsApp/Spotify y minimizar a bandeja: comprobar aviso y tiempo. Pulsar el aviso: abrir tercera pestaña.
- Repetir aviso con sonido activado y desactivado. No molestar puede suprimir la presentación; comprobar el mensaje dentro de Pomodoro.
- Suspender y reanudar en una prueba controlada: no añadir ciclos ficticios. Cerrar y abrir DeskDock: nueva sesión, preferencias conservadas.

Las pruebas automáticas usan un reloj simulado para secuencias, pausas y vencimientos; no prueban sonido audible o avisos reales.

## Skins, audio e icono 0.4 — prueba manual pendiente

- Cambiar Aurora/Eclipse/Minimal en la pestaña WhatsApp, Spotify y con el volumen abierto: superficies y texto actualizados sin volver a vincular WhatsApp.
- Cerrar y abrir: conservar skin, sonido, siempre encima y minimizar a bandeja. Probar actualización desde preferencias de 0.3.
- Confirmar icono propio en EXE, barra de tareas, bandeja y aviso. Anclar el nuevo EXE si el acceso viejo conserva otro icono.
- «Probar aviso»: oír un solo bwuooop, sin tono estándar simultáneo, con volumen de Windows moderado. Desactivar sonido y repetir: ningún sonido.
- Pulsar el aviso: restaurar WhatsApp. Bandeja: doble clic, clic derecho, Mostrar y Salir funcionan. Confirmar retirada del icono al cerrar y recuperación si Explorer se reinicia en una prueba controlada.
- Con No molestar activo, comprobar las decisiones de Windows; DeskDock no debe forzar el aviso ni reproducir sonido antes del evento de presentación.
- Recibir un mensaje real: verificar el comportamiento con las mismas limitaciones de WebView2 y del contador de no leídos documentadas.

Las capturas `preview-*.png` del paquete son vistas sintéticas de las pruebas automáticas, no fotos de una sesión real de Spotify o WhatsApp.

## Interfaz 0.3 — comprobación manual pendiente

- Abrir el menú «⋯» con ratón y teclado; comprobar recargar, preferencias marcables y acceso a estado/avisos.
- Abrir altavoz, mover volumen, silenciar y cerrar el menú; comprobar que sigue el volumen de Windows.
- Cambiar entre Spotify y WhatsApp: iconos de lista/chat solo en WhatsApp, mini reproductor conservado.
- Reproducir/pausar desde ambas vistas y desde Spotify; comprobar que el icono alterna.
- Probar ancho mínimo, ventana maximizada y escala de Windows 125%/150%; controles sin superposición y título largo truncado con descripción completa al pasar el ratón.
- Repetir las pruebas de avisos desde el menú. La reducción visual no cambia los límites documentados de recepción.

## Casos añadidos en 0.2 — pendientes de prueba en el equipo del usuario

| Caso | Resultado esperado | Resultado |
| --- | --- | --- |
| Barra propia | No aparece la barra estándar; minimizar, maximizar/restaurar, doble clic y cerrar funcionan; bordes permiten redimensionar | PENDIENTE |
| Esquinas | En Windows 11 la ventana normal muestra esquinas redondeadas | PENDIENTE |
| Chat estrecho | Abrir un chat a 390/490/650 de ancho de ventana; cabecera, mensajes y caja de escritura dentro del ancho, sin desplazamiento lateral | PENDIENTE |
| Volver a lista | Chats muestra lista completa; seleccionar conversación vuelve al chat o permite pulsar Chat | PENDIENTE |
| Ventana amplia | Ampliar, volver a reducir y alternar Vista adaptable; se restaura diseño original sin paneles ocultos persistentes | PENDIENTE |
| Spotify completo | Portada, título y artista coinciden con mini; ambos juegos de botones controlan la misma sesión y se desactivan al cerrar Spotify | PENDIENTE |
| Aviso de prueba | Probar aviso muestra aviso nativo; pulsarlo restaura WhatsApp | PENDIENTE |
| Aviso web | Activar avisos, activar ajustes WhatsApp y recibir mensaje con otra ventana activa; comprobar evento y aviso | PENDIENTE |
| Aviso por título | Sin evento web, un aumento del contador del título en segundo plano solicita un aviso genérico; no promete uno por cada mensaje | PENDIENTE |
| Sin duplicados | Una notificación web y un cambio del contador próximos no producen dos avisos; no hay avisos de no leídos al cargar | PENDIENTE |

La prueba automatizada de WebView2 usa HTML sintético: no debe marcar estos casos reales como superados.

Estado inicial de todos los casos: **PENDIENTE**. No marcar «probado» sin ejecutar el caso. Un workflow verde solo prueba las comprobaciones descritas en su informe.

Fecha: ______  Versión Windows: ______  Escala de pantalla: ______
Commit de build-info.json: ______  WebView2 Runtime: ______
Spotify: versión ______ / instalación Microsoft Store o web ______

En cada casilla registra PASS, FAIL o NO DISPONIBLE y observaciones. No copies mensajes privados ni códigos QR al informe.

| Caso | Procedimiento y resultado esperado | Resultado |
| --- | --- | --- |
| Arranque | Extraer todo el artefacto en un equipo sin SDK/.NET instalado y abrir DeskDock.exe; aparece la ventana | PENDIENTE |
| Tamaño y DPI | Mover, redimensionar hasta mínimo, cambiar escala/monitor; textos y controles accesibles | PENDIENTE |
| Pestañas | Cambiar WhatsApp/Spotify; el reproductor permanece abajo y los controles responden | PENDIENTE |
| Siempre encima | Activar, enfocar otra ventana normal, desactivar; cerrar/reabrir conserva la opción | PENDIENTE |
| Bandeja | Botón Bandeja y minimizar con opción activa; doble clic/menu restaura | PENDIENTE |
| Barra de tareas | Desactivar minimizar a bandeja y minimizar; vuelve desde la barra de tareas | PENDIENTE |
| Salida | X y menú Salir cierran y retiran icono; abrir de nuevo no duplica instancias | PENDIENTE |
| Sin WebView2 | Solo en VM/equipo que ya carezca del runtime: mensaje claro y app abierta; no desinstalar runtimes compartidos para probar | PENDIENTE |
| QR | Cargar web.whatsapp.com y vincular desde el teléfono; ver chats reales | PENDIENTE |
| Mensajes | Enviar/recibir un mensaje de prueba sin datos privados | PENDIENTE |
| Persistencia | Cerrar con X, abrir; reiniciar Windows y abrir; comprobar si mantiene la vinculación | PENDIENTE |
| Revocación | Desvincular desde el teléfono; verificar que deja de dar acceso a los chats | PENDIENTE |
| Notificación visible | Activar notificaciones y permitir permiso, recibir con otra ventana activa | PENDIENTE |
| Notificación otra pestaña | Repetir en pestaña Spotify | PENDIENTE |
| Notificación bandeja | Repetir con DeskDock oculto, sin cerrarlo; revisar contador al restaurar | PENDIENTE |
| Notificaciones denegadas | Denegar permiso en un perfil de prueba; no afirmar que se recibirán avisos | PENDIENTE |
| Sin red | Desconectar temporalmente y recargar; no cierra toda la aplicación | PENDIENTE |
| Alternativa navegador | Abrir navegador; verificar que no comparte automáticamente sesión | PENDIENTE |
| Sin Spotify | Cerrar Spotify; controles apagados, aviso, sin datos viejos | PENDIENTE |
| Spotify Win32 | Abrir instalación web oficial, reproducir; detectar ID, título, artista y portada | PENDIENTE |
| Spotify Store | Repetir en un equipo con versión Store, si disponible | PENDIENTE |
| Reproducir/pausar | Accionar cada botón; confirmar el efecto en Spotify | PENDIENTE |
| Anterior/siguiente | Verificar cambio real y comportamiento de anterior; probar restricciones/anuncios si corresponde | PENDIENTE |
| Otra aplicación multimedia | Con Spotify y un vídeo del navegador activos, controlar desde DeskDock; solo Spotify debe recibir los comandos | PENDIENTE |
| Otra app sin Spotify | Cerrar Spotify dejando vídeo/audio de WhatsApp; controles deben apagarse | PENDIENTE |
| Reinicio Spotify | Cerrar y abrir Spotify con DeskDock abierto; retirar datos viejos y recuperar sesión | PENDIENTE |
| Cambio canción | Cambiar desde Spotify; título, artista y portada se actualizan | PENDIENTE |
| Volumen | A nivel moderado, mover slider y comprobar volumen Windows; cambiar desde Windows y comprobar lectura | PENDIENTE |
| Silencio | Silenciar y reactivar; Windows y DeskDock coinciden | PENDIENTE |
| Nueva salida | Cambiar salida predeterminada a auriculares; controles siguen la nueva salida | PENDIENTE |
| Sin salida | En VM sin audio o situación ya existente; mensaje y controles apagados, recuperación al volver dispositivo | PENDIENTE |

Si un caso falla, registra el texto de estado y los códigos de `diagnostics.log`, junto con `build-info.json`. Un fallo de notificaciones o compatibilidad WebView2 debe describirse tal cual; no se debe presentar la alternativa en navegador como integración embebida validada.
