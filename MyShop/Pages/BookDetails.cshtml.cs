using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.Pages.Admin.Books;
using MyShop.Pages.Admin.Messages;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data.SqlClient;
namespace MyShop.Pages
{
    public class BookDetailsModel : PageModel
    {
        public BookInfo bookInfo = new BookInfo();

        private readonly IConfiguration config;
        public BookDetailsModel(IConfiguration configuration)
        {
            config = configuration;
        }

        public void OnGet(int? id)
        {
            if (id == null)
            {
                Response.Redirect("/");
                return;
            }

            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM books WHERE id=@id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bookInfo.Id = reader.GetInt32(0);
                                bookInfo.Title = reader.GetString(1);
                                bookInfo.Authors = reader.GetString(2);
                                bookInfo.Isbn = reader.GetString(3);
                                bookInfo.NumPages = reader.GetInt32(4);
                                bookInfo.Price = reader.GetDecimal(5);
                                bookInfo.Category = reader.GetString(6);
                                bookInfo.Description = reader.GetString(7);
                                bookInfo.ImageFileName = reader.GetString(8);
                                bookInfo.CreatedAt = reader.GetDateTime(9).ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                Response.Redirect("/");
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/");
                return;
            }
        }
    }
}
