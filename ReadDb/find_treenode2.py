# -*- coding: utf-8 -*-
import sqlite3, json, os

db_paths = [
    r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db",
    r"E:\lq\AuditAI\AuditAI\bin\Release\net462\Data\1\d58b4b0a-6642-4264-a6d6-8fc6bc63a6f1.db",
]

keywords = ["应收账款", "替代测试", "应收"]

for db_path in db_paths:
    if not os.path.exists(db_path):
        continue
    print(f"\n##### {os.path.basename(db_path)} #####")
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()
    # Show all TreeNode columns
    cur.execute("PRAGMA table_info(TreeNode)")
    cols = [c[1] for c in cur.fetchall()]
    print(f"TreeNode cols: {cols}")
    # Find nodes matching keywords
    cur.execute("SELECT Id, GroupId, ParentId, Name, Type, Level FROM TreeNode")
    rows = cur.fetchall()
    print(f"Total TreeNodes: {len(rows)}")
    matches = []
    for r in rows:
        name = r[3] or ""
        for kw in keywords:
            if kw in name:
                matches.append(r)
                break
    print(f"Matches: {len(matches)}")
    for r in matches:
        print(f"  Id={r[0]}, GroupId={r[1]}, ParentId={r[2]}, Name={r[3]}, Type={r[4]}, Level={r[5]}")
    conn.close()
