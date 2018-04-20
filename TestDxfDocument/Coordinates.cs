using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDxfDocument
{                                   //http://stackoverflow.com/questions/9622211/how-to-make-correct-clone-of-the-listmyobject
    public interface ICloneable<T> //http://www.codinghelmet.com/?path=howto/implement-icloneable-or-not
    {
        T Clone();
    }

    class Coordinates : ICloneable<Coordinates>
    {
        public float x, y;

        public Coordinates(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public float X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }

        public float Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }

        public virtual Coordinates Clone() 
        { 
            return new Coordinates(this.x, this.y); 
        }
    }    
}
