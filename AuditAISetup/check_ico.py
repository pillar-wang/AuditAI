import struct
with open(r'e:\lq\AuditAI\AuditAISetup\app.ico','rb') as f:
    d=f.read()
if d[:4]==b'\x00\x00\x01\x00':
    cnt=struct.unpack('<H',d[4:6])[0]
    print(f'Valid ICO: {cnt} images')
    for i in range(cnt):
        off=6+i*16
        w = d[off]; h = d[off+1]
        bpp = d[off+6]
        sz = struct.unpack('<I', d[off+8:off+12])[0]
        image_off = struct.unpack('<I', d[off+12:off+16])[0]
        print(f'  {w}x{h} {bpp}bpp offset=0x{image_off:x} size={sz}')
else:
    print(f'Invalid ICO header: {d[:8].hex()}')
    print(f'First 64 bytes: {d[:64].hex()}')
