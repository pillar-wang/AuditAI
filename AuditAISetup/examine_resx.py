"""Examine and replace icon data in resx file"""
import xml.etree.ElementTree as ET
import base64
import struct
import io
import os

RESX_PATH = r'e:\lq\AuditAI\PlatformResource\Auditai.PlatformResource.Properties.Resource.resx'
ICO_PATH = r'e:\lq\AuditAI\AuditAISetup\app.ico'

# Read resx
with open(RESX_PATH, 'r', encoding='utf-8') as f:
    content = f.read()

root = ET.fromstring(content)

# Check the format of icon data
for data in root.findall('.//data'):
    name = data.get('name', '')
    if name != '审计协作平台':
        continue
    value = data.find('value')
    if value is not None and value.text:
        raw = value.text.strip()
        print(f"Name: {name}")
        print(f"Length: {len(raw)} chars")
        
        # Try base64 decode
        try:
            decoded = base64.b64decode(raw)
            print(f"Base64 decoded: {len(decoded)} bytes")
            print(f"First 16 bytes hex: {decoded[:16].hex()}")
            print(f"First 32 bytes repr: {decoded[:32]}")
            
            # Check if it's a .NET serialized object
            if decoded[:4] == b'\x00\x01\x00\x00':
                print("This is a .NET BinaryFormatter serialized object")
                # The format is: 0x00010000 + assembly info + type info + data
                # Usually after serialization header, there's the assembly name and type name
                try:
                    text_part = decoded.decode('utf-8', errors='replace')
                    print(f"\nText content: {text_part[:200]}")
                except:
                    pass
            # Check if it's raw ICO data
            if decoded[:4] == b'\x00\x00\x01\x00':
                print("This is RAW ICO data!")
                cnt = struct.unpack('<H', decoded[4:6])[0]
                print(f"ICO count: {cnt}")
        except Exception as e:
            print(f"Not base64: {e}")
            print(f"First 100 chars: {raw[:100]}")

# Also examine the raw value content
for data in root.findall('.//data'):
    name = data.get('name', '')
    value = data.find('value')
    if value is not None and value.text:
        txt = value.text.strip()
        print(f"\n--- {name} ---")
        print(f"First 80 chars: {txt[:80]}")
        print(f"Last 20 chars: {txt[-20:]}")
        print(f"Total chars: {len(txt)}")
    break