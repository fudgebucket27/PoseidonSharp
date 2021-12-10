using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PoseidonSharp
{
    class Poseidon
    {
        public double SNARK_SCALAR_FIELD = 21888242871839275222246405745257275088548364400416034343698204186575808495617.00;
        public double FR_ORDER = 21888242871839275222246405745257275088614511777268538073601725287587578984328.00;
        public int t { get; set; }
        public int nRoundsF{ get; set; }
        public int nRoundsP { get; set; }
        public string seed { get; set; }
        public int e { get; set; }

        public int constantsC { get; set; }
        public int constantsM { get; set; }
        public int securityTarget { get; set; } 


        public Poseidon(int _t, int _nRoundsF, int _nRoundsP, string _seed, int _e, int _constantsC = 0, int _constantsM, int _securityTarget = 0)
        {
            Debug.Assert(_nRoundsF % 2 == 0 && _nRoundsF > 0, "nRoundsF needs to have modulus 2 of 0 and be more than 0");
        }
    }
}
