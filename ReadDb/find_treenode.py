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
    cur.execute("PRAGMA table_info(TreeNode)")
    print("TreeNode columns:")
    for c in cur.fetchall():
        print(f"  {c[1]} ({c[2]})")
    print()
    # Find nodes with 应收账款 or 替代测试
    cur.execute("SELECT * FROM TreeNode LIMIT 1")
    colnames = [d[0] for d in cur.description]
    print(f"Col names: {colnames}")
    # Try to find a name/title/text column
    name_col = None
    for c in colnames:
        if c.lower() in ('name', 'title', 'text', 'caption'):
            name_col = c
            break
    if not name_col:
        # Use Name column if exists
        for c in colnames:
            if 'name' in c.lower():
                name_col = c
                break
    print(f"Using name col: {name_col}")
    if name_col:
        cur.execute(f"SELECT Id, {name_col}, TableId FROM TreeNode WHERE {name_col} LIKE '%应收账款%' OR {name_col} LIKE '%替代测试%' OR {name_col} LIKE '%应收%'")
        for r in cur.fetchall():
            print(f"  MATCH: Id={r[0]}, Name={r[1]}, TableId={r[2]}")
    conn.close()
