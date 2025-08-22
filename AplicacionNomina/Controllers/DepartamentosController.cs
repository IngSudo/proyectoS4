using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using AplicacionNomina.Models;

namespace AplicacionNomina.Controllers
{
    public class DepartamentosController : Controller
    {
        // LISTAR
        [HttpGet]
        [Route("Departamentos")]
        [Route("Departamentos/Index")]
        public ActionResult Index(string q)
        {
            var prm = new SqlParameter("@q", (object)(q ?? string.Empty)) { SqlDbType = SqlDbType.VarChar, Size = 100 };
            var dt = SqlHelper.ExecuteDataTable("dbo.spDept_Listar", prm);

            var list = new List<Department>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new Department
                {
                    DeptNo = Convert.ToInt32(r["dept_no"]),
                    DeptName = Convert.ToString(r["dept_name"]),
                    IsActive = Convert.ToBoolean(r["is_active"])
                });
            }
            ViewBag.Busqueda = q;
            return View(list); // Views/Departamentos/Index.cshtml
        }

        // CREAR (GET)
        [HttpGet]
        [Route("Departamentos/Create")]
        public ActionResult Create()
        {
            var row = SqlHelper.ExecuteDataRow("dbo.spDept_SiguienteDeptNo");
            var nextNo = row != null ? Convert.ToInt32(row["siguiente"]) : 1;
            return View(new Department { DeptNo = nextNo });
        }

        // CREAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Departamentos/Create")]
        public ActionResult Create(Department model)
        {
            if (!ModelState.IsValid) return View(model);

            var row = SqlHelper.ExecuteDataRow("dbo.spDept_Crear",
                new SqlParameter("@dept_no", SqlDbType.Int) { Value = model.DeptNo },
                new SqlParameter("@dept_name", SqlDbType.VarChar, 50) { Value = model.DeptName?.Trim() ?? string.Empty }
            );

            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            if (!ok) { ModelState.AddModelError("", msg); return View(model); }

            TempData["ok"] = msg;
            return RedirectToAction("Index");
        }
    }
}
