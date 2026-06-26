import pefile
pe = pefile.PE(r'e:\lq\AuditAI\AuditAILauncher\bin\x86\Release\net40\AuditAILauncher.exe')
res = pe.DIRECTORY_ENTRY_RESOURCE
for e in res.entries:
    print(f'Type: {e.id} ({hex(e.id)})')
    if e.directory:
        for de in e.directory.entries:
            name = f'Name/id: {de.id}'
            if de.directory:
                for le in de.directory.entries:
                    sz = getattr(le.data, 'size', 0) if le.data else 0
                    print(f'  {name} Lang: {le.id} Size: {sz}')
