using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AplicacionNomina.Models
{
    public class ReportParameters
    {
        [Display(Name = "Departamento")]
        public int DeptNo { get; set; }

        [Display(Name = "Fecha Inicio")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [Display(Name = "Fecha Fin")]
        [DataType(DataType.Date)]
        public DateTime? FechaFin { get; set; }

        [Display(Name = "Tipo de Reporte")]
        public string TipoReporte { get; set; }
    }
}