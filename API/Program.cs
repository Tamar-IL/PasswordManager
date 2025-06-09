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

            builder.Services.Configure<MySetting>(builder.Configuration.GetSection("MySetting"));
            builder.Services.AddScoped<MySetting>();

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
            var setting = app.Services.GetRequiredService<IOptions<MySetting>>().Value;

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
           

            app.UseCors("AllowAll");

            //int[] frequencyArray = CreateFrequencyArray(input);

            //Console.WriteLine("\n������ ������:");
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
            // ���� ���� - ���� ������ �������� Login
           ////--rsa----

            Random random = new Random();

            int[] keyEncryptionKey = builder.Configuration.GetSection("Encryption:MasterKey").Get<int[]>(); // ���� ����� 256

            int[,] initMatrix = GenerateRandomMatrix(13, 13);
            //int[,] initMatrix = GenerateRandomMatrix(5, 5);
            //GenerateKeyEncryption keyEncryption1 = new GenerateKeyEncryption(keyEncryptionKey, initMatrix);
            //generateKeyDecryption generateKeyDecryption1 = new generateKeyDecryption(keyEncryptionKey, initMatrix);
            EncryptionProcess cryptosystem = new EncryptionProcess(keyEncryptionKey, initMatrix,Options.Create(setting));
            DecryptionProcess decryptosystem = new DecryptionProcess(keyEncryptionKey,initMatrix, Options.Create(setting));
           
            Console.WriteLine("\n-----------------------------\n");
            for (int i =0;i< keyEncryptionKey.Length;i++){ Console.Write(" "+keyEncryptionKey[ i]+", "); }
            Console.WriteLine("\n-----------------------------\n");

            // ����� ������
            string message = "" +
                "try15cahr0Vassw";
                            
            Console.WriteLine("Original message: " + message);

            
           
            // ����� ������
            var (encryptedData, vectorOfPositions) = cryptosystem.Encrypt(message);
          

            // ����� ������
            string decryptedMessage = decryptosystem.Decrypt(encryptedData, vectorOfPositions);

            Console.WriteLine("\nmessage encreypt: " + message + "--end");
            Console.WriteLine("\nmessage: " + decryptedMessage+"--end");

            // ����� ������� ������� ��� ������ �������
            Console.WriteLine("\nOriginal equals decrypted: " +
                (message == decryptedMessage ? "Yes" : "No"));
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        /// <summary>
        /// ���� ���� ����� ����� �����
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
        /// ���� ������ ������ ����� �����
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
            int[] frequency = new int[26]; // 26 ������ �������

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
