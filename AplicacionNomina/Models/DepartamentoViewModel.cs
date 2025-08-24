using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AplicacionNomina.Models
{
    public class DepartamentoViewModel
    {
        public int DeptNo { get; set; }
        public string NombreDepartamento { get; set; }
        public string NombreManager { get; set; }
        public string ApellidoManager { get; set; }
        public List<EstructuraOrganizacional> Empleados { get; set; }
    }
}