using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeinMath;
using Nethereum.Model;
using Nethereum.Signer;
using PoseidonSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PoseidonTests
{
    [TestClass]
    public class L2KeyGenerationTests
    {
        [TestMethod]
        [Description("L2 Key Details Generation")]
        public void L2KeyDetailsGenerationTest1()
        {
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
            var xApiSig = eddsa.Sign();
            Assert.AreEqual("0x2ac9c49ed7589025d900a981aa2805bd1b974fe5f0715290929bc46d721cb7a8292805d8a237155099fc7b89170c832552dd7fb0d23790fb09b16ac05c30be3a2d8bc03d3baa31e459c6ee567a2810ad6680d0045c9909eb692def1423e85aea", xApiSig);
        }
    }
}
