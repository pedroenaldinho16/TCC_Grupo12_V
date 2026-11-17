(function () {
    'use strict';

    // Elementos da interface
    const tabBotoes = Array.from(document.querySelectorAll('[data-educacao-tab]'));
    const tabPaineis = Array.from(document.querySelectorAll('[data-educacao-panel]'));
    const listaNotasEl = document.getElementById('listaNotasContainer');
    const badgeTotalNotas = document.getElementById('badgeTotalNotas');
    const badgeSidebarTotal = document.getElementById('badgeSidebarTotal');

    const inputTitulo = document.getElementById('editorTitulo');
    const selectArtigo = document.getElementById('editorArtigoSelect');
    const editorCanvas = document.getElementById('editorCanvas');
    const docStatsEl = document.getElementById('docStats');
    const docStatusEl = document.getElementById('docStatus');

    const btnNovaNota = document.getElementById('btnNovaNota');
    const btnSalvarNota = document.getElementById('btnSalvarNota');
    const btnExcluirNota = document.getElementById('btnExcluirNota');
    const btnExportarDoc = document.getElementById('btnExportarDoc');
    const btnCopiarDoc = document.getElementById('btnCopiarDoc');
    const buscaNotasInput = document.getElementById('buscaNotasInput');

    const CSRF_TOKEN = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

    // Estado da nota atual
    let notaAtual = {
        id: null,
        titulo: '',
        conteudoHtml: '',
        artigoSlug: '',
        artigoTitulo: '',
        ehLocal: false
    };

    let notasLocais = [];
    let timerAutoSave = null;

    // Inicialização do LocalStorage para notas offline/convidado
    function carregarNotasLocais() {
        try {
            const raw = localStorage.getItem('eco_estudo_notas');
            notasLocais = raw ? JSON.parse(raw) : [];
        } catch (e) {
            notasLocais = [];
        }
    }

    function salvarNotasLocaisStorage() {
        try {
            localStorage.setItem('eco_estudo_notas', JSON.stringify(notasLocais));
        } catch (e) {
            console.error('Falha ao salvar no localStorage', e);
        }
    }

    // Gerenciador de Abas (Estudos x Caderno de Notas)
    function ativarAba(nome) {
        tabBotoes.forEach(function (btn) {
            const ativo = btn.getAttribute('data-educacao-tab') === nome;
            btn.classList.toggle('is-active', ativo);
        });

        tabPaineis.forEach(function (painel) {
            const ativo = painel.getAttribute('data-educacao-panel') === nome;
            painel.hidden = !ativo;
            if (ativo) {
                painel.classList.add('is-active');
            } else {
                painel.classList.remove('is-active');
            }
        });

        history.replaceState(null, '', '#' + nome);
    }

    tabBotoes.forEach(function (btn) {
        btn.addEventListener('click', function () {
            ativarAba(btn.getAttribute('data-educacao-tab'));
        });
    });

    // Editor de Documento - Comandos de Formatação
    function executarComando(cmd, valor) {
        if (!editorCanvas) return;
        editorCanvas.focus();
        document.execCommand(cmd, false, valor || null);
        atualizarContadores();
        agendarAutoSave();
    }

    // Configurar botões da barra de ferramentas
    const toolBtns = Array.from(document.querySelectorAll('[data-doc-cmd]'));
    toolBtns.forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const cmd = btn.getAttribute('data-doc-cmd');
            const val = btn.getAttribute('data-doc-val');

            if (cmd === 'callout') {
                inserirCallout();
            } else if (cmd === 'highlight') {
                executarComando('hiliteColor', '#FEF08A');
            } else if (cmd === 'clear') {
                executarComando('removeFormat');
            } else {
                executarComando(cmd, val);
            }
        });
    });

    function inserirCallout() {
        editorCanvas.focus();
        const html = `
            <div class="eco-doc-callout" style="background:#F0FDF4; border:1px solid #BBF7D0; border-radius:10px; padding:14px 18px; margin:16px 0; display:flex; gap:12px; align-items:flex-start;">
                <span style="font-size:1.3rem; line-height:1;">💡</span>
                <div style="flex:1;">
                    <strong style="display:block; color:#065F46; margin-bottom:4px;">Ponto-Chave de Estudo:</strong>
                    <span>Insira aqui a conclusão ou descoberta científica mais importante...</span>
                </div>
            </div><p><br></p>`;
        document.execCommand('insertHTML', false, html);
        atualizarContadores();
        agendarAutoSave();
    }

    // Estatísticas do Documento (Palavras e Caracteres)
    function atualizarContadores() {
        if (!editorCanvas) return;
        const texto = editorCanvas.innerText || '';
        const caracteres = texto.length;
        const palavras = texto.trim() === '' ? 0 : (texto.trim().match(/\S+/g) || []).length;
        const leituraMin = Math.ceil(palavras / 180);

        if (docStatsEl) {
            docStatsEl.textContent = `${palavras} ${palavras === 1 ? 'palavra' : 'palavras'} • ${caracteres} caracteres • ~${leituraMin} min de leitura`;
        }
    }

    // Agendar salvamento automático
    function agendarAutoSave() {
        if (docStatusEl) {
            docStatusEl.innerHTML = '<span class="text-muted"><i class="bi bi-clock-history"></i> Alterações pendentes...</span>';
        }
        clearTimeout(timerAutoSave);
        timerAutoSave = setTimeout(function () {
            salvarNotaAtual(true);
        }, 1800);
    }

    if (editorCanvas) {
        editorCanvas.addEventListener('input', function () {
            atualizarContadores();
            agendarAutoSave();
        });
    }

    if (inputTitulo) {
        inputTitulo.addEventListener('input', function () {
            agendarAutoSave();
        });
    }

    if (selectArtigo) {
        selectArtigo.addEventListener('change', function () {
            agendarAutoSave();
        });
    }

    // Atalhos de teclado
    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
            const painelNotas = document.querySelector('[data-educacao-panel="notas"]');
            if (painelNotas && !painelNotas.hidden) {
                e.preventDefault();
                salvarNotaAtual(false);
            }
        }
    });

    // Salvar nota (Backend com fallback para LocalStorage)
    function salvarNotaAtual(silencioso) {
        const titulo = inputTitulo.value.trim() || 'Minha Anotação de Estudo';
        const conteudo = editorCanvas.innerHTML;
        const artigoOpt = selectArtigo.options[selectArtigo.selectedIndex];
        const artigoSlug = selectArtigo.value;
        const artigoTitulo = artigoSlug ? (artigoOpt.getAttribute('data-titulo') || artigoOpt.text) : '';

        if (!silencioso && docStatusEl) {
            docStatusEl.innerHTML = '<span class="text-primary"><i class="bi bi-arrow-repeat spin"></i> Salvando...</span>';
        }

        const payload = {
            id: notaAtual.id,
            titulo: titulo,
            conteudoHtml: conteudo,
            artigoSlug: artigoSlug,
            artigoTitulo: artigoTitulo
        };

        fetch('/Educacao/SalvarNota', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': CSRF_TOKEN,
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify(payload)
        })
            .then(function (res) {
                if (!res.ok) throw new Error('Falha no servidor');
                return res.json();
            })
            .then(function (data) {
                if (data.sucesso) {
                    notaAtual.id = data.id;
                    notaAtual.titulo = data.titulo;
                    notaAtual.artigoSlug = data.artigoSlug;
                    notaAtual.artigoTitulo = data.artigoTitulo;
                    notaAtual.ehLocal = !!data.ehLocal;

                    if (docStatusEl) {
                        docStatusEl.innerHTML = `<span class="text-success"><i class="bi bi-check-circle-fill"></i> Salvo às ${data.data || new Date().toLocaleTimeString()}</span>`;
                    }

                    // Se estiver em modo local, salva no localStorage também
                    if (data.ehLocal) {
                        salvarNoLocalStorage(notaAtual, conteudo);
                    }

                    atualizarItemNaLista(notaAtual, data.resumo, data.data);
                    if (!silencioso && window.mostrarModalEco) {
                        // feedback discreto ou toast
                    }
                }
            })
            .catch(function () {
                // Fallback automático para localStorage se o usuário não estiver logado ou rede falhar
                salvarNoLocalStorage(notaAtual, conteudo);
                if (docStatusEl) {
                    docStatusEl.innerHTML = `<span class="text-success"><i class="bi bi-hdd-fill"></i> Salvo neste navegador</span>`;
                }
            });
    }

    function salvarNoLocalStorage(nota, conteudo) {
        if (!nota.id || nota.id <= 0) {
            nota.id = -Date.now();
            nota.ehLocal = true;
        }

        const idx = notasLocais.findIndex(function (n) { return n.id === nota.id; });
        const resumo = (editorCanvas.innerText || '').substring(0, 220);
        const dataStr = new Date().toLocaleDateString('pt-BR') + ' ' + new Date().toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });

        const item = {
            id: nota.id,
            titulo: inputTitulo.value.trim() || 'Minha Anotação de Estudo',
            conteudoHtml: conteudo,
            resumoTexto: resumo,
            artigoSlug: selectArtigo.value,
            artigoTitulo: selectArtigo.value ? selectArtigo.options[selectArtigo.selectedIndex].text : '',
            data: dataStr,
            ehLocal: true
        };

        if (idx >= 0) {
            notasLocais[idx] = item;
        } else {
            notasLocais.unshift(item);
        }

        salvarNotasLocaisStorage();
        renderizarTodasNotas();
    }

    // Criar nova anotação
    function novaNota(artigoSlugOpcional, mudarParaAbaNotas) {
        notaAtual = {
            id: null,
            titulo: 'Nova Anotação de Estudo',
            conteudoHtml: '<p>Comece seu resumo ou notas sobre as espécies aqui...</p>',
            artigoSlug: artigoSlugOpcional || '',
            artigoTitulo: '',
            ehLocal: false
        };

        inputTitulo.value = notaAtual.titulo;
        selectArtigo.value = artigoSlugOpcional || '';
        editorCanvas.innerHTML = notaAtual.conteudoHtml;

        // Se veio de um artigo específico, personaliza o título
        if (artigoSlugOpcional && selectArtigo.selectedIndex >= 0) {
            const opt = selectArtigo.options[selectArtigo.selectedIndex];
            inputTitulo.value = 'Estudo: ' + (opt.getAttribute('data-titulo') || opt.text);
        }

        atualizarContadores();
        if (docStatusEl) {
            docStatusEl.innerHTML = '<span class="text-muted"><i class="bi bi-pencil"></i> Nova anotação em edição</span>';
        }

        // Remove seleção de outros cards
        document.querySelectorAll('.eco-doc-card').forEach(function (c) {
            c.classList.remove('is-active');
        });

        if (mudarParaAbaNotas !== false) {
            ativarAba('notas');
            editorCanvas.focus();
        }
    }

    // Abrir nota selecionada
    function abrirNota(id, ehLocal, mudarParaAbaNotas) {
        if (ehLocal) {
            const item = notasLocais.find(function (n) { return n.id === id; });
            if (item) {
                carregarDadosNoEditor(item, mudarParaAbaNotas);
            }
            return;
        }

        fetch('/Educacao/ObterNota/' + id, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(function (r) {
                if (!r.ok) throw new Error('Não encontrada');
                return r.json();
            })
            .then(function (data) {
                carregarDadosNoEditor(data, mudarParaAbaNotas);
            })
            .catch(function () {
                // Tenta no local
                const item = notasLocais.find(function (n) { return n.id === id; });
                if (item) carregarDadosNoEditor(item, mudarParaAbaNotas);
            });
    }

    function carregarDadosNoEditor(dados, mudarParaAbaNotas) {
        notaAtual = {
            id: dados.id,
            titulo: dados.titulo,
            conteudoHtml: dados.conteudoHtml || '',
            artigoSlug: dados.artigoSlug || '',
            artigoTitulo: dados.artigoTitulo || '',
            ehLocal: !!dados.ehLocal
        };

        inputTitulo.value = dados.titulo || 'Sem título';
        selectArtigo.value = dados.artigoSlug || '';
        editorCanvas.innerHTML = dados.conteudoHtml || '';

        atualizarContadores();
        if (docStatusEl) {
            docStatusEl.innerHTML = `<span class="text-muted"><i class="bi bi-clock"></i> Última alteração: ${dados.data || ''}</span>`;
        }

        document.querySelectorAll('.eco-doc-card').forEach(function (c) {
            c.classList.toggle('is-active', c.getAttribute('data-nota-id') == dados.id);
        });

        if (mudarParaAbaNotas !== false) {
            ativarAba('notas');
        }
    }

    // Excluir nota
    function excluirNotaAtual() {
        if (!notaAtual.id) {
            novaNota();
            return;
        }

        const msg = `Tem certeza que deseja excluir a nota "${notaAtual.titulo || 'sem título'}"? Esta ação não pode ser desfeita.`;

        window.confirmarAcaoEco(msg, function () {
            if (notaAtual.ehLocal) {
                notasLocais = notasLocais.filter(function (n) { return n.id !== notaAtual.id; });
                salvarNotasLocaisStorage();
                removerCardDaLista(notaAtual.id);
                novaNota();
                return;
            }

            fetch('/Educacao/ExcluirNota/' + notaAtual.id, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': CSRF_TOKEN,
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(function (r) { return r.json(); })
                .then(function () {
                    removerCardDaLista(notaAtual.id);
                    novaNota();
                })
                .catch(function () {
                    // Fallback
                    removerCardDaLista(notaAtual.id);
                    novaNota();
                });
        }, 'Excluir Anotação');
    }

    function removerCardDaLista(id) {
        const card = document.querySelector(`.eco-doc-card[data-nota-id="${id}"]`);
        if (card) {
            card.remove();
        }
        atualizarTotalBadges();
    }

    function atualizarItemNaLista(nota, resumo, dataStr) {
        let card = document.querySelector(`.eco-doc-card[data-nota-id="${nota.id}"]`);
        const tagArtigo = nota.artigoTitulo ? `<span class="badge bg-success-subtle text-success border border-success-subtle small me-1"><i class="bi bi-tag-fill me-1"></i>${escapar(nota.artigoTitulo)}</span>` : '<span class="badge bg-light text-muted border small me-1">Estudo Geral</span>';

        const html = `
            <div class="d-flex justify-content-between align-items-start gap-2 mb-1">
                <strong class="eco-doc-card__title text-truncate">${escapar(nota.titulo)}</strong>
            </div>
            <p class="eco-doc-card__snippet small text-muted mb-2">${escapar(resumo || 'Sem conteúdo...')}</p>
            <div class="d-flex align-items-center justify-content-between flex-wrap gap-1">
                <div>${tagArtigo}</div>
                <span class="small text-muted" style="font-size:0.75rem;">${dataStr || 'Hoje'}</span>
            </div>
        `;

        if (card) {
            card.innerHTML = html;
            card.classList.add('is-active');
        } else {
            card = document.createElement('div');
            card.className = 'eco-doc-card is-active';
            card.setAttribute('data-nota-id', nota.id);
            if (nota.ehLocal) card.setAttribute('data-nota-local', 'true');
            card.innerHTML = html;

            card.addEventListener('click', function () {
                abrirNota(nota.id, nota.ehLocal);
            });

            if (listaNotasEl) {
                // Remove placeholder vazio se existir
                const vazio = listaNotasEl.querySelector('.eco-doc-empty');
                if (vazio) vazio.remove();
                listaNotasEl.prepend(card);
            }
        }

        atualizarTotalBadges();
    }

    function renderizarTodasNotas() {
        if (!listaNotasEl) return;
        // Mescla notas locais que não estejam no servidor
        notasLocais.forEach(function (n) {
            const existe = document.querySelector(`.eco-doc-card[data-nota-id="${n.id}"]`);
            if (!existe) {
                const card = document.createElement('div');
                card.className = 'eco-doc-card';
                card.setAttribute('data-nota-id', n.id);
                card.setAttribute('data-nota-local', 'true');
                const tagArtigo = n.artigoTitulo ? `<span class="badge bg-success-subtle text-success border border-success-subtle small me-1"><i class="bi bi-tag-fill me-1"></i>${escapar(n.artigoTitulo)}</span>` : '<span class="badge bg-light text-muted border small me-1">Estudo Geral</span>';
                card.innerHTML = `
                    <div class="d-flex justify-content-between align-items-start gap-2 mb-1">
                        <strong class="eco-doc-card__title text-truncate">${escapar(n.titulo)}</strong>
                    </div>
                    <p class="eco-doc-card__snippet small text-muted mb-2">${escapar(n.resumoTexto || 'Sem conteúdo...')}</p>
                    <div class="d-flex align-items-center justify-content-between flex-wrap gap-1">
                        <div>${tagArtigo}</div>
                        <span class="small text-muted" style="font-size:0.75rem;">${n.data || 'Local'}</span>
                    </div>
                `;
                card.addEventListener('click', function () {
                    abrirNota(n.id, true);
                });
                listaNotasEl.appendChild(card);
            }
        });
        atualizarTotalBadges();
    }

    function atualizarTotalBadges() {
        const total = document.querySelectorAll('.eco-doc-card').length;
        if (badgeTotalNotas) badgeTotalNotas.textContent = total;
        if (badgeSidebarTotal) badgeSidebarTotal.textContent = total;
    }

    function escapar(str) {
        if (!str) return '';
        return String(str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    // Busca nas anotações
    if (buscaNotasInput) {
        buscaNotasInput.addEventListener('input', function () {
            const termo = this.value.toLowerCase().trim();
            const cards = Array.from(document.querySelectorAll('.eco-doc-card'));
            cards.forEach(function (c) {
                const texto = c.innerText.toLowerCase();
                c.style.display = termo === '' || texto.includes(termo) ? 'block' : 'none';
            });
        });
    }

    // Ações de Botões
    if (btnNovaNota) {
        btnNovaNota.addEventListener('click', function () {
            novaNota();
        });
    }

    if (btnSalvarNota) {
        btnSalvarNota.addEventListener('click', function () {
            salvarNotaAtual(false);
        });
    }

    if (btnExcluirNota) {
        btnExcluirNota.addEventListener('click', function () {
            excluirNotaAtual();
        });
    }

    // Copiar Texto
    if (btnCopiarDoc) {
        btnCopiarDoc.addEventListener('click', function () {
            const texto = (inputTitulo.value ? inputTitulo.value + '\n\n' : '') + (editorCanvas.innerText || '');
            navigator.clipboard.writeText(texto).then(function () {
                window.mostrarModalEco('Texto Copiado', 'O conteúdo da sua anotação foi copiado para a área de transferência com sucesso!', 'sucesso');
            });
        });
    }

    // Imprimir / Exportar Documento
    if (btnExportarDoc) {
        btnExportarDoc.addEventListener('click', function () {
            window.print();
        });
    }

    // Clique nos cards existentes renderizados pelo servidor
    document.querySelectorAll('.eco-doc-card').forEach(function (card) {
        card.addEventListener('click', function () {
            const id = parseInt(card.getAttribute('data-nota-id'), 10);
            const ehLocal = card.getAttribute('data-nota-local') === 'true';
            abrirNota(id, ehLocal);
        });
    });

    // Inicialização
    carregarNotasLocais();
    renderizarTodasNotas();

    // Checar URL e hash
    const params = new URLSearchParams(window.location.search);
    const abaParam = params.get('aba');
    const artigoParam = params.get('artigo');
    const hash = window.location.hash.replace('#', '');

    if (abaParam === 'notas' || hash === 'notas' || artigoParam) {
        ativarAba('notas');
        if (artigoParam) {
            novaNota(artigoParam);
        } else {
            // Se houver uma primeira nota, abre ela; senão inicia uma nova
            const primeiroCard = document.querySelector('.eco-doc-card');
            if (primeiroCard) {
                primeiroCard.click();
            } else {
                novaNota();
            }
        }
    } else {
        ativarAba('estudos');
        // Se houver nota, pré-seleciona a primeira no editor sem mudar de aba
        const primeiroCard = document.querySelector('.eco-doc-card');
        if (primeiroCard) {
            const id = parseInt(primeiroCard.getAttribute('data-nota-id'), 10);
            const ehLocal = primeiroCard.getAttribute('data-nota-local') === 'true';
            abrirNota(id, ehLocal, false);
        } else {
            novaNota(null, false);
        }
    }

    atualizarContadores();
})();
