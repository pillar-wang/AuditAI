import sqlite3, json, os, sys

# Search all project + template dbs for "应收账款" / "替代测试"
db_paths = [
    r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\00000000-0000-0000-0000-000000000000.db",
    r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db",
    r"E:\lq\AuditAI\AuditAI\bin\Release\net462\Data\1\00000000-0000-0000-0000-000000000000.db",
    r"E:\lq\AuditAI\AuditAI\bin\Release\net462\Data\1\d58b4b0a-6642-4264-a6d6-8fc6bc63a6f1.db",
]

template_dir = r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\Templates"
if os.path.isdir(template_dir):
    for f in os.listdir(template_dir):
        if f.endswith('.db'):
            db_paths.append(os.path.join(template_dir, f))

for db_path in db_paths:
    if not os.path.exists(db_path):
        continue
    print(f"\n##### {db_path} #####")
    try:
        conn = sqlite3.connect(db_path)
        cur = conn.cursor()
        cur.execute("SELECT name FROM sqlite_master WHERE type='table'")
        tables = [r[0] for r in cur.fetchall()]
        print(f"  Tables: {tables}")
        if 'Table' in tables:
            cur.execute("SELECT Id, length(Title) as tlen, length(Ticket) as cklen FROM `Table`")
            for tid, tlen, cklen in cur.fetchall():
                cur2 = conn.cursor()
                cur2.execute("SELECT Title FROM `Table` WHERE Id=?", (tid,))
                title_row = cur2.fetchone()
                if not title_row or not title_row[0]:
                    continue
                try:
                    tj = json.loads(title_row[0])
                    name = tj.get("Name", "")
                except:
                    name = "???"
                if "应收账款" in name or "替代测试" in name or "应收" in name:
                    print(f"  >>> MATCH Id={tid}, Name={name}")
                    print(f"      Title len={tlen}, Ticket len={cklen}")
                    # Print Nav config keys
                    if isinstance(tj, dict):
                        keys_of_interest = [k for k in tj.keys() if 'nav' in k.lower() or 'ticket' in k.lower() or 'combo' in k.lower() or 'cell' in k.lower()]
                        for k in keys_of_interest:
                            v = tj[k]
                            sv = str(v)
                            if len(sv) > 300:
                                sv = sv[:300] + "..."
                            print(f"      {k}: {sv}")
        conn.close()
    except Exception as e:
        print(f"  ERROR: {e}")
