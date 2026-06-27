# -*- coding: utf-8 -*-
import sqlite3, json, os, base64

db_path = r"E:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db"
out_path = r"E:\lq\AuditAI\ReadDb\out_nav_col.txt"
target_field = 11287174055451

with open(out_path, "w", encoding="utf-8") as out:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()
    cur.execute("SELECT Ticket, Title FROM `Table` WHERE Id=11287174055345")
    row = cur.fetchone()
    ticket = json.loads(row[0])
    title = json.loads(row[1])

    # Decode title binary value
    tbv = title.get("TitleCell", {}).get("BinaryValue")
    if tbv:
        try:
            out.write(f"Table title: {base64.b64decode(tbv).decode('utf-8')}\n\n")
        except Exception as e:
            out.write(f"Title decode err: {e}\n\n")

    # Navs config
    out.write(f"Navs: {json.dumps(ticket.get('Navs'), ensure_ascii=False)}\n")
    out.write(f"IsAllowShowVirtualNode: {ticket.get('IsAllowShowVirtualNode')}\n\n")

    # Find the column with Field = target_field
    out.write(f"=== Looking for Column with Field={target_field} ===\n")
    columns = ticket.get("Columns", [])
    out.write(f"Total columns: {len(columns)}\n")
    for i, col in enumerate(columns):
        f = col.get("Field")
        if f == target_field:
            out.write(f"\n*** FOUND at column index {i} ***\n")
            out.write(json.dumps(col, ensure_ascii=False, indent=2) + "\n")
            break
    else:
        out.write(f"  NOT FOUND in Columns. All Fields: {[c.get('Field') for c in columns]}\n")

    # Cells - find cells in column index of target_field, look for ComboList in cell or column
    # Also search all cells for ComboList referencing
    out.write(f"\n=== Searching all Cells for ComboList containing target_field or any ComboList ===\n")
    cells = ticket.get("Cells", [])
    out.write(f"Total cells: {len(cells)}\n")
    # Cells are laid out row by row. Need to find the header cell for our column
    # Print header row cells (first ColumnHeaderRowsCount rows)
    chr_count = ticket.get("ColumnHeaderRowsCount", 1)
    cols_count = len(columns)
    out.write(f"ColumnHeaderRowsCount={chr_count}, cols_count={cols_count}\n")
    for r in range(chr_count):
        for c in range(cols_count):
            idx = r * cols_count + c
            if idx < len(cells):
                cell = cells[idx]
                text = cell.get("Text", "")
                combo = cell.get("ComboList", "")
                formula = cell.get("Formula", "")
                field = cell.get("Field", 0)
                if text or combo or formula:
                    out.write(f"  header[{r},{c}] Field={field} Text='{text}' ComboList='{combo[:120]}' Formula='{formula[:120]}'\n")

    # Search any cell (including data cells) that has ComboList set
    out.write(f"\n=== Cells with non-empty ComboList ===\n")
    found_combo = False
    for idx, cell in enumerate(cells):
        combo = cell.get("ComboList", "")
        if combo:
            found_combo = True
            out.write(f"  cell[{idx}] ComboList='{combo}'\n")
            out.write(f"    Text='{cell.get('Text','')}' Field={cell.get('Field')}\n")
    if not found_combo:
        out.write("  NONE found in Cells\n")

    # Check Columns for ComboList (maybe stored on column not cell)
    out.write(f"\n=== Columns with non-empty ComboList/Formula ===\n")
    for i, col in enumerate(columns):
        combo = col.get("ComboList", "")
        formula = col.get("Formula", "")
        if combo or formula:
            out.write(f"  col[{i}] Field={col.get('Field')} ComboList='{combo[:200]}' Formula='{formula[:200]}'\n")

    conn.close()

print(f"Output: {out_path}")
