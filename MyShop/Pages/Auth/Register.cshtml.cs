using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.MyHelpers;
using MyShop.Pages.Admin.Messages;
using Org.BouncyCastle.Asn1.Ocsp;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
//using System.Net.Mail;

namespace MyShop.Pages.Auth
{
    [RequireNoAuth]
    [BindProperties]
    public class RegisterModel : PageModel
    {
        [Required(ErrorMessage = "The First Name is required.")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "The Last Name is required.")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "The Email is required."), EmailAddress]
        public string Email { get; set; } = "";

        public string? Phone { get; set; } = "";

        [Required(ErrorMessage = "The Address is required.")]
        public string Address { get; set; } = "";

        [Required(ErrorMessage = "The Password is required.")]
        [StringLength(50, ErrorMessage = "Password must be between 5 and 50 characters.", MinimumLength = 5)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "The Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match.")]
        public string ConfirmPassword { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        private readonly IConfiguration config;
        public RegisterModel(IConfiguration configuration)
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
                errorMessage = "data validation failed.";
                return;
            }
            
            //successful data validation
            if (Phone == null) Phone = "";

            //add the user Registration details to the database
            string connectionString = config.GetConnectionString("DefaultConnection");
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO users " +
                        "(firstname, lastname, email, phone, address, password, role) VALUES " +
                        "(@firstname, @lastname, @email, @phone, @address, @password, 'client');";

                    //Encrypt the password so it does not appear as plain text in the db
                    var passwordHasher = new PasswordHasher<IdentityUser>();
                    string hashedPassword = passwordHasher.HashPassword(new IdentityUser(), Password);

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", FirstName);
                        command.Parameters.AddWithValue("@lastname", LastName);
                        command.Parameters.AddWithValue("@email", Email);
                        command.Parameters.AddWithValue("@phone", Phone);
                        command.Parameters.AddWithValue("@address", Address);
                        command.Parameters.AddWithValue("@password", hashedPassword);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(Email))
                {
                    errorMessage = "This email is associated with another user. Please choose another email.";
                }
                else
                {
                    errorMessage = ex.Message;
                }
                return;
            }

            //send confirmation email to the user
            try
            {
                string username = FirstName + " " + LastName;
                string subject = "Account created successfully.";
                string message = "Dear " + username + ",\n\n" +
                    "Your account has been created successfully. \n\n" +
                    "Best Regards"; 
                EmailSender.SendEmail(Email, username, subject, message);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Console.WriteLine("We have an Exception: " + ex.Message);

            }
            //initialize the authenticated session a.k.a. add the user details to the session data
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM users WHERE email=@email";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", Email);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string firstname = reader.GetString(1);
                                string lastname = reader.GetString(2);
                                string email = reader.GetString(3);
                                string phone = reader.GetString(4);
                                string address = reader.GetString(5);
                                //string hashedPassword = reader.GetString(6);
                                string role = reader.GetString(7);
                                string created_at = reader.GetDateTime(8).ToString("MM/dd/yyyy");

                                HttpContext.Session.SetInt32("id", id);
                                HttpContext.Session.SetString("firstname", firstname);
                                HttpContext.Session.SetString("lastname", lastname);
                                HttpContext.Session.SetString("email", email);
                                HttpContext.Session.SetString("phone", phone);
                                HttpContext.Session.SetString("address", address);
                                HttpContext.Session.SetString("role", role);
                                HttpContext.Session.SetString("created_at", created_at);
                            }
                            else
                            {
                                Response.Redirect("/Admin/Books/Index");
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

            successMessage = "Account created successfully.";
            Response.Redirect("/");
        }
    }
}
