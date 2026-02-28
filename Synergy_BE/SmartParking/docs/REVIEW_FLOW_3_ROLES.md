# Luồng Đánh giá & Review - 3 Roles (User, Owner, Admin)

## Tổng quan

| Role | Xem | Tạo | Sửa | Xóa |
|------|-----|-----|-----|-----|
| **User (Driver)** | Reviews bãi xe, Đánh giá của tôi | Sau booking Completed | Review của mình | Review của mình |
| **Owner** | Reviews bãi xe của mình | ❌ | ❌ | ❌ |
| **Admin** | Tất cả reviews | ❌ | ❌ | Xóa (moderation) |

---

## Luồng chi tiết

### 1. User (Driver)

**Xem đánh giá:**
- **Chi tiết bãi xe** → Section "Đánh giá" (avg rating, list reviews)
- **Đánh giá của tôi** → Danh sách reviews đã viết

**Tạo đánh giá:**
- Chỉ khi có **booking Completed**
- Từ **Lịch sử đặt chỗ** → Booking Completed → Nút "Đánh giá"
- Hoặc từ **Chi tiết booking** → Nút "Đánh giá" (nếu chưa review)

**Sửa/Xóa:**
- **Đánh giá của tôi** → Chọn review → Sửa hoặc Xóa

---

### 2. Owner

**Xem đánh giá:**
- **Bãi xe của tôi** → Chi tiết bãi xe → Tab "Đánh giá"
- Xem tất cả reviews của bãi xe mình (để cải thiện dịch vụ)

**Không được:** Tạo, Sửa, Xóa (Owner không đánh giá bãi của mình)

---

### 3. Admin

**Xem đánh giá:**
- **Dashboard** → Recent reviews
- **Quản lý đánh giá** → Danh sách tất cả, filter theo bãi xe, rating

**Xóa (moderation):**
- Xóa review vi phạm (spam, nội dung không phù hợp)

---

## API Endpoints (Backend)

| Method | Endpoint | Role | Mô tả |
|--------|----------|------|-------|
| GET | /api/parking-lots/{id}/reviews | Public | Danh sách reviews bãi xe |
| GET | /api/parking-lots/{id}/reviews/summary | Public | Summary (avg, counts, recent) |
| POST | /api/parking-lots/{id}/reviews | User | Tạo review (body: bookingId, rating, comment) |
| PUT | /api/parking-lots/reviews/{id} | User | Sửa review của mình |
| DELETE | /api/parking-lots/reviews/{id} | User | Xóa review của mình |
| GET | /api/users/reviews | User | Đánh giá của tôi |
| GET | /api/admin/reviews | Admin | Tất cả reviews (filter) |
| DELETE | /api/admin/reviews/{id} | Admin | Xóa (moderation) |

---

## Điều kiện tạo review

- Booking phải **Completed**
- User phải là người đặt (booking.UserId)
- Mỗi booking chỉ review **1 lần**
