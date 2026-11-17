(function () {
    'use strict';

    var config = window.JOGO_CONFIG;
    var canvas = document.getElementById('jogoCanvas');
    if (!config || !config.dados || !canvas) {
        return;
    }

    var ctx = canvas.getContext('2d');
    var dados = config.dados;
    var animaisInfo = config.animais || {};
    var especieAlvo = config.especieAlvo || null;
    var modoTeste = !!config.modoTeste;

    var TILE = 32;
    var LARGURA = Math.max(1, dados.largura | 0);
    var HEIGHT = Math.max(12, dados.altura | 0) || 17;
    var WORLD_W = LARGURA * TILE;
    var WORLD_H = HEIGHT * TILE;
    var VIEW_W = canvas.width;
    var VIEW_H = canvas.height;

    // Física e jogabilidade
    var SPEED = 190;
    var GRAVITY = 1500;
    var JUMP = 640;
    var MAXFALL = 920;
    var VIDAS_INICIO = 3;
    var MAX_DASHES = 3;
    var MAX_ESPETOS = 3;
    var MAX_PULOS_DUPLOS = 3;

    var PLAYER_W = 22;
    var PLAYER_H = 30;

    // ---- Grids -------------------------------------------------------------
    var solid = [];
    var isOneWay = [];
    var blockType = [];
    var blockRot = [];
    var hazard = []; // { tipo: 'fogo'|'agua'|'espinho', dir: 'cima'|'baixo'|'esquerda'|'direita'|'todos' }
    var molas = [];
    var molasAnim = {};

    for (var y = 0; y < HEIGHT; y++) {
        solid.push(new Array(LARGURA).fill(false));
        isOneWay.push(new Array(LARGURA).fill(false));
        blockType.push(new Array(LARGURA).fill(null));
        blockRot.push(new Array(LARGURA).fill('cima'));
        hazard.push(new Array(LARGURA).fill(null));
        molas.push(new Array(LARGURA).fill(false));
    }

    function dentro(x, yy) {
        return x >= 0 && x < LARGURA && yy >= 0 && yy < HEIGHT;
    }

    (dados.plataformas || []).forEach(function (p) {
        var tipo = p.tipo || 'terra';
        var rot = p.rot || 'cima';
        for (var ty = p.y; ty < p.y + (p.h || 1); ty++) {
            for (var tx = p.x; tx < p.x + (p.w || 1); tx++) {
                if (dentro(tx, ty)) {
                    blockType[ty][tx] = tipo;
                    blockRot[ty][tx] = rot;
                    if (tipo === 'mola') {
                        molas[ty][tx] = true;
                    } else if (tipo === 'terra' || tipo === 'madeira' || tipo === 'pedra') {
                        solid[ty][tx] = true;
                    } else if (tipo === 'oneway' || tipo === 'plataforma') {
                        isOneWay[ty][tx] = true;
                    }
                }
            }
        }
    });

    (dados.perigos || []).forEach(function (p) {
        var tipo = p.tipo || 'fogo';
        var dir = p.direcao || p.dir || 'cima';
        for (var ty = p.y; ty < p.y + (p.h || 1); ty++) {
            for (var tx = p.x; tx < p.x + (p.w || 1); tx++) {
                if (dentro(tx, ty)) {
                    hazard[ty][tx] = { tipo: tipo, dir: dir };
                }
            }
        }
    });

    // Plataformas Móveis (com suporte a modificador de velocidade)
    var plataformasMoveis = (dados.plataformasMoveis || []).map(function (pm) {
        return {
            x1: pm.x1, y1: pm.y1,
            x2: pm.x2, y2: pm.y2,
            largura: pm.largura || 2,
            velocidade: pm.velocidade || 1.0,
            curX: pm.x1 * TILE,
            curY: pm.y1 * TILE,
            prevX: pm.x1 * TILE,
            prevY: pm.y1 * TILE,
            dx: 0,
            dy: 0,
            t: 0
        };
    });

    // Espinhos Móveis (com suporte a direção e velocidade)
    var perigosMoveis = (dados.perigosMoveis || []).map(function (pm) {
        return {
            x1: pm.x1, y1: pm.y1,
            x2: pm.x2, y2: pm.y2,
            tipo: pm.tipo || 'espinho',
            dir: pm.direcao || pm.dir || 'cima',
            velocidade: pm.velocidade || 1.0,
            curX: pm.x1 * TILE,
            curY: pm.y1 * TILE,
            prevX: pm.x1 * TILE,
            prevY: pm.y1 * TILE,
            dx: 0,
            dy: 0,
            t: 0
        };
    });

    // Itens colecionáveis e poderes
    var itens = (dados.itens || []).map(function (it) {
        return {
            x: it.x,
            y: it.y,
            tipo: it.tipo || 'dash', // 'dash' | 'espeto' | 'pulo_duplo' | 'cura'
            ativo: true,
            cooldown: 0
        };
    });

    // Checkpoints do nível
    var checkpoints = (dados.checkpoints || []).map(function (cp) {
        return { x: cp.x, y: cp.y };
    });
    var checkpointAtivo = null; // { x, y }

    var animais = (dados.animais || []).map(function (a) {
        var info = animaisInfo[String(a.especieId)] || {};
        return {
            x: a.x,
            y: a.y,
            especieId: a.especieId,
            nome: info.nome || 'Animal',
            emoji: info.emoji || '\uD83D\uDC3E',
            resgatado: false
        };
    });

    var inicio = dados.inicio || { x: 2, y: HEIGHT - 3 };
    var objetivo = dados.objetivo || { x: LARGURA - 4, y: HEIGHT - 3 };

    // ---- State -------------------------------------------------------------
    var player = {
        x: 0, y: 0,
        vx: 0, vy: 0,
        onGround: false,
        face: 1,
        movingPlatform: null
    };

    var vidas = VIDAS_INICIO;
    var dashesRestantes = MAX_DASHES;
    var espetosRestantes = MAX_ESPETOS;
    var pulosDuplosRestantes = 0;
    var doubleJumpDesbloqueado = false;
    var podeDoubleJump = true; // resetado ao pousar; falso após usar o double jump no ar
    var doubleJumpWingAnim = [];
    var toastCuraCooldown = 0;

    var dashTimer = 0;
    var dashCooldown = 0;
    var dashDirX = 1;
    var dashGhosts = [];

    var coyoteTimer = 0;
    var dropThroughTimer = 0;
    var wallJumpLock = 0;
    var wallContact = 0; // -1 esq, 1 dir, 0 nenhum

    var resgatados = [];
    var tempo = 0;
    var rodando = false;
    var terminado = false;
    var invulneravel = 0;

    var camX = 0;
    var camY = 0;
    var ultimo = 0;
    var animT = 0;

    var input = { left: false, right: false, up: false, down: false, jump: false, dash: false };
    var jumpBuffered = false;

    var overlayInicio = document.getElementById('jogoOverlayInicio');
    var overlayVitoria = document.getElementById('jogoOverlayVitoria');
    var overlayDerrota = document.getElementById('jogoOverlayDerrota');
    var resumoVitoria = document.getElementById('jogoResumoVitoria');
    var toast = document.getElementById('jogoToast');
    var form = document.getElementById('jogoForm');

    function mostrar(el) { if (el) { el.classList.remove('is-hidden'); } }
    function esconder(el) { if (el) { el.classList.add('is-hidden'); } }

    var toastTimer = null;
    function dizer(msg) {
        if (!toast) { return; }
        toast.textContent = msg;
        mostrar(toast);
        if (toastTimer) { clearTimeout(toastTimer); }
        toastTimer = setTimeout(function () { esconder(toast); }, 1800);
    }

    function respawn() {
        var spawnPonto = checkpointAtivo || inicio;
        player.x = spawnPonto.x * TILE + (TILE - PLAYER_W) / 2;
        player.y = (spawnPonto.y + 1) * TILE - PLAYER_H;
        player.vx = 0;
        player.vy = 0;
        player.onGround = false;
        player.movingPlatform = null;
        podeDoubleJump = true;
        invulneravel = 1.2;
        dashTimer = 0;
        coyoteTimer = 0;
        dashesRestantes = MAX_DASHES;
        espetosRestantes = MAX_ESPETOS;
        pulosDuplosRestantes = doubleJumpDesbloqueado ? MAX_PULOS_DUPLOS : 0;
    }

    function reiniciar() {
        vidas = VIDAS_INICIO;
        dashesRestantes = MAX_DASHES;
        espetosRestantes = MAX_ESPETOS;
        pulosDuplosRestantes = 0;
        doubleJumpDesbloqueado = false;
        checkpointAtivo = null;
        resgatados = [];
        tempo = 0;
        terminado = false;
        animais.forEach(function (a) { a.resgatado = false; });
        itens.forEach(function (it) { it.ativo = true; it.cooldown = 0; });
        respawn();
        esconder(overlayVitoria);
        esconder(overlayDerrota);
        esconder(overlayInicio);
        rodando = true;
    }

    function colideSolido(x, yy, w, h) {
        var x0 = Math.floor(x / TILE);
        var x1 = Math.floor((x + w - 0.001) / TILE);
        var y0 = Math.floor(yy / TILE);
        var y1 = Math.floor((yy + h - 0.001) / TILE);
        for (var ty = y0; ty <= y1; ty++) {
            if (ty < 0 || ty >= HEIGHT) { continue; }
            for (var tx = x0; tx <= x1; tx++) {
                if (tx < 0 || tx >= LARGURA) { return true; }
                if (solid[ty][tx]) { return true; }
            }
        }
        return false;
    }

    function checarMadeiraParede() {
        if (player.onGround) { return 0; }
        var y0 = Math.floor((player.y + 4) / TILE);
        var y1 = Math.floor((player.y + PLAYER_H - 4) / TILE);

        var txEsq = Math.floor((player.x - 2) / TILE);
        var temMadeiraEsq = false;
        for (var ty = y0; ty <= y1; ty++) {
            if (dentro(txEsq, ty) && blockType[ty][txEsq] === 'madeira') {
                temMadeiraEsq = true; break;
            }
        }
        if (temMadeiraEsq && input.left) { return -1; }

        var txDir = Math.floor((player.x + PLAYER_W + 2) / TILE);
        var temMadeiraDir = false;
        for (var ty2 = y0; ty2 <= y1; ty2++) {
            if (dentro(txDir, ty2) && blockType[ty2][txDir] === 'madeira') {
                temMadeiraDir = true; break;
            }
        }
        if (temMadeiraDir && input.right) { return 1; }

        return 0;
    }

    function checarMola() {
        var peY = player.y + PLAYER_H;
        var xCentro = player.x + PLAYER_W / 2;
        var yCentro = player.y + PLAYER_H / 2;
        var tx = Math.floor(xCentro / TILE);
        var ty = Math.floor((peY + 2) / TILE);

        if (dentro(tx, ty) && molas[ty][tx]) {
            var rot = blockRot[ty][tx] || 'cima';

            if (rot === 'cima') {
                if (player.vy >= -60 && peY >= ty * TILE - 4 && peY <= ty * TILE + 16) {
                    player.vy = -JUMP * 1.68;
                    player.onGround = false;
                    player.movingPlatform = null;
                    podeDoubleJump = true;
                    player.y = ty * TILE - PLAYER_H - 1;
                    molasAnim[tx + ',' + ty] = 0.28;
                    dizer('🌀 BOING! Super Pulo!');
                    return true;
                }
            } else if (rot === 'direita') {
                if (player.x + PLAYER_W >= tx * TILE && player.x <= tx * TILE + 20) {
                    player.vx = SPEED * 2.8;
                    player.vy = -JUMP * 0.45;
                    player.face = 1;
                    player.movingPlatform = null;
                    podeDoubleJump = true;
                    wallJumpLock = 0.18;
                    molasAnim[tx + ',' + ty] = 0.28;
                    dizer('🌀 TURBO! Impulso para a direita!');
                    return true;
                }
            } else if (rot === 'esquerda') {
                if (player.x <= (tx + 1) * TILE && player.x + PLAYER_W >= (tx + 1) * TILE - 20) {
                    player.vx = -SPEED * 2.8;
                    player.vy = -JUMP * 0.45;
                    player.face = -1;
                    player.movingPlatform = null;
                    podeDoubleJump = true;
                    wallJumpLock = 0.18;
                    molasAnim[tx + ',' + ty] = 0.28;
                    dizer('🌀 TURBO! Impulso para a esquerda!');
                    return true;
                }
            } else if (rot === 'baixo') {
                if (player.y <= ty * TILE + TILE && player.y >= ty * TILE) {
                    player.vy = JUMP * 1.5;
                    player.movingPlatform = null;
                    podeDoubleJump = true;
                    molasAnim[tx + ',' + ty] = 0.28;
                    return true;
                }
            }
        }
        return false;
    }

    // Atualização física das plataformas e espinhos móveis
    function atualizarPlataformasMoveis(dt) {
        plataformasMoveis.forEach(function (pm) {
            pm.prevX = pm.curX;
            pm.prevY = pm.curY;
            pm.t += dt * (0.6 * (pm.velocidade || 1.0));

            var prog = 0.5 - 0.5 * Math.cos(pm.t * Math.PI);
            pm.curX = (pm.x1 + (pm.x2 - pm.x1) * prog) * TILE;
            pm.curY = (pm.y1 + (pm.y2 - pm.y1) * prog) * TILE;

            pm.dx = pm.curX - pm.prevX;
            pm.dy = pm.curY - pm.prevY;
        });

        perigosMoveis.forEach(function (pm) {
            pm.prevX = pm.curX;
            pm.prevY = pm.curY;
            pm.t += dt * (0.75 * (pm.velocidade || 1.0));

            var prog = 0.5 - 0.5 * Math.cos(pm.t * Math.PI);
            pm.curX = (pm.x1 + (pm.x2 - pm.x1) * prog) * TILE;
            pm.curY = (pm.y1 + (pm.y2 - pm.y1) * prog) * TILE;

            pm.dx = pm.curX - pm.prevX;
            pm.dy = pm.curY - pm.prevY;
        });
    }

    function passo(dt) {
        var prevX = player.x;
        var prevY = player.y;

        if (dropThroughTimer > 0) { dropThroughTimer -= dt; }
        if (wallJumpLock > 0) { wallJumpLock -= dt; }
        if (dashCooldown > 0) { dashCooldown -= dt; }

        if (player.onGround) {
            coyoteTimer = 0.12;
        } else {
            coyoteTimer -= dt;
        }

        // ---- Dash com Shift ----
        if (dashTimer > 0) {
            dashTimer -= dt;
            player.vx = dashDirX * 520;
            player.vy = 0;

            dashGhosts.push({
                x: player.x,
                y: player.y,
                face: player.face,
                alpha: 0.65
            });

            player.x += player.vx * dt;
            if (colideSolido(player.x, player.y, PLAYER_W, PLAYER_H)) {
                player.x = prevX;
                dashTimer = 0;
            }
            player.x = Math.max(0, Math.min(player.x, WORLD_W - PLAYER_W));
        } else {
            // Se o jogador estiver em uma plataforma móvel e no chão, arrasta junto com deslocamento
            if (player.movingPlatform) {
                var mp = player.movingPlatform;
                var mpW = (mp.largura || 2) * TILE;
                var platX1 = mp.curX - 4;
                var platX2 = mp.curX + mpW + 4;
                var platY = mp.curY + 4;

                // Se o jogador andou para fora da plataforma ou drop-through
                if (dropThroughTimer > 0 || player.x + PLAYER_W <= platX1 || player.x >= platX2) {
                    player.movingPlatform = null;
                } else {
                    player.x += mp.dx;
                    player.y = platY - PLAYER_H;
                    player.vy = 0;
                    player.onGround = true;
                    podeDoubleJump = true;
                }
            }

            // Movimentação normal
            if (wallJumpLock <= 0) {
                player.vx = (input.right ? SPEED : 0) - (input.left ? SPEED : 0);
            }
            if (player.vx > 0) { player.face = 1; }
            else if (player.vx < 0) { player.face = -1; }

            // Movimento Horizontal
            player.x += player.vx * dt;
            if (colideSolido(player.x, player.y, PLAYER_W, PLAYER_H)) {
                if (player.vx > 0) {
                    player.x = Math.floor((player.x + PLAYER_W) / TILE) * TILE - PLAYER_W - 0.01;
                } else if (player.vx < 0) {
                    player.x = (Math.floor(player.x / TILE) + 1) * TILE + 0.01;
                }
                player.vx = 0;
            }
            player.x = Math.max(0, Math.min(player.x, WORLD_W - PLAYER_W));

            // Checagem de Wall Slide na madeira com Espeto de Sapatos
            wallContact = checarMadeiraParede();
            if (wallContact !== 0) {
                if (player.vy > 65) { player.vy = 65; }
                podeDoubleJump = true;

                if ((input.jump || jumpBuffered) && espetosRestantes > 0) {
                    espetosRestantes--;
                    player.vy = -JUMP * 0.95;
                    player.vx = -wallContact * SPEED * 1.35;
                    player.face = -wallContact;
                    wallJumpLock = 0.13;
                    podeDoubleJump = true;
                    jumpBuffered = false;
                    dizer('🥾 Wall Jump na madeira! (' + espetosRestantes + ' restantes)');
                }
            }

            // Checar impulso de mola
            checarMola();

            // Descer de plataforma (Down + Pulo)
            if (input.down && (input.jump || jumpBuffered) && (player.onGround || player.movingPlatform)) {
                dropThroughTimer = 0.22;
                player.onGround = false;
                player.movingPlatform = null;
                player.y += 6;
                jumpBuffered = false;
            }

            // Pulo da plataforma móvel ou do chão
            var pulouDoChao = false;
            if (player.movingPlatform && (input.jump || jumpBuffered) && wallContact === 0) {
                var boostY = player.movingPlatform.dy < 0 ? (player.movingPlatform.dy / dt) * 0.35 : 0;
                var boostX = (player.movingPlatform.dx / dt) * 0.35;
                player.vy = -JUMP + Math.min(0, boostY);
                player.vx += boostX;
                player.movingPlatform = null;
                player.onGround = false;
                coyoteTimer = 0;
                podeDoubleJump = true;
                jumpBuffered = false;
                pulouDoChao = true;
            }

            // Pulo do chão ou com Coyote Time
            if (!pulouDoChao && (input.jump || jumpBuffered) && (player.onGround || coyoteTimer > 0) && wallContact === 0) {
                player.vy = -JUMP;
                player.onGround = false;
                player.movingPlatform = null;
                coyoteTimer = 0;
                podeDoubleJump = true;
                jumpBuffered = false;
                pulouDoChao = true;
            }

            // Pulo Duplo no ar — exige novo toque de tecla (jumpBuffered), limitado a 1 por ar e consome carga
            if (!pulouDoChao && jumpBuffered && !player.onGround && coyoteTimer <= 0 && wallContact === 0) {
                if (pulosDuplosRestantes > 0 && podeDoubleJump) {
                    pulosDuplosRestantes--;
                    podeDoubleJump = false; // Apenas 1 salto duplo até tocar solo ou parede!
                    player.vy = -JUMP * 0.95;
                    doubleJumpWingAnim.push({
                        x: player.x + PLAYER_W / 2,
                        y: player.y + PLAYER_H,
                        alpha: 1.0,
                        scale: 0.8
                    });
                    dizer('🪽 Pulo Duplo! (' + pulosDuplosRestantes + ' restante' + (pulosDuplosRestantes === 1 ? '' : 's') + ')');
                }
            }
            jumpBuffered = false;

            // Pulo variável com tempo apertado na barra de espaço
            if (!input.jump && player.vy < -200) {
                player.vy = Math.max(player.vy * 0.45, -200);
            }

            // Gravidade e velocidade vertical
            if (!player.movingPlatform) {
                player.vy += GRAVITY * dt;
                if (player.vy > MAXFALL) { player.vy = MAXFALL; }
                player.y += player.vy * dt;
                player.onGround = false;

                // Colisão vertical com blocos sólidos (terra, pedra, madeira)
                if (colideSolido(player.x, player.y, PLAYER_W, PLAYER_H)) {
                    if (player.vy > 0) {
                        player.y = Math.floor((player.y + PLAYER_H) / TILE) * TILE - PLAYER_H - 0.01;
                        player.vy = 0;
                        player.onGround = true;
                        podeDoubleJump = true;
                        player.movingPlatform = null;
                    } else if (player.vy < 0) {
                        player.y = (Math.floor(player.y / TILE) + 1) * TILE + 0.01;
                        player.vy = 0;
                    }
                }
            } else {
                // Na plataforma móvel, se colidir a cabeça com teto sólido:
                if (colideSolido(player.x, player.y, PLAYER_W, PLAYER_H)) {
                    player.y = (Math.floor(player.y / TILE) + 1) * TILE + 0.01;
                    player.vy = 0;
                    player.movingPlatform = null;
                }
            }

            // Colisão com Plataformas One-Way
            if (!player.movingPlatform && player.vy >= 0 && dropThroughTimer <= 0) {
                var peAnterior = prevY + PLAYER_H;
                var peAtual = player.y + PLAYER_H;
                var x0 = Math.floor((player.x + 3) / TILE);
                var x1 = Math.floor((player.x + PLAYER_W - 3) / TILE);
                var ty0 = Math.floor(peAnterior / TILE);
                var ty1 = Math.floor((peAtual + 2) / TILE);

                for (var ty = ty0; ty <= ty1; ty++) {
                    if (ty >= 0 && ty < HEIGHT) {
                        for (var tx = x0; tx <= x1; tx++) {
                            if (tx >= 0 && tx < LARGURA && isOneWay[ty][tx]) {
                                var topoPlat = ty * TILE + 4;
                                if (peAnterior <= topoPlat + 6 && peAtual >= topoPlat) {
                                    player.y = topoPlat - PLAYER_H;
                                    player.vy = 0;
                                    player.onGround = true;
                                    podeDoubleJump = true;
                                    player.movingPlatform = null;
                                    break;
                                }
                            }
                        }
                    }
                    if (player.onGround) { break; }
                }
            }

            // ---- PLATAFORMAS MÓVEIS: pega o jogador mesmo sem pular (ex: subindo por baixo da terra) ----
            if (dropThroughTimer <= 0 && !player.movingPlatform) {
                plataformasMoveis.forEach(function (pm) {
                    var pw = (pm.largura || 2) * TILE;
                    var platX1 = pm.curX - 4;
                    var platX2 = pm.curX + pw + 4;
                    var platY = pm.curY + 4;
                    var prevPlatY = pm.prevY + 4;

                    // O jogador está sobre a largura da plataforma
                    if (player.x + PLAYER_W > platX1 && player.x < platX2) {
                        var peY = player.y + PLAYER_H;
                        var pePrevY = prevY + PLAYER_H;

                        // Caso 1: A plataforma está SUBINDO e atinge/varre os pés do jogador (mesmo que parado no chão de terra)
                        var subindoNoJogador = pm.dy < 0 && (
                            (peY >= platY - 4 && peY <= prevPlatY + 16) ||
                            (Math.abs(peY - platY) <= Math.max(16, Math.abs(pm.dy) + 8))
                        );

                        // Caso 2: O jogador está caindo/pousando de cima para baixo na plataforma
                        var caindoNaPlat = player.vy >= 0 && pePrevY <= prevPlatY + 12 && peY >= platY - 4;

                        // Caso 3: O jogador está andando ou parado no mesmo nível
                        var emCimaDaPlat = Math.abs(peY - platY) <= 8 && player.vy >= 0;

                        if (subindoNoJogador || caindoNaPlat || emCimaDaPlat) {
                            // Ergue o jogador junto com a plataforma!
                            player.y = platY - PLAYER_H;
                            player.vy = 0;
                            player.onGround = true;
                            player.movingPlatform = pm;
                            podeDoubleJump = true;
                        }
                    }
                });
            }
        }

        // Caiu no abismo
        if (player.y > WORLD_H + 40) {
            perderVida('Você caiu no abismo! ');
            return;
        }

        if (invulneravel > 0) { invulneravel -= dt; }
        if (toastCuraCooldown > 0) { toastCuraCooldown -= dt; }

        // Checagem de Checkpoints
        checkpoints.forEach(function (cp) {
            var cpx = cp.x * TILE, cpy = cp.y * TILE;
            if (player.x < cpx + TILE && player.x + PLAYER_W > cpx &&
                player.y < cpy + TILE && player.y + PLAYER_H > cpy) {
                if (!checkpointAtivo || checkpointAtivo.x !== cp.x || checkpointAtivo.y !== cp.y) {
                    checkpointAtivo = cp;
                    dizer('🏕️ Checkpoint ativado! Progresso salvo.');
                }
            }
        });

        // Atualização e coleta de itens e poderes
        itens.forEach(function (it) {
            if (!it.ativo) {
                it.cooldown -= dt;
                if (it.cooldown <= 0) { it.ativo = true; }
            } else {
                var itX = it.x * TILE + 4, itY = it.y * TILE + 4;
                if (player.x < itX + 24 && player.x + PLAYER_W > itX &&
                    player.y < itY + 24 && player.y + PLAYER_H > itY) {

                    if (it.tipo === 'dash') {
                        it.ativo = false; it.cooldown = 3.0;
                        dashesRestantes = MAX_DASHES;
                        dizer('⚡ Dashes recarregados (3 disponíveis)!');
                    } else if (it.tipo === 'espeto') {
                        it.ativo = false; it.cooldown = 3.0;
                        espetosRestantes = MAX_ESPETOS;
                        dizer('🥾 Espetos de sapato recarregados (3 wall jumps)!');
                    } else if (it.tipo === 'pulo_duplo') {
                        it.ativo = false; it.cooldown = 3.0;
                        doubleJumpDesbloqueado = true;
                        pulosDuplosRestantes = MAX_PULOS_DUPLOS;
                        podeDoubleJump = true;
                        dizer('🪽 Poder Pulo Duplo recarregado (3 disponíveis)!');
                    } else if (it.tipo === 'cura') {
                        if (vidas < VIDAS_INICIO) {
                            it.ativo = false; it.cooldown = 4.0;
                            vidas = Math.min(VIDAS_INICIO, vidas + 1);
                            dizer('❤️ +1 Vida recuperada! (Total: ' + vidas + ')');
                        } else if (toastCuraCooldown <= 0) {
                            toastCuraCooldown = 2.0;
                            dizer('❤️ Suas vidas já estão cheias (3/3)!');
                        }
                    }
                }
            }
        });

        // Checagem de Perigos e Espinhos Móveis
        if (invulneravel <= 0) {
            var msgPerigo = null;

            var tipoPerigo = tocandoPerigo();
            if (tipoPerigo) {
                if (tipoPerigo === 'espinho') { msgPerigo = 'Ai! Você tocou nos espinhos! 🌵 '; }
                else if (tipoPerigo === 'agua') { msgPerigo = 'Splash! Você caiu na água funda! 💧 '; }
                else if (tipoPerigo === 'fogo') { msgPerigo = 'Ai! Cuidado com o fogo! 🔥 '; }
            }

            if (!msgPerigo) {
                perigosMoveis.forEach(function (pm) {
                    var px = pm.curX, py = pm.curY;
                    var dir = pm.dir || 'cima';
                    var colidiu = false;

                    if (dir === 'todos') {
                        colidiu = (player.x < px + TILE - 2 && player.x + PLAYER_W > px + 2 &&
                                   player.y < py + TILE - 2 && player.y + PLAYER_H > py + 2);
                    } else if (dir === 'cima') {
                        colidiu = (player.x < px + TILE - 4 && player.x + PLAYER_W > px + 4 &&
                                   player.y + PLAYER_H > py + 6 && player.y < py + TILE);
                    } else if (dir === 'baixo') {
                        colidiu = (player.x < px + TILE - 4 && player.x + PLAYER_W > px + 4 &&
                                   player.y < py + TILE - 6 && player.y + PLAYER_H > py);
                    } else if (dir === 'esquerda') {
                        colidiu = (player.x < px + TILE - 6 && player.x + PLAYER_W > px &&
                                   player.y + PLAYER_H > py + 4 && player.y < py + TILE - 4);
                    } else if (dir === 'direita') {
                        colidiu = (player.x + PLAYER_W > px + 6 && player.x < px + TILE &&
                                   player.y + PLAYER_H > py + 4 && player.y < py + TILE - 4);
                    }

                    if (colidiu) {
                        msgPerigo = 'Ai! Cuidado com os espinhos móveis! ⚙️ ';
                    }
                });
            }

            if (msgPerigo) {
                perderVida(msgPerigo);
                return;
            }
        }

        // Animais (resgate por contato no caminho)
        animais.forEach(function (a) {
            if (a.resgatado) { return; }
            var ax = a.x * TILE, ay = a.y * TILE;
            if (player.x < ax + TILE && player.x + PLAYER_W > ax &&
                player.y < ay + TILE && player.y + PLAYER_H > ay) {
                resgatar(a);
            }
        });

        // Objetivo final
        var gx = objetivo.x * TILE, gy = objetivo.y * TILE;
        if (player.x < gx + TILE * 1.5 && player.x + PLAYER_W > gx - TILE * 0.5 &&
            player.y < gy + TILE * 1.5 && player.y + PLAYER_H > gy - TILE * 0.5) {
            vencer();
        }
    }

    function tocandoPerigo() {
        var x0 = Math.floor((player.x + 5) / TILE);
        var x1 = Math.floor((player.x + PLAYER_W - 5) / TILE);
        var y0 = Math.floor((player.y + 4) / TILE);
        var y1 = Math.floor((player.y + PLAYER_H - 2) / TILE);

        for (var ty = y0; ty <= y1; ty++) {
            if (ty < 0 || ty >= HEIGHT) { continue; }
            for (var tx = x0; tx <= x1; tx++) {
                if (tx < 0 || tx >= LARGURA) { continue; }
                var hz = hazard[ty][tx];
                if (hz) {
                    var tipo = typeof hz === 'string' ? hz : hz.tipo;
                    var dir = (typeof hz === 'object' && (hz.dir || hz.direcao)) ? (hz.dir || hz.direcao) : 'cima';

                    if (tipo === 'espinho') {
                        var px = tx * TILE, py = ty * TILE;
                        var colidiu = false;
                        if (dir === 'cima') {
                            colidiu = (player.y + PLAYER_H >= py + 6 && player.x + PLAYER_W > px + 3 && player.x < px + TILE - 3);
                        } else if (dir === 'baixo') {
                            colidiu = (player.y <= py + TILE - 6 && player.x + PLAYER_W > px + 3 && player.x < px + TILE - 3);
                        } else if (dir === 'esquerda') {
                            colidiu = (player.x <= px + TILE - 6 && player.y + PLAYER_H > py + 3 && player.y < py + TILE - 3);
                        } else if (dir === 'direita') {
                            colidiu = (player.x + PLAYER_W >= px + 6 && player.y + PLAYER_H > py + 3 && player.y < py + TILE - 3);
                        } else {
                            colidiu = true;
                        }
                        if (colidiu) { return 'espinho'; }
                    } else {
                        return tipo;
                    }
                }
            }
        }
        return null;
    }

    function resgatar(a) {
        a.resgatado = true;
        resgatados.push(a.especieId);
        dizer('\uD83D\uDC3E ' + a.nome + ' resgatado!');
    }

    function perderVida(prefixo) {
        vidas--;
        if (vidas <= 0) {
            vidas = 0;
            // Se perdeu as 3 vidas, reseta o checkpoint de volta para o começo do nível!
            checkpointAtivo = null;
            doubleJumpDesbloqueado = false;
            pulosDuplosRestantes = 0;
            derrotar();
            return;
        }
        dizer(prefixo + 'Restam ' + vidas + ' vida' + (vidas === 1 ? '' : 's') + '.');
        respawn();
    }

    function vencer() {
        if (terminado) { return; }
        terminado = true;
        rodando = false;
        var total = animais.length;
        var nomeAlvo = especieAlvo ? especieAlvo.nome : 'o animal da serra';
        var emojiAlvo = especieAlvo ? especieAlvo.emoji : '🐾';

        if (resumoVitoria) {
            resumoVitoria.innerHTML = 'Você salvou <strong>' + emojiAlvo + ' ' + nomeAlvo + '</strong> na base final!<br>' +
                'Ganhou <strong>+1 avistamento disponível</strong> na sua Reserva.<br>' +
                '<small class="text-muted">Animais extras resgatados: ' + resgatados.length + ' de ' + total + ' em ' + formatarTempo(tempo) + '.</small>';
        }
        mostrar(overlayVitoria);
    }

    function derrotar() {
        if (terminado) { return; }
        terminado = true;
        rodando = false;
        mostrar(overlayDerrota);
    }

    function formatarTempo(s) {
        s = Math.max(0, Math.floor(s));
        var m = Math.floor(s / 60);
        var r = s % 60;
        return m + ':' + (r < 10 ? '0' : '') + r;
    }

    function enviarResultado() {
        if (modoTeste || !form) { reiniciar(); return; }
        var cv = document.getElementById('jogoCampoVidas');
        var ct = document.getElementById('jogoCampoTempo');
        var ca = document.getElementById('jogoCampoAnimais');
        var ce = document.getElementById('jogoCampoEspecieSalvar');
        if (cv) { cv.value = String(vidas); }
        if (ct) { ct.value = String(Math.floor(tempo)); }
        if (ca) { ca.value = resgatados.join(','); }
        if (ce && especieAlvo) { ce.value = String(especieAlvo.id); }
        form.submit();
    }

    // ---- Render Floresta com Paralax ---------------------------------------
    function desenharParalaxFloresta() {
        var ceu = ctx.createLinearGradient(0, 0, 0, VIEW_H);
        ceu.addColorStop(0, '#7ec8f7');
        ceu.addColorStop(0.65, '#c8eddf');
        ceu.addColorStop(1, '#e8faea');
        ctx.fillStyle = ceu;
        ctx.fillRect(0, 0, VIEW_W, VIEW_H);

        ctx.fillStyle = 'rgba(255, 253, 230, 0.45)';
        ctx.beginPath();
        ctx.arc(VIEW_W * 0.75 - camX * 0.02, 110 - camY * 0.02, 65, 0, Math.PI * 2);
        ctx.fill();

        ctx.fillStyle = 'rgba(102, 163, 142, 0.38)';
        var p1Offset = -camX * 0.08;
        var p1Y = VIEW_H - 120 - camY * 0.04;
        for (var i = -1; i < 7; i++) {
            var px = ((i * 320) + p1Offset) % (VIEW_W + 500) - 200;
            ctx.beginPath();
            ctx.moveTo(px, VIEW_H);
            ctx.lineTo(px, p1Y + 40);
            ctx.quadraticCurveTo(px + 160, p1Y - 90, px + 320, p1Y + 40);
            ctx.lineTo(px + 320, VIEW_H);
            ctx.closePath();
            ctx.fill();
        }

        ctx.fillStyle = 'rgba(67, 134, 98, 0.52)';
        var p2Offset = -camX * 0.22;
        var p2Y = VIEW_H - 80 - camY * 0.1;
        for (var j = -1; j < 8; j++) {
            var mx = ((j * 240) + p2Offset) % (VIEW_W + 400) - 150;
            ctx.beginPath();
            ctx.moveTo(mx, VIEW_H);
            ctx.lineTo(mx, p2Y + 20);
            ctx.quadraticCurveTo(mx + 120, p2Y - 70, mx + 240, p2Y + 20);
            ctx.lineTo(mx + 240, VIEW_H);
            ctx.closePath();
            ctx.fill();

            var tx = mx + 120, ty = p2Y - 65;
            ctx.fillRect(tx - 2, ty, 4, 30);
            ctx.beginPath();
            ctx.ellipse(tx, ty - 6, 22, 6, 0, 0, Math.PI * 2);
            ctx.fill();
        }

        ctx.fillStyle = 'rgba(38, 92, 59, 0.72)';
        var p3Offset = -camX * 0.45;
        var p3Y = VIEW_H - camY * 0.22;
        for (var k = -1; k < 9; k++) {
            var fx = ((k * 180) + p3Offset) % (VIEW_W + 300) - 100;
            ctx.fillRect(fx + 30, p3Y - 140, 10, 140);
            ctx.beginPath();
            ctx.arc(fx + 35, p3Y - 145, 38, 0, Math.PI * 2);
            ctx.fill();
            ctx.beginPath();
            ctx.arc(fx + 15, p3Y - 135, 26, 0, Math.PI * 2);
            ctx.fill();
            ctx.beginPath();
            ctx.arc(fx + 55, p3Y - 135, 26, 0, Math.PI * 2);
            ctx.fill();
        }

        ctx.fillStyle = 'rgba(76, 175, 80, 0.45)';
        for (var f = 0; f < 8; f++) {
            var lfx = (f * 140 + animT * 35 - camX * 0.6) % (VIEW_W + 100) - 50;
            var lfy = (160 + f * 45 + Math.sin(animT * 2 + f) * 20 - camY * 0.3) % (VIEW_H);
            ctx.beginPath();
            ctx.ellipse(lfx, lfy, 5, 2.5, Math.sin(animT + f), 0, Math.PI * 2);
            ctx.fill();
        }
    }

    // ---- Render Principal --------------------------------------------------
    function desenhar() {
        animT += 1 / 60;

        for (var k in molasAnim) {
            if (molasAnim[k] > 0) {
                molasAnim[k] -= 1 / 60;
                if (molasAnim[k] <= 0) { delete molasAnim[k]; }
            }
        }

        dashGhosts = dashGhosts.filter(function (g) {
            g.alpha -= 1 / 22;
            return g.alpha > 0;
        });

        desenharParalaxFloresta();

        ctx.save();
        ctx.translate(-Math.round(camX), -Math.round(camY));

        var tx0 = Math.max(0, Math.floor(camX / TILE) - 1);
        var tx1 = Math.min(LARGURA - 1, Math.ceil((camX + VIEW_W) / TILE) + 1);
        var ty0 = Math.max(0, Math.floor(camY / TILE) - 1);
        var ty1 = Math.min(HEIGHT - 1, Math.ceil((camY + VIEW_H) / TILE) + 1);

        // Blocos do nível
        for (var ty = ty0; ty <= ty1; ty++) {
            for (var tx = tx0; tx <= tx1; tx++) {
                var px = tx * TILE, py = ty * TILE;
                var tipo = blockType[ty][tx];
                var rot = blockRot[ty][tx] || 'cima';

                if (tipo === 'terra') {
                    desenharTerraRotacionada(px, py, rot);
                } else if (tipo === 'madeira') {
                    desenharMadeiraRotacionada(px, py, rot);
                } else if (tipo === 'pedra') {
                    desenharPedraRotacionada(px, py, rot);
                } else if (tipo === 'oneway') {
                    desenharOneWay(px, py, rot);
                } else if (tipo === 'plataforma') {
                    ctx.fillStyle = '#4caf50';
                    ctx.fillRect(px, py + 4, TILE, 6);
                    ctx.fillStyle = '#388e3c';
                    ctx.fillRect(px, py + 8, TILE, 4);
                } else if (tipo === 'mola') {
                    desenharMola(px, py, tx, ty, rot);
                }

                var hz = hazard[ty][tx];
                if (hz) { desenharPerigo(hz, px, py); }
            }
        }

        // Plataformas Móveis
        plataformasMoveis.forEach(function (pm) {
            var pw = (pm.largura || 2) * TILE;
            var px = pm.curX, py = pm.curY + 4;

            ctx.fillStyle = '#29b6f6';
            ctx.fillRect(px, py, pw, 14);
            ctx.fillStyle = '#0288d1';
            ctx.fillRect(px, py + 10, pw, 4);
            ctx.strokeStyle = '#01579b';
            ctx.lineWidth = 1.5;
            ctx.strokeRect(px + 0.5, py + 0.5, pw - 1, 13);

            ctx.fillStyle = '#ffeb3b';
            ctx.fillRect(px + 4, py + 4, 4, 4);
            ctx.fillRect(px + pw - 8, py + 4, 4, 4);
        });

        // Espinhos Móveis (desenha a variante correta, incluindo bloco 4 lados)
        perigosMoveis.forEach(function (pm) {
            desenharEspinho(pm.curX, pm.curY, pm.dir || 'cima');
        });

        // Checkpoints
        checkpoints.forEach(function (cp) {
            var ativo = checkpointAtivo && checkpointAtivo.x === cp.x && checkpointAtivo.y === cp.y;
            if (ativo) {
                // Brilho dourado se ativo
                ctx.fillStyle = 'rgba(76, 175, 80, 0.35)';
                ctx.beginPath();
                ctx.arc(cp.x * TILE + 16, cp.y * TILE + 16, 20, 0, Math.PI * 2);
                ctx.fill();
            }
            desenharEmoji(ativo ? '🏕️' : '⛺', cp.x * TILE + 16, cp.y * TILE + 16, 24);
        });

        // Itens de recarga
        itens.forEach(function (it) {
            if (!it.ativo) { return; }
            var flutua = Math.sin(animT * 4 + it.x) * 4;
            if (it.tipo === 'dash') {
                desenharItemDash(it.x * TILE, it.y * TILE + flutua);
            } else if (it.tipo === 'espeto') {
                desenharItemEspeto(it.x * TILE, it.y * TILE + flutua);
            } else if (it.tipo === 'pulo_duplo') {
                desenharItemPuloDuplo(it.x * TILE, it.y * TILE + flutua);
            } else if (it.tipo === 'cura') {
                desenharItemCura(it.x * TILE, it.y * TILE + flutua);
            }
        });

        desenharObjetivoComEspecieAlvo();

        animais.forEach(function (a) {
            if (a.resgatado) { return; }
            var bob = Math.sin(animT * 3 + a.x) * 3;
            desenharEmoji(a.emoji, a.x * TILE + TILE / 2, a.y * TILE + TILE / 2 + bob, 26);
        });

        dashGhosts.forEach(function (g) {
            ctx.save();
            ctx.globalAlpha = g.alpha * 0.7;
            ctx.fillStyle = '#00e5ff';
            ctx.fillRect(g.x + 3, g.y + 12, PLAYER_W - 6, PLAYER_H - 16);
            ctx.restore();
        });

        doubleJumpWingAnim = doubleJumpWingAnim.filter(function (w) {
            w.alpha -= 1 / 22;
            w.scale += 0.04;
            w.y -= 0.6;
            if (w.alpha <= 0) { return false; }
            ctx.save();
            ctx.globalAlpha = w.alpha * 0.85;
            ctx.font = Math.round(20 * w.scale) + 'px serif';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText('🪽', w.x, w.y);
            ctx.restore();
            return true;
        });

        desenharJogador();

        ctx.restore();

        desenharHud();
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
        ctx.strokeStyle = 'rgba(0,0,0,0.08)';
        ctx.strokeRect(px + 0.5, py + 0.5, TILE - 1, TILE - 1);
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
            ctx.fillRect(px + TILE - 5, py, 3, TILE);
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

    function desenharMola(px, py, tx, ty, rot) {
        var comprimida = molasAnim[tx + ',' + ty] > 0;
        rot = rot || 'cima';

        ctx.save();
        ctx.translate(px + TILE / 2, py + TILE / 2);
        if (rot === 'direita') { ctx.rotate(Math.PI / 2); }
        else if (rot === 'baixo') { ctx.rotate(Math.PI); }
        else if (rot === 'esquerda') { ctx.rotate(-Math.PI / 2); }

        var altEspiral = comprimida ? 6 : 14;
        var topoY = comprimida ? -2 : -8;

        ctx.fillStyle = '#37474f';
        ctx.fillRect(-TILE / 2 + 4, TILE / 2 - 8, TILE - 8, 8);

        ctx.fillStyle = '#ffb300';
        ctx.fillRect(-6, topoY + 4, 12, altEspiral);

        ctx.fillStyle = '#d32f2f';
        ctx.fillRect(-TILE / 2 + 3, topoY, TILE - 6, 6);

        if (comprimida) {
            ctx.fillStyle = 'rgba(255, 235, 59, 0.45)';
            ctx.beginPath();
            ctx.arc(0, topoY, 14, 0, Math.PI * 2);
            ctx.fill();
        }

        ctx.restore();
    }

    function desenharEspinho(px, py, dir) {
        dir = dir || 'cima';

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
            for (var b = 0; b < 4; b++) {
                ctx.beginPath();
                ctx.moveTo(px + b * 8, py + 4);
                ctx.lineTo(px + b * 8 + 4, py + TILE - 4);
                ctx.lineTo(px + b * 8 + 8, py + 4);
                ctx.closePath();
                ctx.fill();
            }
        } else if (dir === 'esquerda') {
            ctx.fillStyle = '#455a64';
            ctx.fillRect(px + TILE - 4, py, 4, TILE);
            ctx.fillStyle = '#90a4ae';
            for (var e = 0; e < 4; e++) {
                ctx.beginPath();
                ctx.moveTo(px + TILE - 4, py + e * 8);
                ctx.lineTo(px + 4, py + e * 8 + 4);
                ctx.lineTo(px + TILE - 4, py + e * 8 + 8);
                ctx.closePath();
                ctx.fill();
            }
        } else if (dir === 'direita') {
            ctx.fillStyle = '#455a64';
            ctx.fillRect(px, py, 4, TILE);
            ctx.fillStyle = '#90a4ae';
            for (var d = 0; d < 4; d++) {
                ctx.beginPath();
                ctx.moveTo(px + 4, py + d * 8);
                ctx.lineTo(px + TILE - 4, py + d * 8 + 4);
                ctx.lineTo(px + 4, py + d * 8 + 8);
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

    function desenharPerigo(hz, px, py) {
        var tipo = typeof hz === 'string' ? hz : hz.tipo;
        var dir = (typeof hz === 'object' && (hz.dir || hz.direcao)) ? (hz.dir || hz.direcao) : 'cima';

        if (tipo === 'agua') {
            ctx.fillStyle = '#2f8fd6';
            ctx.fillRect(px, py + 4, TILE, TILE - 4);
            ctx.fillStyle = 'rgba(255,255,255,0.6)';
            var off = Math.sin(animT * 2 + px * 0.1) * 3;
            ctx.fillRect(px, py + 4 + off, TILE, 3);
            ctx.fillStyle = 'rgba(41, 128, 185, 0.4)';
            ctx.fillRect(px, py + 12, TILE, TILE - 12);
        } else if (tipo === 'espinho') {
            desenharEspinho(px, py, dir);
        } else {
            ctx.fillStyle = '#ff6b35';
            var flick = Math.sin(animT * 8 + px * 0.2) * 3;
            ctx.beginPath();
            ctx.moveTo(px + 2, py + TILE);
            ctx.quadraticCurveTo(px + TILE / 2, py - 4 + flick, px + TILE - 2, py + TILE);
            ctx.closePath();
            ctx.fill();
            ctx.fillStyle = '#ffd23f';
            ctx.beginPath();
            ctx.moveTo(px + 9, py + TILE);
            ctx.quadraticCurveTo(px + TILE / 2, py + 10 + flick, px + TILE - 9, py + TILE);
            ctx.closePath();
            ctx.fill();
        }
    }

    function desenharItemDash(px, py) {
        var brilho = 0.6 + Math.sin(animT * 6) * 0.3;
        ctx.fillStyle = 'rgba(0, 229, 255, ' + (brilho * 0.35) + ')';
        ctx.beginPath(); ctx.arc(px + 16, py + 16, 17, 0, Math.PI * 2); ctx.fill();

        ctx.fillStyle = '#00e5ff';
        ctx.beginPath();
        ctx.moveTo(px + 16, py + 5); ctx.lineTo(px + 26, py + 16); ctx.lineTo(px + 16, py + 27); ctx.lineTo(px + 6, py + 16);
        ctx.closePath(); ctx.fill();
        ctx.strokeStyle = '#ffffff'; ctx.lineWidth = 1.5; ctx.stroke();
        desenharEmoji('⚡', px + 16, py + 16, 15);
    }

    function desenharItemEspeto(px, py) {
        var brilho = 0.6 + Math.sin(animT * 5) * 0.3;
        ctx.fillStyle = 'rgba(255, 183, 77, ' + (brilho * 0.35) + ')';
        ctx.beginPath(); ctx.arc(px + 16, py + 16, 16, 0, Math.PI * 2); ctx.fill();

        ctx.fillStyle = 'rgba(141, 85, 36, 0.4)';
        ctx.beginPath(); ctx.arc(px + 16, py + 16, 13, 0, Math.PI * 2); ctx.fill();
        ctx.strokeStyle = '#ffb74d'; ctx.lineWidth = 2; ctx.stroke();
        desenharEmoji('🥾', px + 16, py + 16, 18);
    }

    function desenharItemPuloDuplo(px, py) {
        var brilho = 0.6 + Math.sin(animT * 6.5) * 0.3;
        ctx.fillStyle = 'rgba(186, 104, 200, ' + (brilho * 0.4) + ')';
        ctx.beginPath(); ctx.arc(px + 16, py + 16, 17, 0, Math.PI * 2); ctx.fill();

        ctx.fillStyle = 'rgba(156, 39, 176, 0.45)';
        ctx.beginPath(); ctx.arc(px + 16, py + 16, 14, 0, Math.PI * 2); ctx.fill();
        ctx.strokeStyle = '#e1bee7'; ctx.lineWidth = 1.8; ctx.stroke();
        desenharEmoji('🪽', px + 16, py + 16, 18);
    }

    function desenharItemCura(px, py) {
        var brilho = 0.6 + Math.sin(animT * 5) * 0.3;
        ctx.fillStyle = 'rgba(233, 30, 99, ' + (brilho * 0.4) + ')';
        ctx.beginPath(); ctx.arc(px + 16, py + 16, 17, 0, Math.PI * 2); ctx.fill();

        ctx.fillStyle = 'rgba(233, 30, 99, 0.45)';
        ctx.beginPath(); ctx.arc(px + 16, py + 16, 14, 0, Math.PI * 2); ctx.fill();
        ctx.strokeStyle = '#f8bbd0'; ctx.lineWidth = 1.8; ctx.stroke();
        desenharEmoji('❤️', px + 16, py + 16, 18);
    }

    function desenharObjetivoComEspecieAlvo() {
        var ox = objetivo.x * TILE + TILE / 2;
        var oy = objetivo.y * TILE + TILE / 2;

        desenharEmoji('\uD83D\uDEA9', ox - 14, oy, 32);

        if (especieAlvo) {
            var pulo = Math.sin(animT * 5) * 4;
            desenharEmoji(especieAlvo.emoji || '\uD83D\uDC3E', ox + 18, oy - 2 + pulo, 32);

            var balloonAlpha = 0.85 + Math.sin(animT * 6) * 0.15;
            ctx.fillStyle = 'rgba(255, 255, 255, ' + balloonAlpha + ')';
            ctx.strokeStyle = '#2e7d32';
            ctx.lineWidth = 1.5;
            ctx.beginPath();
            ctx.roundRect(ox + 2, oy - 38 + pulo, 44, 18, 6);
            ctx.fill();
            ctx.stroke();

            ctx.fillStyle = '#1b5e20';
            ctx.font = 'bold 10px sans-serif';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText('Salve-me!', ox + 24, oy - 29 + pulo);
        }
    }

    function desenharEmoji(emoji, cx, cy, size) {
        ctx.font = size + 'px serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(emoji, cx, cy);
    }

    function desenharJogador() {
        var piscando = invulneravel > 0 && Math.floor(animT * 12) % 2 === 0;
        if (piscando) { ctx.globalAlpha = 0.4; }
        var x = player.x, yy = player.y;

        if (wallContact !== 0) {
            ctx.fillStyle = '#d7ccc8';
            ctx.fillRect(x + (wallContact === -1 ? -2 : PLAYER_W + 1), yy + PLAYER_H - 8, 3, 3);
            ctx.fillRect(x + (wallContact === -1 ? -3 : PLAYER_W + 2), yy + PLAYER_H - 14, 2, 2);
        }

        ctx.fillStyle = 'rgba(0,0,0,0.15)';
        ctx.beginPath();
        ctx.ellipse(x + PLAYER_W / 2, yy + PLAYER_H, PLAYER_W / 2, 4, 0, 0, Math.PI * 2);
        ctx.fill();

        ctx.fillStyle = '#2e7d32';
        ctx.fillRect(x + 3, yy + 12, PLAYER_W - 6, PLAYER_H - 16);

        ctx.fillStyle = '#5d4037';
        ctx.fillRect(x + 4, yy + PLAYER_H - 6, 5, 6);
        ctx.fillRect(x + PLAYER_W - 9, yy + PLAYER_H - 6, 5, 6);

        if (espetosRestantes > 0) {
            ctx.fillStyle = '#ffb74d';
            ctx.fillRect(x + 2, yy + PLAYER_H - 2, 3, 2);
            ctx.fillRect(x + PLAYER_W - 5, yy + PLAYER_H - 2, 3, 2);
        }

        ctx.fillStyle = '#f1c27d';
        ctx.beginPath();
        ctx.arc(x + PLAYER_W / 2, yy + 8, 7, 0, Math.PI * 2);
        ctx.fill();

        ctx.fillStyle = '#795548';
        ctx.fillRect(x + PLAYER_W / 2 - 8, yy + 2, 16, 3);
        ctx.fillRect(x + PLAYER_W / 2 - 5, yy - 2, 10, 5);

        ctx.fillStyle = '#333';
        ctx.beginPath();
        ctx.arc(x + PLAYER_W / 2 + player.face * 3, yy + 8, 1.4, 0, Math.PI * 2);
        ctx.fill();

        ctx.globalAlpha = 1;
    }

    function desenharHud() {
        ctx.save();
        ctx.textAlign = 'left';
        ctx.textBaseline = 'middle';
        ctx.font = 'bold 13px system-ui, sans-serif';

        // Painel Esquerdo com Vidas + Dashes + Espetos + Pulos Duplos
        ctx.fillStyle = 'rgba(0,0,0,0.56)';
        ctx.beginPath();
        ctx.roundRect(10, 10, 410, 36, 18);
        ctx.fill();

        // Vidas ❤️
        var vidasStr = '';
        for (var i = 0; i < VIDAS_INICIO; i++) {
            vidasStr += i < vidas ? '❤️' : '🤍';
        }
        ctx.fillStyle = '#fff';
        ctx.fillText(vidasStr, 20, 28);

        // Dashes ⚡
        var dashStr = '⚡ ';
        for (var d = 0; d < MAX_DASHES; d++) {
            dashStr += d < dashesRestantes ? '●' : '○';
        }
        ctx.fillStyle = dashesRestantes > 0 ? '#00e5ff' : '#9e9e9e';
        ctx.fillText(dashStr, 120, 28);

        // Espetos 🥾
        var espetoStr = '🥾 ';
        for (var s = 0; s < MAX_ESPETOS; s++) {
            espetoStr += s < espetosRestantes ? '●' : '○';
        }
        ctx.fillStyle = espetosRestantes > 0 ? '#ffb74d' : '#9e9e9e';
        ctx.fillText(espetoStr, 215, 28);

        // Pulos Duplos 🪽
        var puloStr = '🪽 ';
        for (var p = 0; p < MAX_PULOS_DUPLOS; p++) {
            puloStr += p < pulosDuplosRestantes ? '●' : '○';
        }
        ctx.fillStyle = pulosDuplosRestantes > 0 ? '#ba68c8' : '#9e9e9e';
        ctx.fillText(puloStr, 310, 28);

        // Badge de Checkpoint ativo
        if (checkpointAtivo) {
            ctx.fillStyle = 'rgba(46, 125, 50, 0.78)';
            ctx.beginPath();
            ctx.roundRect(VIEW_W - 340, 10, 130, 36, 18);
            ctx.fill();
            ctx.fillStyle = '#fff';
            ctx.textAlign = 'center';
            ctx.font = 'bold 12px system-ui, sans-serif';
            ctx.fillText('🏕️ Checkpoint', VIEW_W - 275, 28);
        }

        // Painel Direito: Objetivo & Cronômetro
        ctx.fillStyle = 'rgba(0,0,0,0.56)';
        ctx.beginPath();
        ctx.roundRect(VIEW_W - 200, 10, 190, 36, 18);
        ctx.fill();

        ctx.textAlign = 'right';
        ctx.font = 'bold 13px system-ui, sans-serif';
        var textoHud = '';
        if (especieAlvo) {
            textoHud = '🎯 ' + especieAlvo.emoji + '  ';
        }
        textoHud += '⏱️ ' + formatarTempo(tempo);
        ctx.fillStyle = '#fff';
        ctx.fillText(textoHud, VIEW_W - 20, 28);

        ctx.restore();
    }

    // ---- Loop --------------------------------------------------------------
    function loop(ts) {
        if (!ultimo) { ultimo = ts; }
        var dt = (ts - ultimo) / 1000;
        ultimo = ts;
        if (dt > 1 / 20) { dt = 1 / 20; }

        if (rodando && !terminado) {
            tempo += dt;

            var restante = dt;
            while (restante > 0) {
                var sub = Math.min(restante, 1 / 120);
                atualizarPlataformasMoveis(sub);
                passo(sub);
                restante -= sub;
                if (terminado) { break; }
            }

            var maxCamX = Math.max(0, WORLD_W - VIEW_W);
            var maxCamY = Math.max(0, WORLD_H - VIEW_H);

            var targetCamX = Math.max(0, Math.min(player.x + PLAYER_W / 2 - VIEW_W / 2, maxCamX));
            var targetCamY = Math.max(0, Math.min(player.y + PLAYER_H / 2 - VIEW_H / 2, maxCamY));

            camX += (targetCamX - camX) * Math.min(1, dt * 9);
            camY += (targetCamY - camY) * Math.min(1, dt * 9);
        }

        desenhar();
        requestAnimationFrame(loop);
    }

    // ---- Input -------------------------------------------------------------
    function dispararDash() {
        if (!rodando || terminado) { return; }
        if (dashesRestantes > 0 && dashTimer <= 0 && dashCooldown <= 0) {
            dashesRestantes--;
            dashTimer = 0.16;
            dashCooldown = 0.28;
            dashDirX = input.left ? -1 : (input.right ? 1 : player.face);
            dizer('⚡ Dash! (' + dashesRestantes + ' restantes)');
        }
    }

    function tecla(e, down) {
        var k = e.key.toLowerCase();
        if (k === 'arrowleft' || k === 'a') { input.left = down; e.preventDefault(); }
        else if (k === 'arrowright' || k === 'd') { input.right = down; e.preventDefault(); }
        else if (k === 'arrowdown' || k === 's') { input.down = down; e.preventDefault(); }
        else if (k === 'arrowup' || k === 'w' || k === ' ' || k === 'spacebar') {
            if (down && !input.jump) { jumpBuffered = true; }
            input.jump = down;
            e.preventDefault();
        } else if (k === 'shift') {
            if (down && !input.dash) {
                dispararDash();
            }
            input.dash = down;
            e.preventDefault();
        }
    }

    window.addEventListener('keydown', function (e) { tecla(e, true); });
    window.addEventListener('keyup', function (e) { tecla(e, false); });

    function segurar(el, campo) {
        if (!el) { return; }
        var ligar = function (e) {
            e.preventDefault();
            input[campo] = true;
            if (campo === 'jump') { jumpBuffered = true; }
            if (campo === 'dash') { dispararDash(); }
        };
        var desligar = function (e) { e.preventDefault(); input[campo] = false; };
        el.addEventListener('touchstart', ligar, { passive: false });
        el.addEventListener('touchend', desligar);
        el.addEventListener('touchcancel', desligar);
        el.addEventListener('mousedown', ligar);
        el.addEventListener('mouseup', desligar);
        el.addEventListener('mouseleave', desligar);
    }

    segurar(document.getElementById('jogoTouchEsq'), 'left');
    segurar(document.getElementById('jogoTouchDir'), 'right');
    segurar(document.getElementById('jogoTouchBaixo'), 'down');
    segurar(document.getElementById('jogoTouchDash'), 'dash');
    segurar(document.getElementById('jogoTouchPulo'), 'jump');

    canvas.addEventListener('click', function (e) {
        if (!rodando || terminado) { return; }
        var rect = canvas.getBoundingClientRect();
        var mx = (e.clientX - rect.left) * (VIEW_W / rect.width) + camX;
        var my = (e.clientY - rect.top) * (VIEW_H / rect.height) + camY;
        var alvo = null;
        animais.forEach(function (a) {
            if (a.resgatado) { return; }
            var ax = a.x * TILE, ay = a.y * TILE;
            if (mx >= ax && mx <= ax + TILE && my >= ay && my <= ay + TILE) { alvo = a; }
        });
        if (!alvo) { return; }
        var dx = (alvo.x * TILE + TILE / 2) - (player.x + PLAYER_W / 2);
        var dy = (alvo.y * TILE + TILE / 2) - (player.y + PLAYER_H / 2);
        if (Math.sqrt(dx * dx + dy * dy) <= TILE * 5) {
            resgatar(alvo);
        } else {
            dizer('Chegue mais perto para resgatar ' + alvo.nome + '.');
        }
    });

    function iniciarMissao(e) {
        if (e) { e.preventDefault(); e.stopPropagation(); }
        reiniciar();
    }

    var btnComecar = document.getElementById('jogoBtnComecar');
    if (btnComecar) {
        btnComecar.addEventListener('click', iniciarMissao);
        btnComecar.addEventListener('touchend', iniciarMissao);
    }

    if (overlayInicio) {
        overlayInicio.addEventListener('click', function (e) {
            if (e.target === overlayInicio) {
                iniciarMissao();
            }
        });
    }

    window.addEventListener('keydown', function (e) {
        if (overlayInicio && !overlayInicio.classList.contains('is-hidden')) {
            if (e.key === ' ' || e.key === 'Enter' || e.key === 'spacebar') {
                e.preventDefault();
                iniciarMissao();
            }
        }
    });

    var btnVitoria = document.getElementById('jogoBtnVitoriaOk');
    if (btnVitoria) {
        btnVitoria.addEventListener('click', function () { enviarResultado(); });
        btnVitoria.addEventListener('touchend', function (e) { e.preventDefault(); enviarResultado(); });
    }

    var btnDerrota = document.getElementById('jogoBtnTentarDeNovo');
    if (btnDerrota) {
        btnDerrota.addEventListener('click', function () { reiniciar(); });
        btnDerrota.addEventListener('touchend', function (e) { e.preventDefault(); reiniciar(); });
    }

    respawn();
    mostrar(overlayInicio);
    requestAnimationFrame(loop);
})();
