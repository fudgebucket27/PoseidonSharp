# PoseidonSharp
Poseidon Library in C#, reference implementation was originally in python from https://github.com/Loopring/hello_loopring/blob/loopring-v3/tutorials/hash_and_sign/poseidon_hash_sample.py

For use with the C# Loopring API - https://github.com/taranasus/LoopringAPI

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
  Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128); //Initiate new poseidon
  
  //Poseidon hash
  BigInteger[] inputs = { BigInteger.Parse("1") };
  BigInteger testOne = poseidon.CalculatePoseidonHash(inputs);
  Debug.Assert(testOne == BigInteger.Parse("14018714854885098128064817341184136022863799846023165041562300563859625887667"), "Hash doesn't match expected hash!");
  Console.WriteLine($"Hash of test one is {testOne}");
  
  //EDDSA from poseidon hash
  Eddsa eddsa = new Eddsa(testOne, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
  string signedMessage = eddsa.Sign();
  Debug.Assert(signedMessage == "0x1b45351dfb252eb5455193503b97f45209e1ac21417b0f447d9c6f48c01152af19b7b17134a637c6ee77b198b9f96f16b8f90aa28c49f845ca805878251328822438550e3413dbdb9a6a90aae5fe699a17ff9b0f55d22ea055411f6dad870995", "Signed message doesn't match expected signed message");
  Console.WriteLine($"Signed message: {signedMessage}");
}
```

Fork this repo and do what you want with it! 
