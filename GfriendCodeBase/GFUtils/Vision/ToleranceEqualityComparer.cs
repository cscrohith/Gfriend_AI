using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Utils.Vision
{
    public class ToleranceEqualityComparer : IEqualityComparer<int>
    {
        public int Tolerance { get; set; } = 2;
        
        public ToleranceEqualityComparer(int tolerance)
        {
            Tolerance = tolerance;
        }
        
        public bool Equals(int x, int y)
        {
            return x - Tolerance <= y && x + Tolerance > y;
        }

        //This is to force the use of Equals methods.
        public int GetHashCode(int obj) => 1;
    }
}
