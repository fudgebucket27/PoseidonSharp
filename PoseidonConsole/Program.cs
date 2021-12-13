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
            //Test case 1
            int MAX_INPUT = 1; //Max Input should be the number of BigInteger inputs
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128);
            BigInteger[] inputs = { BigInteger.Parse("1000000000000000000000000") };
            BigInteger testOnePoseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Debug.Assert(testOnePoseidonHash == BigInteger.Parse("13920778905481904875716139411632478880020197875852667364732802355887164497753"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash of test one is {testOnePoseidonHash}");
            Eddsa eddsa = new Eddsa(testOnePoseidonHash, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
            string signedMessage = eddsa.Sign();
            Debug.Assert(signedMessage == "0x2055927e522e0e97e82bfb6195fd23e3163249d01411285d09760244e0c57ea1241952e4472ab3da5a95a6dc1d629c6eb2a12eee684a998fdb722b30be86a3eb260e7bf0d9849619d4859c79855ffb5c8611a35669630a5fe68e8b70e6875163", "Signed message doesn't match expected signed message");
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

            //Test case 3
            int MAX_INPUT_THREE = 13; //Max Input should be the number of BigInteger inputs
            Poseidon poseidonThree = new Poseidon(MAX_INPUT_THREE + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputsThree = { 
                BigInteger.Parse("1233333333333333"), 
                BigInteger.Parse("9400000000000000000000000000"),
                BigInteger.Parse("1223123"), 
                BigInteger.Parse("544343434343434343"),
                BigInteger.Parse("544343434343434343"),
                BigInteger.Parse("544343434343434343"),
                BigInteger.Parse("544343434343434343"),
                BigInteger.Parse("544343434343434343"),
                BigInteger.Parse("544343434343434343"),
                BigInteger.Parse("544343434343434343"),
                BigInteger.Parse("544343434343434343"),
                BigInteger.Parse("544343434343434343"),
                BigInteger.Parse("544343434343434343"),
            };
            BigInteger testThreePoseidonHash = poseidonThree.CalculatePoseidonHash(inputsThree);
            Debug.Assert(testThreePoseidonHash == BigInteger.Parse("5672127111078700825511016759205848053541633732578151671665123112660080656153"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash of test three is {testThreePoseidonHash}");
            Eddsa eddsaThree = new Eddsa(testThreePoseidonHash, Environment.GetEnvironmentVariable("LoopringPrivateKey", EnvironmentVariableTarget.User)); //Put in the calculated poseidon hash in order to Sign
            string signedMessageThree = eddsaThree.Sign();
            Debug.Assert(signedMessageThree == "0x152b971f5796226639add0a1572e348605a4291675fd10f7b8fa989057246e9815fc8e98e246ce212355d49fc4df5a0a6024ee3604164f3c2227b4380e9150c70427e5913a51bc85ead489ef28c97fe85aa399da0d37cb7ff845d511e44a2d50", "Signed message doesn't match expected signed message");
            Console.WriteLine($"Signed message: {signedMessageThree}");

            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
