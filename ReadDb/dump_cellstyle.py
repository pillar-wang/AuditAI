# -*- coding: utf-8 -*-
import sqlite3, json

db_path = r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db"
out_path = r"E:\lq\AuditAI\ReadDb\out_cellstyle.txt"
target_style_id = 11476152614921  # 单位名称列的 StyleId

with open(out_path, "w", encoding="utf-8") as out:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()
    cur.execute("PRAGMA table_info(CellStyle)")
    cols = [c[1] for c in cur.fetchall()]
    out.write(f"CellStyle cols: {cols}\n\n")

    cur.execute("SELECT * FROM CellStyle WHERE Id=?", (target_style_id,))
    r = cur.fetchone()
    out.write(f"=== CellStyle Id={target_style_id} (单位名称列) ===\n")
    if r:
        for i, v in enumerate(r):
            sv = str(v)
            if len(sv) > 2000: sv = sv[:2000] + "..."
            out.write(f"  {cols[i]}: {sv}\n")
        # Parse Format if present
        if 'Format' in cols:
            fi = cols.index('Format')
            if r[fi]:
                try:
                    fj = json.loads(r[fi])
                    out.write(f"\n  Format JSON keys: {list(fj.keys())}\n")
                    for k, v in fj.items():
                        sv = str(v)
                        if len(sv) > 500: sv = sv[:500] + "..."
                        out.write(f"    {k}: {sv}\n")
                except Exception as e:
                    out.write(f"  Format parse err: {e}\n")
    else:
        out.write("  NOT FOUND\n")

    # Also check the table's DefaultStyle
    out.write(f"\n=== Table DefaultStyle for 应收账款替代测试表 ===\n")
    cur.execute("SELECT DefaultStyleId FROM `Table` WHERE Id=11287174055345")
    dsr = cur.fetchone()
    if dsr:
        out.write(f"  DefaultStyleId={dsr[0]}\n")
        cur.execute("SELECT * FROM CellStyle WHERE Id=?", (dsr[0],))
        dr = cur.fetchone()
        if dr:
            for i, v in enumerate(dr):
                sv = str(v)
                if len(sv) > 1500: sv = sv[:1500] + "..."
                out.write(f"  {cols[i]}: {sv}\n")

    # Search ALL CellStyle rows that have ComboList in Format
    out.write(f"\n=== All CellStyle rows with non-empty ComboList in Format ===\n")
    cur.execute("SELECT Id, Format FROM CellStyle")
    cnt = 0
    for cid, fmt in cur.fetchall():
        if not fmt: continue
        try:
            fj = json.loads(fmt)
            cl = fj.get("ComboList", "")
            if cl:
                cnt += 1
                out.write(f"  CellStyle Id={cid} ComboList='{cl[:300]}'\n")
        except: pass
    out.write(f"  Total CellStyle with ComboList: {cnt}\n")

    conn.close()
print(f"Output: {out_path}")
