using NeinMath;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PoseidonSharp
{
    public class SignedMessage
    {
        public (Integer x, Integer y) A = (Integer.Parse("0"), Integer.Parse("0"));
        public Signature Signature { get; set; }

        public Integer Message { get; set; }

        public SignedMessage((Integer x, Integer y) _a, Signature _s, Integer _message)
        {
            A = _a;
            Signature = _s;
            Message = _message;
        }

    }
}
