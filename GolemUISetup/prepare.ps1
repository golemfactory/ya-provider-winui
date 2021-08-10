
function Extract-Package {
	[CmdletBinding()]
	param (
		[Parameter(Mandatory)]
		[string]$BundleName,
		[Parameter(Mandatory)]
		[string]$Url		
	)
		
	$DownloadZipFile = $(Split-Path -Path $Url -Leaf)
	Invoke-WebRequest -Uri $Url -OutFile $DownloadZipFile

	$ExtractShell = New-Object -ComObject Shell.Application 

	$ExtractFiles = $ExtractShell.Namespace($(Resolve-Path $DownloadZipFile).Path).Items()
	New-Item -Path ".\bundling" -Name $BundleName -Force -ItemType "directory"
	$OutputDir=$(Resolve-Path ".\bundling\$BundleName").Path
	$ExtractShell.NameSpace($OutputDir).CopyHere($ExtractFiles)
}
Extract-Package -BundleName "wasi" -Url "https://github.com/golemfactory/ya-runtime-wasi/releases/download/v0.3.0/ya-runtime-wasi-windows-v0.3.0.zip"
Extract-Package -BundleName "gminer" -Url "https://github.com/golemfactory/ya-runtime-gminer/releases/download/v0.2.0/ya-runtime-gminer-windows-v0.2.0.zip"

