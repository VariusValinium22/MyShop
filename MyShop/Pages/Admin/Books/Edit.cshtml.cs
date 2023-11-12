using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyShop.MyHelpers;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace MyShop.Pages.Admin.Books
{
    public class EditModel : PageModel
    {
        [RequireAuth(RequiredRole = "admin")]
        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "The Title is required.")]
        [MaxLength(100, ErrorMessage = "The Title cannot exceed 100 characters.")]
        public string Title { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The Authors are required.")]
        [MaxLength(255, ErrorMessage = "The Authors are required.")]
        public string Authors { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The ISBN is required.")]
        [MaxLength(20, ErrorMessage = "The ISBN cannot exceed 20 characters.")]
        public string ISBN { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The Number of Pages is required.")]
        [Range(1, 10000, ErrorMessage = "The Number of Pages must be in a range from 1 to 10000.")]
        public int NumPages { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "The Price is required.")]
        public decimal Price { get; set; }

        [BindProperty, Required]
        public string Category { get; set; } = "";

        [BindProperty]
        [MaxLength(1000, ErrorMessage = "The Description cannot exceed 1000 characters.")]
        public string? Description { get; set; } = "";//Description is an optional input

        [BindProperty]
        public string ImageFileName { get; set; } = "";

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public string errorMessage = "";
        public string successMessage  = "";

        private IWebHostEnvironment webHostEnvironment;
        
        private readonly IConfiguration config;
        public EditModel(IWebHostEnvironment env, IConfiguration configuration)
        {
            webHostEnvironment = env;
            config = configuration;
        }

        public void OnGet()
        {
            string requestId = Request.Query["id"];

            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM books WHERE id=@id";
                    using (SqlCommand command =new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", requestId);  //replaces the @id with the actual id captured in requestId
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Id = reader.GetInt32(0);
                                Title = reader.GetString(1);
                                Authors = reader.GetString(2);
                                ISBN = reader.GetString(3);
                                NumPages = reader.GetInt32(4);
                                Price = reader.GetDecimal(5);
                                Category = reader.GetString(6);
                                Description = reader.GetString(7);
                                ImageFileName = reader.GetString(8);
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
                Console.WriteLine(ex.Message);
                Response.Redirect("/Admin/Books/Index");
            }
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Data validation failed.";
                return;
            }

            //successful data validation
            if (Description == null) Description = "";

            //if we have a new ImageFile => delete the old image to upload the new image
            string newFileName = ImageFileName;
            if (ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(ImageFile.FileName);

                string imageFolder = webHostEnvironment.WebRootPath + "/images/books/";
                string imageFullPath = Path.Combine(imageFolder, newFileName);

                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    ImageFile.CopyTo(stream);
                }
                //delete old image
                string oldImageFullPath = Path.Combine(imageFolder, ImageFileName);
                System.IO.File.Delete(oldImageFullPath);
                Console.WriteLine("Delete Image " + oldImageFullPath);
            }

            //update the book data in the database
            try
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = " UPDATE books SET title=@title, authors=@authors, isbn=@isbn, " +
                        "num_pages=@num_pages, price=@price, category=@category, " +
                        "description=@description, image_filename=@image_filename WHERE id=@id;";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@title", Title);
                        command.Parameters.AddWithValue("@authors",Authors);
                        command.Parameters.AddWithValue("@isbn",ISBN);
                        command.Parameters.AddWithValue("@num_pages", NumPages);
                        command.Parameters.AddWithValue("@price", Price);
                        command.Parameters.AddWithValue("@category", Category);
                        command.Parameters.AddWithValue("@description", Description);
                        command.Parameters.AddWithValue("@image_filename",newFileName);
                        command.Parameters.AddWithValue("@id", Id);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
            successMessage = "Data saved correctly";
            Response.Redirect("/Admin/Books/Index");
            
        }
     }
}
