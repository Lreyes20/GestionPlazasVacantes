// ===== TIMEOUT DE SESIÓN POR INACTIVIDAD =====
// Cierra automáticamente la sesión después de 30 minutos de inactividad

let inactivityTimer;
const INACTIVITY_TIMEOUT = 30 * 60 * 1000; // 30 minutos en milisegundos
const WARNING_TIME = 5 * 60 * 1000; // Mostrar advertencia 5 minutos antes

let warningShown = false;

// Reiniciar el temporizador de inactividad
function resetInactivityTimer() {
    clearTimeout(inactivityTimer);
    warningShown = false;

    // Ocultar advertencia si está visible
    const warningDiv = document.getElementById('inactivity-warning');
    if (warningDiv) {
        warningDiv.remove();
    }

    // Configurar advertencia (5 minutos antes del cierre)
    setTimeout(() => {
        if (!warningShown) {
            showInactivityWarning();
            warningShown = true;
        }
    }, INACTIVITY_TIMEOUT - WARNING_TIME);

    // Configurar cierre de sesión
    inactivityTimer = setTimeout(() => {
        logoutDueToInactivity();
    }, INACTIVITY_TIMEOUT);
}

// Mostrar advertencia de inactividad
function showInactivityWarning() {
    // Crear div de advertencia si no existe
    let warningDiv = document.getElementById('inactivity-warning');
    if (!warningDiv) {
        warningDiv = document.createElement('div');
        warningDiv.id = 'inactivity-warning';
        warningDiv.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: #ff9800;
            color: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.3);
            z-index: 10000;
            max-width: 350px;
            font-family: Arial, sans-serif;
        `;

        warningDiv.innerHTML = `
            <div style="display: flex; align-items: center; gap: 10px;">
                <i class="bi bi-exclamation-triangle-fill" style="font-size: 24px;"></i>
                <div>
                    <strong>Sesión por expirar</strong>
                    <p style="margin: 5px 0 0 0; font-size: 14px;">
                        Tu sesión se cerrará en 5 minutos por inactividad.
                        Mueve el mouse o haz clic para continuar.
                    </p>
                </div>
            </div>
        `;

        document.body.appendChild(warningDiv);
    }
}

// Cerrar sesión por inactividad
function logoutDueToInactivity() {
    // Mostrar mensaje
    alert('Tu sesión ha expirado por inactividad. Serás redirigido al login.');

    // Redirigir al logout
    window.location.href = '/Account/Logout';
}

// Eventos que reinician el temporizador
const events = ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'];

events.forEach(event => {
    document.addEventListener(event, resetInactivityTimer, true);
});

// Iniciar el temporizador cuando se carga la página
document.addEventListener('DOMContentLoaded', () => {
    resetInactivityTimer();
    console.log('Sistema de timeout de sesión activado: 30 minutos de inactividad');
});
