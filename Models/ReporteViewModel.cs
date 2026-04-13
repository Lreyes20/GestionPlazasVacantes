using GestionPlazasVacantes.Models;

namespace GestionPlazasVacantes.Models
{
    public class ReporteViewModel
    {

        public List<PlazaVacante> PlazasDisponibles { get; set; } = new();
        public List<SeguimientoPostulante> Seguimientos { get; set; } = new();
        public PlazaVacante? Plaza { get; set; }

        public int TotalParticipantes { get; set; }
        public int DocumentacionCompleta { get; set; }
        public int DocumentacionIncompleta { get; set; }
        public int AprobaronTecnica { get; set; }
        public int AprobaronPsicometrica { get; set; }
        public int AprobaronEntrevista { get; set; }
        public int CandidatosElegibles { get; set; }
        public int Seleccionados { get; set; }
    }
}