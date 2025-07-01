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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BL.NewFolder;


namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            string connectionString = builder.Configuration["DB_setting:connectionString"];
            string dbName = builder.Configuration["DB_setting:dbName"];
            string _privateKeyPAth = builder.Configuration["cryptographySetting:PrivateKaeyPath"];

            var jwtSecretKey = builder.Configuration["MySetting:JwtSecretKey"];
            var jwtIssuer = builder.Configuration["MySetting:JwtIssuer"];
            var jwtAudience = builder.Configuration["MySetting:JwtAudience"];
            var requireHttps = builder.Configuration.GetValue<bool>("MySetting:RequireHttps");
            var csrfTokenName = builder.Configuration["MySetting:CsrfTokenName"];
            var csrfCookieName = builder.Configuration["MySetting:CsrfTokenCookieName"];

            var key = Encoding.UTF8.GetBytes(jwtSecretKey);

            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Services.AddLogging();

            builder.Services.AddScoped<IRSAencryption>(provider => new RSAencryption(_privateKeyPAth));
            builder.Services.AddScoped<IEncryptionProcess, EncryptionProcess>();
            builder.Services.AddScoped<IWebSitesBL, WebSitesBL>();
            builder.Services.AddScoped<IUsersBL, UsersBL>();
            builder.Services.AddScoped<IPasswordsBL, PasswordsBL>();
            builder.Services.AddSingleton<IRSAencryption>(provider =>
            new RSAencryption(builder.Configuration["cryptographySetting:PrivateKaeyPath"]));

            builder.Services.AddScoped<IUsersRepository, UsersRepository>();
            builder.Services.AddScoped<IWebSitesRepository, WebSitesRepository>();
            builder.Services.AddScoped<IPasswordsRepository, PasswordsRepository>();
            builder.Services.AddScoped<IAuthenticationBL, AuthenticationBL>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<IEncryptionProcess, EncryptionProcess>();
            builder.Services.AddScoped<IDecryptionProcess, DecryptionProcess>();

            builder.Services.AddSingleton<IMongoClient>(provider => new MongoClient(connectionString));
            builder.Services.AddSingleton(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(dbName));
            builder.Services.AddSingleton<MongoDbService>(sp => new MongoDbService(connectionString, dbName));
            // for make 
            builder.Services.AddHttpClient();

            builder.Services.Configure<MySetting>(builder.Configuration.GetSection("MySetting"));
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "https://localhost:5173", "localhost:7249")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
                });
            });
            builder.Services.AddSecureKeyManagement();
            builder.Services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = requireHttps;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = csrfTokenName;
                options.Cookie.Name = csrfCookieName;
                options.Cookie.HttpOnly = false;
                options.Cookie.SecurePolicy = requireHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
                //options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SameSite = SameSiteMode.None;
            });

            // הגדרת CSRF Protection
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = builder.Configuration["MySetting:CsrfTokenName"] ?? "X-CSRF-TOKEN";
                options.Cookie.Name = builder.Configuration["MySetting:CsrfTokenCookieName"] ?? "__Secure-CsrfToken";
                options.Cookie.HttpOnly = false;
                options.Cookie.SecurePolicy = builder.Configuration.GetValue<bool>("MySetting:RequireHttps") ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
                //options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SameSite = SameSiteMode.None;
            });

            //builder.Services.AddDistributedMemoryCache();

            //builder.Services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromMinutes(20);
            //    options.Cookie.HttpOnly = false;
            //    options.Cookie.IsEssential = true;
            //    options.Cookie.SameSite = SameSiteMode.Lax;
            //    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            //    options.Cookie.Name = "MyAppSession";
            //});

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            //app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();
            app.MapControllers();
            {
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
            }
            // try to enc and dec
            using (var scope = app.Services.CreateScope())
            {
                var keyProvider = scope.ServiceProvider.GetRequiredService<ISecureKeyProvider>();
                var encryptionProcess = scope.ServiceProvider.GetRequiredService<IEncryptionProcess>();
                var decryptionProcess = scope.ServiceProvider.GetRequiredService<IDecryptionProcess>();

                bool keyExists = keyProvider.KeyExists();
                Console.WriteLine($"מפתחות קיימים: {keyExists}");
                string message = "4574gwds%$%@^&#UGFDG743905794bjrkbgh";

                Console.WriteLine("Original message: " + message);

                // הצפנת ההודעה
                var (encryptedData, Positions) = encryptionProcess.Encrypt(message);
                Console.WriteLine("הצפנה הושלמה");

                // פענוח ההודעה
                string decryptedMessage = decryptionProcess.Decrypt(encryptedData, Positions);

                Console.WriteLine("Original message: " + message);
                Console.WriteLine("Decrypted message: " + decryptedMessage);

                // בדיקה שההודעה המקורית זהה להודעה שפוענחה
                Console.WriteLine("Original equals decrypted: " +
                    (message == decryptedMessage ? "Yes" : "No"));
            }
            {

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
            }
            app.Run();
        }

      
        // יוצר מפתח אקראי באורך מבוקש
        private static int[] GenerateRandomKey(int length)
        {
            int[] key = new int[length];

            for (int i = 0; i < length; i++)
            {

                key[i] = RandomNumberGenerator.GetInt32(0, 256);
            }
            return key;
        }
        // יוצר מטריצה אקראית בגודל מבוקש
        
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
