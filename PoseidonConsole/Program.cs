using System;
using System.Diagnostics;
using System.Numerics;
using PoseidonSharp;

namespace PoseidonConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int MAX_INPUT = 13; //Max Input should be the number of inputs
         
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128);

           
            BigInteger[] inputs = { BigInteger.Parse("1") };
            BigInteger testOne = poseidon.CalculatePoseidonHash(inputs);
            Debug.Assert(testOne == BigInteger.Parse("14018714854885098128064817341184136022863799846023165041562300563859625887667"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash of test one is {testOne}");
            Eddsa eddsa = new Eddsa(testOne, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
            string signedMessage = eddsa.Sign();
            Debug.Assert(signedMessage == "0x1b45351dfb252eb5455193503b97f45209e1ac21417b0f447d9c6f48c01152af19b7b17134a637c6ee77b198b9f96f16b8f90aa28c49f845ca805878251328822438550e3413dbdb9a6a90aae5fe699a17ff9b0f55d22ea055411f6dad870995", "Signed message doesn't match expected signed message");
            Console.WriteLine($"Signed message: {signedMessage}");
            

            BigInteger[] inputsTwo = { BigInteger.Parse("1"), BigInteger.Parse("9400000000000000000000000000") };
            BigInteger testTwo = poseidon.CalculatePoseidonHash(inputsTwo);
            Debug.Assert(testTwo == BigInteger.Parse("2838802984016459847807835899395446048073460738419316401120440074779237106208"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash of test two is {testTwo}");

            BigInteger[] inputsThree = { BigInteger.Parse("100000000"), BigInteger.Parse("222222222"), BigInteger.Parse("333333333333"), BigInteger.Parse("44444444444"), BigInteger.Parse("555555555") };
            BigInteger testThree = poseidon.CalculatePoseidonHash(inputsThree);
            Debug.Assert(testThree == BigInteger.Parse("17354009943050785237626458131489048161646600294932268224674589869465464194779"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash of test threeis {testThree}");
            

            BigInteger[] inputsFour = { BigInteger.Parse("9400000000000000000000000000") };
            BigInteger testFour = poseidon.CalculatePoseidonHash(inputsFour);
            Debug.Assert(testFour == BigInteger.Parse("3510836030439778642491924990288731986089348010610686386994359896485942966833"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash of test four is {testFour}");
            Eddsa eddsaTwo = new Eddsa(testFour, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
            string signedMessageTwo = eddsaTwo.Sign();
            Debug.Assert(signedMessageTwo == "0x303e00dea504bd28fbd08006cd6d34d4eb54f7fcfac4b3e6988af7a9f87349de02da19ee8e3b35efc0b6bf67b9636b1b0142ace2b637e6cace4bf1bc59872099220db0a7abc87bf912ec0ed70c799516ceb0470500c96f0c3ce0376f7ff133e5", "Signed message doesn't match expected signed message");
            Console.WriteLine($"Signed message: {signedMessageTwo}");
            
           

            BigInteger[] inputsFive = { BigInteger.Parse("1233333333333333"), BigInteger.Parse("9400000000000000000000000000") };
            BigInteger testFive = poseidon.CalculatePoseidonHash(inputsFive);
            Debug.Assert(testFive == BigInteger.Parse("16505183251109099243420815229620567796375658488928731799784659488985814554027"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash of test five is {testFive}");
            Eddsa eddsaThree = new Eddsa(testFive, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
            string signedMessageThree = eddsaThree.Sign();
            Debug.Assert(signedMessageThree == "0x23c14b068bc535c596ac9e28e16d0839d98c354dd17c2b7153f8ce69ac31b8a130492db4bba2f9524e0fd0b11f010976b349ac9e15409ef5436539ae7bfcf9f50c1785f33fa803a39be71d1d211906f0b4070215569cffcf6e40b4d25e916805", "Signed message doesn't match expected signed message");
            Console.WriteLine($"Signed message: {signedMessageThree}");


            BigInteger[] inputsSix = { BigInteger.Parse("1233333333333333"), BigInteger.Parse("9400000000000000000000000000"), BigInteger.Parse("1223123"), BigInteger.Parse("544343434343434343") };
            BigInteger testSix = poseidon.CalculatePoseidonHash(inputsSix);
            Debug.Assert(testSix == BigInteger.Parse("1655729201495378821295527525575308339135123068455866978713071881795957281232"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash of test six is {testSix}");
            Eddsa eddsaFour = new Eddsa(testSix, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
            string signedMessageFour = eddsaFour.Sign();
            Debug.Assert(signedMessageFour == "0x2b4ee90770a2df74db0715eaa972661231e8c0dd31c4304fd1d7bb63c01e41f62dfb7db543e3a6f8db46389aaea2719873b84d169ffe4d1e4a5bd0f3951ef9683037da020de65862a55330dd3935bcd91ee726462ea48700af83470920f4b1ef", "Signed message doesn't match expected signed message");
            Console.WriteLine($"Signed message: {signedMessageFour}");



            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
