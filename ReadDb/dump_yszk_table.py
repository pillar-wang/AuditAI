# -*- coding: utf-8 -*-
import sqlite3, json, os

db_path = r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db"
out_path = r"E:\lq\AuditAI\ReadDb\out_yszk.txt"

target_node_id = 11287174055345  # 应收账款替代测试表

with open(out_path, "w", encoding="utf-8") as out:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()

    # 1. Confirm the TreeNode
    cur.execute("SELECT Id, GroupId, ParentId, Name, Type, Level FROM TreeNode WHERE Id=?", (target_node_id,))
    r = cur.fetchone()
    out.write(f"=== TreeNode Id={target_node_id} ===\n")
    out.write(f"  Id={r[0]}, GroupId={r[1]}, ParentId={r[2]}, Name={r[3]}, Type={r[4]}, Level={r[5]}\n\n")

    # 2. Parent node
    cur.execute("SELECT Id, Name, Type FROM TreeNode WHERE Id=?", (r[2],))
    pr = cur.fetchone()
    out.write(f"=== Parent TreeNode ===\n  Id={pr[0]}, Name={pr[1]}, Type={pr[2]}\n\n")

    # 3. How is TreeNode linked to Table? Check Table.Id matching TreeNode.Id
    cur.execute("SELECT Id, length(Title) as tlen, length(Ticket) as cklen FROM `Table` WHERE Id=?", (target_node_id,))
    tr = cur.fetchone()
    out.write(f"=== Table with Id={target_node_id} ===\n")
    if tr:
        out.write(f"  Found: Id={tr[0]}, Title len={tr[1]}, Ticket len={tr[2]}\n")
    else:
        out.write(f"  NOT FOUND by Id={target_node_id}\n")
        # Try to find Table by other means - maybe TreeNode.Number or a mapping
        cur.execute("PRAGMA table_info(`Table`)")
        out.write(f"  Table cols: {[c[1] for c in cur.fetchall()]}\n")
        # Maybe Table has a TreeNodeId column? or the link is via Number
        # Let's search all Table rows for any reference
        cur.execute("SELECT Id FROM `Table` LIMIT 5")
        out.write(f"  First 5 Table.Ids: {cur.fetchall()}\n")

    # 4. Dump the Table's Ticket (Nav config) for the target
    cur.execute("SELECT Id, Title, Ticket, ControlFormula FROM `Table` WHERE Id=?", (target_node_id,))
    row = cur.fetchone()
    if row:
        out.write(f"\n=== Full Table row Id={row[0]} ===\n")
        out.write(f"  Title (first 500): {(row[1] or '')[:500]}\n")
        out.write(f"  Ticket (first 2000): {(row[2] or '')[:2000]}\n")
        out.write(f"  ControlFormula (first 1000): {(row[3] or '')[:1000]}\n")
        # Parse Ticket JSON
        if row[2]:
            try:
                tj = json.loads(row[2])
                out.write(f"\n  Ticket JSON keys: {list(tj.keys()) if isinstance(tj, dict) else type(tj)}\n")
                if isinstance(tj, dict):
                    for k, v in tj.items():
                        sv = json.dumps(v, ensure_ascii=False) if not isinstance(v, str) else v
                        if len(sv) > 500:
                            sv = sv[:500] + "..."
                        out.write(f"    {k}: {sv}\n")
            except Exception as e:
                out.write(f"  Ticket JSON parse error: {e}\n")
    conn.close()

print(f"Output written to {out_path}")
