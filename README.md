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
  BigInteger[] inputs = {BigInteger.Parse("1")};
  Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128);
  BigInteger testOne = poseidon.CalculatePoseidonHash(inputs);
  Debug.Assert(testOne == BigInteger.Parse("14018714854885098128064817341184136022863799846023165041562300563859625887667"), "Hash doesn't match expected hash!");
  Console.WriteLine($"Hash of test one is {testOne}");

  BigInteger[] inputsTwo = { BigInteger.Parse("1"), BigInteger.Parse("9400000000000000000000000000") };
  BigInteger testTwo = poseidon.CalculatePoseidonHash(inputsTwo);
  Debug.Assert(testTwo == BigInteger.Parse("2838802984016459847807835899395446048073460738419316401120440074779237106208"), "Hash doesn't match expected hash!");
  Console.WriteLine($"Hash of test two is {testTwo}");

  Console.WriteLine("Enter to exit");
  Console.ReadKey();
}
```
