using System;
using System.Drawing;
using System.IO;
class Check{
 static void Main(){
  var newIco = new Icon(@"e:\lq\AuditAI\AuditAISetup\app.ico", 32, 32);
  var bmpNew = newIco.ToBitmap();
  string[] exes = {
   @"D:\Program Files (x86)\AuditAI\AuditAI.exe",
   @"D:\Program Files (x86)\AuditAI\AuditAILauncher.exe",
   @"e:\lq\AuditAI\AuditAI\bin\Release\net462\AuditAI.exe",
   @"e:\lq\AuditAI\AuditAILauncher\bin\x86\Release\net40\AuditAILauncher.exe"
  };
  foreach(var exe in exes){
   if(!File.Exists(exe)){Console.WriteLine(Path.GetFileName(exe)+": NOT FOUND"); continue;}
   try{
    var exeIcon = Icon.ExtractAssociatedIcon(exe);
    var bmpExe = exeIcon.ToBitmap();
    int diff = 0;
    for(int y=0;y<32;y++) for(int x=0;x<32;x++){
     if(bmpExe.GetPixel(x,y)!=bmpNew.GetPixel(x,y)) diff++;
    }
    Console.WriteLine(Path.GetFileName(exe)+": diff="+diff+(diff<100?" MATCH":" OLD"));
   }catch(Exception ex){Console.WriteLine(Path.GetFileName(exe)+": "+ex.Message.Substring(0,50));}
  }
  bmpNew.Dispose(); newIco.Dispose();
 }
}
