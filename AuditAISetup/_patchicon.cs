using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
class Program{
    [DllImport("kernel32.dll", SetLastError=true)] static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);
    [DllImport("kernel32.dll", SetLastError=true)] static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, byte[] lpData, uint cbData);
    [DllImport("kernel32.dll", SetLastError=true)] static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);
    static void Main(){
        string exePath = @"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe";
        string launcherPath = @"e:\lq\AuditAI\AuditAILauncher\bin\x86\Release\net40\AuditAILauncher.exe";
        string icoPath = @"e:\lq\AuditAI\AuditAISetup\app.ico";
        byte[] icoBytes = File.ReadAllBytes(icoPath);
        IntPtr hUpdate = BeginUpdateResource(exePath, false);
        IntPtr RT_ICON = new IntPtr(3);
        IntPtr RT_GROUP_ICON = new IntPtr(14);
        // Read ICO header to get resource count
        BinaryReader br = new BinaryMemory(icoBytes);
        short reserved = br.ReadInt16();
        short type = br.ReadInt16();
        short count = br.ReadInt16();
        // Build group icon resource
        byte[] groupData = new byte[6 + count * 16];
        BinaryWriter bw = new BinaryWriter(new MemoryStream(groupData));
        bw.Write(reserved);
        bw.Write(type);
        bw.Write(count);
        long offset = 6 + count * 16;
        for(int i=0;i<count;i++){
            byte w = icoBytes[offset];
            byte h = icoBytes[offset+1];
            byte colors = icoBytes[offset+2];
            byte reserved2 = icoBytes[offset+3];
            short planes = BitConverter.ToInt16(icoBytes, offset+4);
            short bits = BitConverter.ToInt16(icoBytes, offset+6);
            int dataSize = BitConverter.ToInt32(icoBytes, offset+8);
            int dataOffset = BitConverter.ToInt32(icoBytes, offset+12);
            bw.Write(w);
            bw.Write(h);
            bw.Write(colors);
            bw.Write(reserved2);
            bw.Write(planes);
            bw.Write(bits);
            bw.Write(dataSize);
            bw.Write(dataOffset);
            offset += 16;
        }
        bw.Flush();
        groupData = ((BinaryWriter)bw.BaseStream).ToArray();
        // Extract icon data chunks
        BinaryReader br2 = new BinaryReader(new MemoryStream(icoBytes));
        br2.ReadBytes(6);
        br2.ReadBytes(count * 16);
        var iconEntries = new List<byte[]>();
        for(int i=0;i<count;i++){
            byte w = br2.ReadByte(), h = br2.ReadByte(), colors = br2.ReadByte(), res = br2.ReadByte();
            short planes = br2.ReadInt16(), bits = br2.ReadInt16();
            int dataSize = br2.ReadInt32(), dataOffset = br2.ReadInt32();
            br2.BaseStream.Position = dataOffset;
            byte[] data = br2.ReadBytes(dataSize);
            iconEntries.Add(data);
        }
        // Update resources
        bool success = true;
        for(int i=0;i<count;i++){
            IntPtr name = new IntPtr(i+1);
            success &= UpdateResource(hUpdate, RT_ICON, name, 0, iconEntries[i], (uint)iconEntries[i].Length);
        }
        success &= UpdateResource(hUpdate, RT_GROUP_ICON, new IntPtr(1), 0, groupData, (uint)groupData.Length);
        EndUpdateResource(hUpdate, false);
        Console.WriteLine("Updated: "+exePath);
        // Also update launcher
        hUpdate = BeginUpdateResource(launcherPath, false);
        for(int i=0;i<count;i++){
            IntPtr name = new IntPtr(i+1);
            UpdateResource(hUpdate, RT_ICON, name, 0, iconEntries[i], (uint)iconEntries[i].Length);
        }
        UpdateResource(hUpdate, RT_GROUP_ICON, new IntPtr(1), 0, groupData, (uint)groupData.Length);
        EndUpdateResource(hUpdate, false);
        Console.WriteLine("Updated: "+launcherPath);
        Console.WriteLine("DONE!");
    }
}
class BinaryWriter : IDisposable{
    MemoryStream ms;
    public BinaryWriter(MemoryStream ms){this.ms=ms;}
    public void Write(short v){ms.Write(BitConverter.GetBytes(v),0,2);}
    public void Write(byte v){ms.WriteByte(v);}
    public void Write(int v){ms.Write(BitConverter.GetBytes(v),0,4);}
    public byte[] ToArray(){return ms.ToArray();}
    public void Flush(){ms.Flush();}
    public void Dispose(){ms.Dispose();}
}
