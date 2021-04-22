using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace QuizApp.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class Users
    {
        public string ConfirmPassword { get; set; }
    }

    public class UserMetadata
    {
        [Display(Name = "Nome")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nome Obbligatorio")]
        public string FirstName { get; set; }

        [Display(Name = "Cognome")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Cognome Obbligatorio")]
        public string LastName { get; set; }
        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email Obbligatoria")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }
        [Display(Name = "Data di Nascita")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime DateOfBirth { get; set; }
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "La Password deve contenere minimo 8 caratteri")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password Obbligatoria")]
        public string Password { get; set; }
        [Display(Name = "Conferma Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La Conferma Password non è corrispondente")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "Creatore dei Questionari")]
        public bool IsAdminSurvey { get; set; }
        
    }
}
