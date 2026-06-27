# -*- coding: utf-8 -*-
import sqlite3, json

db_path = r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db"
out_path = r"E:\lq\AuditAI\ReadDb\out_src_full.txt"
src_table_id = 10337986289169

with open(out_path, "w", encoding="utf-8") as out:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()
    cur.execute("SELECT Ticket FROM `Table` WHERE Id=?", (src_table_id,))
    r = cur.fetchone()
    ticket_json = r[0] if r else None
    out.write(f"=== Full Ticket JSON for Table {src_table_id} (len={len(ticket_json) if ticket_json else 0}) ===\n\n")
    if ticket_json:
        tj = json.loads(ticket_json)
        out.write(f"Top-level keys: {list(tj.keys())}\n\n")
        for k, v in tj.items():
            sv = json.dumps(v, ensure_ascii=False) if not isinstance(v, str) else v
            if len(sv) > 800:
                sv = sv[:800] + "..."
            out.write(f"  {k}: {sv}\n")

    # Also check the target table (应收账款替代测试表) Records
    out.write(f"\n\n=== Target table 11287174055345 (应收账款替代测试表) Records ===\n")
    cur.execute("SELECT Ticket FROM `Table` WHERE Id=11287174055345")
    r2 = cur.fetchone()
    tj2 = json.loads(r2[0])
    out.write(f"Top-level keys: {list(tj2.keys())}\n")
    # Records might be stored differently
    for k in tj2.keys():
        if 'record' in k.lower() or 'ticket' in k.lower() or 'nav' in k.lower():
            v = tj2[k]
            sv = json.dumps(v, ensure_ascii=False)
            if len(sv) > 1000: sv = sv[:1000] + "..."
            out.write(f"  {k}: {sv}\n")
    # Check IsAllowShowVirtualNode
    out.write(f"  IsAllowShowVirtualNode: {tj2.get('IsAllowShowVirtualNode')}\n")
    # Navs
    out.write(f"  Navs: {json.dumps(tj2.get('Navs'), ensure_ascii=False)}\n")
    # DataRowCount
    out.write(f"  DataRowStart: {tj2.get('DataRowStart')}, DataRowCount: {tj2.get('DataRowCount')}\n")
    out.write(f"  Rows: {len(tj2.get('Rows', []))}, Cells: {len(tj2.get('Cells', []))}\n")

    conn.close()
print(f"Output: {out_path}")
