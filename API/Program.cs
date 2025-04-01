using BL;
using DAL;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IBL;
using IDAL;
using System.Numerics;
using Microsoft.AspNetCore.Hosting;


namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            string connectionString = "mongodb+srv://swenlly152:swenl152@cluster0.6yf8j.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0?ssl=false";
            string dbName = "passwordManagement";
            var mongoDbService = new MongoDbService(connectionString, dbName);

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            BigInteger p = new BigInteger(123);
            BigInteger q = new BigInteger(456);
            BigInteger s = new BigInteger(789);

            builder.Services.AddScoped<IkeyGeneration>(provider => new KeyGeneration(p, q, s));
            builder.Services.AddScoped<IBBSRandomGenerator>(provider => new BBSRandomGenerator(p, q, s));
            builder.Services.AddScoped<IEncryptionProcess, EncryptionProcess>();
            builder.Services.AddScoped<IWebSites, WebSites>();
            builder.Services.AddScoped<IUsers, Users>();
            builder.Services.AddScoped<IPasswords, Passwords>();

            builder.Services.AddScoped<IUsersRepository, UsersRepository>();
            builder.Services.AddScoped<IWebSitesRepository, WebSitesRepository>();
            builder.Services.AddScoped<IPasswordsRepository, PasswordsRepository>();

            builder.Services.AddSingleton<MongoDbService>(provider => new MongoDbService(connectionString, dbName));
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
