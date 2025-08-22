using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AplicacionNomina.Models
{
    public class ReportesViewModel
    {
        public ReportParameters Parametros { get; set; } = new ReportParameters();
        public List<Departamento> Departamentos { get; set; } = new List<Departamento>();

        public object DatosReporte { get; set; }

        public List<DepartamentoViewModel> DatosReporteEstructura { get; set; } = new List<DepartamentoViewModel>();

        public string TipoReporte { get; set; }
        public bool MostrarResultados { get; set; } = false;
        public bool SinFecha { get; set; }

    }
}