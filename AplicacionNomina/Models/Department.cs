using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AplicacionNomina.Models
{
    [Table("departments")]
    public class Department
    {
        [Key]
        [Column("dept_no")]
        [Display(Name = "N.º Departamento")]
        public int DeptNo { get; set; }

        [Required, StringLength(50)]
        [Column("dept_name")]
        [Display(Name = "Nombre")]
        public string DeptName { get; set; }

        [Column("is_active")]
        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;
    }
}
