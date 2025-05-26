using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValleyMCP.Api;
using StardewValleyMCP.Api.Controllers;
using StardewValleyMCP.Services;

namespace StardewValleyMCP
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private HttpServer? _httpServer;
        private const int ApiPort = 5000;
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
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

                // Tạo các dịch vụ
                var inventoryService = new InventoryService(this.Monitor);
                var worldService = new WorldService(this.Monitor);

                // Tạo và khởi động máy chủ HTTP
                _httpServer = new HttpServer(ApiPort, this.Monitor);
                
                // Đăng ký các controller
                _httpServer.RegisterController("inventory", new InventoryApiController(this.Monitor, inventoryService));
                _httpServer.RegisterController("world", new WorldApiController(this.Monitor, worldService));
                
                _httpServer.Start();
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Lỗi khi khởi động máy chủ API: {ex.Message}", StardewModdingAPI.LogLevel.Error);
            }
        }

        /// <summary>Dừng máy chủ web API</summary>
        private void StopWebServer()
        {
            if (_httpServer != null)
            {
                try
                {
                    this.Monitor.Log("Đang dừng máy chủ API...", StardewModdingAPI.LogLevel.Info);
                    _httpServer.Stop();
                    _httpServer = null;
                    this.Monitor.Log("Máy chủ API đã được dừng", StardewModdingAPI.LogLevel.Info);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Lỗi khi dừng máy chủ API: {ex.Message}", StardewModdingAPI.LogLevel.Error);
                }
            }
        }


    }
}
