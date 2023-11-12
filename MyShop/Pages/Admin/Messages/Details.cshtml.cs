using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.MyHelpers;
using System.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MyShop.Pages.Admin.Messages
{
    [RequireAuth(RequiredRole = "admin")]
    public class DetailsModel : PageModel
    {   
        public MessageInfo messageInfo = new MessageInfo();

        private readonly IConfiguration config;
        public DetailsModel(IConfiguration configuration)
        {
            config = configuration;
        }

        public void OnGet()
        {
            string requestId = Request.Query["id"];

            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM messages WHERE id=@id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", requestId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            { 
                                messageInfo.Id = reader.GetInt32(0);
                                messageInfo.FirstName = reader.GetString(1);
                                messageInfo.LastName = reader.GetString(2);
                                messageInfo.Email = reader.GetString(3);
                                messageInfo.Phone = reader.GetString(4);
                                messageInfo.Subject = reader.GetString(5);
                                messageInfo.Message = reader.GetString(6);
                                messageInfo.CreatedAt = reader.GetDateTime(7).ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                Response.Redirect("/Admin/Messages/Index");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/Admin/Messages/Index");
            }
        }
    }
}





