// wwwroot/js/dashboard.js
document.addEventListener('DOMContentLoaded', () => {
    const cards = document.getElementById('cards');
    const q = document.getElementById('q');
    const refresh = document.getElementById('refresh');
    const totalPlazas = document.getElementById('totalPlazas');
    const totalPostulantes = document.getElementById('totalPostulantes');
    const ultimaActualizacion = document.getElementById('ultimaActualizacion');
    const baseUrl = window.dashboardCountsUrl || '/Dashboard/Counts';
    const url = `${baseUrl}?groupBy=${groupBy()}`;


    let timer = null;

    // Si no estamos en el dashboard, no hacemos nada
    if (!cards || !q || !refresh || !totalPlazas || !totalPostulantes || !ultimaActualizacion) {
        console.warn("dashboard.js: elementos clave no encontrados, saliendo.");
        return;
    }

    function groupBy() {
        const selected = document.querySelector('input[name="groupBy"]:checked');
        return selected ? selected.value : 'plaza';
    }

    function fmt(n) {
        return new Intl.NumberFormat().format(n ?? 0);
    }

    async function loadData() {
        try {
            const base = window.dashboardCountsUrl || '/Dashboard/Counts';
            const url = `${base}?groupBy=${encodeURIComponent(groupBy())}`;
            console.log("Dashboard → GET", url);

            const res = await fetch(url, {
                cache: 'no-store',
                credentials: 'include'   // no hace daño, y manda cookies igual
            });

            console.log("Dashboard → status:", res.status);

            if (!res.ok) {
                const txt = await res.text();
                console.error("Respuesta no OK:", txt);
                throw new Error(`HTTP ${res.status}`);
            }

            const data = await res.json();
            console.log("Dashboard → datos recibidos:", data);

            if (!Array.isArray(data)) {
                throw new Error("La respuesta no es un arreglo.");
            }

            render(data);
            updateSummary(data);
        } catch (err) {
            console.error("❌ Error cargando datos del Dashboard:", err);
            cards.innerHTML = `<div class="alert alert-danger w-100 text-center">Error al cargar datos del servidor.</div>`;
        }
    }



    function updateSummary(items) {
        const nPlazas = items.length;
        const nPostulantes = items.reduce((sum, x) => sum + (x.totalPostulantes || 0), 0);
        const now = new Date();

        totalPlazas.textContent = fmt(nPlazas);
        totalPostulantes.textContent = fmt(nPostulantes);
        ultimaActualizacion.textContent = now.toLocaleTimeString('es-CR');
    }

    function render(items) {
        const filtro = (q.value || '').trim().toLowerCase();
        cards.innerHTML = '';

        if (!items || items.length === 0) {
            cards.innerHTML = `<p class="text-muted text-center w-100">No hay plazas activas en este momento.</p>`;
            return;
        }

        items
            .filter(x =>
                !filtro ||
                (x.key?.toLowerCase().includes(filtro) ||
                    (x.subKey || '').toLowerCase().includes(filtro))
            )
            .forEach(x => {
                const el = document.createElement('div');
                el.className = 'card-box';
                el.innerHTML = `
                    <div style="display:flex; gap:.9rem; align-items:center;">
                        <div class="ring">${fmt(x.totalPostulantes)}</div>
                        <div>
                            <h5 class="card-h">${x.key}</h5>
                            ${x.subKey ? `<div class="card-sub"><i class="bi bi-building"></i> ${x.subKey}</div>` : ``}
                        </div>
                    </div>
                    <div class="card-footer">
                        <span class="badge-soft">
                            <i class="bi bi-bullseye"></i> ${fmt(x.plazasActivas)} plaza(s)
                        </span>
                        <!-- Por ahora sin botón de Ver/Postulantes para evitar más variables -->
                    </div>
                `;
                cards.appendChild(el);
            });
    }

    // Eventos
    q.addEventListener('input', () => loadData());
    document.querySelectorAll('input[name="groupBy"]').forEach(r =>
        r.addEventListener('change', loadData)
    );
    refresh.addEventListener('click', loadData);

    // Auto-refresh cada 15 s
    function startAuto() {
        if (timer) clearInterval(timer);
        timer = setInterval(loadData, 15000);
    }

    // Init
    loadData();
    startAuto();
});
