import sqlite3
import json

db_path = r"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

table_id = 11287174055345

# Get table info
cur.execute("SELECT Id, Title, Ticket, length(Ticket) as TicketLen FROM `Table` WHERE Id=?", (table_id,))
row = cur.fetchone()
if row:
    print(f"Table Id={row[0]}, TicketLen={row[3]}")
    
    # Parse Title
    try:
        title_json = json.loads(row[1])
        print(f"Title Keys: {list(title_json.keys())}")
        nav_tree = title_json.get("NavTreeCellIdList")
        if nav_tree:
            print(f"NavTreeCellIdList: {nav_tree}")
        else:
            print("NavTreeCellIdList: NOT FOUND")
        
        # Show TitleCell info
        tc = title_json.get("TitleCell")
        if tc:
            print(f"TitleCell Formula: {tc.get('Formula')}")
            print(f"TitleCell ComboList: {tc.get('ComboList')}")
    except Exception as e:
        print(f"Title parse error: {e}")
    
    # Parse Ticket
    ticket = row[2]
    if ticket:
        try:
            ticket_json = json.loads(ticket)
            print(f"\nTicket Keys: {list(ticket_json.keys())}")
            # Show key ticket info
            for key in ['NavTreeId', 'NavTreeName', 'IsAllowShowVirtualNode', 'TicketType']:
                if key in ticket_json:
                    print(f"  {key}: {ticket_json[key]}")
            
            # Show Navs if present
            navs = ticket_json.get("Navs")
            if navs:
                print(f"  Navs count: {len(navs)}")
                for i, nav in enumerate(navs[:10]):
                    print(f"    Nav[{i}]: {json.dumps(nav, ensure_ascii=False)[:200]}")
        except Exception as e:
            print(f"Ticket parse error: {e}")
            print(f"Ticket (first 1000): {ticket[:1000]}")
    else:
        print("Ticket: NULL")

# Get columns
print("\n=== Columns ===")
cur.execute("SELECT * FROM `Column` WHERE TableId=? LIMIT 20", (table_id,))
cols = cur.fetchall()
if cols:
    cur.execute("PRAGMA table_info(`Column`)")
    col_info = cur.fetchall()
    col_names = [c[1] for c in col_info]
    print(f"Column count: {len(cols)}")
    for col in cols[:10]:
        d = dict(zip(col_names, col))
        print(f"  Id={d.get('Id')}, Name={d.get('Name')}, CellId={d.get('CellId')}, ComboList={d.get('ComboList')}, Formula={d.get('Formula')}")

# Get TicketNav
print("\n=== TicketNav ===")
cur.execute("SELECT * FROM TicketNav WHERE TableId=? LIMIT 20", (table_id,))
navs = cur.fetchall()
if navs:
    cur.execute("PRAGMA table_info(TicketNav)")
    nav_info = cur.fetchall()
    nav_names = [n[1] for n in nav_info]
    print(f"TicketNav count: {len(navs)}")
    for nav in navs[:15]:
        d = dict(zip(nav_names, nav))
        print(f"  Id={d.get('Id')}, CellId={d.get('CellId')}, Name={d.get('Name')}, ParentId={d.get('ParentId')}, Level={d.get('Level')}, IsVirtualNode={d.get('IsVirtualNode')}, ComboList={d.get('ComboList')}, SortIndex={d.get('SortIndex')}")

conn.close()
