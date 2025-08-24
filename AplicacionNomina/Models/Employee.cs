using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AplicacionNomina.Models
{
    [Table("employees")]
    public class Employee
    {
        [Key, Column("emp_no")]
        [Display(Name = "N.º Empleado")]
        public int EmpNo { get; set; }

        [Required, Column("ci"), StringLength(50)]
        [Display(Name = "Cédula")]
        public string CI { get; set; }

        [Required, Column("birth_date")]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime BirthDate { get; set; }

        [Required, Column("first_name"), StringLength(50)]
        [Display(Name = "Nombres")]
        public string FirstName { get; set; }

        [Required, Column("last_name"), StringLength(50)]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; }

        [Required, Column("gender")]
        [Display(Name = "Género")]
        public char Gender { get; set; }

        [Required, Column("hire_date")]
        [Display(Name = "Fecha de ingreso")]
        public DateTime HireDate { get; set; }

        [Column("correo"), StringLength(100)]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        [Column("is_active")]
        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
