# PoseidonSharp
## Intro
PoseidonSharp is a C# Poseidon hashing library mainly for use with the (unofficial)(WIP) C# Loopring API - https://github.com/taranasus/LoopringAPI . [Loopring](https://loopring.io/#/) is a layer 2 scaling solution for Ethereum.

This library's reference implementation was originally in python and taken from https://github.com/Loopring/hello_loopring/blob/loopring-v3/tutorials/hash_and_sign/poseidon_hash_sample.py. As there was no C# implementation for Poseidon,this referenced python code was converted to C# by yours truly.

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

static void Main(string[] args)
{
  int MAX_INPUT = 1;
  Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128); //Initiate new poseidon
  
  //Test case 1
  BigInteger[] inputs = { BigInteger.Parse("19254303773071461417973161554248988464997154230097311673556244912844777390355") };//Max Input should be the number of BigInteger inputs
  BigInteger testOnePoseidonHash = poseidon.CalculatePoseidonHash(inputs);
  Debug.Assert(testOnePoseidonHash == BigInteger.Parse("19254303773071461417973161554248988464997154230097311673556244912844777390355"), "Hash doesn't match expected hash!");
  Console.WriteLine($"Hash of test one is {testOne}");
  Eddsa eddsa = new Eddsa(testOne, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
  string signedMessage = eddsa.Sign();
  Debug.Assert(signedMessage == "0x19bdf78654e45f513e3d983c4fa0f90c222ffb37ff1772d6955961f8f414d8f32945dea53a2d12bdcab3a5facaa695503e73608ed75988bfe0df9ae8413bab022e070e3025a288e70f6305e9c44f51480ddc712d8be59870ad0acfdcce9aaa05", "Signed message doesn't match expected signed message");
  Console.WriteLine($"Signed message: {signedMessage}");
  
  //Test case 2
  int MAX_INPUT_TWO = 4; //Max Input should be the number of BigInteger inputs
  Poseidon poseidonTwo = new Poseidon(MAX_INPUT_TWO + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
  BigInteger[] inputsTwo = { BigInteger.Parse("1233333333333333"), BigInteger.Parse("9400000000000000000000000000"), BigInteger.Parse("1223123"), BigInteger.Parse("544343434343434343") };
  BigInteger testTwoPoseidonHash = poseidonTwo.CalculatePoseidonHash(inputsTwo);
  Debug.Assert(testTwoPoseidonHash == BigInteger.Parse("3642840179269730552612336878249257609263354431767353053799083195998559566113"), "Hash doesn't match expected hash!");
  Console.WriteLine($"Hash of test two is {testTwoPoseidonHash}");
  Eddsa eddsaTwo = new Eddsa(testTwoPoseidonHash, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
  string signedMessageTwo = eddsaTwo.Sign();
  Debug.Assert(signedMessageTwo == "0x0b60e3d275b059b7a7f485e8182b32de7d842090b828e0471aad2fee4ad1f58c246cb6d8b538fe9929993b44a86ea90f50bdd346db600c193e1a8c62340a6d871f5aa69ca257feea363ab9b55ca52372f1fcd404964f27c3bae07e5d8f46d53a", "Signed message doesn't match expected signed message");
  Console.WriteLine($"Signed message: {signedMessageTwo}");

  
  //Test case for signed message of get api key url
  //Calculate sha256 specific to loopring of url
  BigInteger testOneInput = SHA256Helper.CalculateSHA256HashNumber("GET&https%3A%2F%2Fuat3.loopring.io%2Fapi%2Fv3%2FapiKey&accountId%3D11087");
  Debug.Assert(testOneInput == BigInteger.Parse("19400808358061590369279192378878962429412529891699423035130831734199348072763"), "Hash doesn't match expected hash!");
  Console.WriteLine($"Hash of test one is {testOneInput}");
  Eddsa eddsaThree = new Eddsa(testOneInput, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
  string signedMessageThree = eddsaThree.Sign();
  Debug.Assert(signedMessageThree == "0x02cd51ee31c9d63e6d9796704249fbccaba8fd287e4c7d412bc4d6d88801bb0a067de03f99a1a1194a098522e686a1940024946535d45cbbd02b3bb38722d9f02fa6e5be861a24168738837e7b7f38e4379b26a54a60673afde303e75f47b769", "Signed message doesn't match expected signed message");
  Console.WriteLine($"Signed message: {signedMessageThree}");
  
  Console.WriteLine("Enter to exit");
  Console.ReadKey();
}
```

## License
Fork this repo and do what you want with it! If you like my work and want to buy me a beer you can send me some ethereum to fudgey.eth ;)
