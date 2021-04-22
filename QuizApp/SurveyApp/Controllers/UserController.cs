using QuizApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace QuizApp.Controllers
{
    public class UserController : Controller
    {
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        //Registration POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] Users user)
        {
            bool Status = false;
            string message = "";
            //
            // Model Validation 
            if (ModelState.IsValid)
            {

                #region Email is already Exist 
                var isExist = IsEmailExist(user.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email già presente nel db");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code 
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region  Password Hashing 
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); 
                #endregion
                user.IsEmailVerified = false;
                

                #region Save to Database
                using (UsersEntities1 dc = new UsersEntities1())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    //Send Email to User
                    SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString());
                    message = " Registrazione completata con successo. Il link per l'attivazione dell'account " +
                        " è stato spedito al tuo indirizzo email: " + user.EmailID;
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Alcuni campi non sono stati compilati";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }
        //Verify Account
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (UsersEntities1 dc = new UsersEntities1())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; //rimuove bug per la conferma password durante il cambio modifiche
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if( v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Richiesta non Valida";
                }
            }
            ViewBag.Status = Status;
            return View();

        }
        //Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login (UserLogin login, string ReturnUrl="")
        {
            string message = "";
            using (UsersEntities1 dc = new UsersEntities1())
            {
                var v = dc.Users.Where(a => a.EmailID == login.EmailID).FirstOrDefault();
                if (v != null)
                {
                    if (!v.IsEmailVerified)
                    {
                        ViewBag.Message = "Per favore ricontrolla l'indirizzo Email";
                        return View();
                    }
                    if (string.Compare(Crypto.Hash(login.Password),v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20;
                        var ticket = new FormsAuthenticationTicket(login.EmailID, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);
                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            var checkAdmin = dc.Users.Where(a => a.EmailID == login.EmailID).FirstOrDefault();
                            if (checkAdmin.IsAdminSurvey == true)
                            {
                                TempData["Viewbag"] = $"Benvenuto Admin {checkAdmin.FirstName}";
                                return RedirectToAction("Admin", "Admin");
                            }
                            else
                            {
                                ViewBag.Message = $"Benvenuto User {checkAdmin.FirstName}";
                            }

                            //TempData["login"] = login.EmailID; 
                            //return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Password non corretta";
                    }

                }
                else
                {
                    message = "Credenziali non valide";
                }
            }
            ViewBag.Message = message;
            return View();
        } 

        //Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }
        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (UsersEntities1 dc = new UsersEntities1())
            {
                var v = dc.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return v != null;
            }
        }
        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("SurveyAppform@gmail.com", "Quiz WebApplication");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "Surv3y4pp2020!"; 
            string subject = "Il tuo Account è stato creato correttamente!";

            string body = "<br/><br/>Siamo lieti di informarti che il tuo account Quiz App è stato" +
                " creato con successo. Cortesemente clicca sul link sottostante per validare l'account" +
                " <br/><br/><a href='" + link + "'>" + link + "</a> ";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
    }
}