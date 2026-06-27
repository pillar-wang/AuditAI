import sqlite3, json, base64

db_path = r"e:\lq\AuditAI\AuditAI\Data\Templates\X004.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Decode the BinaryValue to find table names
cur.execute("SELECT Id, Title FROM `Table`")
rows = cur.fetchall()

print("All tables with decoded names:")
for row in rows:
    tid = row[0]
    title_str = row[1]
    try:
        tj = json.loads(title_str)
        tc = tj.get("TitleCell", {})
        bv = tc.get("BinaryValue", "")
        if bv:
            try:
                decoded = base64.b64decode(bv).decode('utf-8')
            except:
                decoded = f"(base64 decode error: {bv[:50]})"
        else:
            decoded = "(empty)"
        formula = tc.get("Formula", "")
        tlen = len(title_str)
        print(f"Id={tid}, TicketLen={len(title_str)}, Name={decoded}")
    except Exception as e:
        print(f"Id={tid}, Error: {e}")

conn.close()