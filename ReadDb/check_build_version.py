# -*- coding: utf-8 -*-
# 检查用户实际运行的 dll/exe 中是否包含最近新增的同步修复代码
import os, re

# 用户实际运行的目录
run_dir = r"e:\lq\AuditAI\AuditAI\bin\Debug\net48"
# 我编译的目录（MSBuild Any CPU）
my_dir = r"e:\lq\AuditAI\AuditAI\bin\Any CPU\Debug\net48"

# 新引入的方法/字符串特征
features = [
    "AdoptServerStateAsync",   # 问题7：冲突"采用云端"
    "EnsureAllCellsExist",     # 问题1+4：合并后/全量补全单元格
    "GetTableTask",
    "RepairFromCloudAsync",
    "ShowConflictResolutionDialog",
]

def scan(path, feats):
    if not os.path.exists(path):
        return None
    with open(path, "rb") as f:
        data = f.read()
    return {f: (f.encode("utf-8") in data) for f in feats}

for label, d, files in [("用户运行", run_dir, ["ProjectModel.dll","AuditAI.exe"]),
                        ("我的构建", my_dir, ["ProjectModel.dll","AuditAI.exe"])]:
    print("="*60)
    print(f"[{label}] {d}")
    for f in files:
        p = os.path.join(d, f)
        if not os.path.exists(p):
            print(f"  {f}: 不存在")
            continue
        r = scan(p, features)
        mtime = os.path.getmtime(p)
        print(f"  {f} (mtime={mtime:.0f})")
        for feat, present in r.items():
            print(f"     {'V' if present else '.'} {feat}")