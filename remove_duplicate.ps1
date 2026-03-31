$filePath = "e:\COTHUY\H-Th-ng-o-T-o\Controllers\AdminController.cs"
$lines = Get-Content $filePath
$newLines = @()
for ($i = 0; $i -lt $lines.Length; $i++) {
    $ln = $i + 1
    if ($ln -ge 1133 -and $ln -le 1177) {
        continue
    }
    $newLines += $lines[$i]
}
$newLines | Set-Content $filePath
