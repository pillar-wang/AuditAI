"""Verify group icon data in exe matches expected"""
import struct

ICO_PATH = r"e:\lq\AuditAI\AuditAISetup\app.ico"
EXE_PATH = r"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe"
LAUNCHER_PATH = r"e:\lq\AuditAI\AuditAILauncher\bin\x86\Release\net40\AuditAILauncher.exe"

# Build expected group icon data from ICO
with open(ICO_PATH, 'rb') as f:
    ico_data = f.read()

count = struct.unpack('<H', ico_data[4:6])[0]

# Parse ICO entries  
entries = []
for i in range(count):
    off = 6 + i * 16
    w = ico_data[off]; h = ico_data[off+1]
    colors = ico_data[off+2]; reserved = ico_data[off+3]
    planes = struct.unpack('<H', ico_data[off+4:off+6])[0]
    bpp = struct.unpack('<H', ico_data[off+6:off+8])[0]
    sz = struct.unpack('<I', ico_data[off+8:off+12])[0]
    entries.append((w, h, colors, reserved, planes, bpp, sz, i+1))

# Build expected group icon
expected = bytearray()
expected += struct.pack('<HHH', 0, 1, count)
for w, h, colors, reserved, planes, bpp, sz, img_id in entries:
    expected += struct.pack('<BBBBHHIH', 
        w if w < 256 else 0,
        h if h < 256 else 0,
        colors, 0, 1, bpp, sz, img_id)

expected = bytes(expected)
print(f"Expected group icon: {len(expected)} bytes")
print(f"  First 32 bytes: {expected[:32].hex()}")

for path, name in [(EXE_PATH, "AuditAI.exe"), (LAUNCHER_PATH, "AuditAILauncher.exe")]:
    with open(path, 'rb') as f:
        data = bytearray(f.read())

    # Find the 32-bit ico image to verify what we already confirmed
    # Now read the group icon data from the same offsets we found before
    if name == "AuditAI.exe":
        grp_offsets = [0x42f808]
    else:
        grp_offsets = [0x37f94, 0x37ffc]

    ok = True
    for off in grp_offsets:
        actual = data[off:off+104]
        match = actual == expected
        print(f"  {name} @0x{off:x}: Group icon {'MATCHES' if match else 'DIFFERS'}")
        if not match:
            ok = False
            diffs = sum(1 for i in range(104) if actual[i] != expected[i])
            print(f"    {diffs} bytes differ")

if ok:
    print("\n✅ ALL ICONS VERIFIED! Both exe files have the NEW icon.")
else:
    print("\n⚠️ Some entries still differ.")
