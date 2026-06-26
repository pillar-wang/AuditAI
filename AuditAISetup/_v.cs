using System;
using System.Drawing;
using System.IO;
class VerifyIcon{
static void Main(){
 var newIco = new Icon(@"e:\lq\AuditAI\AuditAISetup\app.ico", 32, 32);
 var bmpNew = newIco.ToBitmap();
 var exeIcon = Icon.ExtractAssociatedIcon(@"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe");
 var bmpExe = exeIcon.ToBitmap();
 int diff = 0;
 for(int y=0;y<32;y++) for(int x=0;x<32;x++){
  if(bmpExe.GetPixel(x,y)!=bmpNew.GetPixel(x,y)) diff++;
 }
 Console.WriteLine("Diff pixels at 32x32: "+diff);
 if(diff<100) Console.WriteLine("=> ICON MATCHES! New icon is embedded.");
 else Console.WriteLine("=> ICON IS DIFFERENT.");
 bmpNew.Dispose(); newIco.Dispose();
 bmpExe.Dispose(); exeIcon.Dispose();
}}
