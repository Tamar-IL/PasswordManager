using BL;
using DAL;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IBL;
using IDAL;
using System.Numerics;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Driver;
using System.Reflection;
using BL.encryption;
using BL.NewFolder;
using BL.decryption;
using System;
using BL.RSAForMasterKay;
using IBL.RSAForMasterKey;


namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
         
            string connectionString = "mongodb+srv://swenlly152:swenl152@cluster0.6yf8j.mongodb.net/?appName=Cluster0";
            string dbName = "passwordManagement";
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            BigInteger p = new BigInteger(123);
            BigInteger q = new BigInteger(456);
            BigInteger s = new BigInteger(789);

            builder.Services.AddLogging();
            string _privateKeyPAth = builder.Configuration["cryptographySetting:PrivateKaeyPath"];

            builder.Services.AddScoped<IRSAencryption>(provider => new RSAencryption(_privateKeyPAth));

            builder.Services.AddScoped<IkeyGeneration>(provider => new KeyGeneration(p, q, s));
            builder.Services.AddScoped<IBBSRandomGenerator>(provider => new BBSRandomGenerator(p, q, s));
            builder.Services.AddScoped<IEncryptionProcess, EncryptionProcess>();
          
            builder.Services.AddScoped<IWebSitesBL, WebSitesBL>();
            builder.Services.AddScoped<IUsersBL, UsersBL>();
            builder.Services.AddScoped<IPasswordsBL, PasswordsBL>();
            
            builder.Services.AddSingleton<IRSAencryption>(provider =>
            new RSAencryption(builder.Configuration["cryptographySetting:PrivateKaeyPath"]));
            builder.Services.AddScoped<IUsersRepository, UsersRepository>();
            builder.Services.AddScoped<IWebSitesRepository, WebSitesRepository>();
            builder.Services.AddScoped<IPasswordsRepository, PasswordsRepository>();
            builder.Services.AddSingleton<IMongoClient>(provider => new MongoClient(connectionString));
            builder.Services.AddSingleton(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(dbName));
            builder.Services.AddSingleton<MongoDbService>(sp => new MongoDbService(connectionString, dbName));

            //builder.Services.AddSingleton<MongoDbService>(provider => new MongoDbService(connectionString, dbName));
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
           

            app.UseCors("AllowAll");

            // יצירת מפתח ראשי
            //int[] masterKey = builder.Configuration.GetSection("Encryption:MasterKey").Get<int[]>();

            //int[] masterKey = GenerateRandomKey(256);

            // יצירת מטריצת אתחול

            // יצירת מופע של מערכת ההצפנה'Random random = new Random();
            //string str1 = "this is message for testhhhhhhhhhhhhhhhhhhhhhhhhhfyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyygggggggggggggggggggggggggggggdhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhheererjdlakjfljhdhfjhdhhhkk";
            //string str2 = "hhis is mesyyge forytesthhhhhhhhhfyhyhyhhhyhyhhyhfyyyyyyyyyyyyyyyyyyyyyyyyyyyygyyyygggggghhgggggghgggggggggghhghhhhhhhhhhhhhhhhhhhhhhhhhehhhhhhhhhhhhhhhhheeerjdlakjfldhfjhdhhkk";
            //Console.WriteLine( str1.OrderBy(c => c).SequenceEqual(str2.OrderBy(c => c)));
            //Console.WriteLine("הכנס מחרוזת:");
            //string input = Console.ReadLine();

            //int[] frequencyArray = CreateFrequencyArray(input);

            //Console.WriteLine("\nתדירות התווים:");
            //for (int i = 0; i < frequencyArray.Length; i++)
            //{
            //    if (frequencyArray[i] > 0)
            //        Console.WriteLine($"{(char)(i + 'a')}: {frequencyArray[i]}");
            //}

            ////----rsa--
            //string privateKeyPAth = builder.Configuration["cryptographySetting:PrivateKaeyPath"];
            //RSAencryption rsaEncryption = new RSAencryption(_privateKeyPAth);
            //var (publicKey, PrivateKay) = rsaEncryption.GeneratePairKey();
            //byte[] encrypteData = rsaEncryption.Encrypt("32!676gsgh$^&@hdg", publicKey);
            //string decryptText = rsaEncryption.Decrypt(encrypteData,PrivateKay);
            //Console.WriteLine("orginalData:\n"+ "32!676gsgh$^&@hdg" + "\nencryptData:\n" + encrypteData + "\ndecryptData : \n" + decryptText);
            // מבחן מהיר - הוסף בתחילת הפונקציה Login
           ////--rsa----

            Random random = new Random();

            int[] keyEncryptionKey = new int[256]; // מערך בגודל 64

            // יצירת ערכים אקראיים בין 0 ל-99
            for (int i = 0; i < keyEncryptionKey.Length; i++)
            {
                keyEncryptionKey[i] = random.Next(0, 100); // ערך אקראי בין 0 ל-99
            }
            //int[,] initMatrix = GenerateRandomMatrix(13, 13);
            int[,] initMatrix = GenerateRandomMatrix(5, 5);
            //GenerateKeyEncryption keyEncryption1 = new GenerateKeyEncryption(keyEncryptionKey, initMatrix);
            //generateKeyDecryption generateKeyDecryption1 = new generateKeyDecryption(keyEncryptionKey, initMatrix);
            EncryptionProcess cryptosystem = new EncryptionProcess(keyEncryptionKey, initMatrix);
            DecryptionProcess decryptosystem = new DecryptionProcess(keyEncryptionKey,initMatrix);
           
            Console.WriteLine("\n-----------------------------\n");
            for (int i =0;i< keyEncryptionKey.Length;i++){ Console.Write(" "+keyEncryptionKey[ i]+", "); }
            Console.WriteLine("\n-----------------------------\n");
            
            // הודעה לדוגמה
            string message = "" +
                "trfg6h8j9k" +
                             //"ssage for " +
                             //"testhhhhhh" +

                             //"hhhhhhhhhf" +
                             "55555";
                             //"yyyyyyyyyy" +
                             //"yyyyyyyyyy" +
                             //"yyyggggggg" +
                             //"gggggggggg" +
                             //"gggggggggg" +
                             //"ggdhhhhhhh" +
                             //"hhhhhhhhhh" +
                             //"hhhhhhhhhh" +
                             //"hhhhhhhhhh" +
                             //"hhhheererj" +
                             //"dlakjfljhd" +
                             //"----------";
            Console.WriteLine("Original message: " + message);

            //string message2 = "-----------" +
            //                 "messe ag  2" +
            //                 "Be for you today!" +
            //                 "Be for you today!" +
            //                 "Be for you today!" +
            //                 "Be for you today!" +
            //                 "Be for you today!" +

            //                 "Be for you today!" +
            //                 "Be for you today!"+

            //                 "@@!";
            //int num = message2.Length;
           
            // הצפנת ההודעה
            var (encryptedData, vectorOfPositions) = cryptosystem.Encrypt(message);
            //var (encryptedData1, vectorOfPositions1) = cryptosystem.Encrypt(message2);

            //Console.WriteLine("\nEncrypted data (first 20 values): " +
            //    string.Join(", ", encryptedData.Take(20)) + "...");

            //Console.WriteLine("\nVector of positions: " +
            //    string.Join(", ", vectorOfPositions));

            // פענוח ההודעה
            string decryptedMessage = decryptosystem.Decrypt(encryptedData, vectorOfPositions);
            //string decryptedMessage1 = decryptosystem.Decrypt(encryptedData1, vectorOfPositions1);

            Console.WriteLine("\nmessage encreypt: " + message + "--end");
            Console.WriteLine("\nmessage: " + decryptedMessage+"--end");
            //Console.WriteLine("\nmessageencrypt1: " + message2);
            //Console.WriteLine("\nDecrypted message: " + decryptedMessage1);

            // בדיקה שההודעה המקורית זהה להודעה שפוענחה
            Console.WriteLine("\nOriginal equals decrypted: " +
                (message == decryptedMessage ? "Yes" : "No"));
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        /// <summary>
        /// יוצר מפתח אקראי באורך מבוקש
        /// </summary>
        private static int[] GenerateRandomKey(int length)
        {
            int[] key = new int[length];
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                key[i] = random.Next(0, 256);
            }

            return key;
        }

        /// <summary>
        /// יוצר מטריצה אקראית בגודל מבוקש
        /// </summary>
        private static int[,] GenerateRandomMatrix(int rows, int cols)
        {
            int[,] matrix = new int[rows, cols];
            Random random = new Random();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = random.Next(0, 256);
                }
            }

            return matrix;
        }
        static int[] CreateFrequencyArray(string str)
        {
            int[] frequency = new int[26]; // 26 אותיות באנגלית

            foreach (char c in str.ToLower())
            {
                if (c >= 'a' && c <= 'z')
                    frequency[c - 'a']++;
            }

            return frequency;
        }
        private static int[] GenerateMasterKey()
        {
            var masterKey = new int[256];
            var rng = new Random();

            for (int i = 0; i < masterKey.Length; i++)
            {
                masterKey[i] = rng.Next(1, 1001); // Values from 1 to 1000 as mentioned in paper
            }

            return masterKey;
        }

    }
}
