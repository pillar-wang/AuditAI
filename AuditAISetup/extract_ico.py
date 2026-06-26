"""Extract icon from exe as a proper .ico file, comparing with app.ico"""
import struct, os

ICO_PATH = r"e:\lq\AuditAI\AuditAISetup\app.ico"

def extract_ico_from_exe(exe_path, output_path):
    """Extract the main icon group from exe as a proper .ico file"""
    with open(exe_path, 'rb') as f:
        data = bytearray(f.read())
    
    # Parse PE
    pe_off = struct.unpack('<I', data[0x3C:0x40])[0]
    num_sects = struct.unpack('<H', data[pe_off+6:pe_off+8])[0]
    opt_hdr_size = struct.unpack('<H', data[pe_off+20:pe_off+22])[0]
    sect_hdr_off = pe_off + 24 + opt_hdr_size
    
    # Find .rsrc
    rsrc = None
    for i in range(num_sects):
        sh_off = sect_hdr_off + i * 40
        name = data[sh_off:sh_off+8].rstrip(b'\x00')
        if name == b'.rsrc':
            virt_addr = struct.unpack('<I', data[sh_off+12:sh_off+16])[0]
            raw_size = struct.unpack('<I', data[sh_off+16:sh_off+20])[0]
            raw_off = struct.unpack('<I', data[sh_off+20:sh_off+24])[0]
            rsrc = {'va': virt_addr, 'raw_off': raw_off, 'raw_size': raw_size}
            break
    
    if not rsrc:
        print(f"No .rsrc section in {exe_path}")
        return None
    
    # Navigate resource tree to find RT_GROUP_ICON (type=14) with ID 32512
    rsrc_base = rsrc['raw_off']
    
    def enum_entries(base_off):
        entries = []
        nc = struct.unpack('<H', data[base_off+12:base_off+14])[0]
        ic = struct.unpack('<H', data[base_off+14:base_off+16])[0]
        for i in range(nc + ic):
            off = base_off + 16 + i * 8
            eid = struct.unpack('<I', data[off:off+4])[0]
            eval_ = struct.unpack('<I', data[off+4:off+8])[0]
            entries.append((eid, eval_))
        return entries
    
    # Find RT_GROUP_ICON type
    grp_icon_data = None
    icon_images = {}  # id -> (rva, size)
    
    for tid, tval in enum_entries(rsrc_base):
        if not (tval & 0x80000000):
            continue
        tdir_off = rsrc_base + (tval & 0x7FFFFFFF)
        
        for nid, nval in enum_entries(tdir_off):
            if not (nval & 0x80000000):
                continue
            ldir_off = rsrc_base + (nval & 0x7FFFFFFF)
            
            for _, lval in enum_entries(ldir_off):
                if lval & 0x80000000:
                    continue
                deoff = rsrc_base + (lval & 0x7FFFFFFF)
                drva = struct.unpack('<I', data[deoff:deoff+4])[0]
                dsz = struct.unpack('<I', data[deoff+4:deoff+8])[0]
                
                if tid == 3:  # RT_ICON
                    icon_images[nid] = (drva, dsz)
                elif tid == 14 and nid == 32512:  # RT_GROUP_ICON
                    # Convert data RVA to file offset
                    grp_foff = rsrc_base + (drva - rsrc['va'])
                    grp_icon_data = bytes(data[grp_foff:grp_foff+dsz])
    
    if not grp_icon_data:
        print(f"No RT_GROUP_ICON found in {exe_path}")
        return None
    
    # Parse group icon to get image IDs
    cnt = struct.unpack('<H', grp_icon_data[4:6])[0]
    entries_info = []
    for i in range(cnt):
        off = 6 + i * 14
        w = grp_icon_data[off]; h = grp_icon_data[off+1]
        colors = grp_icon_data[off+2]
        reserved = grp_icon_data[off+3]
        planes = struct.unpack('<H', grp_icon_data[off+4:off+6])[0]
        bpp = struct.unpack('<H', grp_icon_data[off+6:off+8])[0]
        sz = struct.unpack('<I', grp_icon_data[off+8:off+12])[0]
        img_id = struct.unpack('<H', grp_icon_data[off+12:off+14])[0]
        
        if img_id in icon_images:
            irva, isz = icon_images[img_id]
            foff = rsrc_base + (irva - rsrc['va'])
            img_data = bytes(data[foff:foff+isz])
            entries_info.append((w, h, colors, reserved, planes, bpp, len(img_data), img_data))
        else:
            print(f"WARN: Image id={img_id} not found in RT_ICON entries")
    
    # Build .ico file
    ico = bytearray()
    ico += struct.pack('<HHH', 0, 1, len(entries_info))
    
    # Directory entries + data
    data_offset = 6 + len(entries_info) * 16
    image_datas = bytearray()
    current_off = data_offset
    
    for w, h, colors, reserved, planes, bpp, sz, img_data in entries_info:
        # Directory entry
        ico += struct.pack('<BBBBHHII',
            w if w < 256 else 0,
            h if h < 256 else 0,
            colors, reserved,
            planes, bpp,
            sz, current_off)
        image_datas += img_data
        current_off += len(img_data)
    
    ico += image_datas
    
    # Write output
    with open(output_path, 'wb') as f:
        f.write(ico)
    
    return len(entries_info)

# Extract from all files
for exe, label in [
    (r"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe", "Source AuditAI"),
    (r"e:\lq\AuditAI\AuditAILauncher\bin\x86\Release\net40\AuditAILauncher.exe", "Source Launcher"),
    (r"D:\Program Files (x86)\AuditAI\AuditAI.exe", "Installed AuditAI"),
    (r"D:\Program Files (x86)\AuditAI\AuditAILauncher.exe", "Installed Launcher"),
]:
    out = os.path.join(r"e:\lq\AuditAI\AuditAISetup", f"extracted_{label.replace(' ','_')}.ico")
    n = extract_ico_from_exe(exe, out)
    if n:
        # Compare with app.ico
        with open(ICO_PATH, 'rb') as f:
            ico_data = f.read()
        with open(out, 'rb') as f:
            ext_data = f.read()
        
        # Compare at 32x32 level (3rd image usually)
        def get_image_data(ico_bytes, idx):
            c = struct.unpack('<H', ico_bytes[4:6])[0]
            if idx >= c:
                return None
            off = 6 + idx * 16
            img_off = struct.unpack('<I', ico_bytes[off+12:off+16])[0]
            img_sz = struct.unpack('<I', ico_bytes[off+8:off+12])[0]
            return ico_bytes[img_off:img_off+img_sz]
        
        img32_src = get_image_data(ico_data, 2)  # 3rd image (32x32)
        img32_ext = get_image_data(ext_data, 2)
        
        if img32_src and img32_ext and len(img32_src) == len(img32_ext):
            diffs = sum(1 for i in range(len(img32_src)) if img32_src[i] != img32_ext[i])
            status = "MATCH" if diffs == 0 else f"DIFF={diffs}"
        else:
            sizes = f"src={len(img32_src) if img32_src else 'NA'}/ext={len(img32_ext) if img32_ext else 'NA'}" if img32_src and img32_ext else "N/A"
            status = f"CANNOT COMPARE ({sizes})"
        
        print(f"{label}: {n} images, {status}")
        print(f"  Extracted to: {out}")