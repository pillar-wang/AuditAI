import sqlite3
import os

# Search all db files for table containing 应收账款
for root, dirs, files in os.walk(r'e:\lq'):
    for f in files:
        if f.endswith('.db'):
            db_path = os.path.join(root, f)
            try:
                conn = sqlite3.connect(db_path)
                cur = conn.cursor()
                cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='Table'")
                if cur.fetchone():
                    cur.execute('SELECT Id, Title, length(Ticket) as TicketLen FROM `Table` WHERE Title LIKE ? LIMIT 3', ('%应收账款%',))
                    rows = cur.fetchall()
                    if rows:
                        print(f'=== {db_path} ===')
                        for r in rows:
                            print(f'  Id={r[0]}, TicketLen={r[2]}, Title={r[1][:100]}...')
                conn.close()
            except Exception as e:
                pass
