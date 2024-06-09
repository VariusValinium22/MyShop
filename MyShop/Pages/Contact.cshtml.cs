using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyShop.MyHelpers;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;


namespace MyShop.Pages
{
    public class ContactModel : PageModel
    {

        [BindProperty, Required(ErrorMessage = "The First Name is required.")]
        [Display(Name ="First Name*")]
        public string FirstName { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "The Last Name is required.")]
        [Display(Name = "Last Name*")]
        public string LastName { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "The Email is required."), EmailAddress]
        [Display(Name = "Email*")]
        public string Email { get; set; } = "";

        [BindProperty]
        public string? Phone { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "The Subject is required.")]
        [Display(Name = "Subject*")]
        public string Subject { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "The Message is required.")]
        [MinLength(5, ErrorMessage="The message should be at least 5 characters.")]
        [MaxLength(1024, ErrorMessage = "The message should be at less than 1024 characters.")]
        [Display(Name = "Message*")]
        public string Message { get; set; } = "";

        public List<SelectListItem> SubjectList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Order Status", Text = "Order Status" },
            new SelectListItem { Value = "Refund Request", Text = "Refund Request" },
            new SelectListItem { Value = "Job Application", Text = "Job Application" },
            new SelectListItem { Value = "Other", Text = "Other" },
        };

        public string SuccessMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        private readonly IConfiguration config;
        public ContactModel(IConfiguration configuration)
        {
            config = configuration;
        }

        public void OnGet()
        {
        }

        public void OnPost()
        {
            //Check if any required field is empty
            if (!ModelState.IsValid)
            {
                //if any required field(s) is empty, notify the user
                ErrorMessage = "Please fill all required fields";
                return;
            }

            if (Phone == null) Phone = "";

            //Add this user's Contact Data to the database
            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO messages " +
                        "(firstname, lastname, email, phone, subject, message) VALUES " +
                        "(@firstname, @lastname, @email, @phone, @subject, @message);";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", FirstName);
                        command.Parameters.AddWithValue("@lastName", LastName);
                        command.Parameters.AddWithValue("@email", Email);
                        command.Parameters.AddWithValue("@phone", Phone);
                        command.Parameters.AddWithValue("@subject", Subject);
                        command.Parameters.AddWithValue("@message", Message);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {

                ErrorMessage = ex.Message;
                return;
            }

            //Send Confirmation Email to the client
            string username = FirstName + LastName;
            string emailSubject = "About your message";
            string emailMessage = "Dear " + username + ",\n" +
                "We received your message. Thank you for contacting us. \n" +
                "Our team will contact you very soon. \n" +
                "Best Regards\n\nYour message to us:\n" + Message;

            try
            {
                EmailSender.SendEmail(Email, username, emailSubject, emailMessage).Wait();
                SuccessMessage = "Your message has been transmitted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Console.WriteLine("We have an Exception: " + ex.Message);//EmailSender does not verify if the email is real or not.

            }

            //Initialize the properties with blank text to CLEAR the form
            FirstName = "";
            LastName = "";
            Email = "";
            Phone = "";
            Subject = "";
            Message = "";

            ModelState.Clear();
        }
    }
}
