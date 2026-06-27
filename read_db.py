import sqlite3
import os

db_paths = [
    r"e:\lq\Data\1\00000000-0000-0000-0000-000000000000.db",
    r"e:\lq\Data\leqi_audit.db",
    r"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db",
    r"e:\lq\LeqiAudit\演示账套\ABC有限公司_2023.db"
]

for db_path in db_paths:
    if not os.path.exists(db_path):
        print(f"NOT FOUND: {db_path}")
        continue
    
    print(f"\n=== {db_path} ===")
    conn = sqlite3.connect(db_path)
    c = conn.cursor()
    
    c.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
    tables = [row[0] for row in c.fetchall()]
    print(f"Tables: {', '.join(tables)}")
    
    if 'Table' in tables:
        c.execute("SELECT Id, Title, length(Ticket) as TicketLen, Ticket FROM `Table` WHERE Title LIKE '%应收账款%' OR Title LIKE '%测试表%' LIMIT 10")
        rows = c.fetchall()
        for row in rows:
            id_val = row[0]
            title = row[1]
            ticket_len = row[2]
            ticket = row[3]
            print(f"  Id={id_val}, Title={title}, TicketLen={ticket_len}")
            if ticket:
                print(f"  Ticket (first 500 chars): {ticket[:500]}")
            else:
                print(f"  Ticket: NULL")
    
    conn.close()
