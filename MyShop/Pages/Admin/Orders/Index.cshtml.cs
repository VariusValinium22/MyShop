using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.MyHelpers;
using MyShop.Pages.Admin.Books;
using System.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MyShop.Pages.Admin.Orders
{
    [RequireAuth(RequiredRole = "admin")]
    public class IndexModel : PageModel
    {
        public List<OrderInfo> listOrders = new List<OrderInfo>();

        public int page = 1;
        public int totalPages = 0;
        private readonly int pageSize = 5;

        private readonly IConfiguration config;
        public IndexModel(IConfiguration configuration)
        {
            config = configuration;
        }

        public void OnGet()
        {
            try
            {
                string requestPage = Request.Query["page"];
                page = int.Parse(requestPage);
            }
            catch (Exception ex)
            {
                page = 1;
            }
        
            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlCount = "SELECT COUNT(*) FROM orders";
                    using (SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }

                    string sql = "SELECT * FROM orders ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //for every iteration in the while loop, we will read an order from the orders table
                                OrderInfo orderInfo = new OrderInfo();
                                orderInfo.id = reader.GetInt32(0);
                                orderInfo.clientId = reader.GetInt32(1);
                                orderInfo.orderDate = reader.GetDateTime(2).ToString("MM/dd/yyyy");
                                orderInfo.shippingFee = reader.GetDecimal(3);
                                orderInfo.deliveryAddress = reader.GetString(4);
                                orderInfo.paymentMethod = reader.GetString(5);
                                orderInfo.paymentStatus = reader.GetString(6);
                                orderInfo.orderStatus = reader.GetString(7);

                                //items is a list of orderInfo. Fill this List with the method getOrderItems()
                                orderInfo.items = OrderInfo.getOrderItems(orderInfo.id);
                                
                                //add the orderInfo object to the list of listOrders.
                                listOrders.Add(orderInfo);
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

    public class OrderItemInfo
    {
        public int id;
        public int orderId;
        public int bookId;
        public int quantity;
        public decimal unitPrice;

        public BookInfo bookInfo = new BookInfo();
    }

    public class OrderInfo
    {
        public int id;
        public int clientId;
        public string orderDate;
        public decimal shippingFee;
        public string deliveryAddress;
        public string paymentMethod;
        public string paymentStatus;
        public string orderStatus;

        public List<OrderItemInfo> items = new List<OrderItemInfo>();

        //a method that allows us to create the above list of 'items'(total orders) from the db
        public static List<OrderItemInfo> getOrderItems(int orderId)
        {
            List<OrderItemInfo> items = new List<OrderItemInfo>();

            try
            {
                string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myshop;Integrated Security=True";
                //string connectionString = "Server=tcp:portfolioprojectsserver.database.windows.net,1433;Initial Catalog=PortfolioProjectsDatabase;Persist Security Info=False;User ID=MartinYoung;Password=111qqqQQQ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;";
                //string connectionString = config.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //this query reads ALL columns from BOTH tables
                    string sql = "SELECT order_items.*, books.* FROM order_items, books " +
                        "WHERE order_items.order_id=@order_id AND order_items.book_id = books.id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@order_id", orderId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {   //create an object of OrderItemInfo and fill it with the data from the db
                                OrderItemInfo item = new OrderItemInfo();
                                
                                //order_items table data
                                item.id = reader.GetInt32(0);
                                item.orderId = reader.GetInt32(1);
                                item.bookId = reader.GetInt32(2);
                                item.quantity = reader.GetInt32(3);
                                item.unitPrice = reader.GetDecimal(4);
                                //books table data
                                item.bookInfo.Id = reader.GetInt32(5);
                                item.bookInfo.Title = reader.GetString(6);
                                item.bookInfo.Authors = reader.GetString(7);
                                item.bookInfo.Isbn = reader.GetString(8);
                                item.bookInfo.NumPages = reader.GetInt32(9);
                                item.bookInfo.Price = reader.GetDecimal(10);
                                item.bookInfo.Category = reader.GetString(11);
                                item.bookInfo.Description = reader.GetString(12);
                                item.bookInfo.ImageFileName = reader.GetString(13);
                                item.bookInfo.CreatedAt = reader.GetDateTime(14).ToString("MM/dd/yyyy");
                                //Add to the List ALL these 'items' representing ALL orders in the db. Each item will contain ALL the data above.
                                items.Add(item);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return items;
        }
    }
}
