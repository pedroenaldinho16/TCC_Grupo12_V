(function () {
    'use strict';

    const form = document.getElementById('quizForm');
    if (!form) {
        return;
    }

    const perguntas = Array.from(form.querySelectorAll('.eco-quiz-question'));
    if (perguntas.length === 0) {
        return;
    }

    const total = perguntas.length;
    const btnAnterior = document.getElementById('quizAnterior');
    const btnProxima = document.getElementById('quizProxima');
    const btnEnviar = document.getElementById('quizEnviar');
    const textoProgresso = document.getElementById('quizProgressoTexto');
    const barraProgresso = document.getElementById('quizProgressoBarra');
    const stepDots = Array.from(document.querySelectorAll('.eco-stepper-dot'));
    const stepLines = Array.from(document.querySelectorAll('.eco-stepper-dot__line'));

    let atual = 0;

    function estaRespondida(indice) {
        if (indice < 0 || indice >= total) return false;
        return perguntas[indice].querySelector('input[type="radio"]:checked') !== null;
    }

    function mostrarErro(indice) {
        const pergunta = perguntas[indice];
        if (!pergunta) return;
        const alerta = pergunta.querySelector('.eco-quiz-validation-alert');
        if (alerta) {
            alerta.classList.remove('d-none');
            alerta.classList.add('d-flex');
        }
        pergunta.classList.add('has-error');
        setTimeout(function () {
            pergunta.classList.remove('has-error');
        }, 600);
    }

    function ocultarErro(indice) {
        const pergunta = perguntas[indice];
        if (!pergunta) return;
        const alerta = pergunta.querySelector('.eco-quiz-validation-alert');
        if (alerta) {
            alerta.classList.add('d-none');
            alerta.classList.remove('d-flex');
        }
    }

    function atualizarDots() {
        stepDots.forEach(function (dot, i) {
            const respondida = estaRespondida(i);
            dot.classList.toggle('is-active', i === atual);
            dot.classList.toggle('is-completed', respondida && i < atual);
            dot.classList.toggle('is-answered', respondida);
        });

        stepLines.forEach(function (line, i) {
            line.classList.toggle('is-completed', i < atual && estaRespondida(i));
        });
    }

    function mostrar(indice) {
        atual = indice;

        perguntas.forEach(function (pergunta, i) {
            const ativa = i === indice;
            pergunta.classList.toggle('is-active', ativa);
            if (!ativa) {
                ocultarErro(i);
            }
        });

        if (btnAnterior) {
            btnAnterior.style.display = indice === 0 ? 'none' : 'inline-flex';
        }
        if (btnProxima) {
            btnProxima.style.display = indice === total - 1 ? 'none' : 'inline-flex';
        }
        if (btnEnviar) {
            btnEnviar.style.display = indice === total - 1 ? 'inline-flex' : 'none';
        }
        if (textoProgresso) {
            textoProgresso.textContent = 'Etapa ' + (indice + 1) + ' de ' + total;
        }
        if (barraProgresso) {
            barraProgresso.style.width = (((indice + 1) / total) * 100) + '%';
        }

        atualizarDots();

        // Rolagem suave até a pergunta atual se estiver fora do campo de visão
        var el = perguntas[indice];
        if (el) {
            var rect = el.getBoundingClientRect();
            if (rect.top < 80 || rect.bottom > window.innerHeight) {
                el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            }
        }
    }

    // Configurar cards de alternativas
    perguntas.forEach(function (pergunta, pIndex) {
        const cards = Array.from(pergunta.querySelectorAll('.eco-quiz-option-card'));

        cards.forEach(function (card) {
            const radio = card.querySelector('input[type="radio"]');
            if (!radio) return;

            // Se já estiver checado no carregamento
            if (radio.checked) {
                card.classList.add('is-selected');
            }

            card.addEventListener('click', function () {
                cards.forEach(function (c) { c.classList.remove('is-selected'); });
                radio.checked = true;
                card.classList.add('is-selected');
                ocultarErro(pIndex);
                atualizarDots();
            });
        });
    });

    // Navegação via botões
    if (btnProxima) {
        btnProxima.addEventListener('click', function () {
            if (!estaRespondida(atual)) {
                mostrarErro(atual);
                return;
            }
            ocultarErro(atual);
            if (atual < total - 1) {
                mostrar(atual + 1);
            }
        });
    }

    if (btnAnterior) {
        btnAnterior.addEventListener('click', function () {
            ocultarErro(atual);
            if (atual > 0) {
                mostrar(atual - 1);
            }
        });
    }

    // Navegação ao clicar nos dots de etapas
    stepDots.forEach(function (dot) {
        dot.addEventListener('click', function () {
            var alvo = parseInt(dot.getAttribute('data-step-dot'), 10);
            if (isNaN(alvo) || alvo === atual) return;

            // Permite voltar livremente ou avançar se etapas anteriores estiverem respondidas
            if (alvo < atual) {
                ocultarErro(atual);
                mostrar(alvo);
            } else {
                // Para avançar, verifica se a etapa atual está respondida
                if (!estaRespondida(atual)) {
                    mostrarErro(atual);
                    return;
                }
                mostrar(alvo);
            }
        });
    });

    // Validação ao enviar
    form.addEventListener('submit', function (e) {
        if (!estaRespondida(atual)) {
            e.preventDefault();
            e.stopImmediatePropagation();
            mostrarErro(atual);
            return;
        }

        // Verifica se alguma etapa anterior ficou em branco
        for (let i = 0; i < total; i++) {
            if (!estaRespondida(i)) {
                e.preventDefault();
                e.stopImmediatePropagation();
                mostrar(i);
                mostrarErro(i);
                return;
            }
        }
    }, true); // Captura antecipada para validar antes de qualquer outro handler

    mostrar(0);
})();
