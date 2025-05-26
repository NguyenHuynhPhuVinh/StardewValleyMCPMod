using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValleyMCP.Models;

namespace StardewValleyMCP.Services
{
    /// <summary>
    /// Dịch vụ quản lý thế giới game
    /// </summary>
    public class WorldService : IWorldService
    {
        private readonly IMonitor _monitor;

        /// <summary>
        /// Khởi tạo dịch vụ quản lý thế giới game
        /// </summary>
        /// <param name="monitor">Monitor để ghi log</param>
        public WorldService(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Quét các vật thể xung quanh người chơi
        /// </summary>
        /// <param name="radius">Bán kính quét (tính bằng ô)</param>
        /// <returns>Thông tin về các vật thể xung quanh</returns>
        public ScanObjectsResponse ScanObjects(int radius)
        {
            _monitor.Log($"Đang quét vật thể xung quanh với bán kính {radius} ô", LogLevel.Info);

            // Kiểm tra người chơi đã vào thế giới game chưa
            if (!Game1.hasLoadedGame || Game1.player == null || Game1.currentLocation == null)
            {
                _monitor.Log("Không thể quét vật thể: Người chơi chưa vào thế giới game", LogLevel.Error);
                return new ScanObjectsResponse();
            }

            // Lấy thông tin vị trí người chơi
            int playerX = (int)Game1.player.Position.X / Game1.tileSize;
            int playerY = (int)Game1.player.Position.Y / Game1.tileSize;
            string locationName = Game1.currentLocation.Name;

            _monitor.Log($"Vị trí người chơi: {playerX}, {playerY} tại {locationName}", LogLevel.Debug);

            // Tạo kết quả
            var result = new ScanObjectsResponse
            {
                PlayerX = playerX,
                PlayerY = playerY,
                LocationName = locationName,
                Radius = radius
            };

            // Quét các vật thể
            ScanGameObjects(result, playerX, playerY, radius);
            ScanTerrainFeatures(result, playerX, playerY, radius);
            ScanCharacters(result, playerX, playerY, radius);

            _monitor.Log($"Đã tìm thấy {result.Objects.Count} vật thể", LogLevel.Info);

            return result;
        }

        /// <summary>
        /// Quét các vật thể trong game
        /// </summary>
        private void ScanGameObjects(ScanObjectsResponse result, int playerX, int playerY, int radius)
        {
            try
            {
                // Quét các vật thể trong game (đồ vật, máy móc, v.v.)
                foreach (var pair in Game1.currentLocation.objects.Pairs)
                {
                    var position = pair.Key;
                    var obj = pair.Value;

                    // Tính khoảng cách
                   double distance = Math.Sqrt(Math.Pow((int)position.X - playerX, 2) + Math.Pow((int)position.Y - playerY, 2));

                    // Chỉ lấy các vật thể trong bán kính
                    if (distance <= radius)
                    {
                        var objModel = new WorldObjectModel
                        {
                            Name = obj.Name,
                            Type = "Object",
                            X = (int)position.X,
                            Y = (int)position.Y,
                            Distance = Math.Round(distance, 2)
                        };

                        // Thêm thông tin bổ sung
                        if (obj.ParentSheetIndex > 0)
                            objModel.AdditionalInfo["Id"] = obj.ParentSheetIndex;

                        if (obj is Chest chest)
                        {
                            objModel.Type = "Chest";
                          objModel.AdditionalInfo["ItemCount"] = chest.Items?.Count ?? 0;
                        }
                        else if (obj is Furniture furniture)
                        {
                            objModel.Type = "Furniture";
                        }

                        result.Objects.Add(objModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Lỗi khi quét vật thể: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Quét các đặc điểm địa hình
        /// </summary>
        private void ScanTerrainFeatures(ScanObjectsResponse result, int playerX, int playerY, int radius)
        {
            try
            {
                // Quét các đặc điểm địa hình (cây, cỏ, v.v.)
                foreach (var pair in Game1.currentLocation.terrainFeatures.Pairs)
                {
                    var position = pair.Key;
                    var feature = pair.Value;

                    // Tính khoảng cách
                    double distance = Math.Sqrt(Math.Pow((int)position.X - playerX, 2) + Math.Pow((int)position.Y - playerY, 2));

                    // Chỉ lấy các vật thể trong bán kính
                    if (distance <= radius)
                    {
                        var objModel = new WorldObjectModel
                        {
                            Name = GetTerrainFeatureName(feature),
                            Type = "TerrainFeature",
                            X = (int)position.X,
                            Y = (int)position.Y,
                            Distance = Math.Round(distance, 2)
                        };

                        // Thêm thông tin bổ sung dựa trên loại đặc điểm địa hình
                        if (feature is Tree tree)
                        {
                            objModel.Type = "Tree";
                            objModel.AdditionalInfo["GrowthStage"] = tree.growthStage;
                            objModel.AdditionalInfo["TreeType"] = tree.treeType;
                        }
                        else if (feature is HoeDirt dirt)
                        {
                            objModel.Type = "HoeDirt";
                            if (dirt.crop != null)
                            {
                                objModel.Name = "Crop";
                                objModel.AdditionalInfo["CropType"] = dirt.crop.indexOfHarvest;
                                objModel.AdditionalInfo["GrowthStage"] = dirt.crop.currentPhase;
                                objModel.AdditionalInfo["FullyGrown"] = dirt.crop.fullyGrown;
                            }
                        }

                        result.Objects.Add(objModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Lỗi khi quét đặc điểm địa hình: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Quét các nhân vật
        /// </summary>
        private void ScanCharacters(ScanObjectsResponse result, int playerX, int playerY, int radius)
        {
            try
            {
                // Quét các nhân vật (NPC, động vật, v.v.)
                foreach (var character in Game1.currentLocation.characters)
                {
                    // Bỏ qua người chơi
                    if (character is Farmer)
                        continue;

                    // Lấy vị trí theo ô
                    int charX = (int)character.Position.X / Game1.tileSize;
                    int charY = (int)character.Position.Y / Game1.tileSize;

                    // Tính khoảng cách
                    double distance = Math.Sqrt(Math.Pow(charX - playerX, 2) + Math.Pow(charY - playerY, 2));

                    // Chỉ lấy các nhân vật trong bán kính
                    if (distance <= radius)
                    {
                        var objModel = new WorldObjectModel
                        {
                            Name = character.Name,
                            Type = "Character",
                            X = charX,
                            Y = charY,
                            Distance = Math.Round(distance, 2)
                        };

                        // Thêm thông tin bổ sung dựa trên loại nhân vật
                        if (character is NPC npc)
                        {
                            objModel.Type = "NPC";
                            objModel.AdditionalInfo["Friendship"] = Game1.player.friendshipData.ContainsKey(npc.Name) ? 
                                Game1.player.friendshipData[npc.Name].Points : 0;
                        }
                        else if (character is StardewValley.Monsters.Monster monster)
                        {
                            objModel.Type = "Monster";
                            objModel.AdditionalInfo["Health"] = monster.Health;
                            objModel.AdditionalInfo["MaxHealth"] = monster.maxHealth;
                        }

                        result.Objects.Add(objModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Lỗi khi quét nhân vật: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Lấy tên của đặc điểm địa hình
        /// </summary>
        private string GetTerrainFeatureName(TerrainFeature feature)
        {
            if (feature is Tree)
                return "Tree";
            else if (feature is Grass)
                return "Grass";
            else if (feature is HoeDirt)
                return "HoeDirt";
            else if (feature is Bush)
                return "Bush";
            else if (feature is FruitTree)
                return "FruitTree";
            else
                return feature.GetType().Name;
        }
    }
}
