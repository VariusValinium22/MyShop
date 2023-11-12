using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.MyHelpers;
using MyShop.Pages.Admin.Orders;
using System.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MyShop.Pages.Client.Orders
{
    [RequireAuth(RequiredRole = "client")]
    public class DetailsModel : PageModel
    {
        public OrderInfo orderInfo = new OrderInfo();

        private readonly IConfiguration config;
        public DetailsModel(IConfiguration configuration)
        { 
            config = configuration;
        }

        public void OnGet(int id)
        {
            int clientId = HttpContext.Session.GetInt32("id") ?? 0;

            if (id < 1)
            {
                Response.Redirect("/Client/Orders/Index");
                return;
            }

            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM orders WHERE id=@id AND client_id=@client_id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@client_id", clientId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                //if id is found, we will read that SINGLE order from the orders table
                                orderInfo.id = reader.GetInt32(0);
                                orderInfo.clientId = reader.GetInt32(1);
                                orderInfo.orderDate = reader.GetDateTime(2).ToString("MM/dd/yyyy");
                                orderInfo.shippingFee = reader.GetDecimal(3);
                                orderInfo.deliveryAddress = reader.GetString(4);
                                orderInfo.paymentMethod = reader.GetString(5);
                                orderInfo.paymentStatus = reader.GetString(6);
                                orderInfo.orderStatus = reader.GetString(7);

                                //for this SINGLE id, list each item's data. Fill this List with the method getOrderItems()
                                orderInfo.items = OrderInfo.getOrderItems(orderInfo.id);
                            }
                            else
                            {
                                Response.Redirect("/Client/Orders/Index");
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/Client/Orders/Index");
            }
        }
    }
}
