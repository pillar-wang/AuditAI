# -*- coding: utf-8 -*-
# 精确查找"其他应收款明细表"节点，只打印统计摘要
import sqlite3, glob, os

TARGET = "其他应收款明细表"
roots = [r"e:\lq\AuditAI"]
paths = []
for root in roots:
    for p in glob.glob(os.path.join(root, "**", "*.db"), recursive=True):
        if "Templates" in p:
            continue
        paths.append(p)

found = False
for p in paths:
    try:
        conn = sqlite3.connect(p)
        cols = [r[1] for r in conn.execute("PRAGMA table_info(TreeNode)").fetchall()]
        if "Name" not in cols:
            conn.close(); continue
        rows = conn.execute("SELECT Id, Name, Version, Status FROM TreeNode WHERE Name LIKE ?", (f"%{TARGET}%",)).fetchall()
        if not rows:
            conn.close(); continue
        found = True
        print("DB:", p)
        for tid, name, ver, st in rows:
            try:
                t = conn.execute("SELECT Id FROM `Table` WHERE Id=?", (tid,)).fetchone()
                has_table = "有" if t else "无!!"
            except Exception:
                has_table = "?"
            try:
                nrows = conn.execute("SELECT COUNT(*) FROM Row WHERE TableId=? AND Status<2", (tid,)).fetchone()[0]
                ncols = conn.execute("SELECT COUNT(*) FROM `Column` WHERE TableId=? AND Status<2", (tid,)).fetchone()[0]
                nrows_all = conn.execute("SELECT COUNT(*) FROM Row WHERE TableId=?", (tid,)).fetchone()[0]
                ncols_all = conn.execute("SELECT COUNT(*) FROM `Column` WHERE TableId=?", (tid,)).fetchone()[0]
                ncells = conn.execute(
                    "SELECT COUNT(*) FROM Cell c JOIN Row r ON c.RowId=r.Id JOIN `Column` l ON c.ColumnId=l.Id WHERE r.TableId=? AND c.Status<2", (tid,)).fetchone()[0]
                ncells_all = conn.execute(
                    "SELECT COUNT(*) FROM Cell c JOIN Row r ON c.RowId=r.Id JOIN `Column` l ON c.ColumnId=l.Id WHERE r.TableId=?", (tid,)).fetchone()[0]
                flag = "== 不一致(损坏) ==" if nrows*ncols != ncells else "一致"
                print(f"  Id={tid} Ver={ver} Status={st} [Table]={has_table} "
                      f"Rows(活/全)={nrows}/{nrows_all} Cols(活/全)={ncols}/{ncols_all} "
                      f"Cells(活/全)={ncells}/{ncells_all} 期望={nrows*ncols} -> {flag}  | {name}")
            except Exception as e:
                print(f"  Id={tid} Ver={ver} Status={st} 查询异常: {e} | {name}")
        conn.close()
    except Exception:
        pass

if not found:
    print("未找到包含 '其他应收款明细表' 的节点")
print("done")
