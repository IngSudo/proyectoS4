using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AplicacionNomina.Models
{
    public class CambioSalarial
    {
        public int EmpNo { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Departamento { get; set; }
        public decimal SalarioAnterior { get; set; }
        public decimal SalarioNuevo { get; set; }
        public DateTime FechaCambio { get; set; }
        public decimal DiferenciaSalario => SalarioNuevo - SalarioAnterior;
        public decimal PorcentajeCambio => SalarioAnterior != 0 ? ((SalarioNuevo - SalarioAnterior) / SalarioAnterior) * 100 : 0;
    }
}