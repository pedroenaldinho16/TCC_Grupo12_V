(() => {
    'use strict';
    const motion = window.matchMedia('(prefers-reduced-motion: reduce)');
    let saving = false, selected = null, drag = null, scrollFrame = 0, suppressedSource = null;
    const live = document.createElement('div');
    live.className = 'visually-hidden'; live.setAttribute('role', 'status');
    live.setAttribute('aria-live', 'polite'); document.body.append(live);
    const animate = (el, frames, duration = 220) => {
        if (!el || !motionAllowed() || !el.animate) return Promise.resolve();
        return el.animate(frames, { duration, easing: 'ease-in-out' }).finished.catch(() => {});
    };
    function tell(message) {
        const status = document.getElementById('jogo-feedback');
        if (status) status.textContent = message;
        live.textContent = message;
    }
    function clearSelection() {
        document.querySelectorAll('.is-drag-selected,.ec-drop-ready,.ec-drop-hover').forEach(el => {
            el.classList.remove('is-drag-selected', 'ec-drop-ready', 'ec-drop-hover');
            if (el.hasAttribute('data-drag-source')) el.setAttribute('aria-pressed', 'false');
        });
        selected = null;
    }
    const accepts = (target, item) => target && !target.matches(':disabled') && (target.dataset.accept || '').split(' ').includes(item);
    function highlight(item) {
        document.querySelectorAll('[data-drop-target]').forEach(el => el.classList.toggle('ec-drop-ready', accepts(el, item)));
    }
    async function pairEffect(region, photoKey, nameId, success) {
        const photo = [...region.querySelectorAll('.ec-pair-photo')].find(b => b.dataset.focusKey === photoKey);
        const name = [...region.querySelectorAll('.ec-pair-name')].find(b => b.dataset.focusKey === 'nome-' + nameId);
        if (!success) {
            await animate(name, [{ transform: 'translateX(0)' }, { transform: 'translateX(-6px)' }, { transform: 'translateX(6px)' }, { transform: 'translateX(0)' }], 260);
            return;
        }
        photo?.classList.add('is-matched'); name?.classList.add('is-matched');
        const board = region.querySelector('.ec-pair-board');
        if (!photo || !name || !board || !motionAllowed()) return;
        const r = board.getBoundingClientRect(), a = photo.getBoundingClientRect(), b = name.getBoundingClientRect();
        const x1 = a.right - r.left - 8, y1 = a.top + a.height / 2 - r.top;
        const x2 = b.left - r.left + 8, y2 = b.top + b.height / 2 - r.top;
        const middle = (x1 + x2) / 2;
        const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        svg.classList.add('ec-pair-link'); svg.setAttribute('aria-hidden', 'true');
        svg.setAttribute('viewBox', '0 0 ' + r.width + ' ' + r.height);
        const path = document.createElementNS(svg.namespaceURI, 'path');
        path.setAttribute('d', 'M' + x1 + ' ' + y1 + ' C' + middle + ' ' + y1 + ' ' + middle + ' ' + y2 + ' ' + x2 + ' ' + y2);
        svg.append(path); board.append(svg);
        const length = path.getTotalLength();
        path.style.strokeDasharray = length; path.style.strokeDashoffset = length;
        await Promise.all([
            animate(path, [{ strokeDashoffset: length }, { strokeDashoffset: 0 }], 430),
            animate(name, [{ transform: 'scale(.98)' }, { transform: 'scale(1.025)' }, { transform: 'scale(1)' }], 380),
            animate(photo, [{ transform: 'scale(.98)' }, { transform: 'scale(1.02)' }, { transform: 'scale(1)' }], 380)
        ]);
        path.style.strokeDashoffset = 0;
        await animate(svg, [{ opacity: 1 }, { opacity: 0 }], 220);
        svg.remove();
    }
    const tasks = [], pendingMoves = new Map(), releaseWaiters = [];
    let paused = false, navigating = false, visualOpen = document.querySelector('.ec-flashcard.is-flipped')?.dataset.carta || null;
    let motionChoice = null;
    try { motionChoice = localStorage.getItem('ecocanta.gameMotion'); } catch {}
    function motionAllowed() { return motionChoice === 'on' || (motionChoice !== 'off' && !motion.matches); }
    function updateMotion() {
        const enabled = motionAllowed();
        document.documentElement.dataset.gameMotion = enabled ? 'on' : 'off';
        document.querySelectorAll('[data-game-motion-toggle]').forEach(button => {
            button.hidden = false;
            button.setAttribute('aria-pressed', String(enabled));
            button.textContent = enabled ? 'Animações: ligadas' : 'Animações: reduzidas';
            button.title = enabled ? 'Reduzir movimento nas atividades' : 'Ativar animações nas atividades';
        });
    }
    updateMotion(); motion.addEventListener('change', updateMotion);
    document.addEventListener('click', event => {
        if (!event.target.closest('[data-game-motion-toggle]')) return;
        motionChoice = motionAllowed() ? 'off' : 'on';
        try { localStorage.setItem('ecocanta.gameMotion', motionChoice); } catch {}
        updateMotion();
    });
    function showSaving() {
        const root = document.getElementById('atividade');
        if (!root) return;
        if (saving) root.setAttribute('aria-busy', 'true'); else root.removeAttribute('aria-busy');
        root.classList.toggle('is-saving', saving);
        const badge = root.querySelector('[data-save-state]');
        if (badge) { badge.hidden = !saving; badge.textContent = 'Salvando' + (tasks.length ? ' (' + (tasks.length + 1) + ' ações)' : '') + '…'; }
        root.querySelectorAll('[data-drag-source]').forEach(source => source.classList.toggle('is-pending-move', pendingMoves.has(source.dataset.dragSource)));
    }
    function showFaces(root) {
        root.querySelectorAll('.ec-flashcard').forEach(card => {
            const opened = card.dataset.carta === visualOpen;
            card.classList.toggle('is-flipped', opened);
            card.querySelector('.ec-flash-turn')?.setAttribute('aria-expanded', String(opened));
            card.querySelector('.ec-flash-face-front')?.setAttribute('aria-hidden', String(opened));
            card.querySelector('.ec-flash-back')?.setAttribute('aria-hidden', String(!opened));
        });
    }
    function flip(button) {
        const card = button.closest('.ec-flashcard');
        visualOpen = visualOpen === card.dataset.carta ? null : card.dataset.carta;
        showFaces(document.getElementById('atividade'));
        return visualOpen === card.dataset.carta;
    }
    function studyMetadata(result) {
        const root = document.getElementById('atividade');
        root.querySelectorAll('[name=revisao]').forEach(input => input.value = result.revisao);
        const ids = new Set(result.cartasEstudadas.map(String));
        root.querySelectorAll('.ec-flashcard').forEach(card => {
            const studied = ids.has(card.dataset.carta);
            card.dataset.studied = String(studied);
            const badge = card.querySelector('[data-card-seen]');
            if (badge) badge.textContent = studied ? '· Explorada ✓' : '';
        });
        const count = root.querySelector('[data-study-count]');
        const total = root.querySelectorAll('.ec-flashcard').length;
        if (count) count.textContent = ids.size + ' de ' + total + ' cartas exploradas. Toque em cada carta para ver o verso.';
        const start = root.querySelector('input[name=acao][value=associar]')?.form.querySelector('button');
        if (start) start.disabled = ids.size < total;
        tell(result.mensagem);
    }
    const waitForPointer = () => drag ? new Promise(resolve => releaseWaiters.push(resolve)) : Promise.resolve();
    async function runTask(task) {
        let region = document.getElementById('atividade');
        const current = region.querySelector('form[data-game-form]');
        if (!current) return;
        const data = new FormData(current);
        Object.entries(task.fields).forEach(([key, value]) => data.set(key, value));
        data.set('revisao', region.querySelector('[name=revisao]').value);
        const action = data.get('acao'), id = data.get('alvo');
        const photoKey = region.querySelector('.ec-pair-photo[aria-pressed="true"]')?.dataset.focusKey;
        const oldStage = region.querySelector('[data-scene-stage]')?.dataset.sceneStage;
        const references = [...region.querySelectorAll('details[open]')].map(d => d.className);
        const response = await fetch(current.action, { method: 'POST', body: data, credentials: 'same-origin', headers: { Accept: 'application/json' } });
        if (response.redirected) { navigating = true; location.assign(response.url); return; }
        if (!response.ok && response.status !== 409) throw new Error('save');
        const result = await response.json();
        if (Array.isArray(result.cartasEstudadas)) {
            studyMetadata(result);
            return; // Mantém os dois lados da carta e a transição CSS no mesmo elemento.
        }
        if (result.concluida) {
            if (action === 'nome') await pairEffect(region, photoKey, id, true);
            navigating = true; location.assign(result.url); return;
        }
        const page = await fetch(result.url, { credentials: 'same-origin', cache: 'no-store' });
        if (!page.ok) throw new Error('load');
        const doc = new DOMParser().parseFromString(await page.text(), 'text/html');
        const next = doc.getElementById('atividade');
        if (!next) { navigating = true; location.assign(page.url); return; }
        // Não retira do DOM um elemento que o usuário está segurando.
        await waitForPointer();
        region = document.getElementById('atividade');
        next.setAttribute('aria-busy', 'true');
        region.replaceWith(next);
        next.querySelectorAll('details').forEach(d => { if (references.includes(d.className)) d.open = true; });
        if (action === 'estudar' || tasks.some(t => t.fields.acao === 'estudar')) showFaces(next);
        else visualOpen = next.querySelector('.ec-flashcard.is-flipped')?.dataset.carta || null;
        showSaving();
        const status = next.querySelector('#jogo-feedback');
        if (response.status === 409) {
            // A intenção é explícita (abrir/fechar ou destinar um item), permitindo rebase seguro.
            if ((action === 'estudar' || action === 'mover') && task.retries++ < 2) {
                await runTask(task);
                return;
            }
            status.textContent = 'O progresso foi atualizado em outra ação. Confira a etapa atual e tente novamente.';
        }
        live.textContent = status.textContent;
        const animations = [];
        if (action === 'nome' && response.status !== 409) {
            const matched = [...next.querySelectorAll('.ec-pair-name')].find(b => b.dataset.focusKey === 'nome-' + id)?.classList.contains('is-matched');
            animations.push(pairEffect(next, photoKey, id, matched));
        }
        if (action === 'foto') animations.push(animate(next.querySelector('.ec-pair-photo[aria-pressed="true"]'), [{ transform: 'scale(.97)' }, { transform: 'scale(1)' }]));
        if (oldStage !== next.querySelector('[data-scene-stage]')?.dataset.sceneStage) {
            animations.push(animate(next.querySelector('.ec-mission-brief'), [{ opacity: 0, transform: 'translateY(8px)' }, { opacity: 1, transform: 'translateY(0)' }], 240));
        }
        await Promise.all(animations);
        if (!tasks.length) {
            const target = [...next.querySelectorAll('[data-focus-key]')].find(b => b.dataset.focusKey === task.focusKey && !b.matches(':disabled'));
            (target || status)?.focus({ preventScroll: true });
        }
    }
    async function drain() {
        if (saving || paused) return;
        saving = true; showSaving();
        try {
            while (tasks.length && !navigating) {
                const task = tasks.shift();
                showSaving();
                try { await runTask(task); task.resolve(true); }
                catch (error) { task.resolve(false); throw error; }
                finally { if (task.fields.acao === 'mover') pendingMoves.delete(task.fields.alvo); showSaving(); }
            }
        } catch {
            paused = true; cancelDrag(); clearSelection();
            tasks.splice(0).forEach(task => task.resolve(false)); pendingMoves.clear();
            tell('Não foi possível confirmar o salvamento. Recarregue a atividade para recuperar o progresso do servidor.');
            const retry = document.createElement('button');
            retry.type = 'button'; retry.className = 'ec-button ec-button-outline'; retry.textContent = 'Recuperar atividade';
            retry.addEventListener('click', () => location.reload());
            document.getElementById('jogo-feedback')?.append(' ', retry);
        } finally {
            saving = false; showSaving();
            if (navigating) tasks.splice(0).forEach(task => task.resolve(true));
        }
    }
    function save(form, submitter, extra = {}) {
        if (!form || paused) return Promise.resolve(false);
        const fields = Object.fromEntries(new FormData(form));
        delete fields.revisao; delete fields.__RequestVerificationToken;
        if (submitter?.name) fields[submitter.name] = submitter.value;
        Object.assign(fields, extra);
        if (fields.acao === 'mover' && pendingMoves.has(fields.alvo)) return pendingMoves.get(fields.alvo).promise;
        let resolve;
        const promise = new Promise(done => resolve = done);
        const task = { fields, focusKey: submitter?.dataset.focusKey, retries: 0, resolve, promise };
        if (fields.acao === 'mover') pendingMoves.set(fields.alvo, task);
        tasks.push(task); showSaving(); drain();
        return promise;
    }
    document.addEventListener('submit', event => {
        const form = event.target.closest('form[data-game-form]');
        if (!form || !window.fetch || !event.submitter) return;
        event.preventDefault();
        if (paused) return;
        const extra = event.submitter.matches('.ec-flash-turn') ? { aberta: String(flip(event.submitter)) } : {};
        save(form, event.submitter, extra);
    });
    function move(source, target) {
        return save(source.closest('form[data-game-form]'), source, { acao: 'mover', alvo: source.dataset.dragSource, destino: target.dataset.dropTarget });
    }
    document.addEventListener('click', event => {
        const source = event.target.closest('[data-drag-source]');
        const target = event.target.closest('[data-drop-target]');
        if (!source && !target) return;
        if (paused || (event.detail && source === suppressedSource)) { event.preventDefault(); return; }
        if (source && !source.matches(':disabled') && !pendingMoves.has(source.dataset.dragSource)) {
            event.preventDefault();
            const same = selected === source; clearSelection();
            if (same) { tell('Seleção cancelada. Você pode arrastar ou selecionar outro item.'); return; }
            selected = source; source.classList.add('is-drag-selected'); source.setAttribute('aria-pressed', 'true');
            highlight(source.dataset.dragSource);
            tell('Objeto selecionado. Agora ative o destino destacado, ou pressione Escape para cancelar.');
        } else if (target && selected) {
            event.preventDefault();
            if (accepts(target, selected.dataset.dragSource)) { const item = selected; clearSelection(); move(item, target); }
            else tell('Esse não é o destino deste item. Procure o ponto destacado.');
        } else if (source) event.preventDefault();
    });
    function dropAt(x, y, item) {
        const direct = document.elementFromPoint(x, y)?.closest('[data-drop-target]');
        if (accepts(direct, item)) return direct;
        const targets = [...document.querySelectorAll('[data-drop-target]')].filter(t => accepts(t, item));
        let best = null, distance = Infinity;
        for (const target of targets) {
            const r = target.getBoundingClientRect(), padding = 18;
            // Considera o ponto de soltura e a parte central da figura arrastada, com margem nas bordas.
            const near = x >= r.left - padding && x <= r.right + padding && y >= r.top - padding && y <= r.bottom + padding;
            const overlapX = Math.max(0, Math.min(x + 38, r.right) - Math.max(x - 38, r.left));
            const overlapY = Math.max(0, Math.min(y + 42, r.bottom) - Math.max(y - 42, r.top));
            if (!near && overlapX * overlapY < 76 * 84 * .25) continue;
            const delta = Math.hypot(x - (r.left + r.width / 2), y - (r.top + r.height / 2));
            if (delta < distance) { best = target; distance = delta; }
        }
        return best;
    }
    function updateDrop() {
        if (!drag?.active) return;
        const target = dropAt(drag.x, drag.y, drag.source.dataset.dragSource);
        document.querySelectorAll('.ec-drop-hover').forEach(el => el.classList.remove('ec-drop-hover'));
        target?.classList.add('ec-drop-hover');
    }
    function autoScroll() {
        if (!drag?.active) return;
        const amount = drag.y < 55 ? -10 : drag.y > innerHeight - 55 ? 10 : 0;
        if (amount) { window.scrollBy(0, amount); updateDrop(); }
        scrollFrame = requestAnimationFrame(autoScroll);
    }
    function cleanDrag() {
        if (!drag) return null;
        const old = drag; drag = null; cancelAnimationFrame(scrollFrame);
        document.body.classList.remove('ec-is-dragging'); old.source.classList.remove('ec-drag-origin');
        document.querySelectorAll('.ec-drop-ready,.ec-drop-hover').forEach(el => el.classList.remove('ec-drop-ready', 'ec-drop-hover'));
        try { if (old.source.hasPointerCapture(old.pointerId)) old.source.releasePointerCapture(old.pointerId); } catch {}
        releaseWaiters.splice(0).forEach(resolve => resolve());
        return old;
    }
    async function cancelDrag() {
        const old = cleanDrag();
        if (!old?.ghost) return;
        const r = old.source.getBoundingClientRect();
        await animate(old.ghost, [{ transform: 'translate(0,0)' }, { transform: 'translate(' + (r.left + r.width / 2 - old.x) + 'px,' + (r.top + r.height / 2 - old.y) + 'px)', opacity: 0 }], 160);
        old.ghost.remove();
    }
    document.addEventListener('pointerdown', event => {
        suppressedSource = null; // Um novo gesto real nunca é confundido com o clique sintético do anterior.
        const source = event.target.closest('[data-drag-source]');
        if (!source || source.matches(':disabled') || pendingMoves.has(source.dataset.dragSource) || paused || event.button !== 0 || event.isPrimary === false || drag) return;
        clearSelection();
        drag = { source, pointerId: event.pointerId, startX: event.clientX, startY: event.clientY, x: event.clientX, y: event.clientY, active: false };
        try { source.setPointerCapture(event.pointerId); } catch { cleanDrag(); }
    });
    document.addEventListener('pointermove', event => {
        if (!drag || event.pointerId !== drag.pointerId) return;
        drag.x = event.clientX; drag.y = event.clientY;
        if (!drag.active && Math.hypot(drag.x - drag.startX, drag.y - drag.startY) < 5) return;
        event.preventDefault();
        if (!drag.active) {
            drag.active = true;
            const ghost = drag.source.cloneNode(true);
            ghost.querySelectorAll('[id]').forEach(el => el.removeAttribute('id'));
            ghost.removeAttribute('data-focus-key'); ghost.removeAttribute('data-drag-source');
            ghost.classList.add('ec-drag-ghost'); ghost.setAttribute('aria-hidden', 'true'); ghost.tabIndex = -1;
            ghost.style.width = '96px'; ghost.style.height = '112px';
            document.body.append(ghost); drag.ghost = ghost;
            drag.source.classList.add('ec-drag-origin'); document.body.classList.add('ec-is-dragging');
            highlight(drag.source.dataset.dragSource); autoScroll();
        }
        drag.ghost.style.left = (drag.x - 48) + 'px'; drag.ghost.style.top = (drag.y - 56) + 'px';
        updateDrop();
    }, { passive: false });
    document.addEventListener('pointerup', async event => {
        if (!drag || event.pointerId !== drag.pointerId) return;
        if (!drag.active) { cleanDrag(); return; }
        event.preventDefault();
        suppressedSource = drag.source;
        const target = dropAt(event.clientX, event.clientY, drag.source.dataset.dragSource);
        if (!target) { tell('Leve o item ao destino destacado. Você pode tentar novamente.'); await cancelDrag(); return; }
        const old = cleanDrag(), r = target.getBoundingClientRect();
        const fly = animate(old.ghost, [{ transform: 'translate(0,0) scale(1)', opacity: 1 }, {
            transform: 'translate(' + (r.left + r.width / 2 - old.x) + 'px,' + (r.top + r.height / 2 - old.y) + 'px) scale(.2)', opacity: 0
        }], 150);
        const saved = move(old.source, target);
        await fly; old.ghost.remove(); await saved;
    });
    document.addEventListener('pointercancel', event => { if (drag?.pointerId === event.pointerId) { cancelDrag(); clearSelection(); } });
    document.addEventListener('lostpointercapture', event => { if (drag?.pointerId === event.pointerId) cancelDrag(); });
    window.addEventListener('blur', () => { cancelDrag(); clearSelection(); });
    document.addEventListener('visibilitychange', () => { if (document.hidden) { cancelDrag(); clearSelection(); } });
    document.addEventListener('keydown', event => {
        if (event.key !== 'Escape' || (!drag && !selected)) return;
        event.preventDefault(); event.stopImmediatePropagation();
        const source = drag?.source || selected;
        cancelDrag(); clearSelection(); tell('Ação cancelada. Seu progresso continua salvo.');
        source?.focus({ preventScroll: true });
    }, true);
})();
