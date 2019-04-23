<#
.SYNOPSIS
    Adds a 3D Launch Tile to a Visual Studio project.
.DESCRIPTION
    Includes various sanity checks.
.PARAMETER Project
    Which Visual Studio project should be patched?
.PARAMETER Model
    Which .glb to use?
.PARAMETER Force
    Overwrite existing files, risking breaking changes?
.EXAMPLE
    .\add_3d_launcher.ps1 -Project Example.vcproj -Model cube.glb
.NOTES
    Work in progress. Not extensively tested.
    > $DebugPreference = "Continue"
.LINK
    :TODO:
#>
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingWriteHost", "", Scope="Function")]
Param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
    [ValidateScript({ (Get-Item $_ | select -Expand Extension) -eq ".vcxproj" })]
    [string] $Project,
    [Parameter(Mandatory=$true, Position=1)]
    [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
    [ValidateScript({ (Get-Item $_ | select -Expand Extension) -eq ".glb" })]
    [string] $Model,
    [Parameter(Mandatory=$false)]
    [switch] $Force
)

function Get-AppxManifestFromProject
{
    Param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
        [ValidateScript({ (Get-Item $_ | select -Expand Extension) -eq ".vcxproj" })]
        [string] $Project
    )

    Write-Debug "Getting ApxxManifest from $Project"

    [xml]$xml = Get-Content $Project
    $Namespace = @{msbuildns = "http://schemas.microsoft.com/developer/msbuild/2003"}
    $manifest = $Null
    $manifest = Select-Xml -Namespace $Namespace -XPath '/msbuildns:Project/msbuildns:ItemGroup/msbuildns:AppxManifest' -Xml $xml | % {
        $relative = $_.Node.GetAttribute( "Include" )
        $base = [io.path]::GetDirectoryName($Project)
        $absolute = $base + "\" + $relative

        return $absolute
    } | Select -First 1
    return $manifest
}

function Ensure-NoneItemDeploymentInProject
{
    Param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
        [ValidateScript({ (Get-Item $_ | select -Expand Extension) -eq ".vcxproj" })]
        [string] $Project,
        [Parameter(Mandatory=$true, Position=1)]
        [string] $Item
    )

    Write-Debug "Getting Item $Item from $Project"

    $itemNode = $Null
    [xml]$xml = Get-Content $Project
    $Namespace = @{msbuildns = "http://schemas.microsoft.com/developer/msbuild/2003"}
    $itemNode = Select-Xml -Namespace $Namespace -XPath "/msbuildns:Project/msbuildns:ItemGroup/msbuildns:None[@Include='$Item']" -Xml $xml | % {
        Write-Debug "Found $Item"
        Write-Debug $_.Node.OuterXml
        return $_.Node
    } | Select -First 1

    if( -Not $itemNode ) {
        # just create it
        Write-Debug "Creating item: $Item"
        $otherNode = Select-Xml -Namespace $Namespace -XPath "/msbuildns:Project/msbuildns:ItemGroup/msbuildns:None" -Xml $xml | % {
            Write-Debug $_.Node.OuterXml
            return $_.Node
        } | Select -First 1

        $parent = $otherNode.ParentNode
        $itemNode = $parent.OwnerDocument.CreateElement("None", "http://schemas.microsoft.com/developer/msbuild/2003")
        $itemNode.SetAttribute( "Include", $Item )
        $parent.AppendChild( $itemNode )
    } else {
        Write-Debug "Skipping existing item: $Item"
    }

    if( -Not $itemNode ) {
        Write-Host "Item for $Item not found, and failed creating!" -ForegroundColor Red
        return
    }
    $deploymentContent = $itemNode["DeploymentContent"]

    Write-Debug "Old DeploymentContent $deploymentContent"
    if( -Not $deploymentContent ) {
        $deploymentContent = $itemNode.OwnerDocument.CreateElement("DeploymentContent", "http://schemas.microsoft.com/developer/msbuild/2003")
        $itemNode.AppendChild( $deploymentContent )
    }

    $deploymentContent.InnerText = "true"
    $xml.Save( ( Resolve-Path $Project ) )
}


function Get-AssetsDirectoryFromProject
{
    Param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
        [ValidateScript({ (Get-Item $_ | select -Expand Extension) -eq ".vcxproj" })]
        [string] $Project
    )

    $base = [io.path]::GetDirectoryName($Project)
    $relative = "Assets"
    $assets = "$base\$relative"

    # :TODO: validate

    return $assets, $relative
}

function Add-ModelToProject
{
    Param
    (
         [Parameter(Mandatory=$true, Position=0)]
         [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
         [ValidateScript({ (Get-Item $_ | select -Expand Extension) -eq ".vcxproj" })]
         [string] $Project,
         [Parameter(Mandatory=$true, Position=1)]
         [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
         [ValidateScript({ (Get-Item $_ | select -Expand Extension) -eq ".glb" })]
         [string] $Model,
         [Parameter(Mandatory=$false)]
         [boolean] $Force
    )

    $name = [io.path]::GetFileName($Model)
    $assets, $relative = Get-AssetsDirectoryFromProject -Project $Project
    $target = $assets + "\" + $name
    $item = "$relative\$name"

    if( Test-Path $target ) {
        Write-Debug "Target $target already exists"
        if( -Not $Force ) {
            Write-Host "Warning $target already exists, and Force not set" -ForegroundColor Yellow
            return;
        }
    }

    Write-Debug "Copying $Model to $target"
    Copy-Item $Model -Destination $target
    Write-Debug "Returning '$item'"
    $rc = Ensure-NoneItemDeploymentInProject -Project $Project -Item $item

    Write-Debug "Returning '$item'"
    return $item
}

function Add-MixedRealityModelToManifest
{
    Param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
        [ValidateScript({ (Get-Item $_ | select -Expand Extension) -eq ".appxmanifest" })]
        [string] $Manifest,
        [Parameter(Mandatory=$true, Position=1)]
        [string] $Item
    )

    [xml]$xml = Get-Content $Manifest

    $xml["Package"].SetAttribute("xmlns:uap5", "http://schemas.microsoft.com/appx/manifest/uap/windows10/5")
    $ignorableNamespaces = $xml.Package.GetAttribute("IgnorableNamespaces")
    if ( $ignorableNamespaces.IndexOf("uap5") -eq -1 ) {
        $ignorableNamespaces = $ignorableNamespaces + " uap5"
        $xml.Package.SetAttribute("IgnorableNamespaces", $ignorableNamespaces)
    }

    ForEach( $application in $xml.Package.Applications.ChildNodes ) {
        $visualElements = $application["uap:VisualElements"]
        $defaultTile = $visualElements["uap:DefaultTile"]
        $mixedRealityModel = $defaultTile["uap5:MixedRealityModel"]
        if (-Not $mixedRealityModel) {
            Write-Host "Adding 3DTile" -ForegroundColor Green
            # Note: Explicitly setting the correct XML namespace here, actually omits it from the XML which Visual Studio wants
            $mixedRealityModel = $defaultTile.OwnerDocument.CreateElement("uap5:MixedRealityModel", "http://schemas.microsoft.com/appx/manifest/uap/windows10/5")
            
            $mixedRealityModel.SetAttribute("Path",$Item)
            Write-Host $mixedRealityModel.OuterXml
            $defaultTile.AppendChild($mixedRealityModel)

        }
    }


    $xml.Save( ( Resolve-Path $Manifest ) )
}
function Add-Project3dLauncher
{
    Param
    (
         [Parameter(Mandatory=$true, Position=0)]
         [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
         [ValidateScript({ (Get-Item $_ | select -Expand Extension) -eq ".vcxproj" })]
         [string] $Project,
         [Parameter(Mandatory=$true, Position=1)]
         [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
         [ValidateScript({ (Get-Item $_ | select -Expand Extension) -eq ".glb" })]
         [string] $Model,
         [Parameter(Mandatory=$false)]
         [boolean] $Force
    )

    $appxManifest = Get-AppxManifestFromProject $Project

    if( -Not $appxManifest ) {
        Write-Host "Failed finding manifest in project $Project" -ForegroundColor Red
        return
    }
    $t = $appxManifest.GetType().FullName
    Write-Debug "appxManifest: $appxManifest $t"

    $item = Add-ModelToProject -Project $Project -Model $Model -Force $Force

    if( $item ) {
        Write-Host "Added Model to project as '$item'" -ForegroundColor Green
        Add-MixedRealityModelToManifest -Manifest $appxManifest -Item $item
        Write-Host "Added Model to manifest '$appxManifest'" -ForegroundColor Green
    } else {
        Write-Host "Warning: Model not added to project" -ForegroundColor Yellow
    }
}

function Main
{
    [CmdletBinding()]
    Param(
        $Parameters
    )

    Add-Project3dLauncher -Project $Project -Model $Model -Force $Force
}

Main -Parameters $PSBoundParameters
