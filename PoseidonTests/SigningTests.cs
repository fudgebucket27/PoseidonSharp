using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoseidonSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PoseidonTests
{
    [TestClass]
    public class SigningTests
    {
        private static string PrivateKey = "0xff2f95f7f25dd17d160595603d49f9bd0bae765403d5d171fe1db2a3218c91"; // This private key has been unpaired from the real account
        private static string PrivateKey2 = "0x19bd8d52d552d2f112b478686f18577b8088e5b1860c3523c53f943304951c3"; //This private key has been unpaired from the real account
        [TestMethod]
        public void PoseidonEddsaTest1()
        {
            int MAX_INPUT = 13;
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs = { BigInteger.Parse("15262223708097584402615283257936266522564860189809682357548133077263290491192") };
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Assert.AreEqual(BigInteger.Parse("20006412648033755550733273460666169594962583918414636802156211610404306681114"), poseidonHash);
            Eddsa eddsa = new Eddsa(poseidonHash, PrivateKey);
            string signedMessage = eddsa.Sign();
            Assert.AreEqual("0x001306880e5a09076cba1e29a36fc4802be32338e0e6a922094ac2417662e14320a95f7f56d3e6241b6191344f1f9ee69dbc60ea244a06ded7c7b22c55c0af402fd957e3e2bd506741319064ec0e76ab44db7c0da08e8821137f09a6d30f0d68", signedMessage);
        }

        [TestMethod]
        public void PoseidonEddsaTest2()
        {
            int MAX_INPUT = 13;
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs = { BigInteger.Parse("95262223708097584402615283257936266522564860189809682357548133077263290491192") };
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Assert.AreEqual(BigInteger.Parse("133231497719586149593023275410024405046152548029140595410125539923302973012"), poseidonHash);
            Eddsa eddsa = new Eddsa(poseidonHash, PrivateKey2);
            string signedMessage = eddsa.Sign();
            Assert.AreEqual("0x0d24a82d5d6a0548bcae75ca35d42220d314a9a7534c033282e2406ebb76464808ec18c987f64b817cee1eabb5b66df973daf1f240b0fb76de439896eb1d097601f3660c07cbe3fe3b48feab92e5962aa4fc6de16b1236eb7d98eb75bc5b66c7", signedMessage);
        }

    }
}
