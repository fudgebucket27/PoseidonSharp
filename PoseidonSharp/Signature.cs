using NeinMath;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PoseidonSharp
{
    public class Signature
    {
        public (Integer x, Integer y) R = (Integer.Parse("0"), Integer.Parse("0"));
        public Integer S { get; set; }
        public Signature()
        {

        }
        public Signature((Integer x, Integer y) _r, Integer _s)
        {
            R.x = _r.x;
            R.y = _r.y;
            S = _s;
        }
    }
}
