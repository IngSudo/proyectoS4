using AplicacionNomina.DAL;
using AplicacionNomina.Models;
using AplicacionNomina.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace AplicacionNomina.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ReportsDAL reportsDAL;
        private readonly ReportExportService exportService;

        public ReportesController()
        {
            reportsDAL = new ReportsDAL();
            exportService = new ReportExportService();
        }

        // GET: Reportes
        public ActionResult Index()
        {
            var viewModel = new ReportesViewModel();
            viewModel.Departamentos = reportsDAL.ObtenerDepartamentos();

            viewModel.Departamentos.Insert(0, new Departamento
            {
                DeptNo = 0,
                DeptName = "Todos los departamentos"
            });

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult GenerarReporte(ReportesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departamentos = reportsDAL.ObtenerDepartamentos();
                model.Departamentos.Insert(0, new Departamento { DeptNo = 0, DeptName = "Todos los departamentos" });
                return View("Index", model);
            }

            try
            {
                model.Departamentos = reportsDAL.ObtenerDepartamentos();
                model.Departamentos.Insert(0, new Departamento { DeptNo = 0, DeptName = "Todos los departamentos" });
                model.TipoReporte = model.Parametros.TipoReporte;
                model.MostrarResultados = true;

                switch (model.Parametros.TipoReporte)
                {
                    case "nomina-vigente":
                        var deptNo = model.Parametros.DeptNo != 0 ? (int?)model.Parametros.DeptNo : null;

                        DateTime? fecha = model.SinFecha ? (DateTime?)null : model.Parametros.FechaInicio;

                        model.DatosReporte = reportsDAL.ObtenerNominaVigente(deptNo, fecha);
                        break;

                    case "cambios-salariales":
                        if (!model.Parametros.FechaInicio.HasValue || !model.Parametros.FechaFin.HasValue)
                        {
                            ModelState.AddModelError("", "Para el reporte de cambios salariales debe especificar rango de fechas");
                            model.MostrarResultados = false;
                            return View("Index", model);
                        }
                        var deptNoCambios = model.Parametros.DeptNo != 0 ? (int?)model.Parametros.DeptNo : null;
                        model.DatosReporte = reportsDAL.ObtenerCambiosSalariales(
                            model.Parametros.FechaInicio.Value,
                            model.Parametros.FechaFin.Value,
                            deptNoCambios);
                        break;

                    case "estructura-organizacional":
                        var deptNoEstructura = model.Parametros.DeptNo != 0 ? (int?)model.Parametros.DeptNo : null;
                        model.DatosReporte = reportsDAL.ObtenerEstructuraOrganizacional(deptNoEstructura);
                        model.MostrarResultados = true;
                        break;
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al generar el reporte: {ex.Message}");
                model.Departamentos = reportsDAL.ObtenerDepartamentos();
                model.Departamentos.Insert(0, new Departamento { DeptNo = 0, DeptName = "Todos los departamentos" });
                model.MostrarResultados = false;
                return View("Index", model);
            }
        }

        [HttpPost]
        public ActionResult ExportarPDF(string tipoReporte, int? deptNo, DateTime? fechaInicio, DateTime? fechaFin, bool sinFecha)
        {
            try
            {
                object datos = null;
                string nombreArchivo = "";

                DateTime? fecha = sinFecha ? (DateTime?)null : fechaInicio;

                var deptNoEstructura = deptNo.HasValue && deptNo.Value != 0 ? deptNo : null;

                switch (tipoReporte)
                {
                    case "nomina-vigente":
                        var deptNoNomina = deptNo.HasValue && deptNo.Value != 0 ? deptNo : null;
                        datos = reportsDAL.ObtenerNominaVigente(deptNoNomina, fecha);
                        nombreArchivo = "NominaVigente";
                        break;

                    case "cambios-salariales":
                        if (!fechaInicio.HasValue || !fechaFin.HasValue)
                            return Json(new { success = false, message = "Fechas requeridas" });

                        var deptNoCambios = deptNo.HasValue && deptNo.Value != 0 ? deptNo : null;
                        datos = reportsDAL.ObtenerCambiosSalariales(fechaInicio.Value, fechaFin.Value, deptNoCambios);
                        nombreArchivo = "CambiosSalariales";
                        break;

                    case "estructura-organizacional":
                        nombreArchivo = "EstructuraOrganizacional";

                        var departamentos = reportsDAL.ObtenerEstructuraOrganizacional(deptNoEstructura);

                        using (var ms = new MemoryStream())
                        {
                            var doc = new iTextSharp.text.Document();
                            var writer = PdfWriter.GetInstance(doc, ms);
                            doc.Open();

                            foreach (var dept in departamentos)
                            {
                                doc.Add(new Paragraph($"Departamento: {dept.NombreDepartamento}"));
                                doc.Add(new Paragraph($"Manager: {dept.NombreManager} {dept.ApellidoManager}"));
                                doc.Add(new Paragraph("Empleados:"));

                                foreach (var emp in dept.Empleados)
                                {
                                    doc.Add(new Paragraph($" - {emp.Nombre} {emp.Apellido}, {emp.Titulo}, Salario: {emp.Salario}"));
                                }

                                doc.Add(new Paragraph("---------------------------------------------------"));
                            }

                            doc.Close();
                            return File(ms.ToArray(), "application/pdf", $"{nombreArchivo}_{DateTime.Now:yyyyMMdd}.pdf");
                        }

                    default:
                        return Json(new { success = false, message = "Tipo de reporte no válido" });
                }

                var pdfBytes = exportService.ExportarAPDF(datos, tipoReporte, nombreArchivo);
                return File(pdfBytes, "application/pdf", $"{nombreArchivo}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al exportar PDF: {ex.Message}" });
            }
        }

        [HttpPost]
        public ActionResult ExportarExcel(string tipoReporte, int? deptNo, DateTime? fechaInicio, DateTime? fechaFin, bool sinFecha)
        {
            try
            {
                object datos = null;
                string nombreArchivo = "";

                DateTime? fecha = sinFecha ? (DateTime?)null : fechaInicio;

                var deptNoEstructura = deptNo.HasValue && deptNo.Value != 0 ? deptNo : null;

                switch (tipoReporte)
                {
                    case "nomina-vigente":
                        var deptNoNomina = deptNo.HasValue && deptNo.Value != 0 ? deptNo : null;
                        datos = reportsDAL.ObtenerNominaVigente(deptNoNomina, fecha);
                        nombreArchivo = "NominaVigente";
                        break;

                    case "cambios-salariales":
                        if (!fechaInicio.HasValue || !fechaFin.HasValue)
                            return Json(new { success = false, message = "Fechas requeridas" });

                        var deptNoCambios = deptNo.HasValue && deptNo.Value != 0 ? deptNo : null;
                        datos = reportsDAL.ObtenerCambiosSalariales(fechaInicio.Value, fechaFin.Value, deptNoCambios);
                        nombreArchivo = "CambiosSalariales";
                        break;

                    case "estructura-organizacional":
                        var departamentos = reportsDAL.ObtenerEstructuraOrganizacional(deptNoEstructura);

                        var empleadosEstructura = departamentos
                            .SelectMany(d => d.Empleados.Select(e => new EstructuraOrganizacional
                            {
                                DeptNo = d.DeptNo,
                                NombreDepartamento = d.NombreDepartamento,
                                NombreManager = d.NombreManager,
                                ApellidoManager = d.ApellidoManager,
                                EmpNo = e.EmpNo,
                                Nombre = e.Nombre,
                                Apellido = e.Apellido,
                                Salario = e.Salario,
                                FechaContratacion = e.FechaContratacion,
                                Titulo = e.Titulo
                            }))
                            .ToList();

                        datos = empleadosEstructura;
                        nombreArchivo = "EstructuraOrganizacional";
                        break;

                    default:
                        return Json(new { success = false, message = "Tipo de reporte no válido" });
                }

                var excelBytes = exportService.ExportarAExcel(datos, tipoReporte, nombreArchivo);

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"{nombreArchivo}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al exportar Excel: {ex.Message}" });
            }
        }
    }
}