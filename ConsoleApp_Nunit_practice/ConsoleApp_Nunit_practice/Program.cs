using System.Data.SqlClient;



namespace ConsoleApp_Nunit_practice
{
    public class Program
    {
        static void Main(string[] args)
        {
            CallDb obj1 = new CallDb();
            obj1.GetEmployeeEmail("Amy");
        }

    }


    public class CallDb
    {
        // constructor
        public CallDb()
        {

        }

        public string GetEmployeeEmail(string keyword)
        {
            string Constr = @"Data Source=DESKTOP123\SQLEXPRESS;Initial Catalog=khcho_dev;User ID=khcho;Password=khcho";
            string Sqlstr = "SELECT [Name], [Email] FROM [sample_employee] WHERE [Name] = @Name";
            SqlConnection conn = new SqlConnection(Constr);
            SqlCommand cmd = new SqlCommand(Sqlstr, conn);
            conn.Open();
            cmd.Parameters.AddWithValue("@Name", keyword);

            string email = "";

            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                email = dr["Email"].ToString().TrimEnd();
            }
            dr.Close(); dr.Dispose(); conn.Close(); conn.Dispose();

            return email;
        }
    }
}
