using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FP.OAuth.AuthorizationServer.Models
{
    public class LoginModel  : PageModel
    {
        [BindProperty]
        public LoginData loginData { get; set; }

        [BindProperty]
        public string client_id { get; set; }

        [BindProperty]
        public string response_type { get; set; }

        [BindProperty]
        public string redirect_uri { get; set; }

        [BindProperty]
        public string state { get; set; }

        [BindProperty]
        public string scope { get; set; }

        public class LoginData
        {
            [Required]
            public string Username { get; set; }

            [Required, DataType(DataType.Password)]
            public string Password { get; set; }
        }
    }
}
