using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace TestMod_SV
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private int _tickCounter = 0;
        private readonly int _ticksPerInventoryPrint = 300; // 60 ticks = 1 giây, nên 300 ticks = 5 giây
        private IHost? _webHost;
        private const int ApiPort = 5000;
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Game1.hasLoadedGame)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", StardewModdingAPI.LogLevel.Debug);
        }

        /// <summary>Raised when the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Game1.hasLoadedGame)
                return;

            // Tăng bộ đếm
            _tickCounter++;

            // Kiểm tra nếu đã đến thời điểm in thông tin túi đồ
            if (_tickCounter >= _ticksPerInventoryPrint)
            {
                // Reset bộ đếm
                _tickCounter = 0;
                
                // In thông tin túi đồ
                PrintInventoryInfo();
            }
        }

        /// <summary>Raised after the player loads a save.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            // Reset bộ đếm khi tải game
            _tickCounter = 0;
            this.Monitor.Log("Đã tải game. Bắt đầu theo dõi túi đồ.", StardewModdingAPI.LogLevel.Info);
        }

        /// <summary>Raised after the game is launched.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // Khởi động máy chủ web API
            StartWebServer();
        }

        /// <summary>Raised after returning to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            // Dừng máy chủ web API khi quay lại màn hình chính
            StopWebServer();
        }

        /// <summary>Khởi động máy chủ web API</summary>
        private void StartWebServer()
        {
            try
            {
                this.Monitor.Log($"Đang khởi động máy chủ API tại http://localhost:{ApiPort}", StardewModdingAPI.LogLevel.Info);

                _webHost = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>(context => new Startup(this.Monitor));
                        webBuilder.UseUrls($"http://localhost:{ApiPort}");
                    })
                    .Build();

                // Khởi động máy chủ web trong một task riêng biệt
                Task.Run(() => _webHost.Run());

                this.Monitor.Log($"Máy chủ API đã được khởi động tại http://localhost:{ApiPort}", StardewModdingAPI.LogLevel.Info);
                this.Monitor.Log($"Bạn có thể truy cập API túi đồ tại http://localhost:{ApiPort}/api/inventory", StardewModdingAPI.LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Lỗi khi khởi động máy chủ API: {ex.Message}", StardewModdingAPI.LogLevel.Error);
            }
        }

        /// <summary>Dừng máy chủ web API</summary>
        private void StopWebServer()
        {
            if (_webHost != null)
            {
                try
                {
                    this.Monitor.Log("Đang dừng máy chủ API...", StardewModdingAPI.LogLevel.Info);
                    _webHost.StopAsync().Wait();
                    _webHost.Dispose();
                    _webHost = null;
                    this.Monitor.Log("Máy chủ API đã được dừng", StardewModdingAPI.LogLevel.Info);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Lỗi khi dừng máy chủ API: {ex.Message}", StardewModdingAPI.LogLevel.Error);
                }
            }
        }

        /// <summary>In thông tin về túi đồ của người chơi.</summary>
        private void PrintInventoryInfo()
        {
            if (Game1.player?.Items == null)
                return;

            this.Monitor.Log("===== THÔNG TIN TÚI ĐỒ =====", StardewModdingAPI.LogLevel.Info);
            
            // Đếm số lượng vật phẩm
            int totalItems = Game1.player.Items.Count(item => item != null);
            int maxItems = Game1.player.MaxItems;
            this.Monitor.Log($"Tổng số vật phẩm: {totalItems}/{maxItems}", StardewModdingAPI.LogLevel.Info);
            
            // Liệt kê các vật phẩm
            this.Monitor.Log("Danh sách vật phẩm:", StardewModdingAPI.LogLevel.Info);
            for (int i = 0; i < Game1.player.Items.Count; i++)
            {
                var item = Game1.player.Items[i];
                if (item != null)
                {
                    string itemInfo = $"Ô {i+1}: {item.Name}";
                    if (item.Stack > 1)
                        itemInfo += $" (x{item.Stack})";
                    
                    this.Monitor.Log(itemInfo, StardewModdingAPI.LogLevel.Info);
                }
            }
            
            this.Monitor.Log("==============================", StardewModdingAPI.LogLevel.Info);
        }
    }
}
