"""Replace icon data in resx file using pre-generated serialized icon data"""
import xml.etree.ElementTree as ET
import base64

RESX_PATH = r'e:\lq\AuditAI\PlatformResource\Auditai.PlatformResource.Properties.Resource.resx'
ICO_PATH = r'e:\lq\AuditAI\AuditAISetup\app.ico'

# We need to generate .NET serialized Icon data.
# Since we can't do BinaryFormatter serialization in Python,
# we'll use a workaround: replace the image payload inside the serialized stream.

# Read current resx
with open(RESX_PATH, 'r', encoding='utf-8') as f:
    content = f.read()

root = ET.fromstring(content)

# First, let's extract one serialized icon to understand its structure
for data in root.findall('.//data'):
    name = data.get('name', '')
    if name != '审计协作平台':
        continue
    value = data.find('value')
    if value is not None and value.text:
        raw = value.text.strip()
        decoded = base64.b64decode(raw)
        
        print(f"Serialized icon: {len(decoded)} bytes")
        print(f"Header: {decoded[:16].hex()}")
        print(f"BinaryFormatter header: 0x{decoded[0]:02x}{decoded[1]:02x}{decoded[2]:02x}{decoded[3]:02x}")
        print(f"Type: 0x{decoded[8]:02x} (1=sealed class)")
        print(f"Assembly name length: area after type info")
        
        # Find the string "System.Drawing," which is part of the assembly name
        # Position of the assembly qualified type name
        # Format: SerializationHeader(4) + TypeInfo + AssemblyName
            
        # The .NET BinaryFormatter serialized Icon contains:
        # BinaryHeaderEnum (1 byte) + ...
        # But looking at the data, it starts with \x00\x01\x00\x00
        # which is the SerializationHeaderRecord (RootId=1, HeaderId=-1, MajorVersion=1, MinorVersion=0)
        # followed by BinaryTypeEnum, assembly name, class info, etc.
        
        # After header, we have:\x0c\x02\x00\x00\x00QSystem.Drawing, Version=4.0...
        # \x0c = BinaryTypeEnum.String (member type)
        # Then member name: \x02\x00\x00\x00Q = length 81
        # Followed by 79 bytes: "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
        # Then more data...
        
        # Actually, the key insight is: the actual ICO data starts somewhere inside the serialized stream.
        # Specifically, after all the BinaryFormatter type metadata, there are 16 bytes per ICO image
        # The format is: width(1) + height(1) + colors(1) + reserved(1) + planes(2) + bpp(2) + size(4) + offset(4)
        # Then the raw image data follows.
        
        # A much simpler approach: find the ICO header (00 00 01 00) inside the serialized data
        ico_start = decoded.find(b'\x00\x00\x01\x00')
        if ico_start >= 0:
            print(f"\nICO data found at offset {ico_start} in serialized blob")
            
            # Read the ICO structure
            ico_data = decoded[ico_start:]
            cnt = struct.unpack('<H', ico_data[4:6])[0]
            print(f"ICO images in existing resource: {cnt}")
            
            for i in range(cnt):
                off = 6 + i * 16
                w = ico_data[off]
                h = ico_data[off + 1]
                sz = struct.unpack('<I', ico_data[off+8:off+12])[0]
                img_off = struct.unpack('<I', ico_data[off+12:off+16])[0]
                print(f"  {i+1}: {w}x{h} size={sz} at +{ico_start+img_off}")
    
    break