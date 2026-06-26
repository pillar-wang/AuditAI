"""Extract icon from exe and compare with app.ico"""
import struct

ICO_PATH = r"e:\lq\AuditAI\AuditAISetup\app.ico"
EXE_PATH = r"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe"

# Read app.ico to get first 32x32 block
with open(ICO_PATH, 'rb') as f:
    ico_data = f.read()

# Extract first image data from app.ico
count = struct.unpack('<H', ico_data[4:6])[0]
ico_first_off = struct.unpack('<I', ico_data[6+2*16+12:6+2*16+16])[0]  # 3rd entry (32x32)
ico_first_sz = struct.unpack('<I', ico_data[6+2*16+8:6+2*16+12])[0]    # 3rd entry
ico_first_data = ico_data[ico_first_off:ico_first_off+ico_first_sz]

print(f"ICO 32x32 data: offset=0x{ico_first_off:x} size={ico_first_sz}")

# Read exe and extract icon data at reported offset
with open(EXE_PATH, 'rb') as f:
    exe_data = bytearray(f.read())

# From pefile output: RT_ICON name=3 is 32x32 at file_off=0x414afc size=2253
exe_icon_off = 0x414afc
exe_icon_sz = 2253
exe_icon_data = exe_data[exe_icon_off:exe_icon_off+exe_icon_sz]

print(f"EXE 32x32 data at file_off=0x{exe_icon_off:x} size={exe_icon_sz}")
print(f"Data lengths match: {len(exe_icon_data)} == {ico_first_sz}")

# Compare bytes
diff_positions = []
for i in range(min(len(exe_icon_data), len(ico_first_data))):
    if exe_icon_data[i] != ico_first_data[i]:
        diff_positions.append(i)

if len(diff_positions) == 0 and len(exe_icon_data) == len(ico_first_data):
    print("=> EXACT MATCH! Bytes are identical at file offset level.")
elif len(diff_positions) == 0:
    print("=> Data matches for overlapping bytes but lengths differ.")
else:
    print(f"=> DIFFERENT: {len(diff_positions)} byte positions differ "
          f"(out of {min(len(exe_icon_data), len(ico_first_data))})")
    print(f"First 10 diff positions: {diff_positions[:10]}")
    # Check if the data might be our old icon (before patch)
    print(f"First 32 bytes of EXE icon: {exe_icon_data[:32].hex()}")
    print(f"First 32 bytes of ICO: {ico_first_data[:32].hex()}")