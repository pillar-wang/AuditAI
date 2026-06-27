import sqlite3
import json
import os

# Check all project databases for 应收账款替代测试表
db_path = r"e:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\d58b4b0a-6642-4264-a6d6-8fc6bc63a6f1.db"

if not os.path.exists(db_path):
    print(f"NOT FOUND: {db_path}")
    exit()

conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Check tables
cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
tables = [row[0] for row in cur.fetchall()]
print(f"Tables: {', '.join(tables)}")

if "Table" in tables:
    # Find 应收账款替代测试表
    cur.execute("SELECT Id, Title, length(Ticket) as TicketLen, Ticket FROM `Table` WHERE Title LIKE ?", ('%应收账款替代测试表%',))
    rows = cur.fetchall()
    print(f"\nFound {len(rows)} matching tables")
    for row in rows:
        tid = row[0]
        title = row[1]
        ticket_len = row[2]
        ticket = row[3]
        print(f"\nTable Id={tid}, TicketLen={ticket_len}")
        
        # Parse Title
        try:
            tj = json.loads(title)
            print(f"  Title Keys: {list(tj.keys())}")
            nav_tree = tj.get("NavTreeCellIdList")
            if nav_tree:
                print(f"  NavTreeCellIdList: {nav_tree}")
        except Exception as e:
            print(f"  Title parse error: {e}")
        
        # Parse Ticket
        if ticket:
            try:
                tj = json.loads(ticket)
                print(f"  Ticket Keys: {list(tj.keys())}")
                navs = tj.get("Navs", [])
                print(f"  Navs count: {len(navs)}")
                for i, nav in enumerate(navs[:3]):
                    print(f"    Nav[{i}]: {json.dumps(nav, ensure_ascii=False)[:300]}")
                cols = tj.get("Columns", [])
                print(f"  Columns count: {len(cols)}")
                rows_count = tj.get("Rows", [])
                print(f"  Rows count: {len(rows_count)}")
            except Exception as e:
                print(f"  Ticket parse error: {e}")
        else:
            print("  Ticket: NULL/EMPTY")

# Check TreeNode
if "TreeNode" in tables:
    cur.execute("SELECT Id, GroupId, ParentId, Name, Number, Type FROM TreeNode WHERE Name LIKE ?", ('%应收账款替代测试表%',))
    nodes = cur.fetchall()
    print(f"\nTreeNode matches: {len(nodes)}")
    for node in nodes:
        print(f"  Id={node[0]}, GroupId={node[1]}, ParentId={node[2]}, Name={node[3]}, Number={node[4]}, Type={node[5]}")

conn.close()
