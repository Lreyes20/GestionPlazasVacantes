document.addEventListener("DOMContentLoaded", () => {
    const btnEnviar = document.getElementById("btnEnviar");
    const btnGenerarCV = document.getElementById("btnGenerarCV");

    if (btnEnviar) {
        btnEnviar.addEventListener("click", (e) => {
            const nombre = document.querySelector('[name="NombreCompleto"]').value.trim();
            const cedula = document.querySelector('[name="Cedula"]').value.trim();
            const correo = document.querySelector('[name="Correo"]').value.trim();
            const check = document.getElementById("declaracionJurada");

            if (!nombre || !cedula || !correo) {
                e.preventDefault();
                alert("⚠️ Por favor completa los campos obligatorios antes de enviar la postulación.");
                return false;
            }

            if (!check.checked) {
                e.preventDefault();
                alert("⚠️ Debes aceptar la declaración bajo juramento para continuar.");
                return false;
            }
        });
    }

    if (btnGenerarCV) {
        btnGenerarCV.addEventListener("click", () => {
            const nombre = document.querySelector('[name="NombreCompleto"]').value.trim();
            const cedula = document.querySelector('[name="Cedula"]').value.trim();

            if (!nombre || !cedula) {
                alert("⚠️ Debes completar al menos tu nombre y cédula antes de generar el CV.");
                return;
            }

            const form = document.createElement('form');
            form.method = 'POST';
            form.action = '/Postulacion/GenerarCVPrevia';
            form.target = '_blank';

            const campos = [
                'NombreCompleto', 'Cedula', 'Correo', 'Telefono', 'Direccion',
                'PerfilProfesional', 'ExperienciaLaboral', 'FormacionAcademica',
                'Habilidades', 'Idiomas', 'FormacionComplementaria', 'OtrosDatos'
            ];

            campos.forEach(campo => {
                const input = document.querySelector(`[name="${campo}"]`);
                if (input && input.value) {
                    const hidden = document.createElement('input');
                    hidden.type = 'hidden';
                    hidden.name = campo;
                    hidden.value = input.value;
                    form.appendChild(hidden);
                }
            });

            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            if (token) {
                const hiddenToken = document.createElement('input');
                hiddenToken.type = 'hidden';
                hiddenToken.name = '__RequestVerificationToken';
                hiddenToken.value = token.value;
                form.appendChild(hiddenToken);
            }

            document.body.appendChild(form);
            form.submit();
            document.body.removeChild(form);
        });
    }
});