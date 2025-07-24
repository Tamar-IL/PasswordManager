using Microsoft.Extensions.DependencyInjection;

namespace MyProject.Common.Security
{
    // הרחבות עבור הגדרת שירותי אבטחה
   
    public static class SecurityExtensions
    {
        // מוסיף ניהול מפתחות מאובטח לDI Container
        public static IServiceCollection AddSecureKeyManagement(this IServiceCollection services)
        {
            services.AddSingleton<ISecureKeyProvider, FileBasedSecureKeyProvider>();
            return services;
        }
    }
}