(function () {
    const botoes = Array.from(document.querySelectorAll('[data-tab-target]'));
    const paineis = Array.from(document.querySelectorAll('[data-tab-panel]'));
    if (botoes.length === 0) {
        return;
    }

    function abrir(id) {
        botoes.forEach(function (b) {
            b.classList.toggle('is-active', b.getAttribute('data-tab-target') === id);
        });
        paineis.forEach(function (p) {
            p.hidden = p.id !== id;
        });
    }

    botoes.forEach(function (b) {
        b.addEventListener('click', function () {
            var alvo = b.getAttribute('data-tab-target');
            abrir(alvo);
            history.replaceState(null, '', '#' + alvo.replace('tab', '').toLowerCase());
        });
    });

    botoes.forEach(function (b) {
        var alvo = b.getAttribute('data-tab-target');
        if (location.hash === '#' + alvo.replace('tab', '').toLowerCase()) {
            abrir(alvo);
        }
    });

    const form = document.getElementById('quizForm');
    const resultado = document.getElementById('quizResultado');
    const intro = document.getElementById('quizIntro');
    if (!form || !resultado) {
        return;
    }

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        fetch(form.action, {
            method: 'POST',
            body: new FormData(form),
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(function (r) {
                if (!r.ok) { throw new Error('Falha ao enviar o quiz'); }
                return r.text();
            })
            .then(function (html) {
                resultado.innerHTML = html +
                    '<button type="button" class="btn-eco btn-eco--solid" id="quizReiniciar">Tentar novamente</button>';
                form.style.display = 'none';
                if (intro) { intro.style.display = 'none'; }
                resultado.scrollIntoView({ behavior: 'smooth', block: 'start' });

                document.getElementById('quizReiniciar').addEventListener('click', function () {
                    location.hash = '#quiz';
                    location.reload();
                });
            })
            .catch(function () {
                form.submit();
            });
    });
})();
