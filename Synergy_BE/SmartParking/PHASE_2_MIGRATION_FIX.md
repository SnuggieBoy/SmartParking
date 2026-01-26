# 🔧 PHASE 2 MIGRATION FIX GUIDE

**Date:** 2026-01-26  
**Issue:** Migration had 2 errors but most changes succeeded

---

## ✅ **WHAT SUCCEEDED:**

- ✅ BookingTime updated to NOT NULL
- ✅ TotalAmount updated to NOT NULL  
- ✅ CreatedBy column added
- ✅ UpdatedBy column added
- ✅ IsDeleted column added
- ✅ DeletedAt column added
- ✅ DeletedBy column added
- ✅ Audit fields initialized (0 records - table is empty, which is fine)

---

## ❌ **ISSUES FOUND:**

### **Issue 1: CreatedAt Default Constraint**
```
Msg 1781: Column already has a DEFAULT bound to it.
```
**Cause:** `CreatedAt` already had a default constraint with a different name.

**Fix:** Run `Fix_Phase2_BookingMigration.sql` - it will:
- Find and drop the old constraint
- Add the correct `DF_Bookings_CreatedAt` constraint

---

### **Issue 2: Index Creation Syntax Error**
```
Msg 102: Incorrect syntax near 'NOT'.
```
**Cause:** Filtered index with `NOT IN` syntax issue at line 237.

**Fix:** Already fixed in the migration script. The fix script will create the index with correct syntax:
```sql
WHERE [IsDeleted] = 0 AND [Status] <> 'Cancelled' AND [Status] <> 'Completed'
```

---

## 🚀 **HOW TO FIX:**

### **Option 1: Run Fix Script (Recommended)**
```sql
-- In SSMS, execute:
E:\FPT UNIVERSITY\CN8\EXE201_BE\Synergy_BE\SmartParking\Database\Fix_Phase2_BookingMigration.sql
```

**This will:**
1. Fix the CreatedAt default constraint
2. Create all 4 missing indexes
3. Verify everything is correct

---

### **Option 2: Manual Fix**

**1. Fix CreatedAt Constraint:**
```sql
-- Find existing constraint
SELECT name FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('dbo.Bookings')
  AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('dbo.Bookings'), 'CreatedAt', 'ColumnId');

-- Drop it (replace 'ConstraintName' with actual name)
ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [ConstraintName];

-- Add correct constraint
ALTER TABLE [dbo].[Bookings]
ADD CONSTRAINT [DF_Bookings_CreatedAt] DEFAULT (GETUTCDATE()) FOR [CreatedAt];
```

**2. Create Missing Indexes:**
Run the index creation section from `Fix_Phase2_BookingMigration.sql` (STEP 2).

---

## ✅ **AFTER FIX:**

**Expected Result:**
- ✅ All 4 indexes created
- ✅ CreatedAt has correct default constraint
- ✅ All columns verified
- ✅ No errors

---

## 📊 **CURRENT STATUS:**

```
✅ Phase 1: Parking Lot - 100% COMPLETE
🔄 Phase 2: Booking - 95% COMPLETE (just needs fix script)
⏳ Phase 3: Payment - 0%
⏳ Phase 4: Map/Location - 0%
```

---

**Run the fix script and let me know when done!** 🚀
