using Microsoft.Extensions.DependencyInjection;

namespace MyProject.Common.Security
{
    /// <summary>
    /// הרחבות עבור הגדרת שירותי אבטחה
    /// </summary>
    public static class SecurityExtensions
    {
        /// <summary>
        /// מוסיף ניהול מפתחות מאובטח לDI Container
        /// </summary>
        public static IServiceCollection AddSecureKeyManagement(this IServiceCollection services)
        {
            services.AddSingleton<ISecureKeyProvider, FileBasedSecureKeyProvider>();
            return services;
        }
    }
}