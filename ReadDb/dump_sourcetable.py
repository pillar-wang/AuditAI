# -*- coding: utf-8 -*-
import sqlite3, json

db_path = r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db"
out_path = r"E:\lq\AuditAI\ReadDb\out_sourcetable.txt"
src_table_id = 10337986289169  # ComboList 引用的源表

with open(out_path, "w", encoding="utf-8") as out:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()

    # 1. TreeNode info
    cur.execute("SELECT Id, Name, ParentId, Type FROM TreeNode WHERE Id=?", (src_table_id,))
    r = cur.fetchone()
    out.write(f"=== Source TreeNode Id={src_table_id} ===\n")
    if r:
        out.write(f"  Id={r[0]}, Name={r[1]}, ParentId={r[2]}, Type={r[3]}\n\n")
    else:
        out.write("  NOT FOUND\n\n")

    # 2. Table info - how many data rows (Tickets/records)
    cur.execute("SELECT Id, length(Ticket) as tlen FROM `Table` WHERE Id=?", (src_table_id,))
    tr = cur.fetchone()
    out.write(f"=== Source Table Id={src_table_id} ===\n")
    if tr:
        out.write(f"  Id={tr[0]}, Ticket len={tr[1]}\n")
        cur.execute("SELECT Ticket FROM `Table` WHERE Id=?", (src_table_id,))
        ticket_json = cur.fetchone()[0]
        if ticket_json:
            tj = json.loads(ticket_json)
            out.write(f"  DataRowStart={tj.get('DataRowStart')}, DataRowCount={tj.get('DataRowCount')}\n")
            rows = tj.get("Rows", [])
            out.write(f"  Rows count in Ticket JSON: {len(rows)}\n")
            cells = tj.get("Cells", [])
            out.write(f"  Cells count: {len(cells)}\n")
            # Show column count
            cols = tj.get("Columns", [])
            out.write(f"  Columns count: {len(cols)}\n")
            # Find the referenced column 10337986289222 - what's its data?
            # Cells layout: row-major. DataRowStart tells where data begins.
            drs = tj.get("DataRowStart", 0)
            ncols = len(cols)
            out.write(f"\n  === Data rows (from DataRowStart={drs}) ===\n")
            for ri in range(drs, len(rows)):
                for ci in range(ncols):
                    idx = ri * ncols + ci
                    if idx < len(cells):
                        cell = cells[idx]
                        text = cell.get("Text", "")
                        field = cell.get("Field", 0)
                        if text:
                            out.write(f"    [{ri},{ci}] Field={field} Text='{text[:80]}'\n")
                out.write("    ---\n")
    else:
        out.write("  Table NOT FOUND\n")

    # 3. The specific columns referenced in ComboList: 10337986289222, 10660108840281, 10337986289224, 10337986289226, 10337986289228, 10337986289230
    out.write(f"\n=== Referenced columns in ComboList ===\n")
    ref_col_ids = [10337986289222, 10660108840281, 10337986289224, 10337986289226, 10337986289228, 10337986289230]
    for cid in ref_col_ids:
        cur.execute("SELECT Id, TableId, `Index`, Caption FROM `Column` WHERE Id=?", (cid,))
        cr = cur.fetchone()
        if cr:
            out.write(f"  Col Id={cr[0]} TableId={cr[1]} Index={cr[2]} Caption={cr[3]}\n")
        else:
            out.write(f"  Col Id={cid} NOT FOUND\n")

    conn.close()
print(f"Output: {out_path}")
