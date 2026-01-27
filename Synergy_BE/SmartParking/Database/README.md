# 📁 DATABASE SCRIPTS

This folder contains all SQL scripts for SmartParking database.

---

## 🎯 **FOR NEW SETUP (Your Friend)**

If you're setting up SmartParking for the first time:

### **Option 1: Full Database (Recommended)**
```sql
-- Run this if you want EVERYTHING (base + SePay)
-- File: SmartParkingFull.sql (if it includes SePay)
-- OR run both files in order:
1. SmartParkingFull.sql          -- Base database
2. SmartParking_SePay_Complete.sql  -- Add SePay support
```

### **Option 2: Base Only (No SePay)**
```sql
-- Run this if you only want the base system (no SePay)
SmartParkingFull.sql
```

---

## 📄 **FILE DESCRIPTIONS**

### **Production Files (USE THESE)**

| File | Purpose | When to Use |
|------|---------|-------------|
| `SmartParkingFull.sql` | Complete base database schema | First time setup |
| `SmartParking_SePay_Complete.sql` | **SePay integration (ALL-IN-ONE)** | **After base setup, to add SePay** |

### **Development/Debug Files (Optional)**

These files were used during development. You DON'T need them if you use `SmartParking_SePay_Complete.sql`:

| File | Purpose |
|------|---------|
| `Migration_SePay_Integration.sql` | Original migration (superseded by Complete version) |
| `SePay_Step1_AddColumns.sql` | Debug: Add columns only |
| `VerifySePay_Columns.sql` | Debug: Check if columns exist |
| `SePay_Troubleshooting.sql` | Debug: Diagnose issues |

---

## 🚀 **QUICK START FOR YOUR FRIEND**

### **Scenario: Your friend has base SmartParking (no SePay)**

```sql
-- Step 1: Backup first!
BACKUP DATABASE SmartParkingDB TO DISK = 'C:\Backup\SmartParkingDB.bak';

-- Step 2: Run SePay integration
-- Open: SmartParking_SePay_Complete.sql
-- Execute in SSMS (F5)

-- Step 3: Verify
-- You should see:
-- ✅✅✅ MIGRATION COMPLETED SUCCESSFULLY! ✅✅✅
```

---

## ✅ **WHAT SmartParking_SePay_Complete.sql DOES**

1. **Adds 6 columns** to `PaymentTransactions`:
   - `SePayOrderId`
   - `SePayTransactionId`
   - `SePayBankCode`
   - `SePayBankAccount`
   - `SePayTransferContent`
   - `SePayQrCode`

2. **Creates `SePayWebhookLogs` table** (for audit trail)

3. **Creates indexes** (for performance)

4. **Updates `PaymentMethod` constraint** (to allow "SePay")

5. **Verifies everything** (shows summary at end)

---

## 🔒 **SAFETY**

All scripts are **IDEMPOTENT**:
- ✅ Safe to run multiple times
- ✅ Won't break if already applied
- ✅ Checks before making changes

---

## 📖 **AFTER RUNNING SQL**

Your friend needs to:

1. **Set environment variables:**
   ```powershell
   $env:SEPAY_API_KEY = "their-api-key"
   $env:SEPAY_WEBHOOK_SECRET = "their-webhook-secret"
   ```

2. **Update appsettings.Development.json:**
   ```json
   {
     "SePay": {
       "Bank": {
         "Code": "MB",
         "AccountNumber": "their-account",
         "AccountName": "THEIR NAME"
       }
     }
   }
   ```

3. **Rebuild & Run:**
   ```bash
   dotnet build
   dotnet run
   ```

---

## 📚 **DOCUMENTATION**

Full documentation in `docs/` folder:
- `SEPAY_QUICK_SETUP.md` - Quick start guide
- `SEPAY_ENVIRONMENT_VARIABLES.md` - Environment setup
- `SEPAY_PRODUCTION_DEPLOYMENT.md` - Production deployment
- `SEPAY_REFACTOR_SUMMARY.md` - Technical details

---

## 🆘 **TROUBLESHOOTING**

### **Script fails?**

1. Check you're on `SmartParkingDB`:
   ```sql
   SELECT DB_NAME(); -- Should return 'SmartParkingDB'
   ```

2. Check `PaymentTransactions` exists:
   ```sql
   SELECT * FROM sys.tables WHERE name = 'PaymentTransactions';
   ```

3. Run troubleshooting script:
   ```sql
   -- SePay_Troubleshooting.sql
   ```

### **Migration shows incomplete?**

- Review error messages in script output
- Run verification script: `VerifySePay_Columns.sql`
- Check if columns were added manually before

---

## 📞 **SUPPORT**

If your friend encounters issues:

1. Check the script output for specific error messages
2. Run `SePay_Troubleshooting.sql` to diagnose
3. Review `docs/SEPAY_REFACTOR_SUMMARY.md` for migration details

---

**Remember:** Always backup before running migration scripts! 🔐
