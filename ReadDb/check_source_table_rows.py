import sqlite3, json

db_path = r"e:\lq\AuditAI\AuditAI\bin\Debug\net462\Data\1\92410a46-a885-49b4-838b-328a1b78e75d.db"
conn = sqlite3.connect(db_path)
cur = conn.cursor()

# Check rows for source table 10337986289169
cur.execute("SELECT r.Id, r.Role, r.Visible, r.Height, r.Creator, c.Value FROM [Row] r LEFT JOIN Cell c ON c.RowId=r.Id AND c.ColumnId=10337986289222 WHERE r.TableId=10337986289169 ORDER BY r.ServerIndex")
rows = cur.fetchall()
print(f"Source table rows: {len(rows)}")
for row in rows:
    print(f"  RowId={row[0]}, Role={row[1]}, Visible={row[2]}, Value='{row[5]}'")

# Also check what TableList operand's DataTable looks like
# We need to see if the column's cells are loaded

conn.close()
