(function () {
    'use strict';

    var canvas = document.getElementById('editorCanvas');
    var form = document.getElementById('nivelForm');
    var campoJson = document.getElementById('nivelDadosJson');
    var toolbar = document.getElementById('editorToolbar');
    var selectEspecie = document.getElementById('editorEspecie');
    var selectVelocidade = document.getElementById('editorVelocidade');
    var inputLargura = document.getElementById('editorLargura');
    var inputAltura = document.getElementById('editorAltura');
    var lblRotacao = document.getElementById('lblRotacao');
    var btnRotacionar = document.getElementById('btnRotacionar');

    if (!canvas || !form || !campoJson) { return; }

    var ctx = canvas.getContext('2d');
    var TILE = 32;
    var LARG_MIN = 40;
    var LARG_MAX = 400;
    var ALT_MIN = 12;
    var ALT_MAX = 100;

    var largura = 120;
    var altura = 17;

    // Grades em camadas: armazenam false ou a rotação ('cima'|'direita'|'baixo'|'esquerda'|'todos')
    var terra = [];
    var plataforma = [];
    var oneway = [];
    var mola = [];
    var madeira = [];
    var pedra = [];
    var perigo = []; // null ou { tipo: 'fogo'|'agua'|'espinho', dir: 'cima'|'direita'|'baixo'|'esquerda'|'todos' }

    // Listas de entidades
    var plataformasMoveis = []; // { x1, y1, x2, y2, largura: 2, tipo: 'plataforma', velocidade: 1.0 }
    var perigosMoveis = [];     // { x1, y1, x2, y2, tipo: 'espinho', direcao: 'cima', velocidade: 1.0 }
    var itens = [];             // { x, y, tipo: 'dash'|'espeto'|'pulo_duplo'|'cura' }
    var checkpoints = [];       // { x, y }
    var animais = [];           // { x, y, especieId }
    var inicio = null;          // { x, y }
    var objetivo = null;        // { x, y }

    // Rotação & Ferramentas
    var direcoes = ['cima', 'direita', 'baixo', 'esquerda', 'todos'];
    var rotIndex = 0;
    var rotacao = direcoes[rotIndex];

    var ferramenta = 'terra';
    var desenhando = false;
    var valorPincel = true; // true = pinta, false = apaga
    var hover = null;
    var pontoInicialMovel = null; // Para definir começo e parada

    function dentro(x, y) {
        return x >= 0 && x < largura && y >= 0 && y < altura;
    }

    function novasLinhas() {
        terra = []; plataforma = []; oneway = []; mola = []; madeira = []; pedra = []; perigo = [];
        for (var y = 0; y < altura; y++) {
            terra.push(new Array(largura).fill(false));
            plataforma.push(new Array(largura).fill(false));
            oneway.push(new Array(largura).fill(false));
            mola.push(new Array(largura).fill(false));
            madeira.push(new Array(largura).fill(false));
            pedra.push(new Array(largura).fill(false));
            perigo.push(new Array(largura).fill(null));
        }
    }

    function atualizarRotacaoUI() {
        if (!lblRotacao) { return; }
        var nomes = {
            'cima': 'Cima ⬆️',
            'direita': 'Direita ➡️',
            'baixo': 'Baixo ⬇️',
            'esquerda': 'Esquerda ⬅️',
            'todos': '4 Lados 🔄'
        };
        lblRotacao.textContent = nomes[rotacao] || rotacao;
    }

    function rotacionar() {
        rotIndex = (rotIndex + 1) % direcoes.length;
        rotacao = direcoes[rotIndex];
        atualizarRotacaoUI();

        // Se mouse estiver sobre QUALQUER bloco existente, rotaciona o bloco colocado
        if (hover && dentro(hover.x, hover.y)) {
            var hx = hover.x, hy = hover.y;
            if (terra[hy][hx]) { terra[hy][hx] = rotacao; }
            else if (madeira[hy][hx]) { madeira[hy][hx] = rotacao; }
            else if (pedra[hy][hx]) { pedra[hy][hx] = rotacao; }
            else if (plataforma[hy][hx]) { plataforma[hy][hx] = rotacao; }
            else if (oneway[hy][hx]) { oneway[hy][hx] = rotacao; }
            else if (mola[hy][hx]) { mola[hy][hx] = rotacao; }
            else if (perigo[hy][hx]) {
                perigo[hy][hx].dir = rotacao;
            }
            perigosMoveis.forEach(function (pm) {
                if ((pm.x1 === hx && pm.y1 === hy) || (pm.x2 === hx && pm.y2 === hy)) {
                    pm.direcao = rotacao;
                    pm.dir = rotacao;
                }
            });
        }
        desenhar();
    }

    if (btnRotacionar) {
        btnRotacionar.addEventListener('click', function () {
            rotacionar();
        });
    }

    window.addEventListener('keydown', function (e) {
        if (e.target && (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.tagName === 'SELECT')) {
            return;
        }
        if (e.key === 'r' || e.key === 'R') {
            e.preventDefault();
            rotacionar();
        } else if (e.key === 'Escape') {
            pontoInicialMovel = null;
            desenhar();
        }
    });

    function redimensionar(novaLargura, novaAltura, preservar) {
        novaLargura = Math.max(LARG_MIN, Math.min(LARG_MAX, novaLargura | 0));
        novaAltura = Math.max(ALT_MIN, Math.min(ALT_MAX, novaAltura | 0));

        var antiga = {
            terra: terra, plataforma: plataforma, oneway: oneway, mola: mola,
            madeira: madeira, pedra: pedra, perigo: perigo,
            largura: largura, altura: altura
        };

        largura = novaLargura;
        altura = novaAltura;
        novasLinhas();

        if (preservar) {
            var my = Math.min(altura, antiga.altura);
            var mx = Math.min(largura, antiga.largura);
            for (var y = 0; y < my; y++) {
                for (var x = 0; x < mx; x++) {
                    terra[y][x] = antiga.terra[y] ? antiga.terra[y][x] : false;
                    plataforma[y][x] = antiga.plataforma[y] ? antiga.plataforma[y][x] : false;
                    oneway[y][x] = antiga.oneway[y] ? antiga.oneway[y][x] : false;
                    mola[y][x] = antiga.mola[y] ? antiga.mola[y][x] : false;
                    madeira[y][x] = antiga.madeira[y] ? antiga.madeira[y][x] : false;
                    pedra[y][x] = antiga.pedra[y] ? antiga.pedra[y][x] : false;
                    perigo[y][x] = antiga.perigo[y] ? antiga.perigo[y][x] : null;
                }
            }
            animais = animais.filter(function (a) { return a.x < largura && a.y < altura; });
            itens = itens.filter(function (i) { return i.x < largura && i.y < altura; });
            checkpoints = checkpoints.filter(function (c) { return c.x < largura && c.y < altura; });
            plataformasMoveis = plataformasMoveis.filter(function (p) {
                return p.x1 < largura && p.y1 < altura && p.x2 < largura && p.y2 < altura;
            });
            perigosMoveis = perigosMoveis.filter(function (p) {
                return p.x1 < largura && p.y1 < altura && p.x2 < largura && p.y2 < altura;
            });
            if (inicio && (inicio.x >= largura || inicio.y >= altura)) { inicio = null; }
            if (objetivo && (objetivo.x >= largura || objetivo.y >= altura)) { objetivo = null; }
        }

        canvas.width = largura * TILE;
        canvas.height = altura * TILE;
        if (inputLargura) { inputLargura.value = String(largura); }
        if (inputAltura) { inputAltura.value = String(altura); }
        desenhar();
    }

    // ---- Carga do nível existente -----------------------------------------
    function carregar() {
        var raw = campoJson.value;
        novasLinhas();
        if (raw) {
            try {
                var d = JSON.parse(raw);
                largura = Math.max(LARG_MIN, Math.min(LARG_MAX, (d.largura || 120) | 0));
                altura = Math.max(ALT_MIN, Math.min(ALT_MAX, (d.altura || 17) | 0));
                novasLinhas();

                (d.plataformas || []).forEach(function (p) {
                    var tipo = p.tipo || 'terra';
                    var rot = p.rot || 'cima';
                    for (var y = p.y; y < p.y + (p.h || 1); y++) {
                        for (var x = p.x; x < p.x + (p.w || 1); x++) {
                            if (dentro(x, y)) {
                                if (tipo === 'plataforma') { plataforma[y][x] = rot; }
                                else if (tipo === 'oneway') { oneway[y][x] = rot; }
                                else if (tipo === 'mola') { mola[y][x] = rot; }
                                else if (tipo === 'madeira') { madeira[y][x] = rot; }
                                else if (tipo === 'pedra') { pedra[y][x] = rot; }
                                else { terra[y][x] = rot; }
                            }
                        }
                    }
                });

                (d.perigos || []).forEach(function (p) {
                    var tipo = p.tipo || 'fogo';
                    var dir = p.direcao || p.dir || 'cima';
                    for (var y = p.y; y < p.y + (p.h || 1); y++) {
                        for (var x = p.x; x < p.x + (p.w || 1); x++) {
                            if (dentro(x, y)) {
                                perigo[y][x] = { tipo: tipo, dir: dir };
                            }
                        }
                    }
                });

                plataformasMoveis = (d.plataformasMoveis || []).map(function (pm) {
                    return {
                        x1: pm.x1, y1: pm.y1,
                        x2: pm.x2, y2: pm.y2,
                        largura: pm.largura || 2,
                        tipo: pm.tipo || 'plataforma',
                        velocidade: pm.velocidade || 1.0
                    };
                });

                perigosMoveis = (d.perigosMoveis || []).map(function (pm) {
                    return {
                        x1: pm.x1, y1: pm.y1,
                        x2: pm.x2, y2: pm.y2,
                        tipo: pm.tipo || 'espinho',
                        direcao: pm.direcao || pm.dir || 'cima',
                        dir: pm.direcao || pm.dir || 'cima',
                        velocidade: pm.velocidade || 1.0
                    };
                });

                itens = (d.itens || []).map(function (it) {
                    return { x: it.x, y: it.y, tipo: it.tipo || 'dash' };
                });

                checkpoints = (d.checkpoints || []).map(function (cp) {
                    return { x: cp.x, y: cp.y };
                });

                animais = (d.animais || [])
                    .filter(function (a) { return dentro(a.x, a.y); })
                    .map(function (a) { return { x: a.x, y: a.y, especieId: a.especieId }; });

                inicio = d.inicio ? { x: d.inicio.x, y: d.inicio.y } : null;
                objetivo = d.objetivo ? { x: d.objetivo.x, y: d.objetivo.y } : null;
            } catch (e) { /* mantém novo */ }
        }

        canvas.width = largura * TILE;
        canvas.height = altura * TILE;
        if (inputLargura) { inputLargura.value = String(largura); }
        if (inputAltura) { inputAltura.value = String(altura); }
        atualizarRotacaoUI();
        desenhar();
    }

    // ---- Serialização ------------------------------------------------------
    function runs(mapa, tipo) {
        var out = [];
        for (var y = 0; y < altura; y++) {
            var x = 0;
            while (x < largura) {
                var c = mapa[y][x];
                if (c) {
                    var rot = typeof c === 'string' ? c : 'cima';
                    var ini = x;
                    while (x < largura && mapa[y][x]) {
                        var nextRot = typeof mapa[y][x] === 'string' ? mapa[y][x] : 'cima';
                        if (nextRot === rot) {
                            x++;
                        } else {
                            break;
                        }
                    }
                    out.push({ x: ini, y: y, w: x - ini, h: 1, tipo: tipo, rot: rot });
                } else {
                    x++;
                }
            }
        }
        return out;
    }

    function runsPerigos() {
        var out = [];
        for (var y = 0; y < altura; y++) {
            var x = 0;
            while (x < largura) {
                var c = perigo[y][x];
                if (c) {
                    var tipo = typeof c === 'string' ? c : c.tipo;
                    var dir = (typeof c === 'object' && (c.dir || c.direcao)) ? (c.dir || c.direcao) : 'cima';
                    var ini = x;
                    while (x < largura && perigo[y][x]) {
                        var cNext = perigo[y][x];
                        var tNext = typeof cNext === 'string' ? cNext : cNext.tipo;
                        var dNext = (typeof cNext === 'object' && (cNext.dir || cNext.direcao)) ? (cNext.dir || cNext.direcao) : 'cima';
                        if (tNext === tipo && dNext === dir) {
                            x++;
                        } else {
                            break;
                        }
                    }
                    out.push({ x: ini, y: y, w: x - ini, h: 1, tipo: tipo, direcao: dir });
                } else {
                    x++;
                }
            }
        }
        return out;
    }

    function serializar() {
        var plataformas = runs(terra, 'terra')
            .concat(runs(plataforma, 'plataforma'))
            .concat(runs(oneway, 'oneway'))
            .concat(runs(mola, 'mola'))
            .concat(runs(madeira, 'madeira'))
            .concat(runs(pedra, 'pedra'));

        return JSON.stringify({
            largura: largura,
            altura: altura,
            plataformas: plataformas,
            perigos: runsPerigos(),
            plataformasMoveis: plataformasMoveis.map(function (pm) {
                return {
                    x1: pm.x1, y1: pm.y1,
                    x2: pm.x2, y2: pm.y2,
                    largura: pm.largura || 2,
                    tipo: pm.tipo || 'plataforma',
                    velocidade: pm.velocidade || 1.0
                };
            }),
            perigosMoveis: perigosMoveis.map(function (pm) {
                var d = pm.direcao || pm.dir || 'cima';
                return {
                    x1: pm.x1, y1: pm.y1,
                    x2: pm.x2, y2: pm.y2,
                    tipo: pm.tipo || 'espinho',
                    direcao: d,
                    dir: d,
                    velocidade: pm.velocidade || 1.0
                };
            }),
            itens: itens,
            checkpoints: checkpoints,
            animais: animais.map(function (a) { return { x: a.x, y: a.y, especieId: a.especieId }; }),
            inicio: inicio,
            objetivo: objetivo
        });
    }

    // ---- Edição ------------------------------------------------------------
    function limparCelula(x, y, apagarMoveis) {
        if (!dentro(x, y)) { return; }
        terra[y][x] = false;
        plataforma[y][x] = false;
        oneway[y][x] = false;
        mola[y][x] = false;
        madeira[y][x] = false;
        pedra[y][x] = false;
        perigo[y][x] = null;
        animais = animais.filter(function (a) { return !(a.x === x && a.y === y); });
        itens = itens.filter(function (i) { return !(i.x === x && i.y === y); });
        checkpoints = checkpoints.filter(function (cp) { return !(cp.x === x && cp.y === y); });

        // Só apaga trajetos móveis se o usuário estiver usando a borracha ou botão direito
        if (apagarMoveis) {
            plataformasMoveis = plataformasMoveis.filter(function (p) {
                var w = p.largura || 2;
                var tocaP1 = x >= p.x1 && x < p.x1 + w && y === p.y1;
                var tocaP2 = x >= p.x2 && x < p.x2 + w && y === p.y2;
                return !(tocaP1 || tocaP2);
            });
            perigosMoveis = perigosMoveis.filter(function (p) {
                return !((p.x1 === x && p.y1 === y) || (p.x2 === x && p.y2 === y));
            });
        }

        if (inicio && inicio.x === x && inicio.y === y) { inicio = null; }
        if (objetivo && objetivo.x === x && objetivo.y === y) { objetivo = null; }
    }

    function aplicar(x, y) {
        if (!dentro(x, y)) { return; }

        if (!valorPincel || ferramenta === 'borracha') {
            limparCelula(x, y, true);
            desenhar();
            return;
        }

        var vel = parseFloat(selectVelocidade ? selectVelocidade.value : '1.0') || 1.0;

        switch (ferramenta) {
            case 'terra':
                limparCelula(x, y, false); terra[y][x] = rotacao; break;
            case 'plataforma':
                limparCelula(x, y, false); plataforma[y][x] = rotacao; break;
            case 'oneway':
                limparCelula(x, y, false); oneway[y][x] = rotacao; break;
            case 'mola':
                limparCelula(x, y, false); mola[y][x] = rotacao; break;
            case 'madeira':
                limparCelula(x, y, false); madeira[y][x] = rotacao; break;
            case 'pedra':
                limparCelula(x, y, false); pedra[y][x] = rotacao; break;
            case 'fogo':
                limparCelula(x, y, false); perigo[y][x] = { tipo: 'fogo', dir: rotacao }; break;
            case 'espinho':
                limparCelula(x, y, false); perigo[y][x] = { tipo: 'espinho', dir: rotacao }; break;
            case 'agua':
                limparCelula(x, y, false);
                perigo[y][x] = { tipo: 'agua', dir: 'cima' };
                if (y + 1 < altura && terra[y + 1][x]) {
                    terra[y + 1][x] = false;
                }
                break;
            case 'plat_movel':
                if (!pontoInicialMovel) {
                    pontoInicialMovel = { x: x, y: y };
                } else {
                    plataformasMoveis.push({
                        x1: pontoInicialMovel.x, y1: pontoInicialMovel.y,
                        x2: x, y2: y,
                        largura: 2,
                        tipo: 'plataforma',
                        velocidade: vel
                    });
                    pontoInicialMovel = null;
                    desenhando = false;
                }
                break;
            case 'espinho_movel':
                if (!pontoInicialMovel) {
                    pontoInicialMovel = { x: x, y: y };
                } else {
                    perigosMoveis.push({
                        x1: pontoInicialMovel.x, y1: pontoInicialMovel.y,
                        x2: x, y2: y,
                        tipo: 'espinho',
                        direcao: rotacao,
                        dir: rotacao,
                        velocidade: vel
                    });
                    pontoInicialMovel = null;
                    desenhando = false;
                }
                break;
            case 'item_dash':
                limparCelula(x, y, false);
                itens.push({ x: x, y: y, tipo: 'dash' });
                break;
            case 'item_espeto':
                limparCelula(x, y, false);
                itens.push({ x: x, y: y, tipo: 'espeto' });
                break;
            case 'item_pulo_duplo':
                limparCelula(x, y, false);
                itens.push({ x: x, y: y, tipo: 'pulo_duplo' });
                break;
            case 'item_cura':
                limparCelula(x, y, false);
                itens.push({ x: x, y: y, tipo: 'cura' });
                break;
            case 'checkpoint':
                limparCelula(x, y, false);
                checkpoints.push({ x: x, y: y });
                break;
            case 'animal':
                limparCelula(x, y, false);
                animais.push({ x: x, y: y, especieId: parseInt(selectEspecie ? selectEspecie.value : '0', 10) || 0 });
                break;
            case 'inicio':
                inicio = { x: x, y: y }; break;
            case 'objetivo':
                objetivo = { x: x, y: y }; break;
        }
        desenhar();
    }

    function celulaDoEvento(e) {
        var rect = canvas.getBoundingClientRect();
        var sx = canvas.width / rect.width;
        var sy = canvas.height / rect.height;
        var x = Math.floor(((e.clientX - rect.left) * sx) / TILE);
        var y = Math.floor(((e.clientY - rect.top) * sy) / TILE);
        return { x: x, y: y };
    }

    function ferramentaCliqueUnico() {
        return ferramenta === 'animal' || ferramenta === 'inicio' || ferramenta === 'objetivo'
            || ferramenta === 'plat_movel' || ferramenta === 'espinho_movel'
            || ferramenta === 'item_dash' || ferramenta === 'item_espeto'
            || ferramenta === 'item_pulo_duplo' || ferramenta === 'item_cura'
            || ferramenta === 'checkpoint';
    }

    canvas.addEventListener('contextmenu', function (e) { e.preventDefault(); });

    canvas.addEventListener('mousedown', function (e) {
        e.preventDefault();
        var c = celulaDoEvento(e);
        if (e.button === 2) {
            if (pontoInicialMovel) {
                pontoInicialMovel = null;
                desenhar();
                return;
            }
            desenhando = true; valorPincel = false;
            limparCelula(c.x, c.y, true);
            desenhar();
            return;
        }
        if (e.button !== 0) { return; }

        if (ferramenta === 'borracha') {
            desenhando = true; valorPincel = false;
            aplicar(c.x, c.y);
            return;
        }

        if (ferramentaCliqueUnico()) {
            aplicar(c.x, c.y);
            return;
        }

        desenhando = true;
        valorPincel = !e.shiftKey;
        if (!valorPincel) { limparCelula(c.x, c.y, true); desenhar(); }
        else { aplicar(c.x, c.y); }
    });

    window.addEventListener('mousemove', function (e) {
        var rect = canvas.getBoundingClientRect();
        if (e.clientX >= rect.left && e.clientX <= rect.right && e.clientY >= rect.top && e.clientY <= rect.bottom) {
            hover = celulaDoEvento(e);
        } else {
            hover = null;
        }
        if (!desenhando) { desenhar(); return; }
        if (!hover) { return; }
        if (ferramentaCliqueUnico()) { return; }
        if (!valorPincel) {
            if (dentro(hover.x, hover.y)) {
                limparCelula(hover.x, hover.y, true); desenhar();
            }
            return;
        }
        aplicar(hover.x, hover.y);
    });

    window.addEventListener('mouseup', function () {
        desenhando = false;
        valorPincel = true;
    });

    canvas.addEventListener('touchstart', function (e) {
        if (e.touches.length === 1) {
            e.preventDefault();
            var touch = e.touches[0];
            var c = celulaDoEvento(touch);
            hover = c;
            if (ferramentaCliqueUnico()) {
                aplicar(c.x, c.y);
                return;
            }
            desenhando = true;
            valorPincel = true;
            aplicar(c.x, c.y);
        }
    }, { passive: false });

    canvas.addEventListener('touchmove', function (e) {
        if (e.touches.length === 1) {
            e.preventDefault();
            var touch = e.touches[0];
            var c = celulaDoEvento(touch);
            hover = c;
            if (desenhando && !ferramentaCliqueUnico()) {
                aplicar(c.x, c.y);
            } else {
                desenhar();
            }
        }
    }, { passive: false });

    canvas.addEventListener('touchend', function () {
        desenhando = false;
        hover = null;
        desenhar();
    });

    // Toolbar selection
    if (toolbar) {
        toolbar.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-tool]');
            if (!btn) { return; }
            ferramenta = btn.getAttribute('data-tool');
            pontoInicialMovel = null;
            Array.prototype.forEach.call(toolbar.querySelectorAll('[data-tool]'), function (b) {
                b.classList.toggle('is-active', b === btn);
            });
            desenhar();
        });
    }

    if (inputLargura) {
        inputLargura.addEventListener('change', function () {
            redimensionar(parseInt(inputLargura.value, 10) || largura, altura, true);
        });
    }

    if (inputAltura) {
        inputAltura.addEventListener('change', function () {
            redimensionar(largura, parseInt(inputAltura.value, 10) || altura, true);
        });
    }

    if (selectVelocidade) {
        selectVelocidade.addEventListener('change', function () {
            var novaVel = parseFloat(selectVelocidade.value) || 1.0;
            if (hover && dentro(hover.x, hover.y)) {
                var hx = hover.x, hy = hover.y;
                plataformasMoveis.forEach(function (pm) {
                    var w = pm.largura || 2;
                    if ((hx >= pm.x1 && hx < pm.x1 + w && hy === pm.y1) || (hx >= pm.x2 && hx < pm.x2 + w && hy === pm.y2)) {
                        pm.velocidade = novaVel;
                    }
                });
                perigosMoveis.forEach(function (pm) {
                    if ((pm.x1 === hx && pm.y1 === hy) || (pm.x2 === hx && pm.y2 === hy)) {
                        pm.velocidade = novaVel;
                    }
                });
            }
            desenhar();
        });
    }

    form.addEventListener('submit', function (e) {
        for (var y = 0; y < altura; y++) {
            for (var x = 0; x < largura; x++) {
                var c = perigo[y][x];
                if (c && c.tipo === 'agua') {
                    if (y + 1 < altura && terra[y + 1][x]) {
                        e.preventDefault();
                        var msgValidacao = 'A água não pode estar diretamente em cima da terra!<br><br>Foi encontrado um bloco de água em <strong>X: ' + x + ', Y: ' + y + '</strong> com terra diretamente abaixo (Y: ' + (y + 1) + ').<br><br>Por favor, use a borracha para remover a terra abaixo da água e criar o leito do lago/rio.';
                        if (window.mostrarModalEco) {
                            window.mostrarModalEco('Aviso de Validação do Nível', msgValidacao, 'aviso');
                        } else {
                            alert('Atenção: A água não pode estar diretamente em cima da terra!\n\nFoi encontrado um bloco de água em X: ' + x + ', Y: ' + y + ' com terra diretamente abaixo (Y: ' + (y + 1) + ').\nPor favor, use a borracha para remover a terra abaixo da água e criar o leito do lago/rio.');
                        }
                        return false;
                    }
                }
            }
        }
        campoJson.value = serializar();
    });

    // ---- Render Helpers ----------------------------------------------------
    function emoji(txt, cx, cy, size) {
        ctx.font = size + 'px serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(txt, cx, cy);
    }

    function desenharEspinho(px, py, dir) {
        dir = dir || 'cima';
        ctx.fillStyle = '#78909c';

        if (dir === 'cima') {
            ctx.fillStyle = '#455a64';
            ctx.fillRect(px, py + TILE - 4, TILE, 4);
            ctx.fillStyle = '#90a4ae';
            for (var s = 0; s < 4; s++) {
                ctx.beginPath();
                ctx.moveTo(px + s * 8, py + TILE - 4);
                ctx.lineTo(px + s * 8 + 4, py + 4);
                ctx.lineTo(px + s * 8 + 8, py + TILE - 4);
                ctx.closePath();
                ctx.fill();
            }
        } else if (dir === 'baixo') {
            ctx.fillStyle = '#455a64';
            ctx.fillRect(px, py, TILE, 4);
            ctx.fillStyle = '#90a4ae';
            for (var s2 = 0; s2 < 4; s2++) {
                ctx.beginPath();
                ctx.moveTo(px + s2 * 8, py + 4);
                ctx.lineTo(px + s2 * 8 + 4, py + TILE - 4);
                ctx.lineTo(px + s2 * 8 + 8, py + 4);
                ctx.closePath();
                ctx.fill();
            }
        } else if (dir === 'esquerda') {
            ctx.fillStyle = '#455a64';
            ctx.fillRect(px + TILE - 4, py, 4, TILE);
            ctx.fillStyle = '#90a4ae';
            for (var s3 = 0; s3 < 4; s3++) {
                ctx.beginPath();
                ctx.moveTo(px + TILE - 4, py + s3 * 8);
                ctx.lineTo(px + 4, py + s3 * 8 + 4);
                ctx.lineTo(px + TILE - 4, py + s3 * 8 + 8);
                ctx.closePath();
                ctx.fill();
            }
        } else if (dir === 'direita') {
            ctx.fillStyle = '#455a64';
            ctx.fillRect(px, py, 4, TILE);
            ctx.fillStyle = '#90a4ae';
            for (var s4 = 0; s4 < 4; s4++) {
                ctx.beginPath();
                ctx.moveTo(px + 4, py + s4 * 8);
                ctx.lineTo(px + TILE - 4, py + s4 * 8 + 4);
                ctx.lineTo(px + 4, py + s4 * 8 + 8);
                ctx.closePath();
                ctx.fill();
            }
        } else if (dir === 'todos') {
            ctx.fillStyle = '#37474f';
            ctx.fillRect(px + 8, py + 8, TILE - 16, TILE - 16);

            ctx.fillStyle = '#90a4ae';
            ctx.beginPath(); ctx.moveTo(px + 10, py + 8); ctx.lineTo(px + 16, py + 2); ctx.lineTo(px + 22, py + 8); ctx.closePath(); ctx.fill();
            ctx.beginPath(); ctx.moveTo(px + 10, py + TILE - 8); ctx.lineTo(px + 16, py + TILE - 2); ctx.lineTo(px + 22, py + TILE - 8); ctx.closePath(); ctx.fill();
            ctx.beginPath(); ctx.moveTo(px + 8, py + 10); ctx.lineTo(px + 2, py + 16); ctx.lineTo(px + 8, py + 22); ctx.closePath(); ctx.fill();
            ctx.beginPath(); ctx.moveTo(px + TILE - 8, py + 10); ctx.lineTo(px + TILE - 2, py + 16); ctx.lineTo(px + TILE - 8, py + 22); ctx.closePath(); ctx.fill();

            ctx.fillStyle = '#ff7043';
            ctx.fillRect(px + 14, py + 14, 4, 4);
        }
    }

    function desenharTerraRotacionada(px, py, rot) {
        rot = rot || 'cima';
        ctx.fillStyle = '#8a5a2b';
        ctx.fillRect(px, py, TILE, TILE);
        ctx.fillStyle = '#4caf50';
        if (rot === 'baixo') {
            ctx.fillRect(px, py + TILE - 7, TILE, 7);
        } else if (rot === 'esquerda') {
            ctx.fillRect(px, py, 7, TILE);
        } else if (rot === 'direita') {
            ctx.fillRect(px + TILE - 7, py, 7, TILE);
        } else {
            ctx.fillRect(px, py, TILE, 7);
        }
    }

    function desenharMadeiraRotacionada(px, py, rot) {
        ctx.fillStyle = '#8d5524';
        ctx.fillRect(px, py, TILE, TILE);
        ctx.fillStyle = '#5c3818';
        if (rot === 'esquerda' || rot === 'direita') {
            ctx.fillRect(px + 2, py, 4, TILE);
            ctx.fillRect(px + TILE - 5, py, 3, TILE);
        } else {
            ctx.fillRect(px, py + 2, TILE, 4);
            ctx.fillRect(px, py + TILE - 5, TILE, 3);
        }
        ctx.strokeStyle = 'rgba(0,0,0,0.2)';
        ctx.strokeRect(px + 0.5, py + 0.5, TILE - 1, TILE - 1);
    }

    function desenharPedraRotacionada(px, py, rot) {
        ctx.fillStyle = '#78909c';
        ctx.fillRect(px, py, TILE, TILE);
        ctx.fillStyle = '#546e7a';
        if (rot === 'baixo') {
            ctx.fillRect(px + 3, py + 18, 10, 8);
            ctx.fillRect(px + 16, py + 6, 12, 10);
        } else if (rot === 'esquerda') {
            ctx.fillRect(px + 3, py + 3, 8, 10);
            ctx.fillRect(px + 14, py + 16, 10, 12);
        } else if (rot === 'direita') {
            ctx.fillRect(px + 18, py + 3, 8, 10);
            ctx.fillRect(px + 6, py + 16, 10, 12);
        } else {
            ctx.fillRect(px + 3, py + 3, 10, 8);
            ctx.fillRect(px + 16, py + 14, 12, 10);
        }
        ctx.strokeStyle = 'rgba(0,0,0,0.25)';
        ctx.strokeRect(px + 0.5, py + 0.5, TILE - 1, TILE - 1);
    }

    function desenharOneWay(px, py, rot) {
        rot = rot || 'cima';
        ctx.save();
        ctx.translate(px + TILE / 2, py + TILE / 2);
        if (rot === 'direita') { ctx.rotate(Math.PI / 2); }
        else if (rot === 'baixo') { ctx.rotate(Math.PI); }
        else if (rot === 'esquerda') { ctx.rotate(-Math.PI / 2); }

        ctx.fillStyle = '#a1887f';
        ctx.fillRect(-TILE / 2, -10, TILE, 8);
        ctx.fillStyle = '#5d4037';
        ctx.fillRect(-TILE / 2, -4, TILE, 2);

        ctx.fillStyle = '#81c784';
        ctx.beginPath();
        ctx.moveTo(-8, -11); ctx.lineTo(0, -15); ctx.lineTo(8, -11);
        ctx.lineTo(4, -11); ctx.lineTo(4, -9); ctx.lineTo(-4, -9); ctx.lineTo(-4, -11);
        ctx.closePath();
        ctx.fill();
        ctx.restore();
    }

    function desenharMola(px, py, rot) {
        rot = rot || 'cima';
        ctx.save();
        ctx.translate(px + TILE / 2, py + TILE / 2);
        if (rot === 'direita') { ctx.rotate(Math.PI / 2); }
        else if (rot === 'baixo') { ctx.rotate(Math.PI); }
        else if (rot === 'esquerda') { ctx.rotate(-Math.PI / 2); }

        ctx.fillStyle = '#455a64';
        ctx.fillRect(-TILE / 2 + 4, TILE / 2 - 8, TILE - 8, 8);
        ctx.fillStyle = '#ffb300';
        ctx.fillRect(-4, -4, 8, 12);
        ctx.fillStyle = '#d32f2f';
        ctx.fillRect(-TILE / 2 + 3, -8, TILE - 6, 6);
        ctx.restore();
    }

    function desenharItemDash(px, py) {
        ctx.fillStyle = '#00e5ff';
        ctx.beginPath();
        ctx.moveTo(px + 16, py + 4); ctx.lineTo(px + 27, py + 16); ctx.lineTo(px + 16, py + 28); ctx.lineTo(px + 5, py + 16);
        ctx.closePath(); ctx.fill();
        ctx.strokeStyle = '#ffffff'; ctx.lineWidth = 1.5; ctx.stroke();
        emoji('⚡', px + 16, py + 16, 16);
    }

    function desenharItemEspeto(px, py) {
        ctx.fillStyle = 'rgba(141, 85, 36, 0.25)';
        ctx.beginPath(); ctx.arc(px + 16, py + 16, 13, 0, Math.PI * 2); ctx.fill();
        ctx.strokeStyle = '#8d5524'; ctx.lineWidth = 1.5; ctx.stroke();
        emoji('🥾', px + 16, py + 16, 20);
    }

    function desenharItemPuloDuplo(px, py) {
        ctx.fillStyle = 'rgba(156, 39, 176, 0.25)';
        ctx.beginPath(); ctx.arc(px + 16, py + 16, 14, 0, Math.PI * 2); ctx.fill();
        ctx.strokeStyle = '#ba68c8'; ctx.lineWidth = 1.5; ctx.stroke();
        emoji('🪽', px + 16, py + 16, 20);
    }

    function desenharItemCura(px, py) {
        ctx.fillStyle = 'rgba(233, 30, 99, 0.25)';
        ctx.beginPath(); ctx.arc(px + 16, py + 16, 14, 0, Math.PI * 2); ctx.fill();
        ctx.strokeStyle = '#e91e63'; ctx.lineWidth = 1.5; ctx.stroke();
        emoji('❤️', px + 16, py + 16, 18);
    }

    function desenharCheckpoint(px, py) {
        emoji('🏕️', px + 16, py + 16, 22);
    }

    // ---- Render Principal --------------------------------------------------
    function desenhar() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        var ceu = ctx.createLinearGradient(0, 0, 0, canvas.height);
        ceu.addColorStop(0, '#dff2ff');
        ceu.addColorStop(1, '#f2fff0');
        ctx.fillStyle = ceu;
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Grade
        ctx.strokeStyle = 'rgba(0,0,0,0.06)';
        ctx.lineWidth = 1;
        for (var x = 0; x <= largura; x++) {
            ctx.beginPath(); ctx.moveTo(x * TILE + 0.5, 0); ctx.lineTo(x * TILE + 0.5, canvas.height); ctx.stroke();
        }
        for (var y = 0; y <= altura; y++) {
            ctx.beginPath(); ctx.moveTo(0, y * TILE + 0.5); ctx.lineTo(canvas.width, y * TILE + 0.5); ctx.stroke();
        }

        // Blocos e Perigos colocados
        for (var ty = 0; ty < altura; ty++) {
            for (var tx = 0; tx < largura; tx++) {
                var px = tx * TILE, py = ty * TILE;

                if (terra[ty][tx]) {
                    desenharTerraRotacionada(px, py, terra[ty][tx]);
                } else if (plataforma[ty][tx]) {
                    ctx.fillStyle = '#66bb6a'; ctx.fillRect(px, py + 4, TILE, TILE - 12);
                    ctx.fillStyle = '#388e3c'; ctx.fillRect(px, py + 4, TILE, 5);
                } else if (oneway[ty][tx]) {
                    desenharOneWay(px, py, oneway[ty][tx]);
                } else if (mola[ty][tx]) {
                    desenharMola(px, py, mola[ty][tx]);
                } else if (madeira[ty][tx]) {
                    desenharMadeiraRotacionada(px, py, madeira[ty][tx]);
                } else if (pedra[ty][tx]) {
                    desenharPedraRotacionada(px, py, pedra[ty][tx]);
                }

                var hz = perigo[ty][tx];
                if (hz) {
                    var tipo = typeof hz === 'string' ? hz : hz.tipo;
                    var dir = (typeof hz === 'object' && (hz.dir || hz.direcao)) ? (hz.dir || hz.direcao) : 'cima';

                    if (tipo === 'agua') {
                        ctx.fillStyle = '#2f8fd6'; ctx.fillRect(px, py + 6, TILE, TILE - 6);
                        ctx.fillStyle = 'rgba(255,255,255,0.4)'; ctx.fillRect(px, py + 6, TILE, 3);
                        if (ty + 1 < altura && terra[ty + 1][tx]) {
                            ctx.strokeStyle = '#e53935'; ctx.lineWidth = 2;
                            ctx.strokeRect(px + 1, py + 1, TILE - 2, TILE - 2);
                        }
                    } else if (tipo === 'espinho') {
                        desenharEspinho(px, py, dir);
                    } else if (tipo === 'fogo') {
                        ctx.fillStyle = '#ff6b35'; ctx.fillRect(px + 4, py + 8, TILE - 8, TILE - 8);
                    }
                }
            }
        }

        // Plataformas Móveis
        plataformasMoveis.forEach(function (pm) {
            var x1 = pm.x1 * TILE + TILE / 2;
            var y1 = pm.y1 * TILE + TILE / 2;
            var x2 = pm.x2 * TILE + TILE / 2;
            var y2 = pm.y2 * TILE + TILE / 2;

            ctx.strokeStyle = '#0288d1';
            ctx.lineWidth = 3;
            ctx.setLineDash([6, 4]);
            ctx.beginPath(); ctx.moveTo(x1, y1); ctx.lineTo(x2, y2); ctx.stroke();
            ctx.setLineDash([]);

            ctx.fillStyle = '#0288d1';
            ctx.beginPath(); ctx.arc(x1, y1, 5, 0, Math.PI * 2); ctx.fill();
            ctx.fillStyle = '#e91e63';
            ctx.beginPath(); ctx.arc(x2, y2, 5, 0, Math.PI * 2); ctx.fill();

            var platW = (pm.largura || 2) * TILE;
            var platX = pm.x1 * TILE;
            var platY = pm.y1 * TILE + 4;
            ctx.fillStyle = '#4fc3f7';
            ctx.fillRect(platX, platY, platW, 14);
            ctx.strokeStyle = '#0288d1';
            ctx.lineWidth = 2;
            ctx.strokeRect(platX, platY, platW, 14);
            emoji('🚀', platX + platW / 2, platY + 7, 12);

            ctx.fillStyle = '#01579b';
            ctx.font = 'bold 9px sans-serif';
            ctx.fillText((pm.velocidade || 1.0) + 'x', platX + platW / 2, platY + 22);
        });

        // Espinhos Móveis (mantém a direção escolhida: cima, baixo, esq, dir ou todos)
        perigosMoveis.forEach(function (pm) {
            var x1 = pm.x1 * TILE + TILE / 2;
            var y1 = pm.y1 * TILE + TILE / 2;
            var x2 = pm.x2 * TILE + TILE / 2;
            var y2 = pm.y2 * TILE + TILE / 2;

            ctx.strokeStyle = '#e53935';
            ctx.lineWidth = 2.5;
            ctx.setLineDash([5, 4]);
            ctx.beginPath(); ctx.moveTo(x1, y1); ctx.lineTo(x2, y2); ctx.stroke();
            ctx.setLineDash([]);

            ctx.fillStyle = '#e53935';
            ctx.beginPath(); ctx.arc(x1, y1, 4, 0, Math.PI * 2); ctx.fill();
            ctx.fillStyle = '#b71c1c';
            ctx.beginPath(); ctx.arc(x2, y2, 4, 0, Math.PI * 2); ctx.fill();

            var dir = pm.direcao || pm.dir || 'cima';
            desenharEspinho(pm.x1 * TILE, pm.y1 * TILE, dir);
            emoji('⚙️', pm.x1 * TILE + TILE / 2, pm.y1 * TILE + TILE / 2, 14);

            ctx.fillStyle = '#b71c1c';
            ctx.font = 'bold 9px sans-serif';
            ctx.fillText((pm.velocidade || 1.0) + 'x', pm.x1 * TILE + TILE / 2, pm.y1 * TILE + TILE + 10);
        });

        // Trajeto em construção
        if (pontoInicialMovel && hover) {
            var p1x = pontoInicialMovel.x * TILE + TILE / 2;
            var p1y = pontoInicialMovel.y * TILE + TILE / 2;
            var p2x = hover.x * TILE + TILE / 2;
            var p2y = hover.y * TILE + TILE / 2;

            ctx.strokeStyle = ferramenta === 'espinho_movel' ? '#e53935' : '#0288d1';
            ctx.lineWidth = 3;
            ctx.setLineDash([4, 4]);
            ctx.beginPath(); ctx.moveTo(p1x, p1y); ctx.lineTo(p2x, p2y); ctx.stroke();
            ctx.setLineDash([]);

            ctx.fillStyle = '#4caf50';
            ctx.beginPath(); ctx.arc(p1x, p1y, 7, 0, Math.PI * 2); ctx.fill();
            ctx.fillStyle = '#fff';
            ctx.font = 'bold 10px sans-serif';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText('1', p1x, p1y);
        }

        // Itens colocados
        itens.forEach(function (it) {
            if (it.tipo === 'dash') {
                desenharItemDash(it.x * TILE, it.y * TILE);
            } else if (it.tipo === 'espeto') {
                desenharItemEspeto(it.x * TILE, it.y * TILE);
            } else if (it.tipo === 'pulo_duplo') {
                desenharItemPuloDuplo(it.x * TILE, it.y * TILE);
            } else if (it.tipo === 'cura') {
                desenharItemCura(it.x * TILE, it.y * TILE);
            }
        });

        // Checkpoints
        checkpoints.forEach(function (cp) {
            desenharCheckpoint(cp.x * TILE, cp.y * TILE);
        });

        // Animais, Início e Objetivo
        animais.forEach(function (a) { emoji('\uD83D\uDC3E', a.x * TILE + TILE / 2, a.y * TILE + TILE / 2, 24); });
        if (inicio) { emoji('\uD83C\uDFC3', inicio.x * TILE + TILE / 2, inicio.y * TILE + TILE / 2, 26); }
        if (objetivo) { emoji('\uD83D\uDEA9', objetivo.x * TILE + TILE / 2, objetivo.y * TILE + TILE / 2, 28); }

        // Ghost Block
        if (hover && dentro(hover.x, hover.y)) {
            desenharGhostBlock(hover.x, hover.y);
        }
    }

    function desenharGhostBlock(gx, gy) {
        var px = gx * TILE, py = gy * TILE;

        ctx.save();
        ctx.globalAlpha = 0.52;

        switch (ferramenta) {
            case 'terra':
                desenharTerraRotacionada(px, py, rotacao);
                break;
            case 'plataforma':
                ctx.fillStyle = '#66bb6a'; ctx.fillRect(px, py + 4, TILE, TILE - 12);
                ctx.fillStyle = '#388e3c'; ctx.fillRect(px, py + 4, TILE, 5);
                break;
            case 'oneway':
                desenharOneWay(px, py, rotacao);
                break;
            case 'mola':
                desenharMola(px, py, rotacao);
                break;
            case 'madeira':
                desenharMadeiraRotacionada(px, py, rotacao);
                break;
            case 'pedra':
                desenharPedraRotacionada(px, py, rotacao);
                break;
            case 'espinho':
                desenharEspinho(px, py, rotacao);
                break;
            case 'fogo':
                ctx.fillStyle = '#ff6b35'; ctx.fillRect(px + 4, py + 8, TILE - 8, TILE - 8);
                break;
            case 'agua':
                ctx.fillStyle = '#2f8fd6'; ctx.fillRect(px, py + 6, TILE, TILE - 6);
                break;
            case 'item_dash':
                desenharItemDash(px, py);
                break;
            case 'item_espeto':
                desenharItemEspeto(px, py);
                break;
            case 'item_pulo_duplo':
                desenharItemPuloDuplo(px, py);
                break;
            case 'item_cura':
                desenharItemCura(px, py);
                break;
            case 'checkpoint':
                desenharCheckpoint(px, py);
                break;
            case 'animal':
                emoji('\uD83D\uDC3E', px + TILE / 2, py + TILE / 2, 24);
                break;
            case 'inicio':
                emoji('\uD83C\uDFC3', px + TILE / 2, py + TILE / 2, 26);
                break;
            case 'objetivo':
                emoji('\uD83D\uDEA9', px + TILE / 2, py + TILE / 2, 28);
                break;
            case 'plat_movel':
                var velPlat = parseFloat(selectVelocidade ? selectVelocidade.value : '1.0') || 1.0;
                ctx.fillStyle = '#4fc3f7';
                ctx.fillRect(px, py + 4, TILE * 2, 14);
                emoji('🚀', px + TILE, py + 11, 14);
                ctx.fillStyle = '#01579b';
                ctx.font = 'bold 9px sans-serif';
                ctx.fillText(velPlat + 'x', px + TILE, py + 26);
                break;
            case 'espinho_movel':
                var velEsp = parseFloat(selectVelocidade ? selectVelocidade.value : '1.0') || 1.0;
                desenharEspinho(px, py, rotacao);
                emoji('⚙️', px + TILE / 2, py + TILE / 2, 14);
                ctx.fillStyle = '#b71c1c';
                ctx.font = 'bold 9px sans-serif';
                ctx.fillText(velEsp + 'x', px + TILE / 2, py + TILE + 10);
                break;
            case 'borracha':
                ctx.fillStyle = 'rgba(239, 83, 80, 0.35)';
                ctx.fillRect(px, py, TILE, TILE);
                emoji('🧽', px + TILE / 2, py + TILE / 2, 18);
                break;
        }

        ctx.restore();

        ctx.strokeStyle = 'rgba(0, 0, 0, 0.65)';
        ctx.lineWidth = 1.5;
        ctx.strokeRect(px + 0.5, py + 0.5, TILE - 1, TILE - 1);
    }

    carregar();
})();
