using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoseidonSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace PoseidonTests
{
    [TestClass]
    public class SigningTests
    {
        private static string PrivateKey = "0xff2f95f7f25dd17d160595603d49f9bd0bae765403d5d171fe1db2a3218c91"; // This private key has been unpaired from the real account
        private static string PrivateKey2 = "0x19bd8d52d552d2f112b478686f18577b8088e5b1860c3523c53f943304951c3"; //This private key has been unpaired from the real account
        private static string PrivateKey3 = "0x4485ade3c570854e240c72e9a9162e629f8e30db4d8130856da31787e7400f0"; //This private key has been unpaired from the real account
        [TestMethod]
        [Description("Hash and sign with first private key, it generates a negative big integer from the private key and generates a byte array of 32")]
        public void PoseidonEddsaTest1() 
        {
            int MAX_INPUT = 13;
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs = { BigInteger.Parse("15262223708097584402615283257936266522564860189809682357548133077263290491192") };
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Assert.AreEqual(BigInteger.Parse("20006412648033755550733273460666169594962583918414636802156211610404306681114"), poseidonHash, "Hashes don't match!");
            Eddsa.ResetPreComputedPointA();
            Eddsa eddsa = new Eddsa(poseidonHash, PrivateKey);
            string signedMessage = eddsa.Sign();
            Assert.AreEqual("0x001306880e5a09076cba1e29a36fc4802be32338e0e6a922094ac2417662e14320a95f7f56d3e6241b6191344f1f9ee69dbc60ea244a06ded7c7b22c55c0af402fd957e3e2bd506741319064ec0e76ab44db7c0da08e8821137f09a6d30f0d68", signedMessage, "Signed messages don't match!");
        }

        [TestMethod]
        [Description("Hash and sign with second private key, generates a positive big integer from the private key and generates a byte array of 31")]
        public void PoseidonEddsaTest2()
        {
            int MAX_INPUT = 13;
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs = { BigInteger.Parse("95262223708097584402615283257936266522564860189809682357548133077263290491192") };
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Assert.AreEqual(BigInteger.Parse("133231497719586149593023275410024405046152548029140595410125539923302973012"), poseidonHash, "Hashes don't match!");
            Eddsa.ResetPreComputedPointA();
            Eddsa eddsa = new Eddsa(poseidonHash, PrivateKey2);
            string signedMessage = eddsa.Sign();
            Assert.AreEqual("0x0d24a82d5d6a0548bcae75ca35d42220d314a9a7534c033282e2406ebb76464808ec18c987f64b817cee1eabb5b66df973daf1f240b0fb76de439896eb1d097601f3660c07cbe3fe3b48feab92e5962aa4fc6de16b1236eb7d98eb75bc5b66c7", signedMessage, "Signed messages don't match!");
        }

        [TestMethod]
        [Description("Hash and sign with third private key, generates a positive big integer from the private key and generates a byte array of 32")]
        public void PoseidonEddsaTest3()
        {
            int MAX_INPUT = 1;
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs = { BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111") };
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Assert.AreEqual(BigInteger.Parse("4039926191380788949286172671420227647869138762868816863744279168259334375146"), poseidonHash, "Hashes don't match!");
            Eddsa eddsa = new Eddsa(poseidonHash, PrivateKey3, true);
            string signedMessage = eddsa.Sign();
            Eddsa.ResetPreComputedPointA();
            Assert.AreEqual("0x2548f1aa374db001ac03fc9e113a9ba0fddd84070acc9fb47e2aa22d1573c0fc1d58cd798ef835d0d8d24f6ef49402e25d3f14604e142cc575a4199bce62ee4e18477876f7993ff9e109c48f88f9ec25f2251635edb56dd6bb51f512b0a2203d", signedMessage, "Signed messages don't match!");
        }

        [TestMethod]
        [Description("Hash and sign with third private key, large inputs generates a positive big integer from the private key and generates a byte array of 32")]
        public void PoseidonEddsaTest4()
        {
            BigInteger[] inputs = {
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111")
            };
            Poseidon poseidon = new Poseidon(inputs.Length + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Assert.AreEqual(BigInteger.Parse("8668707726223135950189553868157017834953206856133542860940608702666070102166"), poseidonHash, "Hashes don't match!");
            Eddsa eddsa = new Eddsa(poseidonHash, PrivateKey3, true);
            string signedMessage = eddsa.Sign();
            Eddsa.ResetPreComputedPointA();
            Assert.AreEqual("0x034e4750299923e5903d64e7e36d27e19433b2a5d0544b0f68a376cb115fc244076f01c50b1b982a60990d874598a1e89ae395d790c9388b03b9d53fd2e3e4e106e6d49ccb28b27a70ca5b31fb4ce3f247a79d1273b41da99ce12f741ba9ba75", signedMessage, "Signed messages don't match!");
        }

        [TestMethod]
        [Description("Hash and sign with third private key, large inputs generates a positive big integer from the private key and generates a byte array of 32, precompute a")]
        public void PoseidonEddsaTest5()
        {
            BigInteger[] inputs = {
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111")
            };
            Poseidon poseidon = new Poseidon(inputs.Length + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Assert.AreEqual(BigInteger.Parse("8668707726223135950189553868157017834953206856133542860940608702666070102166"), poseidonHash, "Hashes don't match!");
            Eddsa eddsa = new Eddsa(poseidonHash, PrivateKey3, true);
            string signedMessage = eddsa.Sign();
            Eddsa.ResetPreComputedPointA();
            Assert.AreEqual("0x034e4750299923e5903d64e7e36d27e19433b2a5d0544b0f68a376cb115fc244076f01c50b1b982a60990d874598a1e89ae395d790c9388b03b9d53fd2e3e4e106e6d49ccb28b27a70ca5b31fb4ce3f247a79d1273b41da99ce12f741ba9ba75", signedMessage, "Signed messages don't match!");
        }
    }
}
