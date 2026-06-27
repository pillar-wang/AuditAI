import sqlite3
import json
import os

db_path = r"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Check all tables
cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
tables = [row[0] for row in cur.fetchall()]
print("All tables:", tables)

# Check TreeNode
if "TreeNode" in tables:
    cur.execute("PRAGMA table_info(TreeNode)")
    cols = cur.fetchall()
    print("\nTreeNode columns:")
    for c in cols:
        print(f"  {c[1]} ({c[2]})")
    
    cur.execute("SELECT * FROM TreeNode LIMIT 5")
    rows = cur.fetchall()
    print(f"\nTreeNode rows (first 5):")
    for row in rows:
        print(f"  {row}")

# Check if there's a Document table
if "Document" in tables:
    cur.execute("PRAGMA table_info(Document)")
    cols = cur.fetchall()
    print("\nDocument columns:")
    for c in cols:
        print(f"  {c[1]} ({c[2]})")
    
    cur.execute("SELECT * FROM Document LIMIT 3")
    rows = cur.fetchall()
    print(f"\nDocument rows (first 3):")
    for row in rows:
        print(f"  {row}")

conn.close()
