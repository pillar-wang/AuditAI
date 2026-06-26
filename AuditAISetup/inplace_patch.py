"""
Replace icon in exe by directly overwriting RT_ICON and RT_GROUP_ICON data in-place.
Since old and new icons have identical sizes, this is a safe byte-level replacement.
"""
import struct
import os

ICO_PATH = r"e:\lq\AuditAI\AuditAISetup\app.ico"

def parse_ico(ico_path):
    """Parse .ico, return (group_icon_data, [(image_id, image_data), ...])"""
    with open(ico_path, 'rb') as f:
        data = f.read()
    
    count = struct.unpack('<H', data[4:6])[0]
    
    # Parse directory entries
    images = []
    for i in range(count):
        off = 6 + i * 16
        w = data[off]; h = data[off + 1]
        colors = data[off + 2]
        reserved = data[off + 3]
        planes = struct.unpack('<H', data[off+4:off+6])[0]
        bpp = struct.unpack('<H', data[off+6:off+8])[0]
        sz = struct.unpack('<I', data[off+8:off+12])[0]
        img_off = struct.unpack('<I', data[off+12:off+16])[0]
        images.append({
            'w': w, 'h': h, 'colors': colors, 'planes': planes,
            'bpp': bpp, 'sz': sz, 'data': data[img_off:img_off+sz],
            'id': i + 1
        })
    
    # Build group icon data (same format but last field is resource ID)
    grp = bytearray()
    grp += struct.pack('<HHH', 0, 1, count)  # reserved, type=1, count
    for img in images:
        grp += struct.pack('<BBBBHHIH',
            img['w'] if img['w'] < 256 else 0,
            img['h'] if img['h'] < 256 else 0,
            img['colors'], 0,  # reserved
            1,  # planes
            img['bpp'],
            img['sz'],
            img['id']  # resource ID (WORD)
        )
    
    return bytes(grp), images

def patch_exe_inplace(exe_path, ico_path):
    """Replace icon data in exe by overwriting bytes in-place"""
    print(f"\n=== Processing: {os.path.basename(exe_path)} ===")
    
    # Read exe
    with open(exe_path, 'rb') as f:
        exe_data = bytearray(f.read())
    
    # Find PE structures
    pe_off = struct.unpack('<I', exe_data[0x3C:0x40])[0]
    if exe_data[pe_off:pe_off + 4] != b'PE\x00\x00':
        print("ERROR: Not a valid PE")
        return False
    
    # Find sections
    num_sects = struct.unpack('<H', exe_data[pe_off + 6:pe_off + 8])[0]
    opt_hdr_size = struct.unpack('<H', exe_data[pe_off + 20:pe_off + 22])[0]
    sect_hdr_off = pe_off + 24 + opt_hdr_size
    
    # Find .rsrc section
    rsrc_sect = None
    for i in range(num_sects):
        sh_off = sect_hdr_off + i * 40
        name = exe_data[sh_off:sh_off + 8].rstrip(b'\x00').decode('ascii', errors='replace')
        virt_addr = struct.unpack('<I', exe_data[sh_off + 12:sh_off + 16])[0]
        raw_size = struct.unpack('<I', exe_data[sh_off + 16:sh_off + 20])[0]
        raw_off = struct.unpack('<I', exe_data[sh_off + 20:sh_off + 24])[0]
        
        if name == '.rsrc':
            rsrc_sect = {'virt_addr': virt_addr, 'raw_off': raw_off, 'raw_size': raw_size}
            break
    
    if rsrc_sect is None:
        print("ERROR: .rsrc section not found")
        return False
    
    print(f".rsrc at file_offset=0x{rsrc_sect['raw_off']:x} size={rsrc_sect['raw_size']}")
    
    # Parse resource directory tree to find icon data locations
    rsrc_base = rsrc_sect['raw_off']
    
    def get_entries_at(base_offset):
        """Get (name_or_id, data_value) pairs from a resource directory"""
        entries = []
        named_count = struct.unpack('<H', exe_data[base_offset + 12:base_offset + 14])[0]
        id_count = struct.unpack('<H', exe_data[base_offset + 14:base_offset + 16])[0]
        for i in range(named_count + id_count):
            off = base_offset + 16 + i * 8
            e_id = struct.unpack('<I', exe_data[off:off + 4])[0]
            e_val = struct.unpack('<I', exe_data[off + 4:off + 8])[0]
            entries.append((e_id, e_val))
        return entries
    
    # Find icon resources
    icon_data_entries = []  # (rva, size) pairs for RT_ICON data
    group_icon_entries = []  # (rva, size) pairs for RT_GROUP_ICON data
    
    for type_id, type_val in get_entries_at(rsrc_base):
        # Type 3 = RT_ICON, Type 14 = RT_GROUP_ICON
        if type_id not in (3, 14):
            continue
        
        if not (type_val & 0x80000000):
            continue
        
        type_dir_off = rsrc_base + (type_val & 0x7FFFFFFF)
        
        for name_id, name_val in get_entries_at(type_dir_off):
            if not (name_val & 0x80000000):
                continue
            
            lang_dir_off = rsrc_base + (name_val & 0x7FFFFFFF)
            
            for lang_id, lang_val in get_entries_at(lang_dir_off):
                if lang_val & 0x80000000:
                    continue
                
                data_entry_off = rsrc_base + (lang_val & 0x7FFFFFFF)
                data_rva = struct.unpack('<I', exe_data[data_entry_off:data_entry_off + 4])[0]
                data_size = struct.unpack('<I', exe_data[data_entry_off + 4:data_entry_off + 8])[0]
                
                # Convert RVA to file offset
                data_file_off = rsrc_base + (data_rva - rsrc_sect['virt_addr'])
                
                if type_id == 3:
                    icon_data_entries.append({
                        'name': name_id, 'lang': lang_id,
                        'rva': data_rva, 'size': data_size,
                        'file_off': data_file_off
                    })
                elif type_id == 14:
                    group_icon_entries.append({
                        'name': name_id, 'lang': lang_id,
                        'rva': data_rva, 'size': data_size,
                        'file_off': data_file_off
                    })
    
    print(f"RT_ICON entries from PE: {len(icon_data_entries)}")
    for e in icon_data_entries:
        print(f"  name={e['name']} lang={e['lang']} size={e['size']} file_off=0x{e['file_off']:x}")
    
    print(f"RT_GROUP_ICON entries from PE: {len(group_icon_entries)}")
    for e in group_icon_entries:
        print(f"  name={e['name']} lang={e['lang']} size={e['size']} file_off=0x{e['file_off']:x}")
    
    # Parse ICO
    grp_data, ico_images = parse_ico(ico_path)
    print(f"ICO images: {len(ico_images)}")
    for i, img in enumerate(ico_images):
        print(f"  {i+1}: {img['id']} {img['w']}x{img['h']} {img['bpp']}bpp {img['sz']} bytes")
    print(f"Group icon data: {len(grp_data)} bytes")
    
    # Verify sizes match
    if len(ico_images) != len(icon_data_entries):
        print(f"WARN: ICO count ({len(ico_images)}) != PE icon count ({len(icon_data_entries)})")
    
    if len(grp_data) != sum(e['size'] for e in group_icon_entries):
        # Group icon data might be same size anyway
        pass
    
    # Overwrite icon data in exe
    modified = False
    for idx, pe_entry in enumerate(icon_data_entries):
        if idx < len(ico_images):
            ico_img = ico_images[idx]
            if ico_img['sz'] <= pe_entry['size']:
                # Overwrite at file offset
                end = pe_entry['file_off'] + ico_img['sz']
                exe_data[pe_entry['file_off']:end] = ico_img['data']
                print(f"  Overwrote RT_ICON name={pe_entry['name']} "
                      f"at 0x{pe_entry['file_off']:x} ({ico_img['sz']} bytes)")
                modified = True
            else:
                print(f"  SKIP: icon {idx} size mismatch: {ico_img['sz']} > {pe_entry['size']}")
    
    # Overwrite group icon data
    for pe_entry in group_icon_entries:
        if len(grp_data) <= pe_entry['size']:
            end = pe_entry['file_off'] + len(grp_data)
            exe_data[pe_entry['file_off']:end] = grp_data
            print(f"  Overwrote RT_GROUP_ICON name={pe_entry['name']} "
                  f"at 0x{pe_entry['file_off']:x} ({len(grp_data)} bytes)")
            modified = True
        else:
            print(f"  SKIP: group icon size mismatch: {len(grp_data)} > {pe_entry['size']}")
    
    if modified:
        # Write back
        with open(exe_path, 'wb') as f:
            f.write(exe_data)
        print(f"  DONE: Written to {os.path.basename(exe_path)}")
        return True
    else:
        print(f"  NOTHING MODIFIED")
        return False

if __name__ == '__main__':
    for exe in [
        r"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe",
        r"e:\lq\AuditAI\AuditAILauncher\bin\x86\Release\net40\AuditAILauncher.exe"
    ]:
        # Restore from clean backup first (AuditAI was just rebuilt; Launcher has issues)
        try:
            patch_exe_inplace(exe, ICO_PATH)
        except Exception as ex:
            print(f"ERROR: {ex}")
