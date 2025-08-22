using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using AplicacionNomina.Models;

namespace AplicacionNomina.Controllers
{
    public class EmpleadosController : Controller
    {
        // LISTAR: usa spEmpleados_Listar
        [HttpGet]
        [Route("Empleados")]
        [Route("Empleados/Index")]
        public ActionResult Index(string q)
        {
            var prm = new SqlParameter("@q", (object)(q ?? string.Empty)) { SqlDbType = SqlDbType.VarChar, Size = 100 };
            var dt = SqlHelper.ExecuteDataTable("dbo.spEmpleados_Listar", prm);

            var list = new List<Employee>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new Employee
                {
                    EmpNo = Convert.ToInt32(r["emp_no"]),
                    CI = Convert.ToString(r["ci"]),
                    BirthDate = Convert.ToDateTime(r["birth_date"]),
                    FirstName = Convert.ToString(r["first_name"]),
                    LastName = Convert.ToString(r["last_name"]),
                    Gender = Convert.ToString(r["gender"])[0],
                    HireDate = Convert.ToDateTime(r["hire_date"]),
                    Correo = r["correo"] == DBNull.Value ? null : Convert.ToString(r["correo"]),
                    IsActive = Convert.ToBoolean(r["is_active"])
                });
            }

            ViewBag.Busqueda = q;
            return View(list); // Views/Empleados/Index.cshtml
        }

        // GET: /Empleados/Create
        [HttpGet]
        [Route("Empleados/Create")]
        public ActionResult Create()
        {
            ViewBag.Generos = new SelectList(new[]
            {
        new { Id = "M", Texto = "Masculino" },
        new { Id = "F", Texto = "Femenino"  },
        new { Id = "O", Texto = "Otro"      }
    }, "Id", "Texto");

            // 1) Trae correlativo desde el SP
            var row = SqlHelper.ExecuteDataRow("dbo.spEmpleados_SiguienteEmpNo");
            var nextEmpNo = row != null ? Convert.ToInt32(row["siguiente"]) : 1;

            // 2) Devuelve el MODELO con EmpNo ya seteado (así TextBoxFor lo toma)
            var model = new Employee
            {
                EmpNo = nextEmpNo,
                HireDate = DateTime.Today   // opcional: valores por defecto
            };

            return View(model);
        }

        // POST: /Empleados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Empleados/Create")]
        public ActionResult Create(Employee model)
        {
            // Repoblar el combo siempre que vuelves a la vista
            ViewBag.Generos = new SelectList(new[]
            {
        new { Id = "M", Texto = "Masculino" },
        new { Id = "F", Texto = "Femenino"  },
        new { Id = "O", Texto = "Otro"      }
    }, "Id", "Texto", model.Gender);

            if (!ModelState.IsValid)
                return View(model);

            // 3) Normaliza y prepara parámetros
            model.FirstName = model.FirstName?.Trim();
            model.LastName = model.LastName?.Trim();
            model.CI = model.CI?.Trim();
            var correoVal = string.IsNullOrWhiteSpace(model.Correo)
                                ? (object)DBNull.Value
                                : (object)model.Correo.Trim();

            var prms = new[]
            {
        new SqlParameter("@emp_no",     SqlDbType.Int){ Value = model.EmpNo },
        new SqlParameter("@ci",         SqlDbType.VarChar, 50){ Value = model.CI ?? string.Empty },
        new SqlParameter("@birth_date", SqlDbType.Date){ Value = model.BirthDate },
        new SqlParameter("@first_name", SqlDbType.VarChar, 50){ Value = model.FirstName ?? string.Empty },
        new SqlParameter("@last_name",  SqlDbType.VarChar, 50){ Value = model.LastName ?? string.Empty },
        new SqlParameter("@gender",     SqlDbType.Char, 1){ Value = model.Gender },
        new SqlParameter("@hire_date",  SqlDbType.Date){ Value = model.HireDate },
        new SqlParameter("@correo",     SqlDbType.VarChar, 100){ Value = correoVal }
    };

            var row = SqlHelper.ExecuteDataRow("dbo.spEmpleados_Crear", prms);
            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            if (!ok)
            {
                ModelState.AddModelError("", msg);
                return View(model); // vuelve a mostrar errores del SP
            }

            TempData["ok"] = msg; // "Empleado creado correctamente."
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Empleados/Edit/{id}")]
        public ActionResult Edit(int id)
        {
            var r = SqlHelper.ExecuteDataRow("dbo.spEmpleados_Obtener",
                new SqlParameter("@emp_no", SqlDbType.Int) { Value = id });

            if (r == null) return HttpNotFound();

            var model = new Employee
            {
                EmpNo = Convert.ToInt32(r["emp_no"]),
                CI = Convert.ToString(r["ci"]),
                BirthDate = Convert.ToDateTime(r["birth_date"]),
                FirstName = Convert.ToString(r["first_name"]),
                LastName = Convert.ToString(r["last_name"]),
                Gender = Convert.ToString(r["gender"])[0],
                HireDate = Convert.ToDateTime(r["hire_date"]),
                Correo = r["correo"] == DBNull.Value ? null : Convert.ToString(r["correo"]),
                IsActive = Convert.ToBoolean(r["is_active"])
            };

            ViewBag.Generos = new SelectList(new[]
            {
        new { Id = "M", Texto = "Masculino" },
        new { Id = "F", Texto = "Femenino"  },
        new { Id = "O", Texto = "Otro"      }
    }, "Id", "Texto", model.Gender);

            return View(model); // Views/Empleados/Edit.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Empleados/Edit")]
        public ActionResult Edit(Employee model)
        {
            ViewBag.Generos = new SelectList(new[]
            {
        new { Id = "M", Texto = "Masculino" },
        new { Id = "F", Texto = "Femenino"  },
        new { Id = "O", Texto = "Otro"      }
    }, "Id", "Texto", model.Gender);

            if (!ModelState.IsValid) return View(model);

            var correoVal = string.IsNullOrWhiteSpace(model.Correo)
                ? (object)DBNull.Value : (object)model.Correo.Trim();

            var row = SqlHelper.ExecuteDataRow("dbo.spEmpleados_Editar",
                new SqlParameter("@emp_no", SqlDbType.Int) { Value = model.EmpNo },
                new SqlParameter("@ci", SqlDbType.VarChar, 50) { Value = model.CI?.Trim() ?? string.Empty },
                new SqlParameter("@birth_date", SqlDbType.Date) { Value = model.BirthDate },
                new SqlParameter("@first_name", SqlDbType.VarChar, 50) { Value = model.FirstName?.Trim() ?? string.Empty },
                new SqlParameter("@last_name", SqlDbType.VarChar, 50) { Value = model.LastName?.Trim() ?? string.Empty },
                new SqlParameter("@gender", SqlDbType.Char, 1) { Value = model.Gender },
                new SqlParameter("@hire_date", SqlDbType.Date) { Value = model.HireDate },
                new SqlParameter("@correo", SqlDbType.VarChar, 100) { Value = correoVal }
            );

            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            if (!ok) { ModelState.AddModelError("", msg); return View(model); }

            TempData["ok"] = msg; // "Empleado actualizado correctamente."
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Empleados/Desactivar/{id}")]
        public ActionResult Desactivar(int id)
        {
            var row = SqlHelper.ExecuteDataRow("dbo.spEmpleados_Desactivar",
                new SqlParameter("@emp_no", SqlDbType.Int) { Value = id });

            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            TempData[ok ? "ok" : "err"] = msg;
            return RedirectToAction("Index");
        }


    }
}
