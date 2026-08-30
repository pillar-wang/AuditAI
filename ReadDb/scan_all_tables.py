# -*- coding: utf-8 -*-
# 扫描指定库中所有表的完整性 + 状态分布
import sqlite3, sys

def scan(db):
    print("=" * 100)
    print("DB:", db)
    conn = sqlite3.connect(db)
    c = conn.cursor()
    nodes = c.execute("SELECT Id, Name, Version, Status, Dirty FROM TreeNode WHERE Type=0 OR Name IS NOT NULL ORDER BY Id").fetchall()
    # Type 对应：表节点类型需确认，这里按 TreeNode 中 Id 在 [Table] 中是否存在来筛选
    for tid, name, ver, st, dirty in nodes:
        has_table = c.execute("SELECT COUNT(*) FROM `Table` WHERE Id=?", (tid,)).fetchone()[0]
        if not has_table:
            continue
        nrows = c.execute("SELECT COUNT(*) FROM Row WHERE TableId=? AND Status<2", (tid,)).fetchone()[0]
        ncols = c.execute("SELECT COUNT(*) FROM `Column` WHERE TableId=? AND Status<2", (tid,)).fetchone()[0]
        ncells = c.execute(
            "SELECT COUNT(*) FROM Cell c JOIN Row r ON c.RowId=r.Id JOIN `Column` l ON c.ColumnId=l.Id WHERE r.TableId=? AND c.Status<2", (tid,)).fetchone()[0]
        # 状态分布
        row_st = dict(c.execute("SELECT Status, COUNT(*) FROM Row WHERE TableId=? GROUP BY Status", (tid,)).fetchall())
        cell_st = dict(c.execute("SELECT c.Status, COUNT(*) FROM Cell c JOIN Row r ON c.RowId=r.Id WHERE r.TableId=? GROUP BY c.Status", (tid,)).fetchall())
        orphan = c.execute("""
            SELECT COUNT(*) FROM Cell c LEFT JOIN Row r ON c.RowId=r.Id LEFT JOIN `Column` l ON c.ColumnId=l.Id
            WHERE r.Id IS NULL OR l.Id IS NULL
        """).fetchone()[0]
        ok = (nrows * ncols == ncells)
        flag = "  <<< 损坏/不一致" if not ok else ""
        print(f"  Id={tid} Ver={ver} St={st} Dirty={dirty} | {name}")
        print(f"     Rows={nrows} Cols={ncols} Cells={ncells} 期望={nrows*ncols} {'OK' if ok else 'FAIL'} 孤儿Cell={orphan}")
        print(f"     Row状态={row_st} Cell状态={cell_st}")
    conn.close()

for p in sys.argv[1:]:
    scan(p)
print("done")
