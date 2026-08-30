# -*- coding: utf-8 -*-
import sqlite3
DB = r"e:\lq\AuditAI\AuditAI\bin\Debug\net48\Data\1\3741aa97-d3c5-42aa-b2ae-ed9983780fcb.db"
conn = sqlite3.connect(DB)
c = conn.cursor()
# 找"其他应收款明细表"相关节点
rows = c.execute("SELECT Id, Name, Version, Status, Dirty FROM TreeNode WHERE Name LIKE '%其他应收款%'").fetchall()
for r in rows:
    print("Node:", r)
print("-"*80)
# 全表完整性扫描
for tid, name, ver, st, dirty in rows:
    has = c.execute("SELECT COUNT(*) FROM `Table` WHERE Id=?", (tid,)).fetchone()[0]
    if not has: 
        print(f"  {name}: 无[Table]记录"); continue
    nrows=c.execute("SELECT COUNT(*) FROM Row WHERE TableId=? AND Status<2",(tid,)).fetchone()[0]
    ncols=c.execute("SELECT COUNT(*) FROM `Column` WHERE TableId=? AND Status<2",(tid,)).fetchone()[0]
    ncells=c.execute("SELECT COUNT(*) FROM Cell c JOIN Row r ON c.RowId=r.Id JOIN `Column` l ON c.ColumnId=l.Id WHERE r.TableId=? AND c.Status<2",(tid,)).fetchone()[0]
    nrows_all=c.execute("SELECT COUNT(*) FROM Row WHERE TableId=?",(tid,)).fetchone()[0]
    ncols_all=c.execute("SELECT COUNT(*) FROM `Column` WHERE TableId=?",(tid,)).fetchone()[0]
    rc=dict(c.execute("SELECT Status,COUNT(*) FROM Row WHERE TableId=? GROUP BY Status",(tid,)).fetchall())
    cc=dict(c.execute("SELECT c.Status,COUNT(*) FROM Cell c JOIN Row r ON c.RowId=r.Id WHERE r.TableId=? GROUP BY c.Status",(tid,)).fetchall())
    ok = nrows*ncols==ncells
    print(f"  {name} [{tid}] Ver={ver} St={st} Dirty={dirty}")
    print(f"     Rows(活/全)={nrows}/{nrows_all} Cols={ncols}/{ncols_all} Cells(活/全)={ncells}/{nrows*ncols} {'OK' if ok else '<<<损坏'}")
    print(f"     Row状态={rc} Cell状态={cc}")
conn.close()