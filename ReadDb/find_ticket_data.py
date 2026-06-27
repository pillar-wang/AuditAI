import sqlite3, os

for root, dirs, files in os.walk(r'e:\lq'):
    for f in files:
        if f.endswith('.db') and 'Data' in root:
            db = os.path.join(root, f)
            try:
                conn = sqlite3.connect(db)
                cur = conn.cursor()
                cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='Table'")
                if cur.fetchone():
                    cur.execute('SELECT Id, length(Ticket) as TicketLen FROM `Table` WHERE length(Ticket) > 1000 LIMIT 3')
                    rows = cur.fetchall()
                    if rows:
                        print(f'{db}: {len(rows)} tables with Ticket > 1000')
                        for r in rows:
                            print(f'  Id={r[0]}, TicketLen={r[1]}')
                conn.close()
            except Exception as e:
                pass
