using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.Types
{
    /// <summary>
    /// The base class for all other class found inside the engine.<br/>
    /// </summary>
    /// <remarks>
    /// Remark:<br/>
    /// For game component (Like UI, Sprite, ...) you should use <see cref="GameObject"/>.
    /// </remarks>
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
