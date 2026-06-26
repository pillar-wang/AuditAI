using System;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
class Verify{static void Main(){
 var newIco = new Icon(@"e:\lq\AuditAI\AuditAISetup\app.ico", 32, 32);
 var bmp3 = newIco.ToBitmap();
 string[] exes = {
  @"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe",
  @"e:\lq\AuditAI\AuditAILauncher\bin\x86\Release\net40\AuditAILauncher.exe"
 };
 foreach(var exe in exes){
  var exeIcon = Icon.ExtractAssociatedIcon(exe);
  var bmp = exeIcon.ToBitmap();
  bool same = true;
  for(int x=0;x<32;x++) for(int y=0;y<32;y++){
   var p1=bmp.GetPixel(x,y);var p3=bmp3.GetPixel(x,y);
   if(p1.R!=p3.R||p1.G!=p3.G||p1.B!=p3.B){same=false;x=y=999;}
  }
  Console.WriteLine(Path.GetFileName(exe)+": "+(same?"NEW":"OLD"));
  bmp.Dispose();exeIcon.Dispose();
 }
 bmp3.Dispose();newIco.Dispose();
}}
