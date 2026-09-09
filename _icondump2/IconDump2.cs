using System;
using System.Drawing;
using System.IO;
using Auditai.UI.Controls;
using Auditai.UI.Platform;

public class IconDump2
{
    [STAThread]
    public static void Main(string[] args)
    {
        string[] names = { "list", "users", "user", "key", "list-bullets", "magnifying-glass" };
        int size = 32;
        string dir = @"e:\lq\AuditAI\_icondump2";
        Directory.CreateDirectory(dir);
        foreach (string n in names)
        {
            try
            {
                Bitmap bmp = IconLibrary.CreateBitmap(n, size, Color.FromArgb(71, 85, 105), IconLibrary.StyleFill);
                string p = Path.Combine(dir, n + ".png");
                bmp.Save(p, System.Drawing.Imaging.ImageFormat.Png);
                int opaque = 0;
                for (int y = 0; y < bmp.Height; y++)
                    for (int x = 0; x < bmp.Width; x++)
                        if (bmp.GetPixel(x, y).A > 100) opaque++;
                Console.WriteLine(n + ": saved, opaque=" + opaque + "/" + (bmp.Width * bmp.Height) + " (" + (opaque * 100 / (bmp.Width * bmp.Height)) + "%)");
            }
            catch (Exception ex) { Console.WriteLine(n + ": ERROR " + ex.Message); }
        }
    }
}
