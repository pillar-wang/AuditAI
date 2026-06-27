import sqlite3
import json
import os

# Search all db files
for root, dirs, files in os.walk(r'e:\lq'):
    for f in files:
        if f.endswith('.db'):
            db_path = os.path.join(root, f)
            try:
                conn = sqlite3.connect(db_path)
                cur = conn.cursor()
                cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='Table'")
                if cur.fetchone():
                    cur.execute('SELECT Id, Title, length(Ticket) as TicketLen FROM `Table` WHERE Title LIKE ?', ('%应收账款%',))
                    rows = cur.fetchall()
                    if rows:
                        print(f'\n=== {db_path} ===')
                        for r in rows:
                            tid = r[0]
                            title = r[1]
                            tlen = r[2]
                            # Parse title
                            try:
                                tj = json.loads(title)
                                table_name = tj.get("Name", "Unknown")
                                nav_tree = tj.get("NavTreeCellIdList")
                                print(f'  Id={tid}, Name={table_name}, TicketLen={tlen}')
                                if nav_tree:
                                    print(f'    NavTreeCellIdList: {nav_tree}')
                            except:
                                print(f'  Id={tid}, Title parse error, TicketLen={tlen}')
                conn.close()
            except Exception as e:
                pass
