import sqlite3, json, base64

db_path = r"e:\lq\AuditAI\AuditAI\Data\Templates\X004.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Get Ticket data for 应收账款替代测试表 (Id=11287174055345)
cur.execute("SELECT Id, Title, length(Ticket) as TicketLen, Ticket FROM `Table` WHERE Id IN (11287174055345, 11265699218137)")
rows = cur.fetchall()
for row in rows:
    tid = row[0]
    ticket = row[3]
    print(f"\n=== Id={tid}, TicketLen={row[2]} ===")
    
    if ticket:
        try:
            tj = json.loads(ticket)
            print(f"Ticket Keys: {list(tj.keys())}")
            for key in tj:
                val = tj[key]
                if isinstance(val, (str, int, float, bool)) or val is None:
                    print(f"  {key}: {val}")
                elif isinstance(val, list):
                    print(f"  {key}: list[{len(val)}]")
                    if key == "Navs":
                        for i, nav in enumerate(val[:20]):
                            print(f"    Nav[{i}]: {json.dumps(nav, ensure_ascii=False)[:200]}")
                    elif len(val) < 5:
                        print(f"    {json.dumps(val, ensure_ascii=False)[:500]}")
                    else:
                        print(f"    (showing first 3 of {len(val)})")
                        for i, item in enumerate(val[:3]):
                            print(f"    [{i}]: {json.dumps(item, ensure_ascii=False)[:200]}")
                elif isinstance(val, dict):
                    print(f"  {key}: dict {json.dumps(val, ensure_ascii=False)[:300]}")
        except Exception as e:
            print(f"  Parse error: {e}")
            print(f"  Raw (first 2000): {ticket[:2000]}")
    else:
        print("  Ticket: NULL")

conn.close()