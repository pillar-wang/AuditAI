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
                    cur.execute("SELECT Id, Title FROM `Table`")
                    rows = cur.fetchall()
                    for row in rows:
                        tid = row[0]
                        title = row[1]
                        try:
                            tj = json.loads(title)
                            name = tj.get("Name", "")
                            if "应收账款" in name or "替代测试" in name:
                                tlen = len(row[1]) if row[1] else 0
                                print(f"\n=== {f} === Id={tid}")
                                print(f"  Name: {name}")
                                print(f"  Title length: {tlen}")
                                # Show NavTreeCellIdList
                                nav_tree = tj.get("NavTreeCellIdList")
                                if nav_tree:
                                    print(f"  NavTreeCellIdList: {nav_tree}")
                        except:
                            pass
                conn.close()
            except:
                pass