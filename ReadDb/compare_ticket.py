import sqlite3, json, base64

# Original project
orig_path = r"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db"
# Current project template
tpl_path = r"e:\lq\AuditAI\AuditAI\Data\Templates\X004.db"

table_id = 11287174055345

# Get both
for label, db_path in [("Original (LeqiAudit)", orig_path), ("Template (AuditAI)", tpl_path)]:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()
    cur.execute("SELECT Ticket FROM `Table` WHERE Id=?", (table_id,))
    row = cur.fetchone()
    if row and row[0]:
        ticket = json.loads(row[0])
        print(f"\n=== {label} ===")
        print(f"Kind: {ticket.get('Kind')} (2=DynamicRow)")
        print(f"Level: {ticket.get('Level')} (2=Receipt)")
        print(f"DataRowStart: {ticket.get('DataRowStart')}")
        print(f"DataRowCount: {ticket.get('DataRowCount')}")
        print(f"Columns count: {len(ticket.get('Columns', []))}")
        print(f"Rows count: {len(ticket.get('Rows', []))}")
        print(f"Cells count: {len(ticket.get('Cells', []))}")
        print(f"Merges count: {len(ticket.get('Merges', []))}")
        print(f"Navs count: {len(ticket.get('Navs', []))}")
        for i, nav in enumerate(ticket.get('Navs', [])):
            print(f"  Nav[{i}]: {json.dumps(nav, ensure_ascii=False)[:300]}")
    else:
        print(f"\n=== {label} === No Ticket data")
    conn.close()