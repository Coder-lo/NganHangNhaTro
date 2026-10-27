# Đối chiếu wireframe BTL với route trong project

| Wireframe trong báo cáo | View / route triển khai |
|---|---|
| 1. Đăng nhập | `Views/Account/Login.cshtml` — `/Account/Login` |
| 2. Đăng ký | `Views/Account/Register.cshtml` — `/Account/Register` |
| 3. Trang chủ | `Views/Rooms/Index.cshtml` — `/Rooms/Index` |
| 4. Tìm kiếm & lọc | Cùng màn hình trang chủ, lọc bất đồng bộ bằng Fetch API |
| 5. Chi tiết phòng | `Views/Rooms/Details.cshtml` — `/Rooms/Details/{id}` |
| 6. Phòng yêu thích | `Views/Favorites/Index.cshtml` — `/Favorites/Index` |
| 7. Gửi yêu cầu xem phòng | Form liên hệ trên màn hình chi tiết, POST `/Contacts/Create` |
| 8. Chi tiết yêu cầu đã gửi (người thuê) | `Views/Contacts/Details.cshtml` — `/Contacts/Details/{id}` |
| 9. Thông tin cá nhân | `Views/Account/Profile.cshtml` — `/Account/Profile` |
| 10. Tổng quan chủ trọ | `Views/Landlord/Dashboard.cshtml` — `/Landlord/Dashboard` |
| 11. Quản lý tin đăng | `Views/Landlord/Mine.cshtml` — `/Landlord/Mine` |
| 12. Đăng tin | `Views/Landlord/Create.cshtml` — `/Landlord/Create` |
| 13. Chi tiết yêu cầu xem phòng (chủ trọ) | `Views/Landlord/ContactDetails.cshtml` — `/Landlord/ContactDetails/{id}` |
| 14. Dashboard quản trị | `Views/Admin/Dashboard.cshtml` — `/Admin/Dashboard` |
| 15. Kiểm duyệt tin | `Views/Admin/Rooms.cshtml` — `/Admin/Rooms` |
| 16. Quản lý người dùng | `Views/Admin/Users.cshtml` — `/Admin/Users` |
| 17. Quản lý danh mục | `Views/Admin/Categories.cshtml` — `/Admin/Categories` |

Các màn hình dùng chung layout responsive; các thao tác ghi dữ liệu dùng POST có anti-forgery token. Hãy đối chiếu trực tiếp với wireframe gốc trong báo cáo để chỉnh các chi tiết trình bày cuối cùng theo yêu cầu của giảng viên.
