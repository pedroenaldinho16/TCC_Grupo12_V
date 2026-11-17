// Please see documentation at https://learn.microsoft.com/asp.net/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    'use strict';

    var btn = document.getElementById('ecoMenuBtn');
    var header = document.querySelector('.eco-header');
    var nav = document.getElementById('ecoNav');
    if (!btn || !header) { return; }

    function fechar() {
        header.classList.remove('is-open');
        btn.setAttribute('aria-expanded', 'false');
    }

    btn.addEventListener('click', function () {
        var aberto = header.classList.toggle('is-open');
        btn.setAttribute('aria-expanded', aberto ? 'true' : 'false');
    });

    if (nav) {
        nav.addEventListener('click', function (e) {
            if (e.target.closest('a')) { fechar(); }
        });
    }

    document.addEventListener('click', function (e) {
        if (header.classList.contains('is-open') && !header.contains(e.target)) { fechar(); }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') { fechar(); }
    });

    window.addEventListener('resize', function () {
        if (window.innerWidth > 991) { fechar(); }
    });
})();

(function () {
    'use strict';

    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduce || !('IntersectionObserver' in window)) { return; }

    var els = document.querySelectorAll(
        '.eco-card, .eco-stat-card, .eco-explore__head, .eco-section-title, ' +
        '.eco-hero__text, .eco-hero__carousel, .eco-table-wrap, .eco-level-card, ' +
        '.eco-perfil-card, .eco-auth-box, .eco-hero__head'
    );
    if (!els.length) { return; }

    document.documentElement.classList.add('reveal-on');

    var ordemPorPai = new Map();
    els.forEach(function (el) {
        var pai = el.parentElement || document.body;
        var i = ordemPorPai.get(pai) || 0;
        el.style.setProperty('--reveal-delay', (i * 70) + 'ms');
        ordemPorPai.set(pai, i + 1);
        el.setAttribute('data-reveal', '');
    });

    var io = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-in');
                io.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });

    els.forEach(function (el) { io.observe(el); });
})();

// ============================================================
// Sistema Global de Modais EcoCanta (Feedback, Alertas e Confirmações)
// ============================================================
(function () {
    'use strict';

    var icones = {
        sucesso: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>',
        erro: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>',
        aviso: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>',
        info: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>'
    };

    window.mostrarModalEco = function (titulo, mensagem, tipo) {
        tipo = tipo || 'info';
        var modalEl = document.getElementById('ecoModalFeedback');
        if (!modalEl || !window.bootstrap) {
            return;
        }

        var iconEl = document.getElementById('ecoModalIcon');
        var titleEl = document.getElementById('ecoModalTitle');
        var textEl = document.getElementById('ecoModalText');

        if (iconEl) {
            iconEl.className = 'eco-modal__icon-wrap mb-3 eco-modal__icon-wrap--' + tipo;
            iconEl.innerHTML = icones[tipo] || icones.info;
        }
        if (titleEl) {
            titleEl.textContent = titulo || (tipo === 'sucesso' ? 'Sucesso!' : tipo === 'erro' ? 'Atenção!' : 'Aviso');
        }
        if (textEl) {
            textEl.innerHTML = mensagem || '';
        }

        var bsModal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
        bsModal.show();
    };

    // Substituir alert nativo do navegador pelo modal EcoCanta
    window.alert = function (msg) {
        window.mostrarModalEco('Aviso', msg, 'aviso');
    };

    // Modal de Confirmação customizado
    var formPendenteConfirmacao = null;
    var callbackPendente = null;

    window.confirmarAcaoEco = function (mensagem, onConfirmar, titulo) {
        var modalEl = document.getElementById('ecoModalConfirmacao');
        if (!modalEl || !window.bootstrap) {
            if (window.confirm(mensagem)) { onConfirmar(); }
            return;
        }

        var titleEl = document.getElementById('ecoModalConfirmTitle');
        var textEl = document.getElementById('ecoModalConfirmText');
        if (titleEl) { titleEl.textContent = titulo || 'Confirmar ação'; }
        if (textEl) { textEl.textContent = mensagem; }

        callbackPendente = onConfirmar;
        formPendenteConfirmacao = null;

        var bsModal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
        bsModal.show();
    };

    document.addEventListener('DOMContentLoaded', function () {
        var btnConfirmSim = document.getElementById('ecoModalConfirmBtnSim');
        if (btnConfirmSim) {
            btnConfirmSim.addEventListener('click', function () {
                var modalEl = document.getElementById('ecoModalConfirmacao');
                if (modalEl && window.bootstrap) {
                    var bsModal = bootstrap.Modal.getInstance(modalEl);
                    if (bsModal) { bsModal.hide(); }
                }

                if (formPendenteConfirmacao) {
                    var form = formPendenteConfirmacao;
                    formPendenteConfirmacao = null;
                    form.submit();
                } else if (callbackPendente) {
                    var cb = callbackPendente;
                    callbackPendente = null;
                    cb();
                }
            });
        }

        // Interceptar submissão de formulários com data-confirmar
        document.addEventListener('submit', function (e) {
            var form = e.target;
            if (!form || !form.hasAttribute || !form.hasAttribute('data-confirmar')) {
                return;
            }

            e.preventDefault();
            var msg = form.getAttribute('data-confirmar') || 'Deseja realmente prosseguir?';
            formPendenteConfirmacao = form;
            callbackPendente = null;

            var modalEl = document.getElementById('ecoModalConfirmacao');
            if (modalEl && window.bootstrap) {
                var textEl = document.getElementById('ecoModalConfirmText');
                if (textEl) { textEl.textContent = msg; }
                var bsModal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                bsModal.show();
            } else {
                form.submit();
            }
        });

        // Auto-disparo do modal se a página tiver dados de TempData
        var autoModal = document.getElementById('ecoAutoTriggerModal');
        if (autoModal) {
            var tipo = autoModal.getAttribute('data-tipo');
            var titulo = autoModal.getAttribute('data-titulo');
            var msg = autoModal.getAttribute('data-mensagem');
            if (msg) {
                window.mostrarModalEco(titulo, msg, tipo);
            }
        }
    });
})();

