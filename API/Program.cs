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
using System.Runtime;
using Microsoft.Extensions.Options;
using MyProject.Common;
using MyProject.Common.Security;
using System.Security.Cryptography;


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

            builder.Services.AddLogging();
            string _privateKeyPAth = builder.Configuration["cryptographySetting:PrivateKaeyPath"];

            builder.Services.AddScoped<IRSAencryption>(provider => new RSAencryption(_privateKeyPAth));

            //builder.Services.AddScoped<IkeyGeneration>(provider => new KeyGeneration(p, q, s));
            //builder.Services.AddScoped<IBBSRandomGenerator>(provider => new BBSRandomGenerator(p, q, s));
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

            //builder.Services.Configure<MySetting>(builder.Configuration.GetSection("MySetting"));
            //builder.Services.AddScoped<MySetting>();
            builder.Services.Configure<MySetting>(builder.Configuration.GetSection("MySetting"));
            builder.Services.AddScoped<IEncryptionProcess, EncryptionProcess>();
            builder.Services.AddScoped<IDecryptionProcess, DecryptionProcess>();

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
            
            builder.Services.AddSecureKeyManagement();

            builder.Services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var app = builder.Build();
            var setting = app.Services.GetRequiredService<IOptions<MySetting>>().Value;
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

          
            app.UseCors("AllowAll");

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


            //int[] keyEncryptionKey = builder.Configuration.GetSection("Encryption:MasterKey").Get<int[]>(); // מערך בגודל 256

            //int[,] initMatrix = GenerateRandomMatrix(13, 13);
            ////int[,] initMatrix = GenerateRandomMatrix(5, 5);

            //byte[] masterKey = _keyProvider.GetMasterKey();
            //int[,] initMatrix = _keyProvider.GetInitializationMatrix();
            ////GenerateKeyEncryption keyEncryption1 = new GenerateKeyEncryption(keyEncryptionKey, initMatrix);
            ////generateKeyDecryption generateKeyDecryption1 = new generateKeyDecryption(keyEncryptionKey, initMatrix);
            ////EncryptionProcess cryptosystem = new EncryptionProcess(keyEncryptionKey, initMatrix,Options.Create(setting));
            //EncryptionProcess cryptosystem = new EncryptionProcess(Options.Create(setting));
            //DecryptionProcess decryptosystem = new DecryptionProcess(keyEncryptionKey,initMatrix, Options.Create(setting));

            using (var scope = app.Services.CreateScope())
            {
                var keyProvider = scope.ServiceProvider.GetRequiredService<ISecureKeyProvider>();
                var encryptionProcess = scope.ServiceProvider.GetRequiredService<IEncryptionProcess>();
                var decryptionProcess = scope.ServiceProvider.GetRequiredService<IDecryptionProcess>();

                bool keyExists = keyProvider.KeyExists();
                Console.WriteLine($"מפתחות קיימים: {keyExists}");
                string message = "TestPassword123";

                Console.WriteLine("Original message: " + message);

                // הצפנת ההודעה
                var (encryptedData, vectorOfPositions) = encryptionProcess.Encrypt(message);
                Console.WriteLine("הצפנה הושלמה");

                // פענוח ההודעה
                string decryptedMessage = decryptionProcess.Decrypt(encryptedData, vectorOfPositions);

                Console.WriteLine("Original message: " + message);
                Console.WriteLine("Decrypted message: " + decryptedMessage);

                // בדיקה שההודעה המקורית זהה להודעה שפוענחה
                Console.WriteLine("Original equals decrypted: " +
                    (message == decryptedMessage ? "Yes" : "No"));
            }
            //    // הודעה לדוגמה
            //    string message = "" +
            //    "1234567890!@#$%^&*()qwertyuiop[]asdfghjkl;'zxcvbnm,./";

            //Console.WriteLine("Original message: " + message);

            //// הצפנת ההודעה
            //var (encryptedData, vectorOfPositions) = cryptosystem.Encrypt(message);
            //Console.WriteLine("encryptPass:\n" + encryptedData);

            //for (int i = 0; i < encryptedData.Length; i++)
            //{
            //    Console.Write(+encryptedData[i]+" , ");

            //}

            //// פענוח ההודעה
            //string decryptedMessage = decryptosystem.Decrypt(encryptedData, vectorOfPositions);

            //Console.WriteLine("\norginal message: " + message + "--end");
            //Console.WriteLine("\ndecrypt message: " + decryptedMessage+"--end");

            //// בדיקה שההודעה המקורית זהה להודעה שפוענחה
            //Console.WriteLine("\nOriginal equals decrypted: " +
            //    (message == decryptedMessage ? "Yes" : "No"));
            ////QuickTest();
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

            for (int i = 0; i < length; i++)
            {

                key[i] =  RandomNumberGenerator.GetInt32(0, 256);
            }

            return key;
        }

        /// <summary>
        /// יוצר מטריצה אקראית בגודל מבוקש
        /// </summary>
        private static int[,] GenerateRandomMatrix(int rows, int cols)
        {
            int[,] matrix = new int[rows, cols];
            //Random random = new Random();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = RandomNumberGenerator.GetInt32(0, 256);
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
        // פונקציית בדיקה
        //public static void QuickTest()
        //{
        //    Console.WriteLine("=== בדיקה מהירה של ההצפנה ===");

        //    // הגדרות
        //    var settings = new MySetting
        //    {
        //        BlockSize = 78,
        //        subBlockSize = 13,
        //        graphOrder = 13,
        //        keySize = 256,
        //        minPass = 8
        //    };

        //    var masterKey = new int[256];
        //    for (int i = 0; i < 256; i++)
        //    {
        //        masterKey[i] = (i * 5 + 17) % 256;
        //    }

        //    var initMatrix = new int[13, 13];
        //    for (int i = 0; i < 13; i++)
        //    {
        //        for (int j = 0; j < 13; j++)
        //        {
        //            initMatrix[i, j] = (i * j + 3) % 256;
        //        }
        //    }

        //    var options = Microsoft.Extensions.Options.Options.Create(settings);

        //    // בדיקה
        //    string[] testPasswords = { "test", "password123", "mySecret!" };

        //    foreach (string password in testPasswords)
        //    {
        //        Console.WriteLine($"\nבודק סיסמה: '{password}'");

        //        var encryption = new EncryptionProcess(masterKey, initMatrix, options);
        //        var decryption = new DecryptionProcess(masterKey, initMatrix, options);

        //        var (encrypted, vector) = encryption.Encrypt(password);
        //        string decrypted = decryption.Decrypt(encrypted, vector);

        //        Console.WriteLine($"מקורי:    '{password}'");
        //        Console.WriteLine($"מפוענח:   '{decrypted}'");
        //        Console.WriteLine($"תואם:     {password == decrypted}");

        //        if (password != decrypted)
        //        {
        //            Console.WriteLine("? שגיאה בהצפנה/פענוח!");
        //            // הדפס השוואה תו-תו
        //            for (int i = 0; i < Math.Max(password.Length, decrypted.Length); i++)
        //            {
        //                char original = i < password.Length ? password[i] : ' ';
        //                char recovered = i < decrypted.Length ? decrypted[i] : ' ';
        //                if (original != recovered)
        //                {
        //                    Console.WriteLine($"  מיקום {i}: '{original}' ? ? '{recovered}'");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("? הצלחה!");
        //        }
        //    }
        //}

    }
}
