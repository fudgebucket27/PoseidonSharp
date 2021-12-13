# PoseidonSharp
## Intro
This is mainly for use with the (unofficial) C# Loopring API - https://github.com/taranasus/LoopringAPI

This is a Poseidon Hashing Library in C#, the reference implementation was originally in python from https://github.com/Loopring/hello_loopring/blob/loopring-v3/tutorials/hash_and_sign/poseidon_hash_sample.py. This reference code was converted from python to C# by yours truly.

This is probably not production ready...so use at your own risk!

The included PoseidonConsole project contains some demo code on how to use the library.

## Important

1. The private key in PoseidonConsole for EDDSA is pulled from a user environment variable named "LoopringPrivateKey". It needs to be in a hex format, ie "0x1232blahblah".The demo signing tests in PoseidonConsole will fail the assertions because they are using MY private key.

2. The MAX_INPUT variable is important as you will get a different poseidon hash based on this value. Set it to the length of your BigInteger array inputs. So 3 elements would mean a MAX_INPUT of 3. We then also add 1 when passing the MAX_INPUT as the first parameter to the Poseidon class constructor.

3. The EDDSA signed message back is (0x + Rx+  Ry + S) and is specific to Loopring

## Demo Code
```csharp
using System;
using System.Diagnostics;
using System.Numerics;
using PoseidonSharp;

# Demo Code

static void Main(string[] args)
{
  int MAX_INPUT = 1;
  Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128); //Initiate new poseidon
  
  //Test case 1
  BigInteger[] inputs = { BigInteger.Parse("1") };
  BigInteger testOne = poseidon.CalculatePoseidonHash(inputs);
  Debug.Assert(testOne == BigInteger.Parse("11316722965829087614032985243432266723826890185209218714357779037968059437034"), "Hash doesn't match expected hash!");
  Console.WriteLine($"Hash of test one is {testOne}");
  Eddsa eddsa = new Eddsa(testOne, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
  string signedMessage = eddsa.Sign();
  Debug.Assert(signedMessage == "0x1ce11f9e04581491b95282c84ce73c5c7d07d3f5bf976e9f84c548a71216082b0380f2be576e8a2a43da3797f2b5ffbb35e6465848289426495fafa021c6faf421f699f38978c053563eae737af334c3c6708536d004f2a969abd63e176edcbd", "Signed message doesn't match expected signed message");
  Console.WriteLine($"Signed message: {signedMessage}");
  
  //Test case 2
  int MAX_INPUT_TWO = 4; //Max Input should be the number of BigInteger inputs
  Poseidon poseidonTwo = new Poseidon(MAX_INPUT_TWO + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
  BigInteger[] inputsTwo = { BigInteger.Parse("1233333333333333"), BigInteger.Parse("9400000000000000000000000000"), BigInteger.Parse("1223123"), BigInteger.Parse("544343434343434343") };
  BigInteger testTwoPoseidonHash = poseidonTwo.CalculatePoseidonHash(inputsTwo);
  Debug.Assert(testTwoPoseidonHash == BigInteger.Parse("3642840179269730552612336878249257609263354431767353053799083195998559566113"), "Hash doesn't match expected hash!");
  Console.WriteLine($"Hash of test six is {testTwoPoseidonHash}");
  Eddsa eddsaTwo = new Eddsa(testTwoPoseidonHash, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
  string signedMessageTwo = eddsaTwo.Sign();
  Debug.Assert(signedMessageTwo == "0x0b60e3d275b059b7a7f485e8182b32de7d842090b828e0471aad2fee4ad1f58c246cb6d8b538fe9929993b44a86ea90f50bdd346db600c193e1a8c62340a6d871f5aa69ca257feea363ab9b55ca52372f1fcd404964f27c3bae07e5d8f46d53a", "Signed message doesn't match expected signed message");
  Console.WriteLine($"Signed message: {signedMessageTwo}");
  
  Console.WriteLine("Enter to exit");
  Console.ReadKey();
}
```

## License
Fork this repo and do what you want with it! If you like my work and want to buy me a beer you can send me some ethereum to fudgey.eth ;)
