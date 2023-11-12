using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.Pages.Admin.Books;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MyShop.Pages
{
    [BindProperties]
    public class CartModel : PageModel
    {
        public string errorMessage = "";
        public string successMessage = "";

        [Required(ErrorMessage = "The Address is required")]
        public string Address { get; set; } = "";

        [Required]
        public string PaymentMethod { get; set; } = "";

        public List<OrderItem> listOrderItems = new List<OrderItem>();

        public decimal subtotal = 0;
        public decimal shippingFee = 5;
        public decimal total = 0;

        //dictionary method for converting the string to value pairs of bookIds:bookCopies
        private Dictionary<string, int> getBookDictionary()
        {
            //create the dictionary variable
            var bookDictionary = new Dictionary<string, int>();

            //Read the amended bookIds from the cookie
            string cookieValue = Request.Cookies["shopping_cart"] ?? "";

            //Check that the cookie is NOT null
            if (cookieValue.Length > 0)
            {
                //separate the cookie into parts at the hyphens, an array of bookIds
                string[] bookIdArray = cookieValue.Split('-');
                //iterate the array of bookIds
                for (int i = 0; i < bookIdArray.Length; i++)
                {
                    //each iterated bookId instantiates to a variable
                    string bookId = bookIdArray[i];

                    //Check if that next bookId has already been tallied to the dictionary
                    if (bookDictionary.ContainsKey(bookId))
                    {
                        //if bookid had previously been tallied to the dictionary, increment by 1
                        bookDictionary[bookId] += 1;
                    }
                    else
                    {
                        //if the bookId is NOT yet tallied to the dictionary, add it
                        bookDictionary.Add(bookId, 1);
                    }
                }
            }

            return bookDictionary;
        }

        private readonly IConfiguration config;
        public CartModel(IConfiguration configuration)
        {
            config = configuration;
        }

        public void OnGet()
        {
            var bookDictionary = getBookDictionary();

            //action can be null, add, subtract, or delete
            string? action = Request.Query["action"];
            string? id = Request.Query["id"];

            if (action != null && id != null && bookDictionary.ContainsKey(id))
            {
                if (action.Equals("add"))
                {
                    bookDictionary[id] += 1;
                }
                else if (action.Equals("sub"))
                {
                    if (bookDictionary[id] > 1) bookDictionary[id] -= 1;
                }
                else if (action.Equals("delete"))
                {
                    bookDictionary.Remove(id);
                }

                //Rebuild the cookie with the manipulated value
                string newCookieValue = "";
                foreach (var keyValuePair in bookDictionary)
                {
                    for (int i = 0; i < keyValuePair.Value; i++)
                    {
                        newCookieValue += "-" + keyValuePair.Key;
                    }
                }

                if (newCookieValue.Length > 0)
                    newCookieValue = newCookieValue.Substring(1);
                var cookieOptions = new CookieOptions();
                cookieOptions.Expires = DateTime.Now.AddDays(365);
                cookieOptions.Path = "/";

                Response.Cookies.Append("shopping_cart", newCookieValue, cookieOptions);

                //Redirect back to this same page to remove the query string from url and refresh the cart icon to show proper item amount.
                Response.Redirect(Request.Path.ToString());
                return;

            }

            try
            {
               string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM books WHERE id=@id";
                    foreach (var keyValuePair in bookDictionary)
                    {
                        string bookId = keyValuePair.Key;
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@id", bookId);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    OrderItem item = new OrderItem();

                                    item.bookInfo.Id = reader.GetInt32(0);
                                    item.bookInfo.Title = reader.GetString(1);
                                    item.bookInfo.Authors = reader.GetString(2);
                                    item.bookInfo.Isbn = reader.GetString(3);
                                    item.bookInfo.NumPages = reader.GetInt32(4);
                                    item.bookInfo.Price = reader.GetDecimal(5);
                                    item.bookInfo.Category = reader.GetString(6);
                                    item.bookInfo.Description = reader.GetString(7);
                                    item.bookInfo.ImageFileName = reader.GetString(8);
                                    item.bookInfo.CreatedAt = reader.GetDateTime(9).ToString("MM/dd/yyyy");

                                    item.numCopies = keyValuePair.Value;
                                    item.totalPrice = item.numCopies * item.bookInfo.Price;

                                    listOrderItems.Add(item);

                                    subtotal += item.totalPrice;
                                    total = subtotal + shippingFee;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Address = HttpContext.Session.GetString("address") ?? "";

            TempData["Total"] = "" + total;
            TempData["ProductIdentifiers"] = "";
            TempData["DeliveryAddress"] = "";
        }

        public void OnPost()
        {
            int client_id = HttpContext.Session.GetInt32("id") ?? 0;
            if (client_id < 1)
            {
                Response.Redirect("/Auth/Login");
                return;
            }
            if (!ModelState.IsValid)
            {
                errorMessage = "Data validation failed";
                return;
            }

            //Read shopping cart items from cookie
            var bookDictionary = getBookDictionary();
            if (bookDictionary.Count < 1)
            {
                errorMessage = "your cart is empty";
                return;
            }

            string productidentifiers = Request.Cookies["shopping_cart"] ?? "";
            TempData["ProductIdentifiers"] = productidentifiers;
            TempData["DeliveryAddress"] = Address;
            if (PaymentMethod == "credit_card" || PaymentMethod == "paypal")
            {
                Response.Redirect("/Checkout");
            }

            //save the order in the db
            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    int newOrderId = 0;
                    string sqlOrder = "INSERT INTO orders (client_id, order_date, shipping_fee, " +
                        "delivery_address, payment_method, payment_status, order_status) " +
                        "OUTPUT INSERTED.id " +
                        "VALUES (@client_id, CURRENT_TIMESTAMP, @shipping_fee, " +
                        "@delivery_address, @payment_method, 'pending', 'created')";

                    using (SqlCommand command = new SqlCommand(sqlOrder, connection))
                    {
                        command.Parameters.AddWithValue("@client_id", client_id);
                        command.Parameters.AddWithValue("@shipping_fee", shippingFee);
                        command.Parameters.AddWithValue("@delivery_address", Address);
                        command.Parameters.AddWithValue("@payment_method", PaymentMethod);

                        newOrderId = (int)command.ExecuteScalar();
                    }

                    //add the ordered books to the order \_items table
                    string sqlItem = "INSERT INTO order_items (order_id, book_id, quantity, unit_price) " +
                        "VALUES (@order_id, @book_id, @quantity, @unit_price)";

                    foreach (var keyValuePair in bookDictionary)
                    {
                        string bookID = keyValuePair.Key;
                        int quantity = keyValuePair.Value;
                        decimal unitPrice = getBookPrice(bookID);

                        using (SqlCommand command = new SqlCommand(sqlItem, connection))
                        {
                            command.Parameters.AddWithValue("@order_id", newOrderId);
                            command.Parameters.AddWithValue("@book_id", bookID);
                            command.Parameters.AddWithValue("@quantity", quantity);
                            command.Parameters.AddWithValue("@unit_price", unitPrice);

                            command.ExecuteNonQuery();
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
            //Delete the cookie "shopping_cart" from browser.
            Response.Cookies.Delete("shopping_cart");

            successMessage = "Order created successfully";
        }

        private decimal getBookPrice(string bookID)
        {
            decimal price = 0;

            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT price FROM books WHERE id=@id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", bookID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                price = reader.GetDecimal(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return price;
        }
    }

    public class OrderItem
    {
        public BookInfo bookInfo = new BookInfo();
        public int numCopies = 0;
        public decimal totalPrice = 0;
    }
}



