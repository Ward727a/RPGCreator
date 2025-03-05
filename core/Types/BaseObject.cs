using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.Types
{
    class BaseObject
    {
        public Guid ID { get; }
        public Dictionary<string, object> Properties = [];
        
        public BaseObject()
        {
            ID = Guid.NewGuid();
        }

        public BaseObject(BaseObject obj)
        {
            this.ID = obj.ID;
        }
        public override string ToString()
        {
            return "BaseObject";
        }
        public virtual BaseObject Duplicate()
        {
            return new BaseObject() { Properties = this.Properties };
        }

    }
}
