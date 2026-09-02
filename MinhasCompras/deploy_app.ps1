$ErrorActionPreference = 'Stop'
# Script de deploy local para app MAUI Windows empacotado (AppX)
# Build self-contained para embutir o .NET runtime no AppX e evitar
# o dialogo "pede para instalar .NET 8" quando o app e lancado via AUMID.
$base = 'C:\Users\MarBrasil\source\repos\maui-minhascompras\MinhasCompras\bin\Debug\net8.0-windows10.0.19041.0\win10-x64'
$appx = Join-Path $base 'AppX'

# Build self-contained para gerar o .NET runtime junto com o app
Write-Output "Compilando (self-contained)..."
dotnet build "C:\Users\MarBrasil\source\repos\maui-minhascompras\MinhasCompras\MinhasCompras.csproj" -f net8.0-windows10.0.19041.0 -c Debug -p:SelfContained=true -p:RuntimeIdentifier=win10-x64
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build falhou com codigo $LASTEXITCODE"
    exit 1
}

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

# Copia todos os arquivos do build self-contained para a pasta AppX
# O build MAUI gera a pasta AppX sem os DLLs do .NET runtime (coreclr.dll, System.*, etc.)
# e com runtimeconfig.json framework-dependent. Precisamos sobrescrever com os arquivos
# self-contained para que o app empacotado encontre o runtime.
Write-Output "Sincronizando arquivos self-contained com AppX..."
$baseFiles = Get-ChildItem $base -File
foreach ($f in $baseFiles) {
    # Pula arquivos de build que nao pertencem ao AppX
    if ($f.Name -eq 'MinhasCompras.build.appxrecipe') { continue }
    Copy-Item -Path $f.FullName -Destination (Join-Path $appx $f.Name) -Force
}
Write-Output "Arquivos sincronizados com AppX"

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

# Verifica se o app iniciou com sucesso
Start-Sleep -Seconds 5
$proc = Get-Process -Name MinhasCompras -ErrorAction SilentlyContinue
if ($proc) {
    Write-Output "SUCESSO - App rodando! PID: $($proc.Id)"
} else {
    Write-Output "ATENCAO - App nao detectado apos lancamento. Verifique o Event Viewer."
}
