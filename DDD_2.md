# Implementing Domain-Driven Design - Vaughn Vernon (Tóm tắt)

## 1. Giới thiệu chung về cuốn sách
Cuốn sách này là một tài liệu thực hành (implementation guide) cho **Domain-Driven Design (DDD)**, xuất bản năm 2013. Nó được viết bởi Vaughn Vernon, một chuyên gia dày dạn kinh nghiệm trong lĩnh vực này. Cuốn sách tập trung vào việc làm thế nào để áp dụng các nguyên lý của DDD (từ cuốn sách gốc của Eric Evans) vào mã nguồn thực tế, với các ví dụ phong phú và một case study xuyên suốt.

Cuốn sách được chia thành 14 chương và một phụ lục. Nội dung bao gồm hai mảng lớn:
* **Strategic Design (Thiết kế chiến lược):** Các chương 1-4 tập trung vào việc hiểu tổng thể về miền vấn đề, cách phân chia hệ thống thành các Bounded Context, cách chúng tương tác với nhau và các kiến trúc phù hợp.
* **Tactical Design (Thiết kế chiến thuật):** Các chương 5-14 tập trung vào các mẫu thiết kế cụ thể (building blocks) như Entity, Value Object, Aggregate, Repository, Service, Domain Event, v.v., và cách triển khai chúng trong code.

## 2. Phân tích chi tiết từng phần

### Lời giới thiệu và Lời nói đầu (Foreword & Preface)
* **Eric Evans (tác giả cuốn sách gốc)** viết lời giới thiệu. Ông nhấn mạnh rằng cuốn sách của Vaughn Vernon mang đến một cách tiếp cận mới, chi tiết và thực tế hơn, đặc biệt là về các khái niệm trừu tượng như Aggregate và Bounded Context. Cuốn sách cũng cập nhật các xu hướng mới như Domain Events, Event Sourcing, và CQRS.
* **Lời nói đầu (Preface):** Vaughn Vernon giải thích lý do ông viết cuốn sách: để giúp các nhà phát triển "hạ cánh" từ lý thuyết cao vời của DDD xuống thực tế triển khai. Ông cũng giải thích việc chọn Java làm ngôn ngữ chính (mặc dù C# cũng có thể hiểu được) và bảo vệ việc vẫn đề cập đến ORM như Hibernate bên cạnh các công nghệ NoSQL.

### Chương 1: Bắt đầu với DDD (Getting Started with DDD)
Đây là chương khởi động, trả lời câu hỏi "Tại sao nên dùng DDD?" và "Làm thế nào để bắt đầu?".
* **Domain Model (Mô hình miền):** Mô hình phần mềm của một lĩnh vực nghiệp vụ cụ thể.
* **Ubiquitous Language (Ngôn ngữ thống nhất):** Ngôn ngữ chung được chia sẻ bởi cả chuyên gia nghiệp vụ và nhà phát triển. Nó không chỉ là biệt ngữ kinh doanh, mà là ngôn ngữ được thống nhất và thể hiện trực tiếp trong code, giúp loại bỏ sự dịch chuyển (translation) giữa nghiệp vụ và kỹ thuật.
* **Anemic Domain Model (Mô hình miền thiếu máu):** Một anti-pattern phổ biến, nơi các đối tượng chỉ chứa getter/setter mà không có hành vi (business logic) nào.
* **Giá trị kinh doanh (Business Value):** Tổ chức có được một mô hình hữu ích của miền nghiệp vụ, cải thiện trải nghiệm người dùng, phân định ranh giới rõ ràng.
* **Cách tính điểm (Scorecard):** Cung cấp một bảng câu hỏi để xác định dự án có phức tạp đến mức cần đầu tư vào DDD hay không.

### Chương 2: Miền, Miền con và Bối cảnh có ranh giới (Domains, Subdomains, and Bounded Contexts)
* **Domain:** Lĩnh vực hoạt động chính của tổ chức (ví dụ: bán lẻ trực tuyến).
* **Subdomain (Miền con):** 
    * Core Domain (Miền lõi)
    * Supporting Subdomain (Miền hỗ trợ)
    * Generic Subdomain (Miền chung chung)
* **Bounded Context (Bối cảnh có ranh giới):** Là ranh giới rõ ràng, bên trong đó một mô hình (domain model) được định nghĩa và có ý nghĩa nhất quán.

### Chương 3: Bản đồ bối cảnh (Context Maps)
Công cụ để mô tả mối quan hệ giữa các Bounded Context. 
* Các mẫu quan hệ: Partnership, Shared Kernel, Customer-Supplier, Conformist, Anticorruption Layer (ACL), Open Host Service (OHS), Published Language (PL).

### Chương 4: Kiến trúc (Architecture)
* Layered Architecture (Kiến trúc phân lớp)
* Dependency Inversion Principle (DIP)
* Hexagonal Architecture (Ports and Adapters)
* CQRS (Command Query Responsibility Segregation)
* Event-Driven Architecture (EDA) & Event Sourcing

### Phần Chiến Thuật (Tactical Design) - Các Building Blocks
* **Chương 5: Thực thể (Entities):** Khái niệm: Một đối tượng được xác định bởi danh tính (identity), không phải bởi các thuộc tính.
* **Chương 6: Đối tượng giá trị (Value Objects):** Nhấn mạnh tầm quan trọng của Value Objects. Immutable (Bất biến), Conceptual Whole (Toàn khối khái niệm).
* **Chương 7: Dịch vụ (Services):** Domain Service chứa logic nghiệp vụ, Application Service làm nhiệm vụ điều phối.
* **Chương 8: Sự kiện miền (Domain Events):** Ghi nhận một sự kiện đã xảy ra trong domain.
* **Chương 9: Mô-đun (Modules):** Cách tổ chức các đối tượng trong code. Tính kết nội cao (high cohesion).
* **Chương 10: Tổ hợp (Aggregates):** Nhóm các đối tượng (Entity và Value Object) được coi là một đơn vị thống nhất về mặt giao dịch. Có một Root Entity. Model true invariants, thiết kế nhỏ gọn.
* **Chương 11: Nhà máy (Factories):** Đóng gói logic tạo đối tượng phức tạp.
* **Chương 12: Kho lưu trữ (Repositories):** Cung cấp ảo giác về một collection trong bộ nhớ cho các Aggregate.
* **Chương 13: Tích hợp các Bối cảnh có ranh giới:** Triển khai qua REST hoặc Messaging.
* **Chương 14: Ứng dụng (Application):** User Interface, Application Services, và Infrastructure.
* **Phụ lục A: Aggregates and Event Sourcing (A+ES):** Trạng thái được lưu trữ dưới dạng chuỗi sự kiện.

## 3. Tổng kết
Đây là một tài liệu toàn diện, chia làm ba phần chính:
* **Chiến lược (Strategy):** Hiểu domain, phân chia Bounded Context, và ánh xạ chúng.
* **Kiến trúc (Architecture):** Chọn lựa các kiểu kiến trúc.
* **Chiến thuật (Tactics):** Sử dụng các building blocks để triển khai mô hình.
