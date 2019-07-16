using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class BLBase
    {
        public CommonLayer.partydbEntities Entities { get; set; }

        public BLBase()
        {
            this.Entities = new CommonLayer.partydbEntities();
        }

        public BLBase(CommonLayer.partydbEntities Entity)
        {
            this.Entities = Entity;
        }
    }
}
