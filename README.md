# PoseidonSharp
Poseidon Library in C#, reference implementation was originally in python from https://github.com/Loopring/hello_loopring/blob/loopring-v3/tutorials/hash_and_sign/poseidon_hash_sample.py

For use with the Loopring API

Probably not production ready.. use at your own risk!

PoseidonConsole folder contains some demo code on how to use the library.

```csharp
using System;
using System.Diagnostics;
using System.Numerics;
using PoseidonSharp;


static void Main(string[] args)
{
  int MAX_INPUT = 13;
  int[] inputs = {1}; //Int array to pass to poseidon, hex string needs to be converted to base 10 int, max 13 ints
  Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128);
  BigInteger result = poseidon.CalculatePoseidonHash(inputs);
  Debug.Assert(result == BigInteger.Parse("14018714854885098128064817341184136022863799846023165041562300563859625887667"), "Hash doesn't match expected hash!");
  Console.WriteLine($"Hash is {result}");
  Console.WriteLine("Enter to exit");
  Console.ReadKey();
}
```
