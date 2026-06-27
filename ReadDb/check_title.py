import sqlite3
import json
import os

# Get raw Title for one of the tables
db_path = r"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()
cur.execute("SELECT Id, Title FROM `Table` WHERE Title LIKE ? LIMIT 1", ('%应收账款%',))
row = cur.fetchone()
if row:
    print(f"Id={row[0]}")
    print(f"Title (first 500 chars): {row[1][:500]}")
    print(f"Title (last 200 chars): {row[1][-200:]}")
    try:
        tj = json.loads(row[1])
        print(f"Parsed keys: {list(tj.keys())}")
    except Exception as e:
        print(f"Parse error: {e}")
conn.close()
