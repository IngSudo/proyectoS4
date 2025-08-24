using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using AplicacionNomina.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace AplicacionNomina.Services
{
    public class ReportExportService
    {
        public byte[] ExportarAPDF(object datos, string tipoReporte, string nombreArchivo)
        {
            using (var memoryStream = new MemoryStream())
            {
                var document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Fuentes
                var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);

                // Título del documento
                var titulo = new Paragraph($"Reporte: {ObtenerTituloReporte(tipoReporte)}", fontTitle);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 20;
                document.Add(titulo);

                // Fecha de generación
                var fechaGeneracion = new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}", fontNormal);
                fechaGeneracion.Alignment = Element.ALIGN_RIGHT;
                fechaGeneracion.SpacingAfter = 15;
                document.Add(fechaGeneracion);

                switch (tipoReporte)
                {
                    case "nomina-vigente":
                        AgregarTablaNominaVigente(document, (List<NominaVigente>)datos, fontHeader, fontNormal);
                        break;
                    case "cambios-salariales":
                        AgregarTablaCambiosSalariales(document, (List<CambioSalarial>)datos, fontHeader, fontNormal);
                        break;
                    case "estructura-organizacional":
                        AgregarTablaEstructuraOrganizacional(document, (List<EstructuraOrganizacional>)datos, fontHeader, fontNormal);
                        break;
                }

                document.Close();
                writer.Close();

                return memoryStream.ToArray();
            }
        }

        public byte[] ExportarAExcel(object datos, string tipoReporte, string nombreArchivo)
        {
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(ObtenerTituloReporte(tipoReporte));

                switch (tipoReporte)
                {
                    case "nomina-vigente":
                        CrearHojaNominaVigente(worksheet, (List<NominaVigente>)datos);
                        break;
                    case "cambios-salariales":
                        CrearHojaCambiosSalariales(worksheet, (List<CambioSalarial>)datos);
                        break;
                    case "estructura-organizacional":
                        CrearHojaEstructuraOrganizacional(worksheet, (List<EstructuraOrganizacional>)datos);
                        break;
                }

                return package.GetAsByteArray();
            }
        }

        private void AgregarTablaNominaVigente(Document document, List<NominaVigente> datos, Font fontHeader, Font fontNormal)
        {
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 10, 20, 20, 25, 15 });

            // Headers
            var headers = new[] { "No. Emp", "Nombre", "Apellido", "Departamento", "Salario" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, fontHeader))
                {
                    BackgroundColor = new BaseColor(52, 73, 94),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 8
                };
                table.AddCell(cell);
            }

            // Datos
            decimal totalSalarios = 0;
            foreach (var item in datos)
            {
                table.AddCell(new PdfPCell(new Phrase(item.EmpNo.ToString(), fontNormal)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.Nombre, fontNormal)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.Apellido, fontNormal)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.Departamento, fontNormal)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.Salario.ToString("C"), fontNormal))
                {
                    Padding = 5,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });
                totalSalarios += item.Salario;
            }

            // Fila de total
            table.AddCell(new PdfPCell(new Phrase("", fontNormal)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase("", fontNormal)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase("", fontNormal)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase("TOTAL:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8)))
            {
                Padding = 5,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                BackgroundColor = new BaseColor(236, 240, 241)
            });
            table.AddCell(new PdfPCell(new Phrase(totalSalarios.ToString("C"), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8)))
            {
                Padding = 5,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                BackgroundColor = new BaseColor(236, 240, 241)
            });

            document.Add(table);
        }

        private void AgregarTablaCambiosSalariales(Document document, List<CambioSalarial> datos, Font fontHeader, Font fontNormal)
        {
            var table = new PdfPTable(7) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 8, 15, 15, 20, 12, 12, 10 });

            // Headers
            var headers = new[] { "No. Emp", "Nombre", "Apellido", "Departamento", "Sal. Anterior", "Sal. Nuevo", "% Cambio" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, fontHeader))
                {
                    BackgroundColor = new BaseColor(52, 73, 94),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                };
                table.AddCell(cell);
            }

            // Datos
            foreach (var item in datos)
            {
                table.AddCell(new PdfPCell(new Phrase(item.EmpNo.ToString(), fontNormal)) { Padding = 4 });
                table.AddCell(new PdfPCell(new Phrase(item.Nombre, fontNormal)) { Padding = 4 });
                table.AddCell(new PdfPCell(new Phrase(item.Apellido, fontNormal)) { Padding = 4 });
                table.AddCell(new PdfPCell(new Phrase(item.Departamento, fontNormal)) { Padding = 4 });
                table.AddCell(new PdfPCell(new Phrase(item.SalarioAnterior.ToString("C"), fontNormal))
                {
                    Padding = 4,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });
                table.AddCell(new PdfPCell(new Phrase(item.SalarioNuevo.ToString("C"), fontNormal))
                {
                    Padding = 4,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });

                var porcentaje = item.PorcentajeCambio.ToString("F1") + "%";
                var colorFondo = item.PorcentajeCambio >= 0 ? new BaseColor(212, 237, 218) : new BaseColor(248, 215, 218);
                table.AddCell(new PdfPCell(new Phrase(porcentaje, fontNormal))
                {
                    Padding = 4,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    BackgroundColor = colorFondo
                });
            }

            document.Add(table);
        }

        private void AgregarTablaEstructuraOrganizacional(Document document, List<EstructuraOrganizacional> datos, Font fontHeader, Font fontNormal)
        {
            foreach (var departamento in datos)
            {
                // Título del departamento
                var tituloDepto = new Paragraph($"Departamento: {departamento.NombreDepartamento}",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK));
                tituloDepto.SpacingBefore = 15;
                tituloDepto.SpacingAfter = 5;
                document.Add(tituloDepto);

                // Manager
                var manager = new Paragraph($"Manager: {departamento.NombreManager} {departamento.ApellidoManager}",
                    FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.DARK_GRAY));
                manager.SpacingAfter = 10;
                document.Add(manager);

                // Tabla de empleados
                var table = new PdfPTable(5) { WidthPercentage = 90 };
                table.SetWidths(new float[] { 10, 20, 20, 15, 15 });

                // Headers
                var headers = new[] { "No. Emp", "Nombre", "Apellido", "Título", "Salario" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, fontHeader))
                    {
                        BackgroundColor = new BaseColor(149, 165, 166),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                // Empleados
                foreach (var empleado in departamento.Empleados)
                {
                    table.AddCell(new PdfPCell(new Phrase(empleado.EmpNo.ToString(), fontNormal)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(empleado.Nombre, fontNormal)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(empleado.Apellido, fontNormal)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(empleado.Titulo, fontNormal)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(empleado.Salario.ToString("C"), fontNormal))
                    {
                        Padding = 4,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    });
                }

                document.Add(table);
            }
        }

        private void CrearHojaNominaVigente(ExcelWorksheet worksheet, List<NominaVigente> datos)
        {
            // Título
            worksheet.Cells[1, 1].Value = "Reporte de Nómina Vigente";
            worksheet.Cells[1, 1, 1, 5].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Fecha
            worksheet.Cells[2, 1].Value = $"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Cells[2, 1, 2, 5].Merge = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            // Headers
            var headers = new[] { "No. Empleado", "Nombre", "Apellido", "Departamento", "Salario" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[4, i + 1].Value = headers[i];
                worksheet.Cells[4, i + 1].Style.Font.Bold = true;
                worksheet.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Datos
            decimal totalSalarios = 0;
            for (int i = 0; i < datos.Count; i++)
            {
                var row = i + 5;
                worksheet.Cells[row, 1].Value = datos[i].EmpNo;
                worksheet.Cells[row, 2].Value = datos[i].Nombre;
                worksheet.Cells[row, 3].Value = datos[i].Apellido;
                worksheet.Cells[row, 4].Value = datos[i].Departamento;
                worksheet.Cells[row, 5].Value = datos[i].Salario;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                totalSalarios += datos[i].Salario;
            }

            // Total
            var totalRow = datos.Count + 6;
            worksheet.Cells[totalRow, 4].Value = "TOTAL:";
            worksheet.Cells[totalRow, 4].Style.Font.Bold = true;
            worksheet.Cells[totalRow, 5].Value = totalSalarios;
            worksheet.Cells[totalRow, 5].Style.Font.Bold = true;
            worksheet.Cells[totalRow, 5].Style.Numberformat.Format = "#,##0.00";

            // Autoajustar columnas
            worksheet.Cells.AutoFitColumns();
        }

        private void CrearHojaCambiosSalariales(ExcelWorksheet worksheet, List<CambioSalarial> datos)
        {
            // Título
            worksheet.Cells[1, 1].Value = "Reporte de Cambios Salariales";
            worksheet.Cells[1, 1, 1, 7].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Headers
            var headers = new[] { "No. Empleado", "Nombre", "Apellido", "Departamento", "Salario Anterior", "Salario Nuevo", "% Cambio" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                worksheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Datos
            for (int i = 0; i < datos.Count; i++)
            {
                var row = i + 4;
                worksheet.Cells[row, 1].Value = datos[i].EmpNo;
                worksheet.Cells[row, 2].Value = datos[i].Nombre;
                worksheet.Cells[row, 3].Value = datos[i].Apellido;
                worksheet.Cells[row, 4].Value = datos[i].Departamento;
                worksheet.Cells[row, 5].Value = datos[i].SalarioAnterior;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 6].Value = datos[i].SalarioNuevo;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 7].Value = datos[i].PorcentajeCambio / 100;
                worksheet.Cells[row, 7].Style.Numberformat.Format = "0.0%";

                // Color según el cambio
                if (datos[i].PorcentajeCambio >= 0)
                {
                    worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                }
                else
                {
                    worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);
                }
            }

            worksheet.Cells.AutoFitColumns();
        }

        private void CrearHojaEstructuraOrganizacional(ExcelWorksheet worksheet, List<EstructuraOrganizacional> datos)
        {
            worksheet.Cells[1, 1].Value = "Estructura Organizacional";
            worksheet.Cells[1, 1, 1, 6].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            int currentRow = 3;

            foreach (var departamento in datos)
            {
                // Departamento
                worksheet.Cells[currentRow, 1].Value = $"DEPARTAMENTO: {departamento.NombreDepartamento}";
                worksheet.Cells[currentRow, 1, currentRow, 6].Merge = true;
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
                worksheet.Cells[currentRow, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                currentRow++;

                // Manager
                worksheet.Cells[currentRow, 1].Value = $"Manager: {departamento.NombreManager} {departamento.ApellidoManager}";
                worksheet.Cells[currentRow, 1, currentRow, 6].Merge = true;
                worksheet.Cells[currentRow, 1].Style.Font.Italic = true;
                currentRow += 2;

                // Headers empleados
                var headers = new[] { "No. Emp", "Nombre", "Apellido", "Título", "Salario", "Fecha Contratación" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[currentRow, i + 1].Value = headers[i];
                    worksheet.Cells[currentRow, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }
                currentRow++;

                // Empleados
                foreach (var empleado in departamento.Empleados)
                {
                    worksheet.Cells[currentRow, 1].Value = empleado.EmpNo;
                    worksheet.Cells[currentRow, 2].Value = empleado.Nombre;
                    worksheet.Cells[currentRow, 3].Value = empleado.Apellido;
                    worksheet.Cells[currentRow, 4].Value = empleado.Titulo;
                    worksheet.Cells[currentRow, 5].Value = empleado.Salario;
                    worksheet.Cells[currentRow, 5].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[currentRow, 6].Value = empleado.FechaContratacion;
                    worksheet.Cells[currentRow, 6].Style.Numberformat.Format = "dd/mm/yyyy";
                    currentRow++;
                }

                currentRow += 2; // Espacio entre departamentos
            }

            worksheet.Cells.AutoFitColumns();
        }

        private string ObtenerTituloReporte(string tipoReporte)
        {
            switch (tipoReporte)
            {
                case "nomina-vigente":
                    return "Nómina Vigente";
                case "cambios-salariales":
                    return "Cambios Salariales";
                case "estructura-organizacional":
                    return "Estructura Organizacional";
                default:
                    return "Reporte";
            }
        }
    }
}