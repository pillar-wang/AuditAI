import sqlite3, json

db_path = r"e:\lq\AuditAI\AuditAI\Data\Templates\X004.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

cur.execute("SELECT Title FROM `Table` WHERE Id=11287174055345")
row = cur.fetchone()
title = row[0]
print(f"Raw Title (first 500 chars):")
print(title[:500])
print(f"\n--- last 200 chars ---")
print(title[-200:])

print(f"\n--- Parsed keys ---")
tj = json.loads(title)
print(list(tj.keys()))
print(f"\nFull JSON pretty (truncated):")
print(json.dumps(tj, ensure_ascii=False, indent=2)[:2000])

conn.close()