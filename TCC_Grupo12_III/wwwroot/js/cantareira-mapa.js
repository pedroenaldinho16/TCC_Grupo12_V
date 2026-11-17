/* Compatibilidade com Views compiladas antes da retirada do destaque.
 * Elas ainda podem chamar esta função até o próximo reinício do servidor.
 * Nenhum contorno, preenchimento ou alteração de enquadramento é aplicado.
 */
(() => {
    'use strict';
    window.EcoCantaMapa = Object.freeze({
        destacarCantareira() {}
    });
})();
