import sqlite3
import json

db_path = r"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

table_id = 11287174055345

# Get full ticket
cur.execute("SELECT Ticket FROM `Table` WHERE Id=?", (table_id,))
row = cur.fetchone()
if row and row[0]:
    ticket = row[0]
    print(f"Ticket length: {len(ticket)}")
    
    try:
        ticket_json = json.loads(ticket)
        # Print Navs in detail
        navs = ticket_json.get("Navs", [])
        print(f"\nNavs count: {len(navs)}")
        for i, nav in enumerate(navs):
            print(f"\nNav[{i}]:")
            print(json.dumps(nav, ensure_ascii=False, indent=2)[:2000])
        
        # Print Columns
        cols = ticket_json.get("Columns", [])
        print(f"\nColumns count: {len(cols)}")
        for i, col in enumerate(cols[:5]):
            print(f"  Col[{i}]: {json.dumps(col, ensure_ascii=False)[:200]}")
        
        # Print Rows count
        rows = ticket_json.get("Rows", [])
        print(f"\nRows count: {len(rows)}")
        
        # Print Kind
        print(f"Kind: {ticket_json.get('Kind')}")
        print(f"Level: {ticket_json.get('Level')}")
        
    except Exception as e:
        print(f"Parse error: {e}")
        print(f"Ticket (first 2000): {ticket[:2000]}")

conn.close()
