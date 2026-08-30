# -*- coding: utf-8 -*-
import sqlite3

DB = r"e:\lq\AuditAI\AuditAI\bin\Debug\net48\Data\2\64962b92-b997-4290-9711-979c7387a8c4.db"
TID = 10342281249439  # 其他应收款明细表（重分类调整后）

conn = sqlite3.connect(DB)
c = conn.cursor()

print("=== 表元数据 ===")
print("Table cols:", [r[1] for r in c.execute("PRAGMA table_info(`Table`)").fetchall()])
print(c.execute("SELECT * FROM `Table` WHERE Id=?", (TID,)).fetchone())
print("TreeNode:", c.execute("SELECT Id, Name, Version, Status, Dirty, ServerIndex FROM TreeNode WHERE Id=?", (TID,)).fetchone())

print("\n=== Row 分布 (含 Status=2 已删除) ===")
for r in c.execute("SELECT Status, COUNT(*) FROM Row WHERE TableId=? GROUP BY Status", (TID,)).fetchall():
    print("  Status", r[0], "->", r[1], "行")
print("  已删除行 Id:", [r[0] for r in c.execute("SELECT Id FROM Row WHERE TableId=? AND Status=2", (TID,)).fetchall()])

print("\n=== Column 分布 ===")
for r in c.execute("SELECT Status, COUNT(*) FROM `Column` WHERE TableId=? GROUP BY Status", (TID,)).fetchall():
    print("  Status", r[0], "->", r[1], "列")

print("\n=== Cell 与 Row/Column 的关联检查 ===")
# 1) 孤儿 Cell：RowId 或 ColumnId 在 Row/Column 表中不存在
orphan = c.execute("""
SELECT c.Id FROM Cell c
LEFT JOIN Row r ON c.RowId = r.Id
LEFT JOIN `Column` l ON c.ColumnId = l.Id
WHERE r.Id IS NULL OR l.Id IS NULL
""").fetchall()
print("孤儿 Cell(引用了不存在的行/列):", len(orphan), orphan[:10])

# 2) 存活 Cell 引用了已删除(Status=2)的行
cell_in_delrow = c.execute("""
SELECT COUNT(*) FROM Cell c JOIN Row r ON c.RowId=r.Id
WHERE r.TableId=? AND r.Status=2 AND c.Status<2
""", (TID,)).fetchone()[0]
print("存活 Cell 但所在行已删除(Status=2):", cell_in_delrow)

# 3) Cell 状态分布
print("\n=== Cell Status 分布 ===")
for r in c.execute("""
SELECT c.Status, COUNT(*) FROM Cell c JOIN Row r ON c.RowId=r.Id
WHERE r.TableId=? GROUP BY c.Status
""", (TID,)).fetchall():
    print("  Status", r[0], "->", r[1], "个")

# 4) 行索引/列索引是否重复或为负
print("\n=== 索引检查 ===")
rows_idx = c.execute("SELECT Id, `Index`, Status FROM Row WHERE TableId=? ORDER BY `Index`", (TID,)).fetchall()
cols_idx = c.execute("SELECT Id, `Index`, Status FROM `Column` WHERE TableId=? ORDER BY `Index`", (TID,)).fetchall()
print("Row Index 列表:", [r[1] for r in rows_idx])
print("Col Index 列表:", [r[1] for r in cols_idx])
dup_row = [i for i in rows_idx if i[1] < 0]
print("Row 负索引:", dup_row)
dup_col = [i for i in cols_idx if i[1] < 0]
print("Col 负索引:", dup_col)

# 5) CellStyle / Merge 检查
print("\n=== Merge 状态 ===")
for r in c.execute("SELECT Id, TopLeft, BottomRight, Status FROM Merge WHERE TableId=?", (TID,)).fetchall():
    print("  Merge", r)
print("\n=== CellStyle 数量 ===")
print("  CellStyle:", c.execute("SELECT COUNT(*) FROM CellStyle WHERE TableId=?", (TID,)).fetchone()[0])

conn.close()
print("done")
