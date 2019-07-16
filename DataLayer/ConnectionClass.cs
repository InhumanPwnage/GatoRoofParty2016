using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class ConnectionClass
    {
        public CommonLayer.partydbEntities Entities { get; set; }

        public ConnectionClass()
        {
            this.Entities = new CommonLayer.partydbEntities();
        }

        public ConnectionClass(CommonLayer.partydbEntities Entity)
        {
            this.Entities = Entity;
        }
    }
}
