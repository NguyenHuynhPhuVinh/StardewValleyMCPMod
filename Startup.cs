using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StardewModdingAPI;
using TestMod_SV.Services;

namespace TestMod_SV
{
    /// <summary>
    /// Lớp cấu hình cho ứng dụng ASP.NET Core
    /// </summary>
    public class Startup
    {
        private readonly IMonitor _monitor;

        public Startup(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Cấu hình các dịch vụ cho ứng dụng
        /// </summary>
        /// <param name="services">Bộ sưu tập dịch vụ</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Đăng ký dịch vụ túi đồ
            services.AddSingleton<IInventoryService>(new InventoryService(_monitor));

            // Thêm controllers
            services.AddControllers();
        }

        /// <summary>
        /// Cấu hình pipeline xử lý HTTP
        /// </summary>
        /// <param name="app">Ứng dụng</param>
        public void Configure(IApplicationBuilder app)
        {
            // Cấu hình middleware
            app.UseRouting();
            
            // Cấu hình endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
