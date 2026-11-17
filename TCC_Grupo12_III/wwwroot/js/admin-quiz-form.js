(function () {
    const lista = document.getElementById('perguntas');
    const modelo = document.getElementById('modeloPergunta');
    const adicionar = document.getElementById('adicionarPergunta');

    if (!lista || !modelo || !adicionar) {
        return;
    }

    function reindexar() {
        lista.querySelectorAll('[data-pergunta]').forEach(function (bloco, indice) {
            bloco.querySelectorAll('[name]').forEach(function (campo) {
                campo.name = campo.name.replace(/Perguntas\[[^\]]*\]/, `Perguntas[${indice}]`);
            });

            const titulo = bloco.querySelector('[data-pergunta-titulo]');
            if (titulo) {
                titulo.textContent = `Pergunta ${indice + 1}`;
            }
        });
    }

    adicionar.addEventListener('click', function () {
        lista.appendChild(modelo.content.cloneNode(true));
        reindexar();
    });

    lista.addEventListener('click', function (evento) {
        const botao = evento.target.closest('[data-remover-pergunta]');
        if (!botao) {
            return;
        }

        if (lista.querySelectorAll('[data-pergunta]').length === 1) {
            if (window.mostrarModalEco) {
                window.mostrarModalEco('Atenção', 'O quiz precisa de pelo menos uma pergunta.', 'aviso');
            } else {
                window.alert('O quiz precisa de pelo menos uma pergunta.');
            }
            return;
        }

        botao.closest('[data-pergunta]').remove();
        reindexar();
    });

    reindexar();
})();
