using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuizApp.Models
{
    public class UserLogin
    {
        [Display(Name ="Indirizzo Mail")]
        [Required(AllowEmptyStrings = false, ErrorMessage ="Campo Obbligatorio")]
        public string EmailID { get; set; }
        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Campo Obbligatorio")]
        public string Password { get; set; }
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}