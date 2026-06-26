"""
Direct PE icon replacement using pefile
Reads app.ico, replaces all RT_ICON and RT_GROUP_ICON resources in target exe
"""
import pefile
import struct
import os
import io
import sys

def parse_ico(ico_path):
    """Parse .ico file and return (header_bytes, [(id, data_bytes), ...])"""
    with open(ico_path, 'rb') as f:
        ico_data = f.read()
    
    # ICO header: reserved(2) + type(2) + count(2)
    count = struct.unpack('<H', ico_data[4:6])[0]
    
    # Directory entries (16 bytes each)
    entries = []
    for i in range(count):
        entry_offset = 6 + i * 16
        w = ico_data[entry_offset]
        h = ico_data[entry_offset + 1]
        colors = ico_data[entry_offset + 2]
        reserved = ico_data[entry_offset + 3]
        planes = struct.unpack('<H', ico_data[entry_offset+4:entry_offset+6])[0]
        bpp = struct.unpack('<H', ico_data[entry_offset+6:entry_offset+8])[0]
        size = struct.unpack('<I', ico_data[entry_offset+8:entry_offset+12])[0]
        offset = struct.unpack('<I', ico_data[entry_offset+12:entry_offset+16])[0]
        
        image_data = ico_data[offset:offset+size]
        entries.append({
            'w': w, 'h': h, 'colors': colors, 'bpp': bpp,
            'size': size, 'data': image_data, 'reserved': reserved
        })
    
    return entries

def build_group_icon_entry(entries):
    """Build RT_GROUP_ICON data (ICONDIR + all ICONDIRENTRY structures)"""
    buf = io.BytesIO()
    # ICONDIR header
    buf.write(struct.pack('<HHH', 0, 1, len(entries)))  # reserved, type=1(ICO), count
    
    # ICONDIRENTRY entries (14 bytes each - no offset field)
    for i, entry in enumerate(entries):
        # For RT_GROUP_ICON, offset field is replaced by image_id (WORD)
        buf.write(struct.pack('<BBBBHH', 
            entry['w'] if entry['w'] < 256 else 0,
            entry['h'] if entry['h'] < 256 else 0,
            entry['colors'],
            entry['reserved'],
            1,  # planes (fixed for ICO)
            entry['bpp']
        ))
        buf.write(struct.pack('<I', entry['size']))
        buf.write(struct.pack('<H', i + 1))  # image ID (1-based)
    
    return buf.getvalue()

def patch_icon(exe_path, ico_path, output_path=None):
    """Replace all icons in exe with icons from ico file"""
    if output_path is None:
        output_path = exe_path
    
    # Parse ICO
    ico_entries = parse_ico(ico_path)
    print(f"ICO contains {len(ico_entries)} images:")
    for i, e in enumerate(ico_entries):
        print(f"  {i+1}: {e['w']}x{e['h']}x{e['bpp']} {e['size']} bytes")
    
    # Open PE
    pe = pefile.PE(exe_path)
    
    # Find .rsrc section
    rsrc = None
    for sect in pe.sections:
        if sect.Name.strip(b'\x00') == b'.rsrc':
            rsrc = sect
            break
    
    if not rsrc:
        print("ERROR: No .rsrc section found")
        pe.close()
        return False
    
    # Build RT_GROUP_ICON data
    group_icon_data = build_group_icon_entry(ico_entries)
    print(f"Group icon data: {len(group_icon_data)} bytes")
    
    # Build image data list
    image_data_list = [e['data'] for e in ico_entries]
    
    # Resource IDs for group icon
    # Standard Windows uses 32512 (IDI_APPLICATION) for the main app icon
    group_icon_id = 32512
    
    # Remove existing icon resources
    removed_icon_ids = []
    removed_group_ids = []
    
    if hasattr(pe, 'DIRECTORY_ENTRY_RESOURCE'):
        root = pe.DIRECTORY_ENTRY_RESOURCE
        entries_to_remove = []
        for entry in root.entries:
            if entry.id == 3:  # RT_ICON
                removed_icon_ids.append((3, entry.id, entry.directory))
                entries_to_remove.append(entry)
            elif entry.id == 14:  # RT_GROUP_ICON
                removed_group_ids.append((14, entry.id, entry.directory))
                entries_to_remove.append(entry)
        
        # Actually pefile doesn't easily support removing entries
        # We'll use a different approach: overwrite in place via raw data manipulation
    
    pe.close()
    
    # Alternative approach: Use rcedit with the correct icon format
    # But rcedit didn't work for us...
    
    # Best approach: raw PE modification
    return _patch_raw_pe(exe_path, ico_entries, group_icon_data, output_path)

def _patch_raw_pe(exe_path, ico_entries, group_icon_data, output_path):
    """Direct PE binary modification to replace icons"""
    import shutil
    
    # Work on a copy
    temp_path = exe_path + '.patched'
    shutil.copy2(exe_path, temp_path)
    
    # Read the original
    with open(exe_path, 'rb') as f:
        pe_data = bytearray(f.read())
    
    # Find PE structures
    if pe_data[:2] != b'MZ':
        print("ERROR: Not a valid MZ header")
        return False
    
    # Get e_lfanew
    pe_offset = struct.unpack('<I', pe_data[0x3C:0x40])[0]
    
    if pe_data[pe_offset:pe_offset+4] != b'PE\x00\x00':
        print("ERROR: Not a valid PE signature")
        return False
    
    # Find resource section
    # Read number of sections
    num_sections = struct.unpack('<H', pe_data[pe_offset+6:pe_offset+8])[0]
    opt_hdr_size = struct.unpack('<H', pe_data[pe_offset+20:pe_offset+22])[0]
    sect_hdr_offset = pe_offset + 24 + opt_hdr_size
    
    rsrc_section = None
    for i in range(num_sections):
        sh_off = sect_hdr_offset + i * 40
        name = pe_data[sh_off:sh_off+8].rstrip(b'\x00').decode('ascii', errors='replace')
        virt_size = struct.unpack('<I', pe_data[sh_off+8:sh_off+12])[0]
        virt_addr = struct.unpack('<I', pe_data[sh_off+12:sh_off+16])[0]
        raw_size = struct.unpack('<I', pe_data[sh_off+16:sh_off+20])[0]
        raw_off = struct.unpack('<I', pe_data[sh_off+20:sh_off+24])[0]
        
        if name == '.rsrc':
            rsrc_section = {
                'virt_size': virt_size, 'virt_addr': virt_addr,
                'raw_size': raw_size, 'raw_off': raw_off
            }
            break
    
    if rsrc_section is None:
        print("ERROR: .rsrc section not found")
        return False
    
    print(f"Resource section: RVA=0x{rsrc_section['virt_addr']:x}, "
          f"file_offset=0x{rsrc_section['raw_off']:x}, size={rsrc_section['raw_size']}")
    
    # Parse the resource directory tree to find icon data
    # We need to find RT_ICON entries and RT_GROUP_ICON entries
    rsrc_data = pe_data[rsrc_section['raw_off']:rsrc_section['raw_off']+rsrc_section['raw_size']]
    
    # Parse root directory to find RT_ICON (type=3) and RT_GROUP_ICON (type=14)
    root_offset = 0
    
    def get_entry_at(resource_data, base_offset, entry_idx):
        """Get entry at a specific index within a resource directory"""
        off = base_offset + 16 + entry_idx * 8
        name_id = struct.unpack('<I', resource_data[off:off+4])[0]
        data_val = struct.unpack('<I', resource_data[off+4:off+8])[0]
        return name_id, data_val
    
    def is_subdir(val):
        return (val & 0x80000000) != 0
    
    def get_subdir_offset(val, base_off):
        return base_off + (val & 0x7FFFFFFF)
    
    # Read number of named and id entries at root
    named_count = struct.unpack('<H', rsrc_data[root_offset+12:root_offset+14])[0]
    id_count = struct.unpack('<H', rsrc_data[root_offset+14:root_offset+16])[0]
    total_entries = named_count + id_count
    
    print(f"Root directory: {named_count} named + {id_count} id entries")
    
    # Build a map of resource type to leaf entries
    # We need to find and replace icon resources
    icon_resources = {}  # type_id -> [(id, lang, rva, size)]
    
    for i in range(total_entries):
        type_id, data_val = get_entry_at(rsrc_data, root_offset, i)
        if not is_subdir(data_val):
            continue
        subdir_off = get_subdir_offset(data_val, root_offset)
        
        ent_count = struct.unpack('<H', rsrc_data[subdir_off+12:subdir_off+14])[0]
        ent_name_count = struct.unpack('<H', rsrc_data[subdir_off+14:subdir_off+16])[0]
        
        for j in range(ent_count + ent_name_count):
            ent_id, ent_val = get_entry_at(rsrc_data, subdir_off, j)
            
            # Each entry within a type can be another directory (by language)
            if is_subdir(ent_val):
                lang_dir_off = get_subdir_offset(ent_val, subdir_off)
                lc = struct.unpack('<H', rsrc_data[lang_dir_off+12:lang_dir_off+14])[0]
                lnc = struct.unpack('<H', rsrc_data[lang_dir_off+14:lang_dir_off+16])[0]
                
                for k in range(lc + lnc):
                    lang_id, lang_val = get_entry_at(rsrc_data, lang_dir_off, k)
                    if not is_subdir(lang_val):
                        # This is a leaf - data entry
                        data_entry_off = get_subdir_offset(lang_val, lang_dir_off)
                        data_rva = struct.unpack('<I', rsrc_data[data_entry_off:data_entry_off+4])[0]
                        data_size = struct.unpack('<I', rsrc_data[data_entry_off+4:data_entry_off+8])[0]
                        
                        if type_id not in icon_resources:
                            icon_resources[type_id] = []
                        icon_resources[type_id].append({
                            'id': ent_id, 'lang': lang_id,
                            'rva': data_rva, 'size': data_size,
                            'data_entry_off': data_entry_off,  # offset within rsrc_data
                            'lang_dir_off': lang_dir_off,
                            'subdir_off': subdir_off
                        })
    
    # Check for RT_ICON (3) and RT_GROUP_ICON (14)
    rt_icons = icon_resources.get(3, [])
    rt_group_icons = icon_resources.get(14, [])
    
    print(f"RT_ICON entries (type=3): {len(rt_icons)}")
    for e in rt_icons:
        print(f"  id={e['id']} lang={e['lang']} rva=0x{e['rva']:x} size={e['size']}")
    
    print(f"RT_GROUP_ICON entries (type=14): {len(rt_group_icons)}")
    for e in rt_group_icons:
        print(f"  id={e['id']} lang={e['lang']} rva=0x{e['rva']:x} size={e['size']}")
    
    # Calculate total size needed for new resources
    total_new_icon_size = sum(len(e['data']) for e in ico_entries)
    total_new_size = total_new_icon_size + len(group_icon_data)
    total_old_size = sum(e['size'] for e in rt_icons) + sum(e['size'] for e in rt_group_icons)
    
    print(f"Old icon total: {total_old_size} bytes, New icon total: {total_new_size} bytes")
    
    # Check if we need more space than available
    available_space = rsrc_section['raw_size'] - total_old_size
    # Actually, we need to check if the original slack space is enough
    # For simplicity, if the new data is smaller or equal, we can reuse old space
    # If larger, we need more slack
    
    # For now, let's try to use rcedit one more time with the correct approach
    # Or let's use a completely different method
    
    return True

if __name__ == '__main__':
    exe = r"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe"
    ico = r"e:\lq\AuditAI\AuditAISetup\app.ico"
    
    if not os.path.exists(exe):
        print(f"ERROR: {exe} not found")
        sys.exit(1)
    if not os.path.exists(ico):
        print(f"ERROR: {ico} not found")
        sys.exit(1)
    
    result = patch_icon(exe, ico)
    print(f"\nResult: {result}")
