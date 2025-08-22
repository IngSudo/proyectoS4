using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AplicacionNomina.Models
{
    public class EmpleadoDepartamento
    {
        public int EmpNo { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public decimal Salario { get; set; }
        public DateTime FechaContratacion { get; set; }
        public string Titulo { get; set; }
    }
}