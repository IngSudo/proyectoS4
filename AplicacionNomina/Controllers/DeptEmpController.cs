using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using AplicacionNomina.Models;

namespace AplicacionNomina.Controllers
{
    public class DeptEmpController : Controller
    {
        // ---------------------------
        // Helper: cargar combo de departamentos
        // ---------------------------
        private void CargarDepartamentos(int? seleccionado = null)
        {
            var dt = SqlHelper.ExecuteDataTable("dbo.spDept_Listar",
                new SqlParameter("@q", SqlDbType.VarChar, 100) { Value = "" });

            var items = new List<SelectListItem>();
            foreach (DataRow r in dt.Rows)
            {
                var deptNo = Convert.ToInt32(r["dept_no"]);
                items.Add(new SelectListItem
                {
                    Value = deptNo.ToString(),
                    Text = Convert.ToString(r["dept_name"]),
                    Selected = (seleccionado.HasValue && seleccionado.Value == deptNo)
                });
            }
            ViewBag.Departamentos = items; // IEnumerable<SelectListItem>
        }

        // ---------------------------
        // LISTAR asignaciones de un empleado
        // GET: /DeptEmp/{empNo}
        // ---------------------------
        [HttpGet]
        public ActionResult Index(int empNo)
        {
            // nombre del empleado
            var eRow = SqlHelper.ExecuteDataRow("dbo.spEmpleados_Obtener",
                new SqlParameter("@emp_no", SqlDbType.Int) { Value = empNo });
            if (eRow == null) return HttpNotFound();

            ViewBag.EmpNo = empNo;
            ViewBag.Empleado = $"{eRow["first_name"]} {eRow["last_name"]}";

            // asignaciones
            var dt = SqlHelper.ExecuteDataTable("dbo.spDeptEmp_ListarPorEmpleado",
                new SqlParameter("@emp_no", SqlDbType.Int) { Value = empNo });

            var list = new List<DeptEmpVM>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new DeptEmpVM
                {
                    EmpNo = Convert.ToInt32(r["emp_no"]),
                    DeptNo = Convert.ToInt32(r["dept_no"]),
                    DeptName = Convert.ToString(r["dept_name"]),
                    FromDate = Convert.ToDateTime(r["from_date"]),
                    ToDate = Convert.ToDateTime(r["to_date"])
                });
            }
            return View(list); // Views/DeptEmp/Index.cshtml
        }

        // ---------------------------
        // CREAR asignación (GET)
        // GET: /DeptEmp/Create?empNo=101
        // ---------------------------
        [HttpGet]
        public ActionResult Create(int empNo)
        {
            ViewBag.EmpNo = empNo;

            // opcional: nombre del empleado
            var eRow = SqlHelper.ExecuteDataRow("dbo.spEmpleados_Obtener",
                new SqlParameter("@emp_no", SqlDbType.Int) { Value = empNo });
            ViewBag.Empleado = eRow != null ? $"{eRow["first_name"]} {eRow["last_name"]}" : null;

            CargarDepartamentos(null); // combo sin selección

            return View(new DeptEmpVM
            {
                EmpNo = empNo,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today
            });
        }

        // ---------------------------
        // CREAR asignación (POST)
        // ---------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DeptEmpVM model)
        {
            ViewBag.EmpNo = model.EmpNo;

            if (!ModelState.IsValid)
            {
                CargarDepartamentos(model.DeptNo); // recargar combo
                return View(model);
            }

            var row = SqlHelper.ExecuteDataRow("dbo.spDeptEmp_Crear",
                new SqlParameter("@emp_no", SqlDbType.Int) { Value = model.EmpNo },
                new SqlParameter("@dept_no", SqlDbType.Int) { Value = model.DeptNo },
                new SqlParameter("@from_date", SqlDbType.Date) { Value = model.FromDate },
                new SqlParameter("@to_date", SqlDbType.Date) { Value = model.ToDate });

            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            if (!ok)
            {
                ModelState.AddModelError("", msg);
                CargarDepartamentos(model.DeptNo); // mantener selección
                return View(model);
            }

            TempData["ok"] = msg; // "Asignación registrada."
            return RedirectToAction("Index", new { empNo = model.EmpNo });
        }

        // ---------------------------
        // ELIMINAR asignación (por PK)
        // POST: /DeptEmp/Delete
        // ---------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int empNo, int deptNo, DateTime fromDate)
        {
            var row = SqlHelper.ExecuteDataRow("dbo.spDeptEmp_Eliminar",
                new SqlParameter("@emp_no", SqlDbType.Int) { Value = empNo },
                new SqlParameter("@dept_no", SqlDbType.Int) { Value = deptNo },
                new SqlParameter("@from_date", SqlDbType.Date) { Value = fromDate });

            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            TempData[ok ? "ok" : "err"] = msg;
            return RedirectToAction("Index", new { empNo });
        }
    }
}
