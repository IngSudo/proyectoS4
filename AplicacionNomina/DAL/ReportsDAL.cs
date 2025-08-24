using AplicacionNomina.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace AplicacionNomina.DAL
{
    public class ReportsDAL
    {
        private readonly string connectionString;

        public ReportsDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["employees_db"].ConnectionString;
        }

        // Métodos helper para conversión segura
        private int GetSafeInt(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName); 

            if (reader.IsDBNull(ordinal)) 
                return 0;

            var value = reader.GetValue(ordinal); 

            if (value is int intValue)
                return intValue;

            if (value is string stringValue && int.TryParse(stringValue, out int parsedInt))
                return parsedInt;

            return 0;
        }


        private decimal GetSafeDecimal(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName); 

            if (reader.IsDBNull(ordinal)) 
                return 0;

            var value = reader.GetValue(ordinal); 

            if (value is long longValue)
                return (decimal)longValue;

            if (value is decimal decimalValue)
                return decimalValue;

            if (value is int intValue)
                return (decimal)intValue;

            if (value is string stringValue && decimal.TryParse(stringValue, out decimal parsedDecimal))
                return parsedDecimal;

            return 0;
        }


        private string GetSafeString(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
        }

        private DateTime GetSafeDateTime(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
                return DateTime.MinValue;

            var value = reader.GetValue(ordinal);

            if (value is DateTime dateTimeValue)
                return dateTimeValue;

            if (value is string stringValue && DateTime.TryParse(stringValue, out DateTime parsedDate))
                return parsedDate;

            return DateTime.MinValue;
        }


        public List<NominaVigente> ObtenerNominaVigente(int? deptNo = null, DateTime? fecha = null)
        {
            var nominas = new List<NominaVigente>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("sp_NominaVigentePorDepartamento", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@DeptNo", (object)deptNo ?? DBNull.Value);
                command.Parameters.AddWithValue("@Fecha", (object)fecha ?? DBNull.Value);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nominas.Add(new NominaVigente
                        {
                            EmpNo = reader["EmpNo"] != DBNull.Value ? Convert.ToInt32(reader["EmpNo"]) : 0,
                            Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : string.Empty,
                            Apellido = reader["Apellido"] != DBNull.Value ? reader["Apellido"].ToString() : string.Empty,
                            Departamento = reader["Departamento"] != DBNull.Value ? reader["Departamento"].ToString() : string.Empty,
                            Salario = reader["Salario"] != DBNull.Value ? Convert.ToInt64(reader["Salario"]) : 0
                        });
                    }
                }
            }

            return nominas;
        }


        public List<CambioSalarial> ObtenerCambiosSalariales(DateTime fechaInicio, DateTime fechaFin, int? deptNo = null)
        {
            var cambios = new List<CambioSalarial>();
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand("sp_CambiosSalarialesEnRango", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    command.Parameters.AddWithValue("@FechaFin", fechaFin);
                    command.Parameters.AddWithValue("@DeptNo", (object)deptNo ?? DBNull.Value);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var salarioAnterior = GetSafeDecimal(reader, "SalarioAnterior");
                            var salarioNuevo = GetSafeDecimal(reader, "SalarioNuevo");

                            cambios.Add(new CambioSalarial
                            {
                                EmpNo = GetSafeInt(reader, "EmpNo"),
                                Nombre = GetSafeString(reader, "Nombre"),
                                Apellido = GetSafeString(reader, "Apellido"),
                                Departamento = GetSafeString(reader, "Departamento"),
                                FechaCambio = GetSafeDateTime(reader, "FechaCambio"),
                                SalarioAnterior = salarioAnterior,
                                SalarioNuevo = salarioNuevo,
                            });
                        }
                    }
                }
            }
            return cambios;
        }

        public List<DepartamentoViewModel> ObtenerEstructuraOrganizacional(int? deptNo = null)
        {
            var managers = new List<DepartamentoManager>();
            var empleados = new List<EstructuraOrganizacional>();

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand("sp_EstructuraOrganizacional", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DeptNo", (object)deptNo ?? DBNull.Value);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        // Leer la primera tabla: Managers
                        while (reader.Read())
                        {
                            managers.Add(new DepartamentoManager
                            {
                                DeptNo = GetSafeInt(reader, "dept_no"),
                                NombreDepartamento = GetSafeString(reader, "NombreDepartamento"),
                                NombreManager = GetSafeString(reader, "NombreManager"),
                                ApellidoManager = GetSafeString(reader, "ApellidoManager")
                            });
                        }

                        reader.NextResult();

                        while (reader.Read())
                        {
                            empleados.Add(new EstructuraOrganizacional
                            {
                                DeptNo = GetSafeInt(reader, "DeptNo"),
                                EmpNo = GetSafeInt(reader, "EmpNo"),
                                Nombre = GetSafeString(reader, "Nombre"),
                                Apellido = GetSafeString(reader, "Apellido"),
                                Salario = GetSafeDecimal(reader, "Salario"),
                                FechaContratacion = GetSafeString(reader, "FechaContratacion"),
                                Titulo = GetSafeString(reader, "Titulo")
                            });
                        }
                    }
                }
            }

            // Transformar la tupla en List<DepartamentoViewModel>
            var departamentos = managers.Select(m => new DepartamentoViewModel
            {
                DeptNo = m.DeptNo,
                NombreDepartamento = m.NombreDepartamento,
                NombreManager = m.NombreManager,
                ApellidoManager = m.ApellidoManager,
                Empleados = empleados.Where(e => e.DeptNo == m.DeptNo).ToList()
            }).ToList();

            return departamentos;
        }


        public List<Departamento> ObtenerDepartamentos()
        {
            var departamentos = new List<Departamento>();

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand("sp_ObtenerDepartamentos", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departamentos.Add(new Departamento
                            {
                                DeptNo = GetSafeInt(reader, "DeptNo"),
                                DeptName = GetSafeString(reader, "DeptName")
                            });
                        }
                    }
                }
            }
            return departamentos;
        }       
    }
}