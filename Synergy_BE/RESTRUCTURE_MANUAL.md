# 🔧 Hướng Dẫn Restructure SmartParking Solution

## 🎯 Mục Tiêu

Đổi tên và tổ chức lại cấu trúc project từ:
```
SmartParking.API/                    ❌ Tên không rõ ràng
├── SmartParking.API.sln             ❌ Trùng tên với API layer
├── SmartParking.Domain/
├── SmartParking.Application/
├── SmartParking.Infrastructure/
├── SmartParking.API/                ❌ Trùng tên folder gốc
└── Database/
```

Thành:
```
SmartParking/                        ✅ Tên rõ ràng
├── SmartParking.sln                 ✅ Không conflict
├── README.md                        ✅ Documentation
├── src/                             ✅ Source code riêng
│   ├── SmartParking.Domain/
│   ├── SmartParking.Application/
│   ├── SmartParking.Infrastructure/
│   └── SmartParking.API/
├── Database/                        ✅ Database scripts
└── docs/                            ✅ Documentation
```

---

## 🤖 Option 1: Automatic (PowerShell Script) - RECOMMENDED

### Bước 1: Đóng Visual Studio
⚠️ **QUAN TRỌNG**: Đóng Visual Studio trước khi chạy script!

### Bước 2: Mở PowerShell
```powershell
# Right-click "Restructure-Solution.ps1" → Run with PowerShell
# Hoặc:
cd "E:\FPT UNIVERSITY\CN8\EXE201_WEB\Synergy_BE"
powershell -ExecutionPolicy Bypass -File .\Restructure-Solution.ps1
```

### Bước 3: Kiểm tra (Dry Run - Optional)
Nếu muốn xem trước thay đổi mà không apply:
```powershell
.\Restructure-Solution.ps1 -DryRun
```

### Bước 4: Apply Changes
```powershell
.\Restructure-Solution.ps1
```

### Bước 5: Mở lại Visual Studio
```
1. Open: SmartParking\SmartParking.sln
2. Build > Rebuild Solution
3. Verify all projects build successfully
```

---

## 🛠️ Option 2: Manual (Step-by-Step)

### Bước 1: Đóng Visual Studio
⚠️ **QUAN TRỌNG**: Đóng Visual Studio và tất cả terminals!

### Bước 2: Backup (Optional but Recommended)
```powershell
cd "E:\FPT UNIVERSITY\CN8\EXE201_WEB\Synergy_BE"
Copy-Item -Path "SmartParking.API" -Destination "SmartParking.API.backup" -Recurse
```

### Bước 3: Đổi tên folder gốc
```powershell
Rename-Item -Path "SmartParking.API" -NewName "SmartParking"
cd SmartParking
```

### Bước 4: Đổi tên solution
```powershell
Rename-Item -Path "SmartParking.API.sln" -NewName "SmartParking.sln"
```

### Bước 5: Tạo folders mới
```powershell
New-Item -ItemType Directory -Path "src" -Force
New-Item -ItemType Directory -Path "docs" -Force
```

### Bước 6: Di chuyển projects vào src/
```powershell
Move-Item -Path "SmartParking.Domain" -Destination "src\"
Move-Item -Path "SmartParking.Application" -Destination "src\"
Move-Item -Path "SmartParking.Infrastructure" -Destination "src\"
Move-Item -Path "SmartParking.API" -Destination "src\"
```

### Bước 7: Di chuyển docs
```powershell
Move-Item -Path "PROJECT_READY.md" -Destination "docs\" -ErrorAction SilentlyContinue
Move-Item -Path "DEPLOYMENT_GUIDE.md" -Destination "docs\" -ErrorAction SilentlyContinue
Move-Item -Path "RESTRUCTURE_GUIDE.md" -Destination "docs\" -ErrorAction SilentlyContinue
```

### Bước 8: Update Solution File
Mở `SmartParking.sln` bằng Notepad, tìm và thay thế:

**Tìm:**
```
"SmartParking.Domain\SmartParking.Domain.csproj"
"SmartParking.Application\SmartParking.Application.csproj"
"SmartParking.Infrastructure\SmartParking.Infrastructure.csproj"
"SmartParking.API\SmartParking.API.csproj"
```

**Thay bằng:**
```
"src\SmartParking.Domain\SmartParking.Domain.csproj"
"src\SmartParking.Application\SmartParking.Application.csproj"
"src\SmartParking.Infrastructure\SmartParking.Infrastructure.csproj"
"src\SmartParking.API\SmartParking.API.csproj"
```

Hoặc dùng PowerShell:
```powershell
$slnContent = Get-Content "SmartParking.sln" -Raw -Encoding UTF8
$slnContent = $slnContent -replace '"SmartParking\.Domain\\', '"src\SmartParking.Domain\'
$slnContent = $slnContent -replace '"SmartParking\.Application\\', '"src\SmartParking.Application\'
$slnContent = $slnContent -replace '"SmartParking\.Infrastructure\\', '"src\SmartParking.Infrastructure\'
$slnContent = $slnContent -replace '"SmartParking\.API\\', '"src\SmartParking.API\'
Set-Content "SmartParking.sln" -Value $slnContent -NoNewline -Encoding UTF8
```

### Bước 9: Tạo README.md
Tạo file `SmartParking\README.md` với nội dung:

```markdown
# SmartParking Backend API

Modern Smart Parking Management System built with ASP.NET Core 8.0

## 🏗️ Architecture

Clean Architecture with 3-layer separation:
- **Domain**: Entities, Enums, Constants
- **Application**: Business Logic, Services, DTOs
- **Infrastructure**: Data Access, External Services
- **API**: Controllers, Endpoints

## 🚀 Quick Start

1. Open `SmartParking.sln` in Visual Studio
2. Update connection string in `appsettings.json`
3. Run database migration (see `Database/README.md`)
4. Press F5 to run
5. Navigate to `https://localhost:7000/swagger`

## 📚 Documentation

- [Project Status](docs/PROJECT_READY.md)
- [Deployment Guide](docs/DEPLOYMENT_GUIDE.md)
- [Database Setup](Database/README.md)

## 🔑 Features

- JWT Authentication + Refresh Tokens
- Google OAuth Integration
- Vehicle Management
- Booking System
- VNPay Payment Integration

**Status**: Production Ready | **Version**: 1.0
```

### Bước 10: Xác nhận cấu trúc
Chạy lệnh để xem tree:
```powershell
tree /F
```

Kết quả mong đợi:
```
SmartParking\
├── SmartParking.sln
├── README.md
├── src\
│   ├── SmartParking.Domain\
│   ├── SmartParking.Application\
│   ├── SmartParking.Infrastructure\
│   └── SmartParking.API\
├── Database\
│   ├── Migration_*.sql
│   └── SeedData.sql
└── docs\
    ├── PROJECT_READY.md
    └── DEPLOYMENT_GUIDE.md
```

### Bước 11: Build & Test
```powershell
# Open Visual Studio
# File > Open > Solution
# Select: SmartParking\SmartParking.sln

# Build > Rebuild Solution
# Verify: Build succeeded (0 errors, 0 warnings)
```

---

## ✅ Verification Checklist

Sau khi restructure, kiểm tra:

- [ ] Folder gốc đã đổi tên: `SmartParking/`
- [ ] Solution file đã đổi tên: `SmartParking.sln`
- [ ] 4 projects nằm trong `src/` folder
- [ ] `Database/` folder vẫn ở root
- [ ] Docs nằm trong `docs/` folder
- [ ] Solution mở được trong Visual Studio
- [ ] Build thành công (0 errors)
- [ ] All projects load correctly (không có "unavailable")
- [ ] `README.md` tồn tại ở root

---

## 🐛 Troubleshooting

### Lỗi: "Project unavailable" trong Visual Studio

**Nguyên nhân**: Solution file chưa update paths

**Fix**:
1. Đóng Visual Studio
2. Mở `SmartParking.sln` bằng Notepad
3. Kiểm tra paths có prefix `src\` chưa
4. Nếu chưa, chạy lại Bước 8

### Lỗi: "Access Denied" khi rename

**Nguyên nhân**: File đang bị lock bởi VS hoặc process khác

**Fix**:
1. Đóng Visual Studio
2. Mở Task Manager → tìm `devenv.exe` → End Task
3. Chạy lại script/manual steps

### Lỗi: Build failed sau restructure

**Nguyên nhân**: Project references không đúng

**Fix**:
```powershell
cd SmartParking
dotnet clean
dotnet restore
dotnet build
```

### Git conflicts (nếu dùng Git)

Nếu folder này đang trong Git repo:
```bash
# Before restructure
git add .
git commit -m "Backup before restructure"

# After restructure
git add .
git commit -m "Restructure: SmartParking.API → SmartParking"
```

---

## 🎯 Final Structure

```
E:\FPT UNIVERSITY\CN8\EXE201_WEB\Synergy_BE\
└── SmartParking\                              ✅ New name
    ├── SmartParking.sln                       ✅ New name
    ├── README.md                              ✅ New file
    ├── src\                                   ✅ New folder
    │   ├── SmartParking.Domain\
    │   │   ├── Entities\
    │   │   ├── Enums\
    │   │   ├── Constants\
    │   │   └── SmartParking.Domain.csproj
    │   ├── SmartParking.Application\
    │   │   ├── Services\
    │   │   ├── DTOs\
    │   │   ├── Interfaces\
    │   │   └── SmartParking.Application.csproj
    │   ├── SmartParking.Infrastructure\
    │   │   ├── Repositories\
    │   │   ├── Data\
    │   │   ├── Services\
    │   │   └── SmartParking.Infrastructure.csproj
    │   └── SmartParking.API\
    │       ├── Controllers\
    │       ├── Middlewares\
    │       ├── appsettings.json
    │       └── SmartParking.API.csproj
    ├── Database\                              ✅ Kept at root
    │   ├── Migration_*.sql
    │   ├── SeedData.sql
    │   └── README.md
    └── docs\                                  ✅ New folder
        ├── PROJECT_READY.md
        ├── DEPLOYMENT_GUIDE.md
        └── RESTRUCTURE_MANUAL.md
```

---

## 🎉 Benefits of New Structure

| Before | After | Benefit |
|--------|-------|---------|
| `SmartParking.API/` | `SmartParking/` | ✅ Tên rõ ràng, không conflict |
| `SmartParking.API.sln` | `SmartParking.sln` | ✅ Không trùng với API layer |
| Projects ở root | Projects trong `src/` | ✅ Tách biệt source code |
| Docs rải rác | Docs trong `docs/` | ✅ Documentation tập trung |
| Không có README | Có `README.md` | ✅ Clear entry point |

---

## 📞 Support

Nếu gặp vấn đề:
1. Check [Troubleshooting](#troubleshooting) section
2. Restore từ backup: `SmartParking.API.backup/`
3. Chạy lại script/manual steps

---

**Last Updated**: January 25, 2026  
**Version**: 1.0  
**Status**: Ready to Execute
