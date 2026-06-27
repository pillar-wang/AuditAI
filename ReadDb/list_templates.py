import sqlite3, json, os

# Find all template db files
template_dir = r"e:\lq\AuditAI\AuditAI\Data\Templates"
if not os.path.exists(template_dir):
    template_dir = r"e:\lq\AuditAI\Data\Templates"

for root, dirs, files in os.walk(template_dir):
    for f in files:
        if f.endswith('.db'):
            db_path = os.path.join(root, f)
            try:
                conn = sqlite3.connect(db_path)
                cur = conn.cursor()
                cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='Table'")
                if cur.fetchone():
                    # Find all tables with Ticket data
                    cur.execute("SELECT Id, Title, length(Ticket) as TicketLen FROM `Table` WHERE length(Ticket) > 100 ORDER BY TicketLen DESC LIMIT 10")
                    rows = cur.fetchall()
                    if rows:
                        print(f"\n=== {f} ===")
                        for row in rows:
                            tid = row[0]
                            title = row[1]
                            tlen = row[2]
                            # Extract table name from Title JSON
                            try:
                                tj = json.loads(title)
                                name = tj.get("Name", "Unknown")
                            except:
                                name = "??? (parse error)"
                            print(f"  Id={tid}, TicketLen={tlen}, Name={name[:50]}")
                conn.close()
            except Exception as e:
                print(f"  Error reading {f}: {e}")