"""List all resource entries from the resx file"""
import xml.etree.ElementTree as ET

with open(r'e:\lq\AuditAI\PlatformResource\Auditai.PlatformResource.Properties.Resource.resx', 'r', encoding='utf-8') as f:
    content = f.read()

root = ET.fromstring(content)

print("=== ALL RESOURCE NAMES ===")
for data in root.findall('.//data'):
    name = data.get('name', '')
    type_attr = data.get('type', '')
    mimetype = data.get('mimetype', '')
    value = data.find('value')
    sz = len(value.text) if value is not None and value.text else 0
    
    is_icon = 'Icon' in type_attr or '.ico' in name or 'Icon' in type_attr
    prefix = "[ICON]" if is_icon else "       "
    t = type_attr[:60] if type_attr else "-"
    print(f'{prefix} name="{name}" type={t} size={sz}')