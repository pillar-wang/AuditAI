import sqlite3
import os

db_path = r"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Search for 应收账款 in TreeNode
cur.execute("SELECT Id, GroupId, ParentId, Name, Number, Type FROM TreeNode WHERE Name LIKE ? ORDER BY Id", ('%应收账款%',))
rows = cur.fetchall()
print(f"TreeNode rows matching 应收账款: {len(rows)}")
for row in rows:
    print(f"  Id={row[0]}, GroupId={row[1]}, ParentId={row[2]}, Name={row[3]}, Number={row[4]}, Type={row[5]}")

# Also check if any table matches
if rows:
    table_ids = [str(row[0]) for row in rows]
    placeholders = ','.join(['?'] * len(table_ids))
    cur.execute(f"SELECT Id, Title, length(Ticket) as TicketLen FROM `Table` WHERE Id IN ({placeholders})", table_ids)
    tables = cur.fetchall()
    print(f"\nMatching Table rows: {len(tables)}")
    for t in tables:
        print(f"  Id={t[0]}, TicketLen={t[2]}")

conn.close()
