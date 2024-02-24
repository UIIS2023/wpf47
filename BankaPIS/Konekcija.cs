using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BankaPIS
{
    public class Konekcija
    {
        public SqlConnection KreirajKonekciju()
        {
            SqlConnectionStringBuilder ccnSb = new SqlConnectionStringBuilder
            {
                DataSource = @"DESKTOP-6AE00S0\SQLEXPRESS",
                InitialCatalog = "DbBanka",
                IntegratedSecurity = true,
            };
            string conn = ccnSb.ToString();
            SqlConnection konekcija = new SqlConnection(conn);
            return konekcija;
        }
    }
}