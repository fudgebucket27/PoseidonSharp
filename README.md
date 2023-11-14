# PoseidonSharp
## Intro
PoseidonSharp is a C# Poseidon hashing/signing library used for hashing/signing requests to the Loopring API. 

[Loopring](https://loopring.io/#/) is a layer 2 scaling solution for Ethereum.

This library's reference implementation was originally in python and taken from https://github.com/Loopring/hello_loopring/blob/loopring-v3/sdk/ethsnarks/poseidon/permutation.py. As there was no C# implementation for Poseidon,this referenced python code was converted to C# by yours truly.

The included PoseidonConsole project contains some demo code on how to use the library. PoseidonTests contains the unit tests for MSTest.

## Adding PoseidonSharp to your project
You can either submodule this repository or add it as a dependency to your project via NuGet Package Manager in Visual Studio with the following command:

    Install-Package PoseidonSharp -Version 1.0.8

## Important
1. When passing BigInteger array inputs to Poseidon use the length of the input array + 1

2. The EDDSA signed message back is (0x + Rx+  Ry + S) and is specific to Loopring

3. Signed Messages can be verified with the Verify method from the Eddsa class like below:

```csharp
            using NeinMath;
            using PoseidonSharp;
            using System.Numerics;

            //Verify Correct Key Is Used
            BigInteger privateKeyBigInteger = EddsaHelper.PrivateKeyHexStringToBigInteger(PrivateKey3);
            Signature signatureObject = EddsaHelper.SignatureStringToSignatureObject(signedMessage);
            SignedMessage verifySignedMessage = new SignedMessage(EddsaHelper.CalculatePointA(privateKeyBigInteger), signatureObject, Integer.Parse(poseidonHash.ToString())); 
            bool verifyTrue = eddsa.Verify(verifySignedMessage);

            //Verify Incorrect Key Is used
            BigInteger privateKeyBigIntegerIncorrect = EddsaHelper.PrivateKeyHexStringToBigInteger(PrivateKey);
            SignedMessage verifySignedMessageIncorrect = new SignedMessage(EddsaHelper.CalculatePointA(privateKeyBigIntegerIncorrect), signatureObject, Integer.Parse(poseidonHash.ToString()));
            bool verifyFalse = eddsa.Verify(verifySignedMessageIncorrect);
```
4. In version 1.0.7 and above there is a helper method for generating the Loopring L2 Details. Look at the following demo code to see how it works. Through an actual wallet through the browser you will need to use ethereum 'personal_sign' with the Loopring keyseed as the message and the eth address of the requester.
```csharp
            using Nethereum.Signer;
            using PoseidonSharp;
            using System.Numerics;

           //Generating the l2 key details in this block
            var messageToSign = "Sign this message to access Loopring Exchange: 0x0BABA1Ad5bE3a5C0a66E7ac838a129Bf948f1eA4 with key nonce: 0"; //this is the loopring key seed with nonce - 1
            var l1PrivateKey = "8fe76a950a68a723e9ecd0c256045266e94ed5a1e846ca2112a9ecb61c1d28db"; //L1 private key
            var ethAddress = "0x991B6fE54d46e5e0CEEd38911cD4a8694bed386A"; //eth address
            var skipPublicKeyCalculation = false; //set to false to generate the public key details as well, set to true to skip public key generation which makes it run faster

            var signer = new EthereumMessageSigner();
            var signedMessageECDSA = signer.EncodeUTF8AndSign(messageToSign, new EthECKey(l1PrivateKey));
            var l2KeyDetails = LoopringL2KeyGenerator.GenerateL2KeyDetails(signedMessageECDSA, ethAddress, skipPublicKeyCalculation);
            
            //Generating the x-api-sig header details for the get loopring api key endpoint
            string apiSignatureBase = "GET&https%3A%2F%2Fapi3.loopring.io%2Fapi%2Fv3%2FapiKey&accountId%3D" + 136736; //replace 136736 is the loopring account id for the request
            BigInteger apiSignatureBaseBigInteger = SHA256Helper.CalculateSHA256HashNumber(apiSignatureBase);
            Eddsa eddsa = new Eddsa(apiSignatureBaseBigInteger, l2KeyDetails.secretKey); //l2KeyDetails.secretKey is the Loopring L2 Private Key
            var xApiSig = eddsa.Sign(); //Add this string as the x-api-sig header in the request to the get loopring api key endpoint
```

5. View the tests: https://github.com/fudgebucket27/PoseidonSharp/tree/master/PoseidonTests to see additional examples on how to use this library

6. In version 1.0.8 and above you can help speed up signing for the same private key by setttng the last parameter in the Eddsa constructor to true like below. This will precompute points A for further signings.
```csharp
    Eddsa eddsa = new Eddsa(poseidonHash, PrivateKey3, true);
```

7. The precomputed point A can be reset as below using the static method 
```csharp
 Eddsa.ResetPreComputedPointA()
```

# Benchmarks
A Xeon W-10855M CPU @ 2.80GHz can generate 1000 poseidon hashes and sign them in about 13 seconds.

# Thanks to
[Taranasus](https://github.com/taranasus) for code contributions and reviewing. 

[Leppaludi](https://github.com/leppaludi) for the idea of precalculating the poseidon constant and matrix which lead to a signficiant speed increase for hashing.

[Saucecontrol](https://github.com/saucecontrol) for the Blake2Fast library. 

[Axel Heer](https://github.com/axelheer) for the NeinMath library which lead to major speed improvments with signing.

## License
Fork this repo and do what you want with it! If you like my work and want to buy me a beer you can send me some ethereum to fudgey.eth or loopring to fudgey.loopring.eth ;)
