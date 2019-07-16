using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Net.Mail;
using CommonLayer;
using System.Net;

namespace GatoRoofParty.Controllers
{
    public class RsvpController : Controller
    {
        // GET: Rsvp
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Example()
        {
            return View();
        }

        public ActionResult Confirmation()
        {
            return View();
        }

        public RsvpController()
        { }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(string emailfield, string namefield, string surnamefield, DateTime datefield)
        {
            if (verifyEmail(emailfield) && isName(namefield) && isSurname(surnamefield) && isDate(datefield))
            {
                BusinessLayer.Users use = new BusinessLayer.Users();

                User Existing = use.GetUser(emailfield);//check for an existing email
                int count = use.GetVerifiedUsers().Count();

                if (Existing == null && count < 220)//200 = maximum number of confirmed guests
                {
                    User u = new User() { DoB = datefield, Name = FirstCharToUpper(namefield), Surname = FirstCharToUpper(surnamefield), Email = emailfield, Code = Guid.NewGuid(), TimeSubmitted = DateTime.Now, Verified = false };
                    count = use.GetUsers().Count();//get actual number of guests
                    u.InvitationNumber = use.GetMaxInvitationNumber() + 1;//max from column + 1
                    use.Register(u);

                    return RedirectToAction("ConfirmEmail", "Rsvp", new { gud = u.Code.ToString(), email = u.Email });
                }
                else if (Existing != null && !Existing.Verified)
                {
                    TempData["error"] = "Email already submitted. <strong><a style=\"text-decoration: underline\" href=\"/Rsvp/ConfirmEmail?gud=" + Existing.Code + "&email=" + Existing.Email + "\">Click here to resend invite.</a></strong>";
                    return RedirectToAction("Index", "Rsvp");
                }
                else if (count >= 220)
                {
                    TempData["error"] = "Sorry, the event is fully booked!";
                    return RedirectToAction("Index", "Rsvp");
                }
                else if (Existing.Verified)
                {
                    TempData["error"] = "Email already verified! Check your email for your ticket.";
                    return RedirectToAction("Index", "Rsvp");
                }
            }

            TempData["error"] = "Error: Something went wrong!";
            return RedirectToAction("Index", "Rsvp");
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmEmail(string gud, string email)
        {
            Guid guid;

            if (Guid.TryParse(gud, out guid))//if valid code
            {
                BusinessLayer.Users use = new BusinessLayer.Users();
                User u = use.GetUserByCode(guid);

                if (!string.IsNullOrEmpty(email))//send email if provided
                {
                    if (u == null)
                    {
                        TempData["error"] = "Error: Email does not exist!";
                        return RedirectToAction("Index", "Rsvp");
                    }
                    else if (u.Verified)//if a verified user tries to re-verify
                    {
                        TempData["error"] = "Email already verified! Check your email for your ticket.";
                        return RedirectToAction("Index", "Rsvp");
                    }
                    else
                    {
                        sendMail(email, "Click here to confirm your attendance: " + Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath + "Rsvp/ConfirmEmail?gud=" + gud.ToString());
                        //TempData["message"] = "Verification email sent! Please check your email.";
                        return RedirectToAction("Confirmation", "Rsvp");
                    }
                }
                else //if both are included, VERIFY HERE
                {
                    int count = use.GetVerifiedUsers().Count();

                    if ((u != null && count < 220) && !u.Verified)//USER FOUND and NOT verified
                    {
                        u.IPAddress = Request.UserHostAddress;
                        Random r = new Random();

                        decimal t = 0;

                        do
                        {
                            t = r.Next(1, 3);//10000, 100000
                        }
                        while (u.TicketCode == use.GetTicketCode(t));

                        u.TicketCode = t;

                        use.VerifyGuest(u);
                        TempData["error"] = "Verified successfully! See you there.";
                        return RedirectToAction("Index", "Rsvp");
                    }
                    else if (u == null)
                    {
                        TempData["error"] = "Error: The code provided does not match your Email!";
                        return RedirectToAction("Index", "Rsvp");
                    }
                    else if (count >= 220)
                    {
                        TempData["error"] = "Sorry, the event is fully booked!";
                        return RedirectToAction("Index", "Rsvp");
                    }
                    else if(u.Verified)//if verified already
                    {
                        TempData["error"] = "Email already verified! Check your email for more information.";
                        return RedirectToAction("Index", "Rsvp");
                    }
                }
            }
            TempData["error"] = "Error: Invalid verification code!";
            return RedirectToAction("Index", "Rsvp");
        }

        

        void sendMail(string email, string body)
        {
            SmtpClient SmtpServer = new SmtpClient("relay-hosting.secureserver.net");//("smtp-mail.outlook.com");
            NetworkCredential basicCredential = new NetworkCredential("info@gatoroofparty.com.mt", "pdpcxk");
            var mail = new MailMessage();

            mail.From = new MailAddress("info@gatoroofparty.com.mt");
            mail.To.Add(email);
            mail.Subject = "Verify your attendance to Gato Roof Party 2016";
            mail.IsBodyHtml = true;
            mail.Body = body;

            SmtpServer.Credentials = basicCredential;
            SmtpServer.Port = 25;//587
            SmtpServer.EnableSsl = false;
            SmtpServer.Send(mail);
        }


        public string FirstCharToUpper(string input)
        {
            //return input.First().ToString().ToUpper() + String.Join("", input.Skip(1));
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        #region Input Validation
        public bool verifyEmail(string email)
        {
            var name = ""; //johndoe <-- @
            var domain = ""; //site  <-- .com
            var extension = ""; //.com

            if (email != null && email != "")
            {
                //get @ symbol, split name
                for (var i = 0; i < email.Length; i++)
                {
                    if (email[i] == '@')
                    {
                        name = email.Substring(0, i);
                        domain = email.Substring(i + 1);
                        break;
                    }
                }

                //get first . symbol, split domain
                for (var i = 0; i < domain.Length; i++)
                {
                    if (domain[i] == '.')
                    {
                        extension = domain.Substring(i + 1);
                        domain = domain.Substring(0, i);
                        break;
                    }
                }

                if (isAlphanumeric(name) && isDomain(domain) && isExtension(extension))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public bool verifyAge(DateTime date)
        {
            //first check for valid date sent
            if (isDate(date))
            {
                //now check user's age
                DateTime today = DateTime.Today;
                int age = today.Year - date.Year;

                if (date > today.AddYears(-age))
                    age--;

                if (age >= 18)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }




        public bool isAlphanumeric(string str)
        {
            int code = 0;
            bool dotbefore = false;//if true, previous letter was a .

            for (var i = 0; i < str.Length; i++)
            {
                char s = str[i];
                code = (int)s; //get code 
                               //$('#name').append(str[i]);

                //NOT numeric, upper-alpha, lower-alpha, dot/period. Space = 32
                if (!(code > 47 && code < 58) && !(code > 64 && code < 91) && !(code > 96 && code < 123) && code != 46)
                {
                    return false;
                }
                else if (code == 46)  //check it's not followed by another dot/period
                {
                    //if followed by a dot, or is at start/end
                    if ((i == 0 || i == str.Length - 1) || dotbefore)
                        return false;
                    else
                        dotbefore = true; //set flag
                }
                else
                {
                    dotbefore = false; //reset
                }
            }
            return true;
        }

        public bool isDomain(string str)
        {
            int code = 0;
            var dashbefore = false;//if true, previous letter was a -

            for (var i = 0; i < str.Length; i++)
            {
                char s = str[i];
                code = (int)s; //get code 
                               //$('#domain').append(str[i]);

                //NOT numeric, upper-alpha, lower-alpha, dash
                if (!(code > 47 && code < 58) && !(code > 64 && code < 91) && !(code > 96 && code < 123) && code != 45)
                {
                    return false;
                }
                else if (code == 45)  //check it's not followed by another dash
                {
                    if (!dashbefore)//if no dash was found
                    {
                        if ((i == 0 || i == str.Length - 1))
                            return false;
                        else
                            dashbefore = true; //set flag
                    }
                    else
                        return false;
                }
            }
            return true;
        }

        public bool isExtension(string str)
        {
            int code = 0;
            bool dotbefore = false;//if true, previous letter was a .

            for (var i = 0; i < str.Length; i++)
            {
                char s = str[i];
                code = (int)s; //get code 
                               //$('#extension').append(str[i]);

                //NOT numeric, upper-alpha, lower-alpha, dot/period
                if (!(code > 47 && code < 58) && !(code > 64 && code < 91) && !(code > 96 && code < 123) && code != 46)
                {
                    return false;
                }
                else if (code == 46)  //check it's not followed by another dot/period
                {
                    if (!dotbefore)//if no dot was found
                    {
                        if ((i == 0 || i == str.Length - 1))
                            return false;
                        else
                            dotbefore = true; //set flag
                    }
                    else
                        return false;
                }

            }
            return true;
        }

        public bool isName(string str)
        {
            //allow only one space and one dash
            int code = 0;
            bool dashbefore = false;//if true, previous letter was a -
            bool spacebefore = false;//if true, space was used at least once already

            if (String.IsNullOrEmpty(str) || String.IsNullOrWhiteSpace(str) || str.Length >= 100)
                return false;

            for (var i = 0; i < str.Length; i++)
            {
                char s = str[i];
                code = (int)s; //get code 
                               //$('#domain').append(str[i]);

                //NOT numeric, upper-alpha, lower-alpha, dash
                if (!(code > 47 && code < 58) && !(code > 64 && code < 91) && !(code > 96 && code < 123) && code != 45 && code != 32)
                {
                    return false;
                }
                else if (code == 45)//check it's not followed by another dash
                {
                    if (!dashbefore)//if no dash was found
                    {
                        if ((i == 0 || i == str.Length - 1))
                            return false;
                        else
                            dashbefore = true; //set flag
                    }
                    else
                        return false;
                }
                else if (code == 32)//check if one space already used
                {
                    if (!spacebefore)//if no space was found
                    {
                        if ((i == 0 || i == str.Length - 1))
                            return false;
                        else
                            spacebefore = true; //set flag
                    }
                    else
                        return false;
                }
            }
            return true;
        }

        public bool isSurname(string str)
        {
            //allow only one space and one dash
            int code = 0;
            bool dashbefore = false;//if true, previous letter was a -
            bool spacebefore = false;//if true, space was used at least once already

            if (String.IsNullOrEmpty(str) || String.IsNullOrWhiteSpace(str) || str.Length >= 100)
                return false;

            for (var i = 0; i < str.Length; i++)
            {
                char s = str[i];
                code = (int)s; //get code 
                               //$('#domain').append(str[i]);

                //NOT numeric, upper-alpha, lower-alpha, dash
                if (!(code > 47 && code < 58) && !(code > 64 && code < 91) && !(code > 96 && code < 123) && code != 45 && code != 32)
                {
                    return false;
                }
                else if (code == 45)//check it's not followed by another dash
                {
                    if (!dashbefore)//if no dash was found
                    {
                        if ((i == 0 || i == str.Length - 1))
                            return false;
                        else
                            dashbefore = true; //set flag
                    }
                    else
                        return false;
                }
                else if (code == 32)//check if one space already used
                {
                    if (!spacebefore)//if no space was found
                    {
                        if ((i == 0 || i == str.Length - 1))
                            return false;
                        else
                            spacebefore = true; //set flag
                    }
                    else
                        return false;
                }
            }
            return true;
        }

        public bool isDate(DateTime date)
        {
            if (DateTime.TryParse(date.ToString(), out date))//"MM/dd/yyyy"
                return true;
            else
                return false;
        }
        #endregion
    }
}