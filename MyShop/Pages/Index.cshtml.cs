using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.Pages.Admin.Books;
using System.Data.SqlClient;
using Firebase.Database;
using Firebase.Database.Query;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Pages
{
    public class IndexModel : PageModel
    {
        public List<BookInfo> listNewestBooks = new List<BookInfo>();
        public List<BookInfo> listTopSales = new List<BookInfo>();

        private readonly IConfiguration config;
        public IndexModel(IConfiguration configuration)
        {
            config = configuration;
        }

        public void OnGet()
        {
            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); //in our query, we only need to retrieve the 4 NEWEST BOOKS as there is only 4 cards in our View
                    string sql = "SELECT TOP 4 * FROM books ORDER BY created_at DESC";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {// we do need to read/retrieve EACH of the book data from the db.
                             // But then simply place it into an object of bookInfo created as we retrieve the data for each book!
                             // Then Add the object of bookInfo to the ListNewestBooks List object we created at the top of the page
                                BookInfo bookInfo = new BookInfo();
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

                                listNewestBooks.Add(bookInfo);
                            }
                        }
                    }
                    sql = "SELECT TOP 4 books.*, (" +
                        "SELECT SUM(order_items.quantity) FROM order_items WHERE books.id = order_items.book_id" +
                        ") AS total_sales " +
                        "FROM books " +
                        "ORDER BY total_sales DESC";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BookInfo bookInfo = new BookInfo();
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

                                listTopSales.Add(bookInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message); //no form here so we use Console.WriteLine()
            }
        }
    }
}
