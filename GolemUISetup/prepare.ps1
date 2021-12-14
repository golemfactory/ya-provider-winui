
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

function Extract-Zip {
	[CmdletBinding()]
	param (
		[Parameter(Mandatory)]
		[string]$BundleName,
		[Parameter(Mandatory)]
		[string]$Path		
	)
	$ExtractShell = New-Object -ComObject Shell.Application 

	$ExtractFiles = $ExtractShell.Namespace($(Resolve-Path $Path).Path).Items()
	New-Item -Path ".\bundling" -Name $BundleName -Force -ItemType "directory"
	$OutputDir=$(Resolve-Path ".\bundling\$BundleName").Path
	$ExtractShell.NameSpace($OutputDir).CopyHere($ExtractFiles)
}

Extract-Package -BundleName "wasi" -Url "https://github.com/golemfactory/ya-runtime-wasi/releases/download/v0.3.0/ya-runtime-wasi-windows-v0.3.0.zip"
Extract-Zip -BundleName "gminer" -Path ".\ExternalBinaries\ya-runtime-gminer-windows-v0.3.1.zip"

#Extract-Package -BundleName "gminer" -Url "https://github.com/golemfactory/ya-runtime-gminer/releases/download/v0.1.4/ya-runtime-gminer-windows-v0.1.4.zip"

