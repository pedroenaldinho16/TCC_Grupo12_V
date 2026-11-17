(() => {
    'use strict';
    const root = document.documentElement;
    const key = 'ecocanta.theme';
    const system = window.matchMedia('(prefers-color-scheme: dark)');
    let choice = null;
    try {
        const stored = localStorage.getItem(key);
        if (stored === 'dark' || stored === 'light') choice = stored;
    } catch { /* A alternância continua funcionando com armazenamento indisponível. */ }

    function apply(theme) {
        root.dataset.theme = theme;
        root.dataset.bsTheme = theme;
        root.style.colorScheme = theme;
        document.querySelector('meta[name="theme-color"]')?.setAttribute('content', theme === 'dark' ? '#0f1c17' : '#f1faf7');
        document.querySelectorAll('[data-theme-toggle]').forEach(button => {
            button.hidden = false;
            button.setAttribute('aria-pressed', String(theme === 'dark'));
            button.title = theme === 'dark' ? 'Ativar modo claro' : 'Ativar modo escuro';
            button.querySelector('[data-theme-label]').textContent = theme === 'dark' ? 'Escuro' : 'Claro';
        });
    }

    // Executado no head antes das folhas de estilo, evitando um clarão ao navegar.
    apply(choice || (system.matches ? 'dark' : 'light'));
    function ready() {
        apply(root.dataset.theme);
        document.querySelectorAll('[data-theme-toggle]').forEach(button => {
            button.addEventListener('click', () => {
                choice = root.dataset.theme === 'dark' ? 'light' : 'dark';
                try { localStorage.setItem(key, choice); } catch { /* Preferência mantida nesta página. */ }
                apply(choice);
            });
        });
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', ready, { once: true });
    else ready();
    system.addEventListener('change', () => { if (!choice) apply(system.matches ? 'dark' : 'light'); });
    window.addEventListener('storage', event => {
        if (event.key !== key && event.key !== null) return;
        choice = event.newValue === 'light' || event.newValue === 'dark' ? event.newValue : null;
        apply(choice || (system.matches ? 'dark' : 'light'));
    });
})();
