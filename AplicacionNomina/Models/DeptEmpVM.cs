using System;
using System.ComponentModel.DataAnnotations;

namespace AplicacionNomina.Models
{
    public class DeptEmpVM
    {
        [Display(Name = "N.º Empleado")]
        public int EmpNo { get; set; }

        [Display(Name = "N.º Departamento")]
        public int DeptNo { get; set; }

        [Display(Name = "Departamento")]
        public string DeptName { get; set; }

        [Display(Name = "Desde")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [Display(Name = "Hasta")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }
    }
}
