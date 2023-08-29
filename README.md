# PoseidonSharp
## Intro
PoseidonSharp is a C# Poseidon hashing/signing library used for hashing/signing requests to the Loopring API. 

[Loopring](https://loopring.io/#/) is a layer 2 scaling solution for Ethereum.

This library's reference implementation was originally in python and taken from https://github.com/Loopring/hello_loopring/blob/loopring-v3/sdk/ethsnarks/poseidon/permutation.py. As there was no C# implementation for Poseidon,this referenced python code was converted to C# by yours truly.

The included PoseidonConsole project contains some demo code on how to use the library. PoseidonTests contains the unit tests for MSTest.

## Adding PoseidonSharp to your project
You can either submodule this repository or add it as a dependency to your project via NuGet Package Manager in Visual Studio with the following command:

    Install-Package PoseidonSharp -Version 1.0.7

## Important
1. When passing BigInteger array inputs to Poseidon use the length of the input array + 1

2. The EDDSA signed message back is (0x + Rx+  Ry + S) and is specific to Loopring

3. Signed Messages can be verified with the Verify method from the Eddsa class like below:

```csharp
    using NeinMath;
  //Verify Correct Key Is Used
            BigInteger privateKeyBigInteger = EddsaHelper.PrivateKeyHexStringToBigInteger(PrivateKey3);
            Signature signatureObject = EddsaHelper.SignatureStringToSignatureObject(signedMessage);
            SignedMessage verifySignedMessage = new SignedMessage(EddsaHelper.CalculatePointA(privateKeyBigInteger), signatureObject, Integer.Parse(poseidonHash.ToString()));
            Assert.IsTrue(eddsa.Verify(verifySignedMessage));

            //Verify Incorrect Key Is used
            BigInteger privateKeyBigIntegerIncorrect = EddsaHelper.PrivateKeyHexStringToBigInteger(PrivateKey);
            SignedMessage verifySignedMessageIncorrect = new SignedMessage(EddsaHelper.CalculatePointA(privateKeyBigIntegerIncorrect), signatureObject, Integer.Parse(poseidonHash.ToString()));
            Assert.IsFalse(eddsa.Verify(verifySignedMessageIncorrect));
```
4. In version 1.0.7 and above there is a helper method for generating the Loopring L2 Details. Look at the following demo code to see how it works.
```csharp
           //Generating the l2 key details in this block
            var messageToSign = "Sign this message to access Loopring Exchange: 0x0BABA1Ad5bE3a5C0a66E7ac838a129Bf948f1eA4 with key nonce: 0";
            var l1PrivateKey = "8fe76a950a68a723e9ecd0c256045266e94ed5a1e846ca2112a9ecb61c1d28db";
            var ethAddress = "0x991B6fE54d46e5e0CEEd38911cD4a8694bed386A";
            var skipPublicKeyCalculation = false; //set to false to generate the public key details as well, set to true to skip public key generation which makes it run faster

            var signer = new EthereumMessageSigner();
            var signedMessageECDSA = signer.EncodeUTF8AndSign(messageToSign, new EthECKey(l1PrivateKey));
            var l2KeyDetails = LoopringL2KeyGenerator.GenerateL2KeyDetails(signedMessageECDSA, ethAddress, skipPublicKeyCalculation);
            Assert.AreEqual("0x0ec8709bb559eb1a38da8331039d979a46b9b8e9b399ab1adf477d55630f485d", l2KeyDetails.publicKeyX);
            Assert.AreEqual("0x064ab5befba8359524fa0c16021660870266ef62edc6aee52d75aece9057323a", l2KeyDetails.publicKeyY);
            Assert.AreEqual("0x03630456a1f23e7d61eb0f52e4abb67761417c1663320f133e15fe3111f6ef3b", l2KeyDetails.secretKey); //this is the l2 private key
            
            //Generating the x-api-sig header details for the get loopring api key endpoint
            string apiSignatureBase = "GET&https%3A%2F%2Fapi3.loopring.io%2Fapi%2Fv3%2FapiKey&accountId%3D" + 136736;
            BigInteger apiSignatureBaseBigInteger = SHA256Helper.CalculateSHA256HashNumber(apiSignatureBase);
            Eddsa eddsa = new Eddsa(apiSignatureBaseBigInteger, l2KeyDetails.secretKey);
            var xApiSig = eddsa.Sign(); //Add this string as the x-api-sig header in the request to the get loopring api key endpoint
```

# Thanks to
[Taranasus](https://github.com/taranasus) for code contributions and reviewing. 

[Leppaludi](https://github.com/leppaludi) for the idea of precalculating the poseidon constant and matrix which lead to a signficiant speed increase for hashing.

[Saucecontrol](https://github.com/saucecontrol) for the Blake2Fast library. 

## License
Fork this repo and do what you want with it! If you like my work and want to buy me a beer you can send me some ethereum to fudgey.eth or loopring to fudgey.loopring.eth ;)
