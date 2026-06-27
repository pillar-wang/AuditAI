# -*- coding: utf-8 -*-
import sqlite3

db_path = r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db"
out_path = r"E:\lq\AuditAI\ReadDb\out_rowcell.txt"
src_table_id = 10337986289169

with open(out_path, "w", encoding="utf-8") as out:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()

    # Row table schema
    cur.execute("PRAGMA table_info(Row)")
    out.write(f"Row cols: {[c[1] for c in cur.fetchall()]}\n")
    cur.execute("PRAGMA table_info(Cell)")
    out.write(f"Cell cols: {[c[1] for c in cur.fetchall()]}\n\n")

    # Count rows for source table
    cur.execute("SELECT COUNT(*) FROM Row WHERE TableId=?", (src_table_id,))
    out.write(f"=== Source table {src_table_id} ===\n")
    out.write(f"  Row count: {cur.fetchone()[0]}\n")

    cur.execute("SELECT COUNT(*) FROM Cell WHERE ColumnId IN (SELECT Id FROM `Column` WHERE TableId=?)", (src_table_id,))
    out.write(f"  Cell count (via Column): {cur.fetchone()[0]}\n")

    # Show actual rows
    cur.execute("SELECT Id, `Index`, TableId FROM Row WHERE TableId=? ORDER BY `Index` LIMIT 20", (src_table_id,))
    rows = cur.fetchall()
    out.write(f"\n  Rows ({len(rows)} shown):\n")
    for r in rows:
        out.write(f"    Id={r[0]}, Index={r[1]}, TableId={r[2]}\n")

    # Show cells for column 10337986289222 (单位名称) - the ComboList referenced column
    out.write(f"\n  === Cells for Column 10337986289222 (单位名称) ===\n")
    cur.execute("SELECT Id, RowId, ColumnId, Value FROM Cell WHERE ColumnId=10337986289222 LIMIT 20")
    cells = cur.fetchall()
    out.write(f"  Cell count: {len(cells)}\n")
    for c in cells:
        out.write(f"    Cell Id={c[0]}, RowId={c[1]}, Value='{c[3][:80] if c[3] else ''}'\n")

    # Also check target table rows/cells
    out.write(f"\n=== Target table 11287174055345 (应收账款替代测试表) ===\n")
    cur.execute("SELECT COUNT(*) FROM Row WHERE TableId=11287174055345")
    out.write(f"  Row count: {cur.fetchone()[0]}\n")
    cur.execute("SELECT COUNT(*) FROM Cell WHERE ColumnId IN (SELECT Id FROM `Column` WHERE TableId=11287174055345)")
    out.write(f"  Cell count: {cur.fetchone()[0]}\n")
    cur.execute("SELECT Id, `Index` FROM Row WHERE TableId=11287174055345 ORDER BY `Index`")
    for r in cur.fetchall():
        out.write(f"    Row Id={r[0]}, Index={r[1]}\n")

    conn.close()
print(f"Output: {out_path}")
