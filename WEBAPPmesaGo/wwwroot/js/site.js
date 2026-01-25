// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// --- VALIDACIONES GLOBALES ---

// 1. Solo Números (clase: val-numeros)
document.addEventListener('input', function (e) {
    if (e.target.classList.contains('val-numeros')) {
        // Reemplaza todo lo que NO sea dígito (0-9) por vacío
        e.target.value = e.target.value.replace(/[^0-9]/g, '');
    }
});

// 2. Solo Letras (clase: val-letras)
document.addEventListener('input', function (e) {
    if (e.target.classList.contains('val-letras')) {
        // Permite letras (a-z, A-Z), espacios (\s) y acentos/ñ
        // Reemplaza todo lo que NO sea eso por vacío
        e.target.value = e.target.value.replace(/[^a-zA-Z\sñÑáéíóúÁÉÍÓÚüÜ]/g, '');
    }
});
