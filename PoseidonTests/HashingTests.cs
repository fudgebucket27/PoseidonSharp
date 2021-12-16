using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoseidonSharp;
using System.Numerics;

namespace PoseidonTests
{
    [TestClass]
    public class HashingTests
    {
        [TestMethod]
        public void PoseidonHashTest1()
        {
            int MAX_INPUT = 13;
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs = { BigInteger.Parse("1")};
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Assert.AreEqual(BigInteger.Parse("14018714854885098128064817341184136022863799846023165041562300563859625887667"), poseidonHash);
        }

        [TestMethod]
        public void PoseidonHashTest2()
        {
            int MAX_INPUT = 13;
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs = {BigInteger.Parse("14018714854885098128064817341184136022863799846023165041562300563859625887667")};
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Assert.AreEqual(BigInteger.Parse("14909270658865229119931025210898882982405891235271722645312816457103330375266"), poseidonHash);
        }

        [TestMethod]
        public void PoseidonHashTest3()
        {
            int MAX_INPUT = 13;
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs = { 
                BigInteger.Parse("1401871485488509812806481"),
                BigInteger.Parse("900000000"),
                BigInteger.Parse("2000000000"),
                BigInteger.Parse("300000000"),
                BigInteger.Parse("500000000"),
                BigInteger.Parse("1000000000000")
            };
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Assert.AreEqual(BigInteger.Parse("12787485214590893264756354332223190110048608099767720695619651876987364797309"), poseidonHash);
        }

        [TestMethod]
        public void SHA256HelperHashTest1()
        {
            BigInteger sha256HashNumber = SHA256Helper.CalculateSHA256HashNumber("GET&https%3A%2F%2Fuat3.loopring.io%2Fapi%2Fv3%2FapiKey&accountId%3D11087");
            Assert.AreEqual(BigInteger.Parse("19400808358061590369279192378878962429412529891699423035130831734199348072763"), sha256HashNumber);
        }
    }
}
