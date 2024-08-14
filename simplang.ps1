$langs = Get-ChildItem -Path ".\temp\"
foreach ($i in $langs) {
    $cont = Get-Content -Path $i.FullName
    $cont = $cont | Where-Object {
        $_.TrimStart().Contains('block.minecraft.') -and -not
        $_.TrimStart().Contains('block.minecraft.banner')
    }
    if ($null -ne $cont) {
        $cont = [regex]::Replace($cont,'\\u[0-9a-f]{4}',{[char][int]($args[0] -replace '\\u','0x')})
        $cont = '{'+$cont.TrimEnd(',')+'}' | ConvertFrom-Json
        $cont = $cont | ConvertTo-Json
    }
    $cont | Out-File $i.FullName
}