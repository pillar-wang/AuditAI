"""Compare PE resource icon data before and after rcedit"""
import struct, os

def dump_icon_info(exe_path, label):
    with open(exe_path, 'rb') as f:
        data = f.read()
    
    # Find PE signature
    dos_hdr = struct.unpack('<I', data[0x3C:0x40])[0]
    pe_sig = data[dos_hdr:dos_hdr+4]
    if pe_sig != b'PE\x00\x00':
        print(f"  Not a valid PE file")
        return
    
    # Find resource directory
    # Skip to optional header
    coff_hdr_size = struct.unpack('<H', data[dos_hdr+16:dos_hdr+18])[0]
    opt_hdr_start = dos_hdr + 20 + coff_hdr_size
    
    # Get number of data directory entries
    magic = struct.unpack('<H', data[opt_hdr_start:opt_hdr_start+2])[0]
    if magic == 0x10b:  # PE32
        dir_count_offset = opt_hdr_start + 92
        dir_entry_size = 8
        first_dir_offset = opt_hdr_start + 96
    elif magic == 0x20b:  # PE32+
        dir_count_offset = opt_hdr_start + 108
        dir_entry_size = 8
        first_dir_offset = opt_hdr_start + 112
    else:
        print(f"  Unknown magic: {hex(magic)}")
        return
    
    # Resource directory entry is index 2
    res_rva_offset = first_dir_offset + 2 * dir_entry_size
    res_rva = struct.unpack('<I', data[res_rva_offset:res_rva_offset+4])[0]
    res_size = struct.unpack('<I', data[res_rva_offset+4:res_rva_offset+8])[0]
    
    print(f"  {label}: Resource RVA=0x{res_rva:x}, Size={res_size}")
    
    # Find section containing resource data
    sect_count = struct.unpack('<H', data[dos_hdr+6:dos_hdr+8])[0]
    sect_hdr_start = opt_hdr_start + (dir_count_offset - opt_hdr_start) + 4  # approximate
    
    # Actually parse section headers
    if magic == 0x10b:
        sect_hdr_offset = opt_hdr_start + 0xF8 - 4  # PE32 section header offset calculation
    else:
        sect_hdr_offset = opt_hdr_start + 0xF8  # PE32+ section header offset
    
    # Find .rsrc section
    res_file_offset = None
    for i in range(sect_count):
        sh_offset = sect_hdr_offset + i * 40
        sect_name = data[sh_offset:sh_offset+8].rstrip(b'\x00').decode('ascii', errors='replace')
        virt_addr = struct.unpack('<I', data[sh_offset+12:sh_offset+16])[0]
        raw_size = struct.unpack('<I', data[sh_offset+16:sh_offset+20])[0]
        raw_offset = struct.unpack('<I', data[sh_offset+20:sh_offset+24])[0]
        if virt_addr <= res_rva < virt_addr + raw_size:
            res_file_offset = raw_offset + (res_rva - virt_addr)
            break
    
    if res_file_offset is None:
        print(f"  Cannot find resource section")
        return
    
    print(f"  Resource at file offset: 0x{res_file_offset:x}")
    
    # Read resource directory
    def read_res_dir_entry(offset):
        name_or_id = struct.unpack('<I', data[offset:offset+4])[0]
        data_entry = struct.unpack('<I', data[offset+4:offset+8])[0]
        return name_or_id, data_entry
    
    def is_name_entry(val):
        return (val & 0x80000000) != 0
    
    def is_subdir_entry(val):
        return (val & 0x80000000) != 0
    
    # Parse root directory
    root_entries = struct.unpack('<HH', data[res_file_offset+12:res_file_offset+16])[0]
    name_entries = struct.unpack('<HH', data[res_file_offset+14:res_file_offset+16])[0]
    total_entries = root_entries  # Actually the sum of named + id entries
    
    entry_count = struct.unpack('<H', data[res_file_offset+12:res_file_offset+14])[0]
    name_count = struct.unpack('<H', data[res_file_offset+14:res_file_offset+16])[0]
    
    icon_types_found = []
    
    for e in range(entry_count + name_count):
        entry_offset = res_file_offset + 16 + e * 8
        e_id, e_val = read_res_dir_entry(entry_offset)
        
        type_name = str(e_id)
        if e_id == 3: type_name = "RT_ICON"
        elif e_id == 14: type_name = "RT_GROUP_ICON"
        
        if e_id in (3, 14):
            # This is a subdirectory
            subdir_offset = res_file_offset + (e_val & 0x7FFFFFFF)
            sub_count = struct.unpack('<H', data[subdir_offset+12:subdir_offset+14])[0]
            sub_name_count = struct.unpack('<H', data[subdir_offset+14:subdir_offset+16])[0]
            
            for s in range(sub_count + sub_name_count):
                s_entry_offset = subdir_offset + 16 + s * 8
                s_id, s_val = read_res_dir_entry(s_entry_offset)
                
                if s_val & 0x80000000:  # subdir
                    s_subdir = subdir_offset + (s_val & 0x7FFFFFFF)
                    s_sub_count = struct.unpack('<H', data[s_subdir+12:s_subdir+14])[0]
                    for ss in range(s_sub_count):
                        ss_entry = s_subdir + 16 + ss * 8
                        _, ss_val = read_res_dir_entry(ss_entry)
                        if not (ss_val & 0x80000000):
                            data_rva = struct.unpack('<I', data[ss_val:ss_val+4])[0]
                            data_size = struct.unpack('<I', data[ss_val+4:ss_val+8])[0]
                            icon_types_found.append((type_name, s_id, data_size))
    
    for t, sid, sz in icon_types_found:
        print(f"    {t}: id={sid}, data_size={sz}")

print("=== 原始文件（备份） ===")
dump_icon_info(r"e:\lq\AuditAI\AuditAISetup\app.ico", "ICO file... nah")
print()

# We don't have a backup anymore, let me check current state
import pefile
for exe_path in [
    r"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe",
    r"e:\lq\AuditAI\AuditAILauncher\bin\x86\Release\net40\AuditAILauncher.exe"
]:
    print(f"\n=== {os.path.basename(exe_path)} ===")
    pe = pefile.PE(exe_path)
    try:
        for e in pe.DIRECTORY_ENTRY_RESOURCE.entries:
            tname = {3:'RT_ICON',14:'RT_GROUP_ICON',16:'RT_VERSION',24:'RT_MANIFEST'}.get(e.id, str(e.id))
            if e.directory:
                for de in e.directory.entries:
                    if de.directory:
                        for le in de.directory.entries:
                            sz = le.data.struct.Size if le.data else 0
                            print(f"  {tname}: name={de.id} lang={le.id} size={sz}")
    except Exception as ex:
        print(f"  Error: {ex}")
    pe.close()
