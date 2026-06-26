Add-Type -TypeDefinition @"
using System;
using System.IO;
using System.Runtime.InteropServices;

public class IconPatcher
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr BeginUpdateResource(string fileName, bool deleteExistingResources);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, byte[] lpData, uint cbData);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool EndUpdateResource(IntPtr hUpdate, bool discard);

    public static void Patch(string exePath, string icoPath)
    {
        byte[] ico = File.ReadAllBytes(icoPath);
        int count = BitConverter.ToUInt16(ico, 4);
        var chunks = new byte[count][];
        for (int i = 0; i < count; i++)
        {
            int pos = 6 + i * 16;
            int size = BitConverter.ToInt32(ico, pos + 8);
            int offset = BitConverter.ToInt32(ico, pos + 12);
            chunks[i] = new byte[size];
            Buffer.BlockCopy(ico, offset, chunks[i], 0, size);
        }
        byte[] grp = new byte[6 + count * 16];
        BitConverter.GetBytes((ushort)0).CopyTo(grp, 0);
        BitConverter.GetBytes((ushort)1).CopyTo(grp, 2);
        BitConverter.GetBytes((ushort)count).CopyTo(grp, 4);
        for (int i = 0; i < count; i++)
        {
            int pos = 6 + i * 16;
            int grpPos = 6 + i * 16;
            grp[grpPos] = ico[pos]; grp[grpPos+1] = ico[pos+1];
            grp[grpPos+2] = ico[pos+2]; grp[grpPos+3] = 0;
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)1), 0, grp, grpPos+4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)32), 0, grp, grpPos+6, 2);
            BitConverter.GetBytes(chunks[i].Length).CopyTo(grp, grpPos+8);
            BitConverter.GetBytes(i + 1).CopyTo(grp, grpPos+12);
        }
        IntPtr hUpdate = BeginUpdateResource(exePath, true);  // Delete ALL existing resources first
        if (hUpdate == IntPtr.Zero) throw new Exception("BeginUpdateResource: " + Marshal.GetLastWin32Error());
        try
        {
            // Remove the old group icon (already deleted by bDeleteExistingResources=true)
            IntPtr RT_ICON = new IntPtr(3);
            IntPtr RT_GROUP_ICON = new IntPtr(14);
            IntPtr MAIN_ICON = new IntPtr(32512);
            
            // Add new individual icon images with IDs 1..count
            for (int i = 0; i < count; i++)
            {
                bool ok = UpdateResource(hUpdate, RT_ICON, new IntPtr(i+1), 0, chunks[i], (uint)chunks[i].Length);
                if (!ok) Console.WriteLine("  WARN: RT_ICON " + (i+1) + ": " + Marshal.GetLastWin32Error());
            }
            // Add new group icon (references icon IDs 1..count)
            bool ok2 = UpdateResource(hUpdate, RT_GROUP_ICON, MAIN_ICON, 0, grp, (uint)grp.Length);
            if (!ok2) throw new Exception("RT_GROUP_ICON: " + Marshal.GetLastWin32Error());
            
            if (!EndUpdateResource(hUpdate, false))
                throw new Exception("EndUpdateResource: " + Marshal.GetLastWin32Error());
            
            Console.WriteLine("  OK: " + Path.GetFileName(exePath));
        }
        catch { EndUpdateResource(hUpdate, true); throw; }
    }
}
"@

Write-Host "Patching icons..."
[IconPatcher]::Patch("E:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe", "E:\lq\AuditAI\AuditAISetup\app.ico")
[IconPatcher]::Patch("E:\lq\AuditAI\AuditAILauncher\bin\x86\Release\net40\AuditAILauncher.exe", "E:\lq\AuditAI\AuditAISetup\app.ico")
Write-Host "DONE!"
