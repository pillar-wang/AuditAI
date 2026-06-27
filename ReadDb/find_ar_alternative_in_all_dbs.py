import sqlite3, json, os

# Find all db files
db_dir = r"e:\lq\AuditAI\AuditAI\bin\Debug\net462\Data"
for root, dirs, files in os.walk(db_dir):
    for f in files:
        if f.endswith('.db'):
            db_path = os.path.join(root, f)
            conn = sqlite3.connect(db_path)
            cur = conn.cursor()
            try:
                cur.execute("SELECT Id, Title, length(Ticket) FROM [Table] WHERE Title LIKE ?", ('%应收账款替代测试表%',))
                rows = cur.fetchall()
                if rows:
                    print(f"\n{'='*80}")
                    print(f"FOUND in: {db_path}")
                    print(f"{'='*80}")
                    for row in rows:
                        tid = row[0]
                        title_raw = row[1]
                        ticket_len = row[2]
                        print(f"\nTable Id={tid}, TitleLen={len(title_raw)}, TicketLen={ticket_len}")
                        
                        # Parse Title
                        try:
                            tj = json.loads(title_raw)
                            print(f"  Title keys: {list(tj.keys())}")
                            for k, v in tj.items():
                                if isinstance(v, str):
                                    print(f"  {k}: '{v[:200]}'")
                                elif isinstance(v, list):
                                    print(f"  {k}: list of {len(v)} items")
                                elif isinstance(v, dict):
                                    print(f"  {k}: dict with keys {list(v.keys())[:10]}")
                                else:
                                    print(f"  {k}: {v}")
                        except Exception as e:
                            print(f"  Title parse error: {e}")
                        
                        # Parse Ticket
                        if ticket_len > 0:
                            cur.execute("SELECT Ticket FROM [Table] WHERE Id=?", (tid,))
                            ticket_data = cur.fetchone()[0]
                            try:
                                tj = json.loads(ticket_data)
                                print(f"\n  Ticket keys: {list(tj.keys())}")
                                
                                # Check Navs
                                navs = tj.get("Navs", [])
                                print(f"  Navs count: {len(navs)}")
                                for i, nav in enumerate(navs):
                                    print(f"    Nav[{i}]: {json.dumps(nav, ensure_ascii=False)[:500]}")
                                
                                # Check Columns
                                cols = tj.get("Columns", [])
                                print(f"\n  Columns count: {len(cols)}")
                                for i, col in enumerate(cols):
                                    if isinstance(col, dict):
                                        caption = col.get("Caption", "")
                                        combo_list = col.get("ComboList", "")
                                        print(f"    Col[{i}]: Caption='{caption}', ComboList='{combo_list[:150] if combo_list else ''}'")
                                    else:
                                        print(f"    Col[{i}]: {str(col)[:200]}")
                                
                                # Check Rows
                                rows_data = tj.get("Rows", [])
                                print(f"\n  Rows count: {len(rows_data)}")
                                if rows_data:
                                    print(f"    First row sample: {json.dumps(rows_data[0], ensure_ascii=False)[:500]}")
                                    
                            except Exception as e:
                                print(f"  Ticket parse error: {e}")
            except Exception as e:
                pass
            conn.close()

print("\nDone.")
