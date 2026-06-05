Get-ChildItem -Path "." -Recurse -File -Exclude '*.targets', '*.props', '*.ps1' | Where-Object {
    $include = $_.FullName -notmatch '(?x)
        examples\\.+\\src\\YourCompany(?:\.|\w)*Migrations |
        src\\YourCompany.*\\obj\\ |
        src\\YourCompany.*\\bin\\'
    if (-not $include) { return $false; }
    $bytes = Get-Content $_.FullName -Encoding Byte -TotalCount 3
    $bytes -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
} | Select-Object FullName
