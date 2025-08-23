using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

//librerias conectar SQL
using System.Data;
using System.Data.SqlClient;

using AplicacionNomina.Models;
using System.Text;
using System.Configuration;
using System.Web.UI.WebControls;

//encriptar
using System.Security.Cryptography;

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
        public ActionResult Autenticar(Employee oEmpleado)
        {
            try
            {
                using(SqlConnection cn=new SqlConnection(ConfigurationManager.ConnectionStrings["NominaContext"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("Select * from employees");
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                ViewBag.Error = "Usuario o clave incorrectos.";
                return View();
            }
        }
    }
}
