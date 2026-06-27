﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;

string icoPath = @"e:\lq\AuditAI\AuditAISetup\app.ico";

// Load the new icon
Icon newIcon = new Icon(icoPath);

// Create 32x32 bitmap from the icon
Bitmap bmp32 = newIcon.ToBitmap();

// Serialize Icon via BinaryFormatter
byte[] serializedIcon;
using (MemoryStream ms = new MemoryStream())
{
    new BinaryFormatter().Serialize(ms, newIcon);
    serializedIcon = ms.ToArray();
}
string base64Icon = Convert.ToBase64String(serializedIcon, Base64FormattingOptions.InsertLineBreaks);

// Serialize Bitmap (32x32) via BinaryFormatter
byte[] serializedBmp;
using (MemoryStream ms = new MemoryStream())
{
    new BinaryFormatter().Serialize(ms, bmp32);
    serializedBmp = ms.ToArray();
}
string base64Bmp = Convert.ToBase64String(serializedBmp, Base64FormattingOptions.InsertLineBreaks);

Console.WriteLine($"Serialized Icon: {serializedIcon.Length} bytes → {base64Icon.Length} chars");
Console.WriteLine($"Serialized Bitmap (32x32): {serializedBmp.Length} bytes → {base64Bmp.Length} chars");

// ========== Replace Auditai.UI.Platform.Properties.Resources.resx ==========
string resxPath = @"e:\lq\AuditAI\AuditAI\Auditai.UI.Platform.Properties.Resources.resx";
File.Copy(resxPath, resxPath + ".bak", true);

XDocument doc = XDocument.Load(resxPath);
XNamespace ns = doc.Root!.GetDefaultNamespace();

// List of icon entries to replace (Bitmap types)
string[] bmpKeys = [
    "frmLoginIcon_Audit", "frmLoginIcon_Manager", "frmLoginIcon_Report", "frmLoginIcon_Table",
    "frmRegisterIcon_Audit", "frmRegisterIcon_Manager", "frmRegisterIcon_Report", "frmRegisterIcon_Table",
    "LoginIcon", "iconSample"
];

// For Bitmap entries, we use base64Bmp; for Icon entries, we use base64Icon
int replacedBmp = 0;
int replacedIcon = 0;

foreach (var data in doc.Descendants(ns + "data"))
{
    string name = (string)data.Attribute("name")!;
    var valEl = data.Element(ns + "value");
    if (valEl == null) continue;

    if (bmpKeys.Contains(name))
    {
        valEl.Value = "\n" + base64Bmp + "\n";
        replacedBmp++;
        Console.WriteLine($"  [Bitmap] Replaced: {name}");
    }
    else if (name == "icon")
    {
        valEl.Value = "\n" + base64Icon + "\n";
        replacedIcon++;
        Console.WriteLine($"  [Icon] Replaced: {name}");
    }
}

doc.Save(resxPath);
Console.WriteLine($"\nDone! Replaced {replacedBmp} Bitmap icons and {replacedIcon} Icon entries.");
Console.WriteLine($"Saved to: {resxPath}");

newIcon.Dispose();
bmp32.Dispose();