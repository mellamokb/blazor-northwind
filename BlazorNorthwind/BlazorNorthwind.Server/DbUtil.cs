using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorNorthwind.Server
{
    public static class DbUtil
    {
        public static string GetConnectionString()
        {
            var cs = new SqlConnectionStringBuilder();
            cs.DataSource = @"localhost\sqlexpress";
            cs.InitialCatalog = "Northwind";
            cs.IntegratedSecurity = true;
            cs.ApplicationName = "Blazor";
            return cs.ConnectionString;
        }
    }
}
