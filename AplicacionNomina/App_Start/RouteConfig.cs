using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AplicacionNomina
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            // Rutas específicas para reportes
            routes.MapRoute(
                name: "ReportesExportPDF",
                url: "Reportes/ExportarPDF",
                defaults: new { controller = "Reportes", action = "ExportarPDF" }
            );

            routes.MapRoute(
                name: "ReportesExportExcel",
                url: "Reportes/ExportarExcel",
                defaults: new { controller = "Reportes", action = "ExportarExcel" }
            );

            routes.MapRoute(
                name: "Reportes",
                url: "Reportes/{action}",
                defaults: new { controller = "Reportes", action = "Index" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
