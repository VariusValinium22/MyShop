using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.MyHelpers;
using System.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MyShop.Pages.Admin.Books
{
    [RequireAuth(RequiredRole = "admin")]
    public class IndexModel : PageModel
    {
        public List<BookInfo> listBooks = new List<BookInfo>();
        public string search = "";

        public int page = 1;
        public int totalPages = 0;
        private readonly int pageSize = 5;

        public string column = "id";
        public string order = "desc";

        private readonly IConfiguration config;
        public IndexModel(IConfiguration configuration)
        {
            config = configuration;
        }

        public void OnGet()
        {
            search = Request.Query["search"];
            if (search == null) search = "";

            page = 1;
            string requestPage = Request.Query["page"];
            if (requestPage != null)
            {
                try
                {
                    page = int.Parse(requestPage);
                }
                catch (Exception ex)
                {
                    page = 1;
                }
            }

            string[] validColumns = { "id", "title", "authors", "num_pages", "price", "category", "created_at" };
            column = Request.Query["column"];
            //check if column value is null and doesn't exist in our validColumns list, initialize the column value to "id" to sort by id by default, else we use the column value that was recieved into the request
            if (column == null || !validColumns.Contains(column))
            {
                column = "id";
            }

            //check if order is null and it is not asc, it will instantiate as desc by default
            order = Request.Query["order"];
            if (order == null || !order.Equals("asc"))
            {
                order = "desc";
            }

            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    string sqlCount = "SELECT COUNT(*) FROM books";
                    if (search.Length > 0)      //If possible 'search' text is provided by the user, append the query
                    {
                        sqlCount += " WHERE title LIKE @search OR authors LIKE @search";
                    }

                    using (SqlCommand command = new SqlCommand(sqlCount, connection)) //indicate the user's 'search' value
                    {
                        command.Parameters.AddWithValue("@search", "%" + search + "%");

                        decimal countOfBooks = (int)command.ExecuteScalar();//ExecuteScalar returns the first column and row of the query.In this case, returns the count of books.
                        totalPages = (int)Math.Ceiling(countOfBooks / pageSize);//total count of books in the db / 5 messages per page = totalPages
                    }

                    string sql = "SELECT * FROM books";
                    if (search.Length > 0)
                    {
                        sql += " WHERE title LIKE @search OR authors LIKE @search";
                    }
                    sql += " ORDER BY " + column + " " + order;//" ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + search + "%");
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //we need to read the data of only ONE row
                                BookInfo bookInfo = new BookInfo();
                                //fill this object with the data we are retrieving
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
                                //add this object into the List object at top
                                listBooks.Add(bookInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 Console.WriteLine(ex.Message);
            }
        }
    }

    public class BookInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Authors { get; set; } = "";
        public string Isbn { get; set; } = "";
        public int NumPages { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageFileName { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }
}




