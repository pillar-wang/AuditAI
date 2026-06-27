import sqlite3
import json
import os

db_paths = [
    r"e:\lq\Data\1\00000000-0000-0000-0000-000000000000.db",
    r"e:\lq\Data\leqi_audit.db",
]

for db_path in db_paths:
    if not os.path.exists(db_path):
        print(f"NOT FOUND: {db_path}")
        continue

    print(f"\n=== {db_path} ===")
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()
    
    cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
    tables = [row[0] for row in cur.fetchall()]
    print(f"Tables: {', '.join(tables)}")
    
    if "Table" not in tables:
        continue
    
    # Find 应收账款替代测试表
    cur.execute("SELECT Id, Title, length(Ticket) as TicketLen FROM `Table` WHERE Title LIKE ?", ('%应收账款替代测试表%',))
    rows = cur.fetchall()
    print(f"Found {len(rows)} tables")
    for row in rows:
        tid = row[0]
        title = row[1]
        tlen = row[2]
        print(f"\n  Id={tid}, TicketLen={tlen}")
        try:
            tj = json.loads(title)
            print(f"    Title Keys: {list(tj.keys())}")
            nav_tree = tj.get("NavTreeCellIdList")
            if nav_tree:
                print(f"    NavTreeCellIdList: {nav_tree}")
        except Exception as e:
            print(f"    Title parse error: {e}")
    
    conn.close()
