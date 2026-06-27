import sqlite3
import json
import os

db_paths = [
    r"e:\lq\Data\1\00000000-0000-0000-0000-000000000000.db",
    r"e:\lq\Data\leqi_audit.db",
    r"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db",
]

for db_path in db_paths:
    if not os.path.exists(db_path):
        print(f"NOT FOUND: {db_path}")
        continue

    print(f"\n=== {db_path} ===")
    conn = sqlite3.connect(db_path)
    conn.row_factory = sqlite3.Row
    cur = conn.cursor()

    cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
    tables = [row[0] for row in cur.fetchall()]
    print(f"Tables: {', '.join(tables)}")

    if "Table" not in tables:
        continue

    # First find the exact table
    cur.execute("SELECT Id, Title, length(Ticket) as TicketLen FROM `Table` WHERE Title LIKE '%应收账款替代测试表%' LIMIT 5")
    rows = cur.fetchall()
    if not rows:
        print("No table found with name containing '应收账款替代测试表'")
        continue

    for row in rows:
        tid = row["Id"]
        title = row["Title"]
        ticket_len = row["TicketLen"]
        print(f"\nTable Id={tid}, TicketLen={ticket_len}")
        
        # Parse Title JSON to get NavTreeCellIdList
        try:
            title_json = json.loads(title)
            nav_tree = title_json.get("NavTreeCellIdList")
            print(f"  NavTreeCellIdList: {nav_tree}")
            print(f"  Title Keys: {list(title_json.keys())}")
        except:
            print(f"  Title parse error")

        # Check TicketNav for this table
        if "TicketNav" in tables:
            cur.execute("SELECT * FROM TicketNav WHERE TableId=?", (tid,))
            navs = cur.fetchall()
            print(f"  TicketNav count: {len(navs)}")
            for nav in navs[:3]:
                nav_dict = dict(nav)
                # Print key fields
                keys = ["Id", "TableId", "Name", "CellId", "ParentId", "IsVirtualNode", "SortIndex", "Level", "ComboList"]
                for k in keys:
                    if k in nav_dict:
                        print(f"    {k}: {nav_dict[k]}")

        # Check Column for this table
        if "Column" in tables:
            cur.execute("SELECT * FROM `Column` WHERE TableId=? LIMIT 5", (tid,))
            cols = cur.fetchall()
            print(f"  Column count (first 5): {len(cols)}")
            for col in cols:
                col_dict = dict(col)
                keys = ["Id", "TableId", "Name", "CellId", "DataFormat", "ComboList", "Formula"]
                for k in keys:
                    if k in col_dict:
                        val = col_dict[k]
                        if val and len(str(val)) > 200:
                            val = str(val)[:200] + "..."
                        print(f"    {k}: {val}")

        # Check TableRow for this table
        if "TableRow" in tables:
            cur.execute("SELECT count(*) FROM `TableRow` WHERE TableId=?", (tid,))
            row_count = cur.fetchone()[0]
            print(f"  TableRow count: {row_count}")

    conn.close()
