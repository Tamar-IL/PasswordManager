
using DAL;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            builder.Services.AddSingleton<MongoDbService>(provider => new MongoDbService(connectionString,dbName));
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer(); // Uncomment this line to register endpoints API explorer
            builder.Services.AddSwaggerGen();
            // Add this line to register authorization services

            // Add services to the container.

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
          

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
