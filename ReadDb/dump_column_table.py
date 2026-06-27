# -*- coding: utf-8 -*-
import sqlite3, json, os

db_path = r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db"
out_path = r"E:\lq\AuditAI\ReadDb\out_column_table.txt"
target_field = 11287174055451

with open(out_path, "w", encoding="utf-8") as out:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()
    cur.execute("PRAGMA table_info(Column)")
    out.write(f"Column table cols: {[c[1] for c in cur.fetchall()]}\n\n")

    # Find the column row with Id = target_field
    cur.execute("SELECT * FROM `Column` WHERE Id=?", (target_field,))
    r = cur.fetchone()
    out.write(f"=== Column Id={target_field} ===\n")
    if r:
        cur.execute("PRAGMA table_info(Column)")
        cols = [c[1] for c in cur.fetchall()]
        for i, v in enumerate(r):
            sv = str(v)
            if len(sv) > 1500:
                sv = sv[:1500] + "..."
            out.write(f"  {cols[i]}: {sv}\n")
    else:
        out.write(f"  NOT FOUND by Id={target_field}\n")
        # Show all columns belonging to this table
        # How to link Column to Table? Check if Column has TableId
        cur.execute("PRAGMA table_info(Column)")
        cnames = [c[1] for c in cur.fetchall()]
        out.write(f"  Column table schema: {cnames}\n")
        # Try TableId column
        if 'TableId' in cnames:
            cur.execute("SELECT Id, TableId, Name FROM `Column` WHERE TableId=11287174055345")
            for cr in cur.fetchall():
                out.write(f"  Col Id={cr[0]} Name={cr[2]}\n")

    # List ALL columns for this table - need to find the link
    # Maybe Column table has a reference. Let's dump a sample
    out.write(f"\n=== First 5 rows of Column table ===\n")
    cur.execute("SELECT * FROM `Column` LIMIT 5")
    cur.execute("PRAGMA table_info(Column)")
    cnames = [c[1] for c in cur.fetchall()]
    out.write(f"Schema: {cnames}\n")
    cur.execute("SELECT * FROM `Column` LIMIT 5")
    for r in cur.fetchall():
        out.write(f"  {r}\n")

    # Search for 11287174055451 anywhere in Column table
    out.write(f"\n=== Search 11287174055451 in any text column of Column table ===\n")
    cur.execute("PRAGMA table_info(Column)")
    cnames = [c[1] for c in cur.fetchall()]
    for cn in cnames:
        try:
            cur.execute(f"SELECT Id FROM `Column` WHERE CAST(`{cn}` AS TEXT) LIKE '%11287174055451%' LIMIT 3")
            hits = cur.fetchall()
            if hits:
                out.write(f"  Found in column '{cn}': {hits}\n")
        except: pass

    conn.close()
print(f"Output: {out_path}")
