using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizedCatalogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description_en",
                schema: "catalog",
                table: "products",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_vi",
                schema: "catalog",
                table: "products",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name_en",
                schema: "catalog",
                table: "products",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name_vi",
                schema: "catalog",
                table: "products",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_en",
                schema: "catalog",
                table: "categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_vi",
                schema: "catalog",
                table: "categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name_en",
                schema: "catalog",
                table: "categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name_vi",
                schema: "catalog",
                table: "categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE catalog.categories
                SET
                    name_en = name,
                    description_en = description,
                    name_vi = CASE name
                        WHEN 'Accessories' THEN 'Phụ kiện'
                        WHEN 'Electronics' THEN 'Điện tử'
                        WHEN 'Home & Kitchen' THEN 'Nhà cửa & Nhà bếp'
                        WHEN 'Others' THEN 'Khác'
                        WHEN 'Shoes' THEN 'Giày'
                        WHEN 'Sports & Outdoors' THEN 'Thể thao & Ngoài trời'
                        WHEN 'T-Shirts' THEN 'Áo thun'
                        ELSE name
                    END,
                    description_vi = CASE name
                        WHEN 'Accessories' THEN 'Phụ kiện thời trang'
                        WHEN 'Electronics' THEN 'Thiết bị điện tử'
                        WHEN 'Home & Kitchen' THEN 'Đồ dùng thiết yếu cho gia đình'
                        WHEN 'Others' THEN 'Các mặt hàng hằng ngày và phụ kiện nhỏ.'
                        WHEN 'Shoes' THEN 'Giày thể thao'
                        WHEN 'Sports & Outdoors' THEN 'Dụng cụ tập luyện và hoạt động ngoài trời'
                        WHEN 'T-Shirts' THEN 'Thời trang nam'
                        ELSE description
                    END
                WHERE name_en IS NULL OR name_vi IS NULL;

                UPDATE catalog.products
                SET
                    name_en = name,
                    description_en = description,
                    name_vi = CASE name
                        WHEN 'Ankle Boots' THEN 'Bốt cổ thấp'
                        WHEN 'Backpack' THEN 'Ba lô'
                        WHEN 'Badminton Set' THEN 'Bộ cầu lông'
                        WHEN 'Bamboo Cutting Board' THEN 'Thớt tre'
                        WHEN 'Baseball Cap' THEN 'Mũ bóng chày'
                        WHEN 'Basketball Sneakers' THEN 'Giày bóng rổ'
                        WHEN 'Bath Towel Set' THEN 'Bộ khăn tắm'
                        WHEN 'Beanie Hat' THEN 'Mũ len'
                        WHEN 'Black T-Shirt' THEN 'Áo thun đen'
                        WHEN 'Bluetooth Earbuds' THEN 'Tai nghe nhét tai Bluetooth'
                        WHEN 'Bluetooth Headphones' THEN 'Tai nghe Bluetooth'
                        WHEN 'Camping Tent 2-Person' THEN 'Lều cắm trại 2 người'
                        WHEN 'Canvas Sneakers' THEN 'Giày vải'
                        WHEN 'Casual Sneakers' THEN 'Giày sneaker thường ngày'
                        WHEN 'Ceramic Coffee Mug' THEN 'Cốc cà phê gốm'
                        WHEN 'Crossbody Bag' THEN 'Túi đeo chéo'
                        WHEN 'Crew Neck Sweatshirt' THEN 'Áo sweatshirt cổ tròn'
                        WHEN 'Cutlery Set' THEN 'Bộ dao nĩa'
                        WHEN 'Cycling Helmet' THEN 'Mũ bảo hiểm xe đạp'
                        WHEN 'Denim Jacket' THEN 'Áo khoác denim'
                        WHEN 'Desk Organizer Tray' THEN 'Khay sắp xếp bàn làm việc'
                        WHEN 'Digital Gift Card' THEN 'Thẻ quà tặng điện tử'
                        WHEN 'Dish Rack' THEN 'Kệ úp bát'
                        WHEN 'Dumbbell Set 10kg' THEN 'Bộ tạ tay 10kg'
                        WHEN 'Electric Kettle' THEN 'Ấm đun nước điện'
                        WHEN 'Flannel Shirt' THEN 'Áo sơ mi flannel'
                        WHEN 'Flip Flops' THEN 'Dép xỏ ngón'
                        WHEN 'Foam Roller' THEN 'Ống lăn giãn cơ'
                        WHEN 'Formal Oxford Shoes' THEN 'Giày Oxford trang trọng'
                        WHEN 'Graphic Hoodie' THEN 'Áo hoodie in họa tiết'
                        WHEN 'High-Top Sneakers' THEN 'Giày sneaker cổ cao'
                        WHEN 'Hiking Backpack 30L' THEN 'Ba lô leo núi 30L'
                        WHEN 'Hiking Boots' THEN 'Giày leo núi'
                        WHEN 'Jump Rope' THEN 'Dây nhảy'
                        WHEN 'Knife Set' THEN 'Bộ dao bếp'
                        WHEN 'Laptop Stand' THEN 'Giá đỡ laptop'
                        WHEN 'Leather Belt' THEN 'Thắt lưng da'
                        WHEN 'Leather Loafers' THEN 'Giày loafer da'
                        WHEN 'Leather Wallet' THEN 'Ví da'
                        WHEN 'Long Sleeve Henley' THEN 'Áo Henley dài tay'
                        WHEN 'Mechanical Keyboard' THEN 'Bàn phím cơ'
                        WHEN 'Navy Blue T-Shirt' THEN 'Áo thun xanh navy'
                        WHEN 'Non-Stick Frying Pan' THEN 'Chảo chống dính'
                        WHEN 'Notebook Set' THEN 'Bộ sổ tay'
                        WHEN 'Oversized T-Shirt' THEN 'Áo thun oversize'
                        WHEN 'Phone Case' THEN 'Ốp điện thoại'
                        WHEN 'Phone Lanyard' THEN 'Dây đeo điện thoại'
                        WHEN 'Portable SSD 1TB' THEN 'Ổ SSD di động 1TB'
                        WHEN 'Power Bank 20000mAh' THEN 'Sạc dự phòng 20000mAh'
                        WHEN 'Print Graphic Tee' THEN 'Áo thun in hình'
                        WHEN 'Resistance Bands Set' THEN 'Bộ dây kháng lực'
                        WHEN 'Reusable Tote Bag' THEN 'Túi tote tái sử dụng'
                        WHEN 'Running Jersey' THEN 'Áo chạy bộ'
                        WHEN 'Running Shoes' THEN 'Giày chạy bộ'
                        WHEN 'Sandals' THEN 'Xăng đan'
                        WHEN 'Scented Soy Candle' THEN 'Nến thơm sáp đậu nành'
                        WHEN 'Silk Necktie' THEN 'Cà vạt lụa'
                        WHEN 'Skate Shoes' THEN 'Giày trượt ván'
                        WHEN 'Sleeping Bag' THEN 'Túi ngủ'
                        WHEN 'Slip-On Shoes' THEN 'Giày lười'
                        WHEN 'Smart Plug' THEN 'Ổ cắm thông minh'
                        WHEN 'Smart Watch' THEN 'Đồng hồ thông minh'
                        WHEN 'Sports Cap' THEN 'Mũ thể thao'
                        WHEN 'Stainless Steel Water Bottle' THEN 'Bình nước thép không gỉ'
                        WHEN 'Stainless Travel Mug' THEN 'Ly giữ nhiệt du lịch'
                        WHEN 'Storage Container Set' THEN 'Bộ hộp đựng thực phẩm'
                        WHEN 'Striped Polo Shirt' THEN 'Áo polo kẻ sọc'
                        WHEN 'Sunglasses' THEN 'Kính râm'
                        WHEN 'Swim Goggles' THEN 'Kính bơi'
                        WHEN 'Table Lamp' THEN 'Đèn bàn'
                        WHEN 'Tank Top' THEN 'Áo ba lỗ'
                        WHEN 'Tennis Racket' THEN 'Vợt tennis'
                        WHEN 'Throw Pillow Cover' THEN 'Vỏ gối trang trí'
                        WHEN 'Tote Bag' THEN 'Túi tote'
                        WHEN 'Trail Running Shoes' THEN 'Giày chạy địa hình'
                        WHEN 'Trekking Poles' THEN 'Gậy leo núi'
                        WHEN 'USB-C Hub' THEN 'Bộ chia USB-C'
                        WHEN 'V-Neck T-Shirt' THEN 'Áo thun cổ chữ V'
                        WHEN 'Wall Clock' THEN 'Đồng hồ treo tường'
                        WHEN 'Water Bottle Insulated' THEN 'Bình nước giữ nhiệt'
                        WHEN 'Webcam HD' THEN 'Webcam HD'
                        WHEN 'White T-Shirt' THEN 'Áo thun trắng'
                        WHEN 'Winter Gloves' THEN 'Găng tay mùa đông'
                        WHEN 'Wireless Charger' THEN 'Đế sạc không dây'
                        WHEN 'Wireless Mouse' THEN 'Chuột không dây'
                        WHEN 'Wireless Speaker' THEN 'Loa không dây'
                        WHEN 'Wool Scarf' THEN 'Khăn len'
                        WHEN 'Wrist Watch' THEN 'Đồng hồ đeo tay'
                        WHEN 'Yoga Mat' THEN 'Thảm yoga'
                        WHEN 'Zip-Up Hoodie' THEN 'Áo hoodie khóa kéo'
                        ELSE name
                    END
                WHERE name_en IS NULL OR name_vi IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description_en",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "description_vi",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "name_en",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "name_vi",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "description_en",
                schema: "catalog",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "description_vi",
                schema: "catalog",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "name_en",
                schema: "catalog",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "name_vi",
                schema: "catalog",
                table: "categories");
        }
    }
}
