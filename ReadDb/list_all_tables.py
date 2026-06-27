import sqlite3, json, os

db_paths = [
    r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db",
    r"E:\lq\AuditAI\AuditAI\bin\Release\net462\Data\1\d58b4b0a-6642-4264-a6d6-8fc6bc63a6f1.db",
]

for db_path in db_paths:
    if not os.path.exists(db_path):
        continue
    print(f"\n##### {os.path.basename(db_path)} #####")
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()
    # Schema of Table
    cur.execute("PRAGMA table_info(`Table`)")
    cols = cur.fetchall()
    print("Table columns:")
    for c in cols:
        print(f"  {c[1]} ({c[2]})")
    # List all tables with their Title
    cur.execute("SELECT Id, Title, length(Ticket) as cklen FROM `Table` ORDER BY Id")
    rows = cur.fetchall()
    print(f"\nAll tables ({len(rows)}):")
    for tid, title, cklen in rows:
        name = "???"
        try:
            tj = json.loads(title) if title else {}
            name = tj.get("Name", "") or tj.get("Title", "") or str(tj)[:60]
        except:
            name = (title or "")[:60]
        print(f"  Id={tid}, TicketLen={cklen}, Name={name[:60]}")
    conn.close()
