import sqlite3
import os

db_path = r"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Check TreeNode
cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='TreeNode'")
if cur.fetchone():
    cur.execute("PRAGMA table_info(TreeNode)")
    cols = cur.fetchall()
    print("TreeNode columns:")
    for c in cols:
        print(f"  {c[1]} ({c[2]})")
    
    cur.execute("SELECT * FROM TreeNode LIMIT 5")
    rows = cur.fetchall()
    print(f"\nTreeNode rows (first 5):")
    for row in rows:
        print(f"  {row}")

# Check TreeGroup
cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='TreeGroup'")
if cur.fetchone():
    cur.execute("PRAGMA table_info(TreeGroup)")
    cols = cur.fetchall()
    print("\nTreeGroup columns:")
    for c in cols:
        print(f"  {c[1]} ({c[2]})")
    
    cur.execute("SELECT * FROM TreeGroup LIMIT 3")
    rows = cur.fetchall()
    print(f"\nTreeGroup rows (first 3):")
    for row in rows:
        print(f"  {row}")

conn.close()
