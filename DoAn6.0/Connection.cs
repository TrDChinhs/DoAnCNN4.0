using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn6._0
{
    class Connection
    {
        private static string stringConnection = @"Data Source=TrDChinhs;Initial Catalog=QuanLyNongTrai;Integrated Security=True;";

        public static SqlConnection GetSqlConnection()
        {
            return new SqlConnection(stringConnection);
        }
    }
}
