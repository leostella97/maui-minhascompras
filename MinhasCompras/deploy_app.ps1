$ErrorActionPreference = 'Stop'
$base = 'C:\Users\MarBrasil\source\repos\maui-minhascompras\MinhasCompras\bin\Debug\net8.0-windows10.0.19041.0\win10-x64'
$appx = Join-Path $base 'AppX'

# Fecha o app se estiver aberto (libera o dll pra poder copiar)
$proc = Get-Process -Name MinhasCompras -ErrorAction SilentlyContinue
if ($proc) {
    $proc | Stop-Process -Force
    Write-Output "App fechado"
    Start-Sleep -Seconds 2
}

# Remove o pacote antigo antes de copiar (libera os arquivos do AppX)
$existing = Get-AppxPackage -Name com.companyname.minhascompras -ErrorAction SilentlyContinue
if ($existing) {
    Remove-AppxPackage -Package $existing.PackageFullName
    Write-Output "Pacote antigo removido"
    Start-Sleep -Seconds 1
}

# Copia o dll recem compilado para a pasta de layout do AppX
Copy-Item -Path (Join-Path $base 'MinhasCompras.dll') -Destination (Join-Path $appx 'MinhasCompras.dll') -Force
Copy-Item -Path (Join-Path $base 'MinhasCompras.pdb') -Destination (Join-Path $appx 'MinhasCompras.pdb') -Force -ErrorAction SilentlyContinue
Write-Output "DLL copiado para AppX"

# registra o pacote a partir do layout (AppxManifest) sem precisar de assinatura
$manifest = Join-Path $appx 'AppxManifest.xml'
Add-AppxPackage -Path $manifest -Register
Write-Output "Pacote registrado via layout"

$pkg = Get-AppxPackage -Name com.companyname.minhascompras
$manifestObj = Get-AppxPackageManifest $pkg
$appId = $manifestObj.Package.Applications.Application.Id
$aumid = "$($pkg.PackageFamilyName)!$appId"
Write-Output "AUMID: $aumid"

# Limpa log anterior para diagnostico limpo
$logPath = Join-Path $env:LOCALAPPDATA 'minhascompras.log'
if (Test-Path $logPath) { Remove-Item $logPath -Force }

# Lanca o aplicativo via AUMID
Start-Process 'explorer.exe' -ArgumentList "shell:AppsFolder\$aumid"
Write-Output "App lancado"
