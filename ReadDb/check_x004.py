import sqlite3, os

# Check 应收账款 tables specifically in AuditAI templates
db_path = r"e:\lq\AuditAI\AuditAI\Data\Templates\X004.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# First find 应收账款 tables
cur.execute("SELECT Id, Title, length(Ticket) as TicketLen FROM `Table` WHERE Title LIKE ?", ('%应收账款%',))
rows = cur.fetchall()
print(f"X004.db - Found {len(rows)} 应收账款 tables")
for row in rows:
    print(f"  Id={row[0]}, TicketLen={row[2]}")

# Check all tables with Ticket > 1000
print("\nAll tables with Ticket > 1000 in X004.db:")
cur.execute("SELECT Id, length(Ticket) as TicketLen FROM `Table` WHERE length(Ticket) > 1000")
rows = cur.fetchall()
for row in rows:
    print(f"  Id={row[0]}, TicketLen={row[1]}")

conn.close()
