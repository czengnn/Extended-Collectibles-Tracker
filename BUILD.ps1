<#
.SYNOPSIS
Build the mod and install it into the game, either labelled for development or ready to publish.

.DESCRIPTION
  .\BUILD.ps1              Build only.

  .\BUILD.ps1 -Deploy      Build and install into the game's mods folder with the mod id and
                           name suffixed, so this copy is distinguishable from the Steam
                           Workshop one. Without the suffix both share a BepInEx GUID and one
                           is silently skipped, leaving it ambiguous which build is running.

  .\BUILD.ps1 -Publish     Build and install the same files, but with the real id and name
                           from mod/modinfo.json, so the folder is ready to upload from the
                           game (Remix menu -> the mod's info box -> upload arrow).

Publishing uploads the mod folder itself and takes its metadata from the modinfo.json inside
it, so the folder must carry the real id at upload time - hence the two modes. Run -Deploy
again afterwards to go back to a clearly-labelled development build.

mod/modinfo.json is never modified; the suffix exists only in the installed copy.
#>
param(
	[switch]$Deploy,
	[switch]$Publish,
	[string]$GameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Rain World',
	[string]$FolderName = 'extendedcollectiblestracker_fixed'
)

$ErrorActionPreference = 'Stop'
$repo = $PSScriptRoot
$modSource = Join-Path $repo 'mod'
$modInfoPath = Join-Path $modSource 'modinfo.json'
$target = Join-Path $GameDir "RainWorld_Data\StreamingAssets\mods\$FolderName"

if ($Deploy -and $Publish) { throw 'pick either -Deploy or -Publish, not both' }

dotnet build
if ($LASTEXITCODE -ne 0) { throw 'build failed' }
if (-not ($Deploy -or $Publish)) { return }

if (Get-Process -Name RainWorld -ErrorAction SilentlyContinue) {
	throw 'Rain World is running and holds the plugin dll - close the game first'
}

$modInfo = Get-Content -Raw $modInfoPath | ConvertFrom-Json

# The version lives in three places and has drifted apart before. A version that does not
# change is also how a Workshop update silently looks like a no-op on other devices.
if ($Publish) {
	$pluginVersion = (Select-String -Path (Join-Path $repo 'src\Plugin.cs') -Pattern 'VERSION\s*=\s*"([^"]+)"').Matches[0].Groups[1].Value
	$csprojVersion = (Select-String -Path (Join-Path $repo 'ExtendedCollectiblesTracker.csproj') -Pattern '<Version>([^<]+)</Version>').Matches[0].Groups[1].Value
	if ($modInfo.version -ne $pluginVersion -or $modInfo.version -ne $csprojVersion) {
		throw "version mismatch - modinfo.json=$($modInfo.version) Plugin.VERSION=$pluginVersion csproj=$csprojVersion"
	}
}

# Copy files in without clearing the folder: workshopdata.json is written there by the game
# when the mod is first published and holds the WorkshopID. Delete it and the next upload
# creates a new Workshop item instead of updating the existing one.
New-Item -ItemType Directory -Force -Path (Join-Path $target 'plugins') | Out-Null
Copy-Item (Join-Path $modSource 'atlases') $target -Recurse -Force
Copy-Item (Join-Path $modSource 'thumbnail.png') $target -Force
Get-ChildItem (Join-Path $modSource 'plugins') -Filter *.dll |
	Copy-Item -Destination (Join-Path $target 'plugins') -Force

$info = Get-Content -Raw $modInfoPath | ConvertFrom-Json
if ($Deploy) {
	$info.id = "$($modInfo.id).dev"
	$info.name = "$($modInfo.name) DEV"
}

# Write without a BOM: Set-Content -Encoding utf8 emits one on Windows PowerShell, and the
# game's json parsing does not expect it.
[System.IO.File]::WriteAllText(
	(Join-Path $target 'modinfo.json'),
	($info | ConvertTo-Json -Depth 5),
	(New-Object System.Text.UTF8Encoding($false)))

$published = Test-Path (Join-Path $target 'workshopdata.json')

Write-Output ''
Write-Output "installed -> $target"
Write-Output "  id      $($info.id)"
Write-Output "  name    $($info.name)"
Write-Output "  version $($info.version)"

if ($Deploy) {
	Write-Output ''
	Write-Output 'Development build. Enable this one and disable the Workshop copy in Remix.'
	if ($published) {
		Write-Output 'NOTE: this folder is linked to a Workshop item. Do not upload while it is'
		Write-Output '      labelled DEV - run -Publish first.'
	}
} else {
	Write-Output ''
	Write-Output 'Ready to publish. Launch the game, open Remix, select this mod and use the'
	Write-Output 'upload arrow in its info box.'
	if (-not $published) {
		Write-Output 'NOTE: no workshopdata.json here yet, so this uploads as a NEW Workshop item.'
		Write-Output '      To update an existing one, publish from the folder holding its'
		Write-Output '      workshopdata.json instead.'
	}
	Write-Output 'Afterwards run -Deploy to relabel this copy as DEV.'
}
