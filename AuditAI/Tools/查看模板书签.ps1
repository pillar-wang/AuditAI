# View all bookmarks in template database Paragraph.Stream
param(
    [string]$DbPath = "e:\lq\AuditAI\AuditAI\Data\Templates\X004模板.db"
)

# Load SQLite assembly
$sqliteDll = "e:\lq\AuditAI\Libs\System.Data.SQLite.dll"
$interopX64 = "e:\lq\AuditAI\x64\SQLite.Interop.dll"
$interopX86 = "e:\lq\AuditAI\x86\SQLite.Interop.dll"