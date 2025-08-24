using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AplicacionNomina.Models
{
    public class EstructuraOrganizacional
    {
        public int DeptNo { get; set; }
        public int EmpNo { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public decimal Salario { get; set; }
        public string FechaContratacion { get; set; }
        public string Titulo { get; set; }
        public string NombreDepartamento { get; set; }
        public string NombreManager { get; set; }
        public string ApellidoManager { get; set; }
        public List<EmpleadoDepartamento> Empleados { get; set; } = new List<EmpleadoDepartamento>();
    }
}