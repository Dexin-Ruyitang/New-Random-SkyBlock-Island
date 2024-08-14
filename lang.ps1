$assetspath = "D:\My_Software\PCL2\.minecraft\assets\"
$indexpath = $assetspath + "indexes\"
$objectspath = $assetspath + "objects\"
$indices = Get-ChildItem -Path $indexpath
foreach ($i in $indices) {
    $jsondata = Get-Content -Raw -Path $i.FullName | ConvertFrom-Json
    $hash = $jsondata.objects.'minecraft/lang/zh_cn.lang'.hash
    if ($hash.Length -gt 0) {
        $p = $objectspath+$hash[0]+$hash[1]+"\"+$hash
        $d = ".\temp\lang_"+$i.Name
        Copy-Item -Path $p -Destination $d
    }
    $hash = $jsondata.objects.'minecraft/lang/zh_cn.json'.hash
    if ($hash.Length -gt 0) {
        $p = $objectspath+$hash[0]+$hash[1]+"\"+$hash
        $d = ".\temp\lang_"+$i.Name
        Copy-Item -Path $p -Destination $d
    }
}