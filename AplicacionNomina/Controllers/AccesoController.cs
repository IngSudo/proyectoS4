using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AplicacionNomina.Controllers
{
    public class AccesoController : Controller
    {

        //GET: Acceso
        public ActionResult Index()
        {
            return View();
        }   

        public ActionResult Autenticar()
        {
            return View();
        }

        //Post: Acceso
        [HttpPost]
        public ActionResult Autenticar(string usuario, string clave)
        {
            if (usuario == "admin" && clave == "1234")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Usuario o clave incorrectos.";
                return View();
            }
        }
    }
}
