# Đề xuất Tech Stack tối ưu

* **Frontend:** React + Vite + TypeScript. Trong việc lựa chọn giữa Angular và React, React linh hoạt hơn, có cộng đồng hỗ trợ lớn và cực kỳ mạnh mẽ trong việc tích hợp các component chat AI. Nếu sau này bạn muốn làm Telegram Mini App, hệ sinh thái React cũng hỗ trợ rất tốt.
* **Backend:** .NET 10 (ASP.NET Core Web API). *(Cập nhật: chuyển từ .NET 8 sang .NET 10 — bản LTS mới nhất, hỗ trợ dài hạn hơn, theo góp ý mentor)*. Đây là framework hiệu năng cao, rất mạnh trong việc xử lý các nghiệp vụ phức tạp và đồng thời như quản lý kho và đơn hàng.
* **Kiến trúc:** Modular Monolith áp dụng Domain-Driven Design (DDD) — 1 solution, 1 database, chia theo Bounded Context (Identity, Catalog, Inventory, Ordering, AI Advisory, Notification), mỗi module có tầng Domain/Application/Infrastructure riêng.
* **Database:** PostgreSQL. Hệ thống quản lý kho, xuất/nhập/tồn và giao dịch tài chính đòi hỏi tính toàn vẹn dữ liệu cực kỳ nghiêm ngặt (chuẩn ACID). Cơ sở dữ liệu quan hệ (SQL) là lựa chọn bắt buộc cho bài toán này.
* **Telegram Bot:** Sử dụng thư viện Telegram.Bot cho .NET để nhận/gửi tin nhắn, xử lý webhook. **Telegram là kênh bán hàng đầy đủ, ngang hàng với Web** — hỗ trợ trọn vẹn luồng tìm sản phẩm → thêm giỏ hàng → checkout, không chỉ dùng để nhận thông báo.
* **AI API:** Google Gemini API. Backend .NET gọi API này để xử lý ngôn ngữ tự nhiên cho 2 tính năng: chat tư vấn cho Buyer và phân tích xu hướng cho Seller.

# Lộ trình các bước hoàn thành dự án từ đầu

## 1. Phân tích và Thiết kế hệ thống (System Design)
Xác định luồng người dùng (User Flows): Vẽ luồng thao tác riêng biệt cho Người mua (tìm kiếm, chat AI, đặt hàng) và Người bán (đăng nhập, nhập kho, duyệt đơn, xem báo cáo AI).

*(Hình ảnh: Sơ đồ Use Case Hệ thống Bán hàng & Quản lý)*

### 1. Luồng của Người Mua (Buyer Flow):
Mục tiêu là giúp người mua dễ dàng tiếp cận sản phẩm trên cả 2 nền tảng (Web và Telegram) và tận dụng AI để chốt đơn nhanh hơn.
* **Điểm chạm (Touchpoints):** Website Frontend hoặc Telegram Bot — **cả hai đều hỗ trợ đầy đủ luồng mua hàng**, không phân biệt kênh chính/phụ.
* **Hành trình cơ bản:**
  * Người dùng truy cập vào hệ thống (Web hoặc Telegram).
  * Tại đây, họ có 2 hướng đi: (1) Tự tìm kiếm, lọc theo danh mục hoặc (2) Chat với AI để hỏi về sản phẩm (VD: "Có áo thun nào màu đen giá dưới 200k không?").
  * Hệ thống/AI trả về danh sách sản phẩm phù hợp.
  * Khách hàng chọn sản phẩm -> Thêm vào giỏ hàng.
  * Chốt đơn hàng (Checkout) -> chọn phương thức thanh toán (COD hoặc chuyển khoản) -> Gửi yêu cầu đặt hàng.
  * Hệ thống xác nhận và gửi thông báo (qua Web hoặc Telegram) tình trạng đơn chờ duyệt.

### 2. Luồng của Người Bán (Seller) / Quản trị viên (Admin):
Mục tiêu là quản lý tập trung và ra quyết định kinh doanh dựa trên dữ liệu. *(Cập nhật: tách rõ 2 vai trò thay vì gộp chung)*.
* **Admin (Quản trị viên):** toàn quyền hệ thống — tạo/quản lý tài khoản Seller, phân quyền. Tài khoản Seller **không tự đăng ký công khai được**, chỉ do Admin tạo. Hệ thống luôn có sẵn 1 tài khoản Admin mặc định để khởi tạo các tài khoản Seller đầu tiên.
* **Seller (Nhân viên bán hàng):** đăng nhập vào Dashboard, có 3 tác vụ chính:
  * **Nghiệp vụ Đơn hàng:** Xem danh sách đơn mới (Pending) -> Kiểm tra tồn kho khả dụng -> Quyết định Duyệt (Approved) hoặc Hủy (Rejected) -> Xác nhận đã nhận thanh toán (nếu chuyển khoản thủ công).
  * **Nghiệp vụ Kho hàng:** Khi có hàng mới về, người bán tạo Phiếu Nhập Kho (Transaction Type: IN). Hệ thống tự động cộng dồn số lượng vào tồn kho của Sản phẩm.
  * **Phân tích AI:** Truy cập module Báo cáo AI. Hệ thống tự động đẩy dữ liệu thô (doanh số, hàng tồn, chu kỳ bán) cho AI. AI phân tích và trả về báo cáo ngôn ngữ tự nhiên (VD: "Sản phẩm A sắp hết, nên nhập thêm 50 chiếc. Sản phẩm B tồn kho quá 60 ngày, nên giảm giá").
  * Admin có đầy đủ các quyền của Seller ở trên, cộng thêm quyền quản lý tài khoản người dùng.

### 3. Luồng Thanh toán (Payment) — *bổ sung mới*
Ở bước Checkout, Buyer chọn 1 trong 2 phương thức: **COD** (thanh toán khi nhận hàng) hoặc **Chuyển khoản thủ công** (Buyer chuyển khoản, Seller vào hệ thống xác nhận đã nhận tiền để đơn chuyển sang trạng thái "Đã thanh toán"). Chưa tích hợp cổng thanh toán tự động (VNPay/Momo) ở giai đoạn này.

## Thiết kế Database (ERD)
Lên cấu trúc các bảng cốt lõi bắt buộc phải có: Users, Roles, Products, Categories, Inventory_Transactions (phiếu nhập/xuất kho), Orders, và Order_Details.

*(Hình ảnh: Sơ đồ ERD Database)*

Mô hình dữ liệu quan hệ này đảm bảo tính chặt chẽ (ACID) của nghiệp vụ bán hàng và quản lý kho.

**Giải thích các bảng và liên kết:**
* **ROLES & USERS:** Quản lý tài khoản (1 Role có nhiều Users). **3 role: Admin, Seller, Buyer** *(cập nhật — trước đây gộp chung Admin/Seller)*. Đăng ký công khai chỉ tạo được Buyer; Seller/Admin do Admin tạo. Bảng Users có trường `TelegramID` để map tài khoản Web với tài khoản chat Telegram.
* **CATEGORIES & PRODUCTS:** Quản lý danh mục hàng hóa (1 Category chứa nhiều Products). Bảng Product giữ biến `StockQuantity` (tồn kho hiện tại), được cập nhật liên tục mỗi khi có giao dịch nhập/xuất kho.
* **ORDERS & ORDER_DETAILS:** Hệ thống hóa đơn. Bảng Order lưu tổng quan đơn hàng của User, **kèm `PaymentMethod` (COD/BankTransfer) và `PaymentStatus` (Unpaid/Paid/Refunded)** *(bổ sung mới — trước đây chưa có luồng thanh toán)*. Bảng Order_Details lưu chi tiết từng mặt hàng trong đơn, giữ lại `UnitPrice` tại thời điểm mua (vì giá sản phẩm có thể thay đổi trong tương lai).
* **INVENTORY_TRANSACTIONS:** Đây là bảng quan trọng nhất để quản lý kho. Thay vì chỉ sửa số tồn kho ở bảng Products, mọi thay đổi (nhập hàng, bán hàng, hàng hoàn trả) đều phải ghi lại 1 dòng vào bảng này với loại giao dịch (`IN` hoặc `OUT`). Bảng này giúp bạn truy vết (audit) dữ liệu khi xảy ra mất mát hoặc sai lệch.
