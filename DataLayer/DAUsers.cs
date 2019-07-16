using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLayer;
using System.Data.SqlClient;
using System.Data;

namespace DataLayer
{
    public class DAUsers : ConnectionClass
    {
        public DAUsers() : base() { }
        public DAUsers(partydbEntities Entities) : base(Entities) { }

        public IQueryable<User> GetUsers()
        {
            return this.Entities.Users;
        }

        public IQueryable<User> GetVerifiedUsers()
        {
            return this.Entities.Users.Where(p => p.Verified == true);
            //return (from users in this.Entities.Users where users.Verified == true select users);
        }

        public User GetUser(string email)
        {
            return this.Entities.Users.SingleOrDefault(p => p.Email.ToLower().Equals(email.ToLower()));
        }

        public User GetUserByCode(Guid gud)
        {
            return this.Entities.Users.SingleOrDefault(p => p.Code == gud);
        }

        public void VerifyGuest(User u)
        {
            User guest = this.GetUser(u.Email);
            guest.IPAddress = u.IPAddress;
            guest.Verified = true;
            guest.TimeVerified = DateTime.Now;
            //guest.TicketCode = u.TicketCode;
            this.Entities.SaveChanges();
        }

        public decimal? GetTicketCode(decimal code)
        {
            return this.Entities.Users.SingleOrDefault(p => p.TicketCode == code).TicketCode;
        }

        public int GetMaxInvitationNumber()
        {
            return this.Entities.Users.Max(x => x.InvitationNumber);

        }

        public void AddUser(User user)
        {
            SqlConnection conn = new SqlConnection();

            //details removed for security and privacy
            string connString = "Data Source=; Integrated Security=False; User ID=;Password=; Connect Timeout = 15; Encrypt = False; Packet Size = 4096";
            conn.ConnectionString = connString;
            conn.Open();

            SqlCommand cmdAddUser = new SqlCommand("dbo.AddUserToDb", conn);
            cmdAddUser.CommandType = CommandType.StoredProcedure;

            // Begin the transaction and enlist the commands.
            SqlTransaction tran = conn.BeginTransaction();
            cmdAddUser.Transaction = tran;

            try
            {
                cmdAddUser.Parameters.AddWithValue("@inv", user.InvitationNumber);
                cmdAddUser.Parameters.AddWithValue("@dob", user.DoB);
                cmdAddUser.Parameters.AddWithValue("@name", user.Name);
                cmdAddUser.Parameters.AddWithValue("@surname", user.Surname);
                cmdAddUser.Parameters.AddWithValue("@email", user.Email);
                cmdAddUser.Parameters.AddWithValue("@verified", user.Verified);
                cmdAddUser.Parameters.AddWithValue("@code", user.Code);
                cmdAddUser.Parameters.AddWithValue("@timesubmitted", user.TimeSubmitted);

                cmdAddUser.ExecuteNonQuery();

                tran.Commit();
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                tran.Rollback();
            }
            finally
            {
                conn.Close();
            }
        }


    }
}
