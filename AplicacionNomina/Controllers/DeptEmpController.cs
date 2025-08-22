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
        // LISTAR asignaciones de un empleado
        [HttpGet]
        [Route("DeptEmp/{empNo:int}")]
        public ActionResult Index(int empNo)
        {
            // Trae nombre del empleado (usamos sp existente)
            var eRow = SqlHelper.ExecuteDataRow("dbo.spEmpleados_Obtener",
                new SqlParameter("@emp_no", SqlDbType.Int) { Value = empNo });
            if (eRow == null) return HttpNotFound();

            ViewBag.EmpNo = empNo;
            ViewBag.Empleado = $"{eRow["first_name"]} {eRow["last_name"]}";

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

        // GET: crear asignación
        [HttpGet]
        [Route("DeptEmp/Create/{empNo:int}")]
        public ActionResult Create(int empNo)
        {
            ViewBag.EmpNo = empNo;

            // combo de departamentos activos
            var deps = SqlHelper.ExecuteDataTable("dbo.spDept_Listar",
                new SqlParameter("@q", SqlDbType.VarChar, 100) { Value = "" });

            ViewBag.Departamentos = new SelectList(deps.AsEnumerable(),
                dataValueField: "dept_no",
                dataTextField: "dept_name");

            // valores por defecto
            return View(new DeptEmpVM
            {
                EmpNo = empNo,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today
            });
        }

        // POST: crear asignación
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("DeptEmp/Create")]
        public ActionResult Create(DeptEmpVM model)
        {
            ViewBag.EmpNo = model.EmpNo;

            var deps = SqlHelper.ExecuteDataTable("dbo.spDept_Listar",
                new SqlParameter("@q", SqlDbType.VarChar, 100) { Value = "" });
            ViewBag.Departamentos = new SelectList(deps.AsEnumerable(), "dept_no", "dept_name", model.DeptNo);

            if (!ModelState.IsValid) return View(model);

            var row = SqlHelper.ExecuteDataRow("dbo.spDeptEmp_Crear",
                new SqlParameter("@emp_no", SqlDbType.Int) { Value = model.EmpNo },
                new SqlParameter("@dept_no", SqlDbType.Int) { Value = model.DeptNo },
                new SqlParameter("@from_date", SqlDbType.Date) { Value = model.FromDate },
                new SqlParameter("@to_date", SqlDbType.Date) { Value = model.ToDate });

            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            if (!ok) { ModelState.AddModelError("", msg); return View(model); }

            TempData["ok"] = msg;
            return RedirectToAction("Index", new { empNo = model.EmpNo });
        }

        // POST: eliminar asignación (por PK)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("DeptEmp/Delete")]
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
