using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using AplicacionNomina.Models;

public class DeptEmpController : Controller
{
    string connectionString = "Data Source=.;Initial Catalog=Nomina;Integrated Security=True";

    // MÉTODO AUXILIAR → llena el ViewBag con los departamentos
    private void CargarDepartamentos(string seleccionado = null)
    {
        var items = new List<SelectListItem>();

        using (var conn = new SqlConnection(connectionString))
        using (var cmd = new SqlCommand("sp_ListarDepartamentos", conn)) // tu SP que lista departamentos
        {
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new SelectListItem
                {
                    Value = reader["dept_no"].ToString(),
                    Text = reader["dept_name"].ToString(),
                    Selected = (seleccionado != null && seleccionado == reader["dept_no"].ToString())
                });
            }
        }

        ViewBag.Departamentos = items;
    }

    // GET: DeptEmp/Create
    public ActionResult Create(int empNo)
    {
        var model = new DeptEmpVM { EmpNo = empNo };

        ViewBag.EmpNo = empNo;
        ViewBag.Empleado = ObtenerNombreEmpleado(empNo); // opcional

        CargarDepartamentos();

        return View(model);
    }

    // POST: DeptEmp/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(DeptEmpVM model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("sp_AsignarEmpleadoDepartamento", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmpNo", model.EmpNo);
                    cmd.Parameters.AddWithValue("@DeptNo", model.DeptNo);
                    cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", model.ToDate);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("Index", new { empNo = model.EmpNo });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
            }
        }

        // Si hubo error, recargar combos
        CargarDepartamentos(model.DeptNo?.ToString());

        return View(model);
    }

    private string ObtenerNombreEmpleado(int empNo)
    {
        string nombre = "";
        using (var conn = new SqlConnection(connectionString))
        using (var cmd = new SqlCommand("sp_ObtenerEmpleadoPorId", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@EmpNo", empNo);
            conn.Open();
            var reader = cmd.ExecuteReader();
            if (reader.Read())
                nombre = reader["FullName"].ToString();
        }
        return nombre;
    }
}
