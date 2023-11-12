using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.MyHelpers;
using MyShop.Pages.Admin.Messages;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MyShop.Pages.Auth
{
    [RequireNoAuth]
    [BindProperties]
    public class LoginModel : PageModel
    {
        [Required(ErrorMessage = "The Email is Required"), EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "The Password is Required")]
        public string Password { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        private readonly IConfiguration config;
        public LoginModel(IConfiguration configuration)
        {
            config = configuration;
        }

        public void OnGet()
        {
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Data validation failed";
                return;
            }

            //successful data validation

            //connect to db, check the user credentials
            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM users WHERE email=@email";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", Email); //inject the provided email address into your SQL query
                        using (SqlDataReader reader = command.ExecuteReader())//use the SqlDataReader to read all the details about the user
                        {
                            if (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string firstname = reader.GetString(1);
                                string lastname = reader.GetString(2);
                                string email = reader.GetString(3);
                                string phone = reader.GetString(4);
                                string address = reader.GetString(5);
                                string hashedPassword = reader.GetString(6);
                                string role = reader.GetString(7);
                                string created_at = reader.GetDateTime(8).ToString("MM/dd/yyyy");

                                //verify the password
                                var passwordHasher = new PasswordHasher<IdentityUser>();
                                //use the above variable to verify the password
                                var result = passwordHasher.VerifyHashedPassword(new IdentityUser(), hashedPassword, Password);

                                if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
                                {
                                    //successful password verification => intialize the session
                                    HttpContext.Session.SetInt32("id", id);
                                    HttpContext.Session.SetString("firstname", firstname);
                                    HttpContext.Session.SetString("lastname", lastname);
                                    HttpContext.Session.SetString("email", email);
                                    HttpContext.Session.SetString("phone", phone);
                                    HttpContext.Session.SetString("address", address);
                                    HttpContext.Session.SetString("role", role);
                                    HttpContext.Session.SetString("created_at", created_at);

                                    //the user is authenticated successfully => redirect to Home
                                    Response.Redirect("/");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            // Wrong Email or Password
            errorMessage = "Wrong Email or Password";
        }
    }
}