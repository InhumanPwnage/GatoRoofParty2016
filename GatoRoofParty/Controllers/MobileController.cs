using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonLayer;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace GatoRoofParty.Controllers
{
    public class MobileController : Controller
    {
        // GET: Mobile
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Rsvp()
        {
            return View();
        }

        
        public ActionResult Confirmation()
        {
            return View();
        }

        
        public ActionResult Confirmed()
        {
            return View();
        }

        
        public ActionResult FullyBooked()
        {
            return View();
        }

        public MobileController()
        { }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Rsvp(string emailfield, string namefield, string surnamefield, DateTime datefield)
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

                    return RedirectToAction("ConfirmEmail", "Mobile", new { gud = u.Code.ToString(), email = u.Email });
                }
                else if (Existing != null && !Existing.Verified)
                {
                    TempData["error"] = "Email already submitted. <strong><a style=\"text-decoration: underline\" href=\"/Mobile/ConfirmEmail?gud=" + Existing.Code + "&email=" + Existing.Email + "\">Click here to resend invite.</a></strong>";
                    return RedirectToAction("Rsvp", "Mobile");
                }
                else if (count >= 220)
                {
                    //TempData["error"] = "Sorry, the event is fully booked!";
                    return RedirectToAction("FullyBooked", "Mobile");
                }
                else if (Existing.Verified)
                {
                    TempData["error"] = "Email already verified! Check your email for your ticket.";
                    return RedirectToAction("Rsvp", "Mobile");
                }
            }

            TempData["error"] = "Error: Something went wrong!";
            return RedirectToAction("Rsvp", "Mobile");
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
                        return RedirectToAction("Rsvp", "Mobile");
                    }
                    else if (u.Verified)//if a verified user tries to re-verify
                    {
                        TempData["error"] = "Email already verified! Check your email for your ticket.";
                        return RedirectToAction("Rsvp", "Mobile");
                    }
                    else
                    {
                        string path = Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath;

                        sendConfirmMail(email, "<!DOCTYPE HTML><html><meta http-equiv=\"X-UA-Compatible\" content=\"IE=9;IE=10;IE=11;IE=Edge,chrome=1\"/><head><title>Gato Roof Party</title><meta charset=\"utf-8\" /><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" /></head><body style=\"background-color:#e2e2e2;text-align:center;\"><img src=\"http://gatoroofparty.com.mt/images/confirm1.png \"/><br/><a href='" + path + "Mobile/ConfirmEmail?gud=" + gud.ToString() + "'>" + path + "Mobile/ConfirmEmail?gud=" + gud.ToString() + "</a><br/><img src=\"http://gatoroofparty.com.mt/images/confirm2.png \"/></body></html>");
                        return RedirectToAction("Confirmation", "Mobile");
                    }
                }
                else //if both are included, VERIFY HERE
                {
                    int count = use.GetVerifiedUsers().Count();

                    if ((u != null && count < 220) && !u.Verified)//USER FOUND and NOT verified
                    {
                        u.IPAddress = Request.UserHostAddress;
                        use.VerifyGuest(u);
                        //TempData["error"] = "Verified successfully! See you there.";
                        sendTicketMail(u, "<!DOCTYPE HTML><html><meta http-equiv=\"X-UA-Compatible\" content=\"IE=9;IE=10;IE=11;IE=Edge,chrome=1\"/><head><title>Gato Roof Party</title><meta charset=\"utf-8\" /><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" /></head><img src=\"http://gatoroofparty.com.mt/images/dlticket.png \"></body></html>");
                        return RedirectToAction("Confirmed", "Mobile");
                    }
                    else if (u == null)
                    {
                        TempData["error"] = "Error: The code provided does not match your Email!";
                        return RedirectToAction("Rsvp", "Mobile");
                    }
                    else if (count >= 220)
                    {
                        //TempData["error"] = "Sorry, the event is fully booked!";
                        return RedirectToAction("FullyBooked", "Mobile");
                    }
                    else if (u.Verified)//if verified already
                    {
                        TempData["error"] = "Email already verified! Check your email for more information.";
                        return RedirectToAction("Rsvp", "Mobile");
                    }
                }
            }
            TempData["error"] = "Error: Invalid verification code!";
            return RedirectToAction("Rsvp", "Mobile");
        }



        void sendConfirmMail(string email, string body)
        {
            SmtpClient SmtpServer = new SmtpClient("relay-hosting.secureserver.net");//("smtp-mail.outlook.com");
            NetworkCredential basicCredential = new NetworkCredential("info@gatoroofparty.com.mt", "pdpcxk");
            var mail = new MailMessage();

            mail.From = new MailAddress("info@gatoroofparty.com.mt");
            mail.To.Add(email);
            mail.Subject = "Confirm your attendance to Gato Roof Party 2016";
            mail.IsBodyHtml = true;
            mail.Body = body;

            SmtpServer.Credentials = basicCredential;
            SmtpServer.Port = 25;//587
            SmtpServer.EnableSsl = false;
            SmtpServer.Send(mail);
        }

        void sendTicketMail(User u, string body)
        {
            SmtpClient st = new SmtpClient("relay-hosting.secureserver.net");//smtp.office365.com
            NetworkCredential basicCredential = new NetworkCredential("info@gatoroofparty.com.mt", "pdpcxk");
            //julian.brincat.a100652@mcast.edu.mt

            st.Credentials = basicCredential;
            st.Port = 25;//587
            st.EnableSsl = false;//true

            var mail = new MailMessage();
            mail = GetMail(u, basicCredential);
            mail.IsBodyHtml = true;
            mail.Body = body;

            st.Send(mail);
        }

        //Server.MapPath("~/Content/Pdf/yourFileName.pdf")
        public MailMessage GetMail(User u, NetworkCredential nc)
        {
            string str = BuildDocument(u);
            //string css = GetCSS();

            StringReader sr = new StringReader(str);
            Document pdfDoc = new Document(PageSize.A4);//, 10f, 10f, 10f, 0f

            HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
            MemoryStream memorystream = new MemoryStream();

            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memorystream);
            pdfDoc.Open();
            htmlparser.Parse(sr);
            pdfDoc.Close();
            byte[] bytes = memorystream.ToArray();
            memorystream.Close();

            Attachment a = new Attachment(new MemoryStream(bytes), u.InvitationNumber + ".pdf");

            var mail = new MailMessage();
            mail.From = new MailAddress(nc.UserName);
            mail.To.Add(u.Email);
            mail.Subject = "Your Gato Roof Party 2016 Ticket";
            mail.IsBodyHtml = true;
            mail.Attachments.Add(a);
            mail.Body = "Please check the attachments of this email to print your free ticket to Gato Roof Party 2016. See you there!";
            return mail;
        }

        public string BuildDocument(User u)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<!doctype html>");
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("<meta charset=\"utf - 8\">");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append("<header>");

            sb.Append("<table border=\"0\" cellspacing=\"10\" cellpadding=\"0\">");
            sb.Append("<tbody>");
            sb.Append("<tr>");
            sb.Append("<td>");



            sb.Append("<table  cellpadding=\"0\">");//
            sb.Append("<span style=\"border: 1px; border-style: solid; \">");
            sb.Append("<tr>");
            sb.Append("<td>");

            sb.Append("<span style=\"font-family: Poppins; font-size: 15px; font-weight: 400;\">");

            sb.Append("<table bgcolor=\"#fee4aa\" width=\"530\">");
            sb.Append("<tbody>");
            sb.Append("<tr>");
            sb.Append("<th scope=\"col\" width=\"20%\">" + u.Name + "</th>");
            sb.Append("<th scope=\"col\" width=\"20%\">" + u.Surname + "</th>");
            sb.Append("<th scope=\"col\" width=\"40%\">Ticket Code:</th>");
            sb.Append("<th scope=\"col\" width=\"20%\">" + u.InvitationNumber + "</th>");
            sb.Append("</tr>");
            sb.Append("</tbody>");
            sb.Append("</table>");

            sb.Append("</span>");

            sb.Append("<table>");
            sb.Append("<tbody>");
            sb.Append("<tr><td> </td></tr>");
            sb.Append("<tr><td> </td></tr>");
            sb.Append("<tr><td> </td></tr>");
            sb.Append("<tr><td> </td></tr>");
            sb.Append("<tr>");
            sb.Append("<td colspan=\"4\" >");
            sb.Append("<img src=\"http://gatoroofparty.com.mt/images/Asset1.png \">");//../images/Asset1.png\
            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</tbody>");
            sb.Append("</table>");

            sb.Append("</td>");
            sb.Append("</tr>");

            sb.Append("</span>");

            sb.Append("</table>");

            sb.Append("</tr>");
            sb.Append("</td>");



            sb.Append("<tr>");
            sb.Append("<td>");

            sb.Append("<table width=\"537\" cellpadding=\"10\">");
            sb.Append("<tbody>");
            sb.Append("<tr><td> </td></tr>");
            sb.Append("<tr><td> </td></tr>");
            sb.Append("<tr><td> </td></tr>");
            sb.Append("<tr>");
            sb.Append("<td colspan=\"4\">");
            sb.Append("<img src=\"http://gatoroofparty.com.mt/images/Asset2.png \"/>");
            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</tbody>");
            sb.Append("</table>");

            sb.Append("</tr>");
            sb.Append("</td>");

            sb.Append("</tbody>");
            sb.Append("</table>");

            sb.Append("</header>");
            sb.Append("</body>");
            sb.Append("</html>");

            string str = sb.ToString();
            return str;
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