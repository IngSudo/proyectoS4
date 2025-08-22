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
        // LISTAR + BUSCAR
        [HttpGet]
        [Route("Departamentos")]
        [Route("Departamentos/Index")]
        public ActionResult Index(string q)
        {
            var prm = new SqlParameter("@q", (object)(q ?? string.Empty))
            { SqlDbType = SqlDbType.VarChar, Size = 100 };

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
            var next = row != null ? Convert.ToInt32(row["siguiente"]) : 1;
            return View(new Department { DeptNo = next });
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
                new SqlParameter("@dept_name", SqlDbType.VarChar, 50) { Value = model.DeptName?.Trim() ?? string.Empty });

            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            if (!ok) { ModelState.AddModelError("", msg); return View(model); }

            TempData["ok"] = msg;
            return RedirectToAction("Index");
        }

        // EDITAR (GET)
        [HttpGet]
        [Route("Departamentos/Edit/{id:int}")]
        public ActionResult Edit(int id)
        {
            var r = SqlHelper.ExecuteDataRow("dbo.spDept_Obtener",
                new SqlParameter("@dept_no", SqlDbType.Int) { Value = id });

            if (r == null) return HttpNotFound();

            var model = new Department
            {
                DeptNo = Convert.ToInt32(r["dept_no"]),
                DeptName = Convert.ToString(r["dept_name"]),
                IsActive = Convert.ToBoolean(r["is_active"])
            };
            return View(model); // Views/Departamentos/Edit.cshtml
        }

        // EDITAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Departamentos/Edit")]
        public ActionResult Edit(Department model)
        {
            if (!ModelState.IsValid) return View(model);

            var row = SqlHelper.ExecuteDataRow("dbo.spDept_Editar",
                new SqlParameter("@dept_no", SqlDbType.Int) { Value = model.DeptNo },
                new SqlParameter("@dept_name", SqlDbType.VarChar, 50) { Value = model.DeptName?.Trim() ?? string.Empty });

            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            if (!ok) { ModelState.AddModelError("", msg); return View(model); }

            TempData["ok"] = msg;
            return RedirectToAction("Index");
        }

        // DESACTIVAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Departamentos/Desactivar/{id:int}")]
        public ActionResult Desactivar(int id)
        {
            var row = SqlHelper.ExecuteDataRow("dbo.spDept_Desactivar",
                new SqlParameter("@dept_no", SqlDbType.Int) { Value = id });

            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            TempData[ok ? "ok" : "err"] = msg;
            return RedirectToAction("Index");
        }

    }
}
