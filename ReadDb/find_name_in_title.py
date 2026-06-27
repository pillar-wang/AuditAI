import sqlite3, json

db_path = r"e:\lq\AuditAI\AuditAI\Data\Templates\X004.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Check TitleCell field which might contain the table name
cur.execute("SELECT Title FROM `Table` WHERE Id=11287174055345")
title = cur.fetchone()[0]
tj = json.loads(title)

print("TitleCell:")
tc = tj.get("TitleCell")
print(json.dumps(tc, ensure_ascii=False, indent=2))

# Check if there's a Name or Title field
for key in tj:
    if key not in ('Rows', 'Merges', 'Columns', 'Version', 'TitleCell', 'TitleHeight', 'HasBinaryValue'):
        print(f"\nOther key: {key} = {tj[key]}")

# Get all tables and their names from the Title JSON to find 应收账款替代测试表
print("\n\n============ Searching all tables ============")
cur.execute("SELECT Id, Title FROM `Table`")
rows = cur.fetchall()
for row in rows:
    tid = row[0]
    title_str = row[1]
    try:
        tj2 = json.loads(title_str)
        tc2 = tj2.get("TitleCell", {})
        # The table name might be in Cell.Formula or Cell.ComboList
        formula = tc2.get("Formula", "") if tc2 else ""
        if "应收账款" in formula or "替代测试" in formula:
            print(f"\nId={tid}")
            print(f"  TitleCell Formula: {formula}")
            print(f"  TitleCell: {json.dumps(tc2, ensure_ascii=False)[:500]}")
    except:
        # Maybe title is a different format
        if "应收账款" in title_str:
            print(f"\nId={tid}, Title contains '应收账款': {title_str[:200]}")

conn.close()