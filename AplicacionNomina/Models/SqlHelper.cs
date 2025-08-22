using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AplicacionNomina.Models
{
    public static class SqlHelper
    {
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["NominaContext"].ConnectionString;

        public static DataTable ExecuteDataTable(string spName, params SqlParameter[] parameters)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(spName, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                var dt = new DataTable();
                cn.Open();
                da.Fill(dt);
                return dt;
            }
        }

        public static DataRow ExecuteDataRow(string spName, params SqlParameter[] parameters)
        {
            var dt = ExecuteDataTable(spName, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static int ExecuteNonQuery(string spName, params SqlParameter[] parameters)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(spName, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
