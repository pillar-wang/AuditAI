import sqlite3
import json
import os

db_path = r"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Check all columns in Table
cur.execute("PRAGMA table_info(`Table`)")
cols = cur.fetchall()
print("Table columns:")
for c in cols:
    print(f"  {c[1]} ({c[2]})")

# Get one row with all columns
cur.execute("SELECT * FROM `Table` WHERE Title LIKE ? LIMIT 1", ('%应收账款%',))
row = cur.fetchone()
if row:
    print(f"\nRow data (all columns):")
    for i, c in enumerate(cols):
        val = row[i]
        if val and isinstance(val, str) and len(val) > 200:
            val = val[:200] + "..."
        print(f"  {c[1]}: {val}")

conn.close()
