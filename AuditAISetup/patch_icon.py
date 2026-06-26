"""Patch icon by directly modifying PE resource section using pefile"""
import pefile, struct, os, shutil

def ico_parse(ico_path):
    with open(ico_path, 'rb') as f:
        data = f.read()
    count = struct.unpack('<H', data[4:6])[0]
    chunks = []
    for i in range(count):
        pos = 6 + i * 16
        w, h, colors, rsv = data[pos], data[pos+1], data[pos+2], data[pos+3]
        planes, bpp = struct.unpack('<HH', data[pos+4:pos+8])
        size, offset = struct.unpack('<II', data[pos+8:pos+16])
        chunks.append({
            'w': w, 'h': h, 'colors': colors, 'planes': 1, 'bpp': 32,
            'size': size, 'offset': offset, 'data': data[offset:offset+size]
        })
    return chunks

def patch(exe_path, ico_path):
    chunks = ico_parse(ico_path)
    print(f"  ICO: {len(chunks)} entries")
    
    # Build group icon (ResDir format - same data but last DWORD is resource ID)
    grp_data = bytearray()
    grp_data += struct.pack('<HHH', 0, 1, len(chunks))
    for i, c in enumerate(chunks):
        grp_data += struct.pack('<BBBBHHII',
            c['w'], c['h'], c['colors'], 0,
            c['planes'], c['bpp'], c['size'],
            i + 1)  # RT_ICON resource ID
    
    backup = exe_path + ".bak"
    shutil.copy2(exe_path, backup)
    
    try:
        pe = pefile.PE(exe_path)
        
        # Find resource section
        res_rva = pe.OPTIONAL_HEADER.DATA_DIRECTORY[pefile.DIRECTORY_ENTRY['IMAGE_DIRECTORY_ENTRY_RESOURCE']].VirtualAddress
        # Convert RVA to file offset
        res_offset = pe.get_offset_from_rva(res_rva)
        
        # Read current resource data
        res_section = pe.sections[pe.get_section_by_rva(res_rva).name.strip(b'\x00')]
        
        # Build new resource entries
        # We'll use a simpler approach: replace RT_ICON and RT_GROUP_ICON entries in place
        
        def find_resource_dir(parent_dir, res_id):
            """Find a resource directory entry by ID"""
            for entry in parent_dir.entries:
                if getattr(entry, 'id', None) == res_id:
                    return entry
            return None
        
        # Find RT_ICON directory
        icon_dir = find_resource_dir(pe.DIRECTORY_ENTRY_RESOURCE, 3)
        grp_icon_dir = find_resource_dir(pe.DIRECTORY_ENTRY_RESOURCE, 14)
        
        if icon_dir is None or grp_icon_dir is None:
            raise Exception("No icon resources found")
        
        # Get existing icon entries
        old_entries = []
        for de in icon_dir.directory.entries:
            if de.directory:
                for le in de.directory.entries:
                    old_entries.append({
                        'id': de.id,
                        'lang': le.id,
                        'data_rva': le.data.struct.OffsetToData,
                        'size': le.data.struct.Size
                    })
        
        print(f"  Old icons: {len(old_entries)} entries")
        
        # Build replacement resource data blob
        # Format: [ICON_DIR_ENTRIES][ICON_DIR_ENTRY w/ ID=1...n][GROUP_ICON_DATA]
        # We need to replace the actual resource data in the PE
        
        # First, remove old RT_ICON entries by overwriting their data
        for i, old in enumerate(old_entries):
            # Overwrite old data with zeroes
            raw_offset = pe.get_offset_from_rva(old['data_rva'])
            with open(exe_path, 'r+b') as f:
                f.seek(raw_offset)
                f.write(b'\x00' * old['size'])
        
        # Remove old group icon data
        for de in grp_icon_dir.directory.entries:
            if de.directory:
                for le in de.directory.entries:
                    raw_offset = pe.get_offset_from_rva(le.data.struct.OffsetToData)
                    with open(exe_path, 'r+b') as f:
                        f.seek(raw_offset)
                        f.write(b'\x00' * le.data.struct.Size)
        
        pe.close()
        
        # Now use Windows API to add the new resources
        # This is cleaner since the file is clean
        import ctypes
        
        # Use proper function definitions
        kernel32 = ctypes.windll.kernel32
        
        BeginUpdateResourceW = kernel32.BeginUpdateResourceW
        BeginUpdateResourceW.argtypes = [ctypes.c_wchar_p, ctypes.c_int]
        BeginUpdateResourceW.restype = ctypes.c_void_p
        
        UpdateResourceW = kernel32.UpdateResourceW
        UpdateResourceW.argtypes = [
            ctypes.c_void_p,
            ctypes.c_void_p,
            ctypes.c_void_p,
            ctypes.c_uint16,
            ctypes.c_void_p,
            ctypes.c_uint32
        ]
        UpdateResourceW.restype = ctypes.c_bool
        
        EndUpdateResourceW = kernel32.EndUpdateResourceW
        EndUpdateResourceW.argtypes = [ctypes.c_void_p, ctypes.c_int]
        EndUpdateResourceW.restype = ctypes.c_bool
        
        RT_ICON = ctypes.c_void_p(3)
        RT_GROUP_ICON = ctypes.c_void_p(14)
        MAIN_ICON = ctypes.c_void_p(32512)
        
        hUpdate = BeginUpdateResourceW(exe_path, 0)
        if not hUpdate:
            err = ctypes.get_last_error()
            raise Exception(f"BeginUpdateResourceW: error={err}")
        
        try:
            # Add icon images
            for i, c in enumerate(chunks):
                img_data = (ctypes.c_uint8 * len(c['data'])).from_buffer_copy(c['data'])
                id_icon = ctypes.c_void_p(i + 1)
                ok = UpdateResourceW(hUpdate, RT_ICON, id_icon, 0, img_data, len(c['data']))
                if not ok:
                    err = ctypes.get_last_error()
                    print(f"    WARN: icon {i+1} failed error={err}")
            
            # Add group icon
            grp_bytes = bytes(grp_data)
            grp_buffer = (ctypes.c_uint8 * len(grp_bytes)).from_buffer_copy(grp_bytes)
            ok = UpdateResourceW(hUpdate, RT_GROUP_ICON, MAIN_ICON, 0, grp_buffer, len(grp_bytes))
            if not ok:
                err = ctypes.get_last_error()
                raise Exception(f"GROUP_ICON error={err}")
            
            ok = EndUpdateResourceW(hUpdate, 0)
            if not ok:
                err = ctypes.get_last_error()
                raise Exception(f"EndUpdateResourceW error={err}")
            
            print(f"  OK: {os.path.basename(exe_path)}")
            os.remove(backup)
        except:
            EndUpdateResourceW(hUpdate, 1)
            raise
            
    except Exception as e:
        print(f"  FAILED: {os.path.basename(exe_path)}: {e}")
        shutil.copy2(backup, exe_path)
        os.remove(backup)

print("Patching icons...")
ico_path = r"e:\lq\AuditAI\AuditAISetup\app.ico"
for exe in [
    r"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe",
    r"e:\lq\AuditAI\AuditAILauncher\bin\x86\Release\net40\AuditAILauncher.exe"
]:
    patch(exe, ico_path)
print("Done!")
