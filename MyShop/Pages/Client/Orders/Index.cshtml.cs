using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.MyHelpers;
using MyShop.Pages.Admin.Orders;
using System.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MyShop.Pages.Client.Order
{
    [RequireAuth(RequiredRole = "client")]
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
            int clientId = HttpContext.Session.GetInt32("id") ?? 0;
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

                    string sqlCount = "SELECT COUNT(*) FROM orders WHERE client_id=@client_id";
                    using (SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        command.Parameters.AddWithValue("@client_id", clientId);

                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }

                    string sql = "SELECT * FROM orders WHERE client_id=@client_id ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@client_id", clientId);
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
}
