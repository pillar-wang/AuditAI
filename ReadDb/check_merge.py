# -*- coding: utf-8 -*-
import sqlite3
conn = sqlite3.connect(r"e:\lq\AuditAI\AuditAI\bin\Debug\net48\Data\2\64962b92-b997-4290-9711-979c7387a8c4.db")
c = conn.cursor()
TID = 10342281249439
print("Merge 引用单元格是否存活 (Cell.Status, Row.Status, Col.Status):")
for m in c.execute("SELECT Id, TopLeft, BottomRight, Status FROM Merge WHERE TableId=?", (TID,)).fetchall():
    tl = c.execute("SELECT c.Status, r.Status, l.Status FROM Cell c JOIN Row r ON c.RowId=r.Id JOIN `Column` l ON c.ColumnId=l.Id WHERE c.Id=?", (m[1],)).fetchone()
    br = c.execute("SELECT c.Status, r.Status, l.Status FROM Cell c JOIN Row r ON c.RowId=r.Id JOIN `Column` l ON c.ColumnId=l.Id WHERE c.Id=?", (m[2],)).fetchone()
    print("  Merge", m[0], "TopLeft", m[1], tl, "BottomRight", m[2], br)
print("Index=0 的两行:")
for r in c.execute("SELECT Id, `Index`, Status FROM Row WHERE TableId=? AND `Index`=0", (TID,)).fetchall():
    print("  Row", r)
conn.close()
print("done")
