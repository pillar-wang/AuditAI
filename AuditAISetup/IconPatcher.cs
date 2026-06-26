﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.IO;
using System.Runtime.InteropServices;

class IconPatcher
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr BeginUpdateResource(string fileName, bool deleteExistingResources);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, byte[] lpData, uint cbData);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool EndUpdateResource(IntPtr hUpdate, bool discard);

    static IntPtr MAKEINTRESOURCE(int id) { return (IntPtr)id; }

    static void Main(string[] args)
    {
        if (args.Length != 2) { Console.WriteLine("Usage: IconPatcher.exe <exePath> <icoPath>"); return; }
        string exePath = args[0];
        string icoPath = args[1];
        if (!File.Exists(exePath)) { Console.WriteLine("EXE not found: " + exePath); return; }
        if (!File.Exists(icoPath)) { Console.WriteLine("ICO not found: " + icoPath); return; }

        byte[] ico = File.ReadAllBytes(icoPath);
        int count = BitConverter.ToUInt16(ico, 4);
        
        // Extract icon image data
        byte[][] images = new byte[count][];
        for (int i = 0; i < count; i++)
        {
            int pos = 6 + i * 16;
            int size = BitConverter.ToInt32(ico, pos + 8);
            int offset = BitConverter.ToInt32(ico, pos + 12);
            images[i] = new byte[size];
            Buffer.BlockCopy(ico, offset, images[i], 0, size);
        }

        // Build RT_GROUP_ICON data
        byte[] groupData = new byte[6 + count * 16];
        BitConverter.GetBytes((ushort)0).CopyTo(groupData, 0);          // wReserved
        BitConverter.GetBytes((ushort)1).CopyTo(groupData, 2);          // wType (icon)
        BitConverter.GetBytes((ushort)count).CopyTo(groupData, 4);      // wCount
        
        for (int i = 0; i < count; i++)
        {
            int pos = 6 + i * 16;
            int gp = 6 + i * 16;
            groupData[gp] = ico[pos];                       // bWidth
            groupData[gp + 1] = ico[pos + 1];               // bHeight
            groupData[gp + 2] = ico[pos + 2];               // bColorCount
            groupData[gp + 3] = 0;                          // bReserved
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)1), 0, groupData, gp + 4, 2);  // wPlanes
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)32), 0, groupData, gp + 6, 2); // wBitCount
            BitConverter.GetBytes(images[i].Length).CopyTo(groupData, gp + 8);            // dwBytesInRes
            BitConverter.GetBytes(i + 1).CopyTo(groupData, gp + 12);                      // dwID (icon resource ID)
        }

        // Make backup
        File.Copy(exePath, exePath + ".bak", true);
        
        try
        {
            IntPtr hUpdate = BeginUpdateResource(exePath, false);
            if (hUpdate == IntPtr.Zero)
                throw new Exception("BeginUpdateResource failed: " + Marshal.GetLastWin32Error());

            try
            {
                IntPtr RT_ICON = MAKEINTRESOURCE(3);
                IntPtr RT_GROUP_ICON = MAKEINTRESOURCE(14);
                IntPtr MAIN_ICON = MAKEINTRESOURCE(32512);

                // Delete old group icon (if exists)
                UpdateResource(hUpdate, RT_GROUP_ICON, MAIN_ICON, 0, null, 0);
                
                // Delete old individual icons (IDs 1-10)
                for (int i = 1; i <= 10; i++)
                    UpdateResource(hUpdate, RT_ICON, MAKEINTRESOURCE(i), 0, null, 0);

                // Add new icon images as RT_ICON
                for (int i = 0; i < count; i++)
                {
                    bool ok = UpdateResource(hUpdate, RT_ICON, MAKEINTRESOURCE(i + 1), 0, images[i], (uint)images[i].Length);
                    if (!ok) Console.WriteLine("  WARN: Failed to add icon " + (i + 1) + " error=" + Marshal.GetLastWin32Error());
                }

                // Add new group icon
                bool ok2 = UpdateResource(hUpdate, RT_GROUP_ICON, MAIN_ICON, 0, groupData, (uint)groupData.Length);
                if (!ok2) throw new Exception("Failed to add GROUP_ICON: " + Marshal.GetLastWin32Error());

                bool saved = EndUpdateResource(hUpdate, false);
                if (!saved) throw new Exception("EndUpdateResource failed: " + Marshal.GetLastWin32Error());

                Console.WriteLine("OK: " + Path.GetFileName(exePath));
                File.Delete(exePath + ".bak");
            }
            catch
            {
                EndUpdateResource(hUpdate, true);
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("FAILED: " + exePath + " - " + ex.Message);
            File.Copy(exePath + ".bak", exePath, true);
            File.Delete(exePath + ".bak");
        }
    }
}
