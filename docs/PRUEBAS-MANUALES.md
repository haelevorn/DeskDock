# Pruebas del MVP en Windows

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
