using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using AplicacionNomina.Models;

namespace AplicacionNomina.Controllers
{
    public class AccesoController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View(new LoginVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginVM model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var row = SqlHelper.ExecuteDataRow("dbo.spAcceso_Login",
                new SqlParameter("@usuario", SqlDbType.VarChar, 100) { Value = model.Usuario?.Trim() ?? "" },
                new SqlParameter("@clave_plana", SqlDbType.VarChar, 200) { Value = model.Clave ?? "" });

            var ok = row != null && Convert.ToInt32(row["ok"]) == 1;
            var msg = row != null ? Convert.ToString(row["mensaje"]) : "Error inesperado.";

            if (!ok)
            {
                ModelState.AddModelError("", msg);
                return View(model);
            }

            // Guardar sesión mínima
            Session["EmpNo"] = Convert.ToInt32(row["emp_no"]);
            Session["Usuario"] = model.Usuario;

            // Redirigir
            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Salir()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
