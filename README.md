# Stardew Valley MCP Mod

## Giới thiệu

Stardew Valley MCP Mod là một mod cho game Stardew Valley, cung cấp API HTTP để truy cập thông tin túi đồ của người chơi. Mod này cho phép các ứng dụng bên ngoài có thể truy cập và hiển thị thông tin về các vật phẩm trong túi đồ của người chơi thông qua giao thức HTTP.

## Tính năng

- API HTTP đơn giản sử dụng HttpListener
- Cung cấp thông tin chi tiết về túi đồ của người chơi
- Hỗ trợ truy vấn thông tin từng vật phẩm cụ thể
- Tương thích với SMAPI 4.0.0 trở lên

## Cài đặt

1. Cài đặt [SMAPI](https://smapi.io/)
2. Tải xuống phiên bản mới nhất của mod từ [Releases](https://github.com/yourusername/StardewValleyMCP/releases)
3. Giải nén và đặt thư mục vào thư mục `Mods` của Stardew Valley
4. Khởi động game thông qua SMAPI

## Sử dụng API

API được chạy tại địa chỉ `http://localhost:5000/` và cung cấp các endpoint sau:

### Lấy toàn bộ thông tin túi đồ

```
GET http://localhost:5000/api/inventory
```

Kết quả trả về:

```json
{
  "totalItems": 10,
  "maxItems": 36,
  "items": [
    {
      "slotNumber": 0,
      "name": "Parsnip",
      "description": "A spring tuber.",
      "quantity": 5,
      "quality": "Normal",
      "category": "Vegetable",
      "sellPrice": 35
    },
    // Các vật phẩm khác...
  ],
  "timestamp": "2025-05-26T12:13:08+07:00"
}
```

### Lấy thông tin một vật phẩm cụ thể

```
GET http://localhost:5000/api/inventory/item/{slotNumber}
```

Kết quả trả về:

```json
{
  "slotNumber": 0,
  "name": "Parsnip",
  "description": "A spring tuber.",
  "quantity": 5,
  "quality": "Normal",
  "category": "Vegetable",
  "sellPrice": 35
}
```

## Phát triển

### Yêu cầu

- .NET 6.0 SDK
- SMAPI 4.0.0 trở lên

### Biên dịch

1. Clone repository
2. Mở solution trong Visual Studio hoặc JetBrains Rider
3. Biên dịch dự án

## Giấy phép

Dự án này được phân phối dưới giấy phép MIT. Xem tệp [LICENSE](LICENSE) để biết thêm chi tiết.
