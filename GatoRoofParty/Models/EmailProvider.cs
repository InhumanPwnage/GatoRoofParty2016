using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace GatoRoofParty.Models
{
    public class EmailProvider : SmtpClient
    {
        public string UserName { get; set; }

        public EmailProvider() : base( ConfigurationManager.AppSettings["mailHost"], Int32.Parse(ConfigurationManager.AppSettings["mailPort"]) )
        {
            //Get values from web.config file:
            this.UserName = ConfigurationManager.AppSettings["mailUserName"];
            this.EnableSsl = Boolean.Parse(ConfigurationManager.AppSettings["mailSsl"]);
            this.UseDefaultCredentials = false;
            this.Credentials = new System.Net.NetworkCredential(this.UserName, ConfigurationManager.AppSettings["mailPassword"]);
        }
    }
}