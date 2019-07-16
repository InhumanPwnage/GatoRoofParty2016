using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class Users : BLBase
    {
        public Users() : base() { }
        public Users(CommonLayer.partydbEntities Entities) : base(Entities) { }

        public void Register(CommonLayer.User user)
        {
            //CommonLayer.User Existing = this.GetUser(user.Email);//check for an existing email
            //int count = this.GetVerifiedUsers().Count(); //get how many confirmed guests

            //get total, and increment
            this.AddUser(user);
        }

        public IQueryable<CommonLayer.User> GetVerifiedUsers()
        {
            //DataLayer.DAUsers dauser = new DataLayer.DAUsers(this.Entities);
            return new DataLayer.DAUsers(this.Entities).GetVerifiedUsers();
        }

        public IQueryable<CommonLayer.User> GetUsers()
        {
            //DataLayer.DAUsers dauser = new DataLayer.DAUsers(this.Entities);
            return new DataLayer.DAUsers(this.Entities).GetUsers();
        }

        public CommonLayer.User GetUser(string email)
        {
            //DataLayer.DAUsers dauser = new DataLayer.DAUsers(this.Entities);
            return new DataLayer.DAUsers(this.Entities).GetUser(email);
        }

        public CommonLayer.User GetUserByCode(Guid gud)
        {
            //DataLayer.DAUsers dauser = new DataLayer.DAUsers(this.Entities);
            return new DataLayer.DAUsers(this.Entities).GetUserByCode(gud);
        }

        public void AddUser(CommonLayer.User User)
        {
            new DataLayer.DAUsers(this.Entities).AddUser(User);
        }

        public void VerifyGuest(CommonLayer.User u)
        {
            new DataLayer.DAUsers(this.Entities).VerifyGuest(u);
        }

        public int GetMaxInvitationNumber()
        {
            return new DataLayer.DAUsers(this.Entities).GetMaxInvitationNumber();
        }

        public decimal? GetTicketCode(decimal code)
        {
            return new DataLayer.DAUsers(this.Entities).GetTicketCode(code);
        }
    }
}
