import sqlite3, json

db_path = r"e:\lq\AuditAI\AuditAI\Data\Templates\X004.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Check table 11287174055345 specifically
cur.execute("SELECT Id, Title, length(Ticket) as TicketLen, Ticket FROM `Table` WHERE Id=?", (11287174055345,))
row = cur.fetchone()
if row:
    print(f"Table Id={row[0]}, TicketLen={row[2]}")
    try:
        tj = json.loads(row[1])
        print(f"Title Keys: {list(tj.keys())}")
        # Check for Name in Title
        print(f"TitleCell: {tj.get('TitleCell', {})}")
    except Exception as e:
        print(f"Title parse error: {e}")
    
    # Parse Ticket
    ticket = row[3]
    if ticket:
        try:
            tj = json.loads(ticket)
            print(f"\nTicket Keys: {list(tj.keys())}")
            navs = tj.get("Navs", [])
            print(f"Navs count: {len(navs)}")
            for i, nav in enumerate(navs):
                print(f"  Nav[{i}]: {json.dumps(nav, ensure_ascii=False)[:300]}")
            cols = tj.get("Columns", [])
            print(f"Columns count: {len(cols)}")
            rows = tj.get("Rows", [])
            print(f"Rows count: {len(rows)}")
            print(f"Kind: {tj.get('Kind')}")
            print(f"Level: {tj.get('Level')}")
        except Exception as e:
            print(f"Ticket parse error: {e}")
else:
    print("Table not found")

# Also check TreeNode
cur.execute("SELECT Id, GroupId, ParentId, Name, Number, Type FROM TreeNode WHERE Id=?", (11287174055345,))
node = cur.fetchone()
if node:
    print(f"\nTreeNode: Id={node[0]}, Name={node[3]}, Number={node[4]}, Type={node[5]}")
else:
    print("\nTreeNode not found")

conn.close()
