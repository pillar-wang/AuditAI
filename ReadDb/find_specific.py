import sqlite3, json, os

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
                    # Get table 11287174055345 which we know is 应收账款替代测试表 from original project
                    cur.execute("SELECT Id, Title, length(Ticket) as TicketLen FROM `Table` WHERE Id=?", (11287174055345,))
                    row = cur.fetchone()
                    if row:
                        title = row[1]
                        try:
                            tj = json.loads(title)
                            name = tj.get("Name", "???")
                            print(f"{f}: Id={row[0]}, Name={name}, TicketLen={row[2]}")
                        except:
                            print(f"{f}: Id={row[0]}, Title parse error")
                conn.close()
            except Exception as e:
                print(f"Error {f}: {e}")