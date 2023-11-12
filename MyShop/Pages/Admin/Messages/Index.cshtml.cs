using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.MyHelpers;
using System.Data.SqlClient;

namespace MyShop.Pages.Admin.Messages
{
    public class IndexModel : PageModel
    {
        [RequireAuth(RequiredRole = "admin")]
        public List<MessageInfo> listMessages = new List<MessageInfo>();//List to collect all the messages in the db

        public int page = 1; //the current html page
        public int totalPages = 0; //stores the number of pages
        private readonly int pageSize = 5; //each page shows this amount of messages

        private readonly IConfiguration config;
        public IndexModel(IConfiguration configuration)
        {
            config = configuration;
        }

        public void OnGet()
        {
            page = 1; //Initialize the page with 1 by default to display the first page of messages
            string requestPage = Request.Query["page"]; //Query the page from the db
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
            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlCount = "SELECT COUNT(*) FROM messages";
                    using (SqlCommand command = new SqlCommand(sqlCount, connection))//Execute the query through this connection with the below conditions.
                    {
                        decimal countOfMessages = (int)command.ExecuteScalar();     //ExecuteScalar returns the first column and row of the query.In this case, returns the count of messages.
                        totalPages = (int)Math.Ceiling(countOfMessages / pageSize); //total count of messages in the db / 5 messages per page = totalPages
                    }
                     
                    string sql = "SELECT * FROM messages ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                MessageInfo messageInfo = new MessageInfo();
                                messageInfo.Id = reader.GetInt32(0);
                                messageInfo.FirstName = reader.GetString(1);
                                messageInfo.LastName = reader.GetString(2);
                                messageInfo.Email = reader.GetString(3);
                                messageInfo.Phone = reader.GetString(4);
                                messageInfo.Subject = reader.GetString(5);  
                                messageInfo.Message = reader.GetString(6);
                                messageInfo.CreatedAt = reader.GetDateTime(7).ToString("MM/dd/yyyy");

                                listMessages.Add(messageInfo);
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
    //Create a class to store that info for you when message from the db. List the received messages
    public class MessageInfo
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
        public string CreatedAt{ get; set; } = "";
    }
    
}









