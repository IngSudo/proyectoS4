using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AplicacionNomina.Models
{
    public class NominaVigente
    {
        public int EmpNo { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Departamento { get; set; }
        public long Salario { get; set; }

    }
}