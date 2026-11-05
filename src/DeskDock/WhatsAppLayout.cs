namespace DeskDock;

internal static class WhatsAppLayout
{
    // Solo ajusta contenedores identificados por IDs; no lee mensajes ni contactos.
    // Si WhatsApp cambia su estructura, se devuelve unsupported y se usa ajuste por zoom.
    public const string InstallScript = """
    (() => {
      if (window.__deskDockLayout) return window.__deskDockLayout.update();
      const root = document.documentElement;
      let enabled = true, mode = 'auto', scheduled = false, viewportWidth = window.innerWidth;
      const marked = new Set();
      const style = document.createElement('style');
      style.id = 'deskdock-compact-style';
      style.textContent = `
        html[data-dd-view] [data-dd-stretch] { min-width:0!important; max-width:100%!important; width:100%!important; }
        html[data-dd-view] [data-dd-host] { min-width:0!important; width:100%!important; max-width:100%!important; }
        html[data-dd-view="chat"] [data-dd-list], html[data-dd-view="list"] [data-dd-chat] { display:none!important; }
        html[data-dd-view="chat"] [data-dd-chat], html[data-dd-view="list"] [data-dd-list] {
          display:flex!important; flex:1 1 100%!important; width:100%!important; min-width:0!important; max-width:100%!important;
        }
        html[data-dd-view="chat"] #main { width:100%!important; min-width:0!important; max-width:100%!important; }
        html[data-dd-view="list"] #side, html[data-dd-view="list"] #pane-side { min-width:0!important; width:100%!important; max-width:100%!important; }
        html[data-dd-view] [data-dd-divider] { display:none!important; }
        html[data-dd-view] [data-dd-divider-before]::before,
        html[data-dd-view] [data-dd-divider-after]::after { content:none!important; display:none!important; }
      `;
      (document.head || root).appendChild(style);
      function attr(element, name, value = '') {
        if (element.getAttribute(name) !== value) element.setAttribute(name, value);
        marked.add(element);
      }
      function clear() {
        root.removeAttribute('data-dd-view');
        for (const element of marked) {
          for (const name of ['data-dd-chat','data-dd-list','data-dd-host','data-dd-stretch','data-dd-divider','data-dd-divider-before','data-dd-divider-after']) element.removeAttribute(name);
        }
        marked.clear();
      }
      function length(value, reference) {
        return value.endsWith('%') ? parseFloat(value) * reference / 100 : parseFloat(value);
      }
      function dividers() {
        // Solo contenedores de columnas y sus hermanos decorativos inmediatos.
        // No se recorren mensajes, portadas, campos de escritura ni sus bordes.
        const containers = [...marked].filter(e => !e.hasAttribute('data-dd-divider'));
        for (const container of containers) {
          const bounds = container.getBoundingClientRect();
          if (bounds.height < 80) continue;
          for (const pseudo of ['before', 'after']) {
            const css = getComputedStyle(container, '::' + pseudo);
            if (css.content === 'none' || css.content === 'normal' || css.display === 'none') continue;
            const width = length(css.width, bounds.width), height = length(css.height, bounds.height);
            const border = parseFloat(css.borderLeftWidth) + parseFloat(css.borderRightWidth);
            if (width >= 0 && width + border > 0 && width + border <= 3 && height >= Math.max(80, bounds.height * .5))
              attr(container, 'data-dd-divider-' + pseudo);
          }
          for (const child of container.children) {
            if (child.id === 'main' || child.id === 'side' || child.id === 'pane-side' ||
                child.hasAttribute('data-dd-chat') || child.hasAttribute('data-dd-list') ||
                child.querySelector('#main, #side, #pane-side')) continue;
            const rect = child.getBoundingClientRect();
            if (rect.width <= 0 || rect.width > 3 || rect.height < Math.max(80, bounds.height * .5)) continue;
            if (child.matches('button, input, textarea, select, a, [tabindex], [role="button"]') ||
                child.querySelector('button, input, textarea, select, a, [tabindex], [role="button"]')) continue;
            if (getComputedStyle(child).pointerEvents === 'none' || child.getAttribute('role') === 'separator' || child.childElementCount === 0)
              attr(child, 'data-dd-divider');
          }
        }
      }
      function update() {
        if (!enabled || viewportWidth >= 720) {
          if (root.hasAttribute('data-dd-view') || marked.size) clear();
          return { state: enabled ? 'wide' : 'off' };
        }
        const main = document.getElementById('main');
        const side = document.getElementById('side') || document.getElementById('pane-side');
        if (!side) { if (marked.size) clear(); return { state:'unsupported' }; }
        // Antes de abrir el primer chat WhatsApp puede no crear #main todavía.
        // Mostrar la lista legible en lugar de reducir toda la página por zoom.
        if (!main) {
          if (![...marked].every(e => e.isConnected) || root.getAttribute('data-dd-view') !== 'list') clear();
          attr(side, 'data-dd-list');
          for (let parent = side.parentElement; parent && parent !== document.body; parent = parent.parentElement)
            attr(parent, 'data-dd-stretch');
          if (root.getAttribute('data-dd-view') !== 'list') root.setAttribute('data-dd-view', 'list');
          dividers();
          return { state:'list' };
        }
        let common = main.parentElement;
        while (common && !common.contains(side)) common = common.parentElement;
        if (!common || common === document.body || common === root) {
          if (marked.size) clear(); return { state:'unsupported' };
        }
        let chatBranch = main, listBranch = side;
        while (chatBranch.parentElement !== common) chatBranch = chatBranch.parentElement;
        while (listBranch.parentElement !== common) listBranch = listBranch.parentElement;
        if (chatBranch === listBranch) { if (marked.size) clear(); return { state:'unsupported' }; }
        // Reponer marcas si React sustituye alguno de los paneles.
        if (![...marked].every(e => e.isConnected) || !chatBranch.hasAttribute('data-dd-chat') || !listBranch.hasAttribute('data-dd-list')) clear();
        attr(chatBranch, 'data-dd-chat'); attr(listBranch, 'data-dd-list'); attr(common, 'data-dd-host');
        for (let parent = common; parent && parent !== document.body; parent = parent.parentElement) attr(parent, 'data-dd-stretch');
        const view = mode === 'list' ? 'list' : 'chat';
        if (root.getAttribute('data-dd-view') !== view) root.setAttribute('data-dd-view', view);
        dividers();
        return { state:view };
      }
      const observer = new MutationObserver(() => {
        if (!scheduled) { scheduled = true; requestAnimationFrame(() => { scheduled = false; update(); }); }
      });
      observer.observe(root, { childList:true, subtree:true });
      window.addEventListener('resize', update);
      document.addEventListener('click', event => {
        if (mode !== 'list' || !(event.target instanceof Element)) return;
        const row = event.target.closest('[role="row"], [role="listitem"]');
        if (row && row.closest('[data-dd-list]')) { mode = 'auto'; setTimeout(update, 100); }
      }, true);
      window.__deskDockLayout = {
        update,
        configure(on, nextMode, width) { enabled = on; if (nextMode) mode = nextMode; viewportWidth = width; return update(); }
      };
      return update();
    })()
    """;
}
