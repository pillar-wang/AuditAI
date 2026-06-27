import sqlite3, json

db_path = r"e:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Find 应收账款替代测试表
cur.execute("SELECT Id, Title, length(Ticket) FROM [Table] WHERE Title LIKE ?", ('%应收账款替代测试表%',))
rows = cur.fetchall()
print(f"Found {len(rows)} tables")
for row in rows:
    tid = row[0]
    title_raw = row[1]
    ticket_len = row[2]
    print(f"\nTable Id={tid}, TitleLen={len(title_raw)}, TicketLen={ticket_len}")
    
    # Parse Title JSON
    try:
        tj = json.loads(title_raw)
        print(f"  Title keys: {list(tj.keys())}")
        # Check if it has NavTreeCellIdList or similar
        for k, v in tj.items():
            if isinstance(v, str) and len(v) > 50:
                print(f"  {k}: {v[:200]}...")
            elif isinstance(v, list):
                print(f"  {k}: list of {len(v)} items")
            elif isinstance(v, dict):
                print(f"  {k}: dict with keys {list(v.keys())[:10]}")
            else:
                print(f"  {k}: {v}")
    except Exception as e:
        print(f"  Title parse error: {e}")
    
    # Parse Ticket JSON
    if ticket_len > 0:
        cur.execute("SELECT Ticket FROM [Table] WHERE Id=?", (tid,))
        ticket_data = cur.fetchone()[0]
        try:
            tj = json.loads(ticket_data)
            print(f"\n  Ticket keys: {list(tj.keys())}")
            
            # Check Navs
            navs = tj.get("Navs", [])
            print(f"  Navs count: {len(navs)}")
            for i, nav in enumerate(navs):
                print(f"    Nav[{i}]: {json.dumps(nav, ensure_ascii=False)[:500]}")
            
            # Check Columns
            cols = tj.get("Columns", [])
            print(f"\n  Columns count: {len(cols)}")
            for i, col in enumerate(cols):
                if isinstance(col, dict):
                    caption = col.get("Caption", "")
                    combo_list = col.get("ComboList", "")
                    print(f"    Col[{i}]: Caption='{caption}', ComboList='{combo_list[:100] if combo_list else ''}'")
                else:
                    print(f"    Col[{i}]: {str(col)[:200]}")
            
            # Check Rows
            rows_data = tj.get("Rows", [])
            print(f"\n  Rows count: {len(rows_data)}")
            if rows_data:
                print(f"    First row: {json.dumps(rows_data[0], ensure_ascii=False)[:500]}")
                
        except Exception as e:
            print(f"  Ticket parse error: {e}")
    
    # Check TreeNode
    cur.execute("SELECT Id, GroupId, ParentId, Name, Number, Type FROM TreeNode WHERE ParentId IS NOT NULL")
    tree_nodes = cur.fetchall()
    print(f"\n  TreeNode count: {len(tree_nodes)}")
    for tn in tree_nodes[:20]:
        print(f"    Id={tn[0]}, ParentId={tn[2]}, Name={tn[3]}, Number={tn[4]}, Type={tn[5]}")

conn.close()
