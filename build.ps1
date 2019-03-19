<#
.SYNOPSIS
    Builds all the projects and gathers the artifacts.
.DESCRIPTION
    Supports revisions and multi config / platform builds
.PARAMETER Configuration
    What configurations should be built?
.PARAMETER Platform
    What platforms should be built?
.PARAMETER Revision
    What revision should the output be labeled with?
    If -1 is specified, the current revision is retrieved from the package.appxmanifest file and increased by one.
.PARAMETER NugetSourceNames
    What Nuget package sources you will provide authentication tokens for?
.PARAMETER NugetSourceAuthTokens
    What authentication tokens to use for package sources?
.PARAMETER IncludePackageSource
    Path to Package Source Location to include when staging Artifacts
.PARAMETER GitCredential
    The Git Credentials to use when snapshotting Package Source
.PARAMETER SkipUnityBuild
    If provided, the Unity Project build step is skipped
.PARAMETER SkipMSBuild
    If provided, the MSBuild build step is skipped
.PARAMETER SkipStageArtifacts
    If provided, staging of Artifacts (excluding Package Sources) is skipped
.PARAMETER UnityProjectBasePath
    Under what base pattern should we look for Unity projects? Defaults to '$PWD'.
.PARAMETER VsSolutionBasePath
    Under what base pattern should we look for VS Solutions? Defaults to '$PWD'.
.EXAMPLE
    .\build.ps1
.EXAMPLE
    .\build.ps1 -Configuration Master,Release,Debug -Platform x86,x64,ARM -Revision 1337
.EXAMPLE
    .\build.ps1 -IncludePackageSource .\
.NOTES
    If build.PreMSBuildSteps.ps1 is present alongide this script, the commands there will be executed prior to each invocation of Invoke-MSBuild
.LINK
    https://wwhs.visualstudio.com/SharedTech/_git/ProjectTemplateUnity
#>
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingWriteHost", "", Scope="Function")]
param(
    [Parameter(Mandatory=$False,ParameterSetName='Default')]
    [Parameter(Mandatory=$False,ParameterSetName='NugetSourceCredentials')]
    [string[]]$Configuration = 'Master',
    [Parameter(Mandatory=$False,ParameterSetName='Default')]
    [Parameter(Mandatory=$False,ParameterSetName='NugetSourceCredentials')]
    [string[]]$Platform = 'x86',
    [Parameter(Mandatory=$False,ParameterSetName='Default')]
    [Parameter(Mandatory=$False,ParameterSetName='NugetSourceCredentials')]
    [int]$Revision=0,
    [Parameter(Mandatory=$True,ParameterSetName='NugetSourceCredentials')]
    [String[]]$NugetSourceNames,
    [Parameter(Mandatory=$True,ParameterSetName='NugetSourceCredentials')]
    [SecureString[]]$NugetSourceAuthTokens,
    [ValidateNotNullOrEmpty()]
    [Parameter(Mandatory=$False)]
    [string]$IncludePackageSource,
    [Parameter(Mandatory=$False)]
    [pscredential]$GitCredential,
    [Parameter(Mandatory=$false)]
    [switch]$SkipUnityBuild,
    [Parameter(Mandatory=$false)]
    [Alias("SkipVisualStudioBuild")]
    [switch]$SkipMSBuild,
    [Parameter(Mandatory=$false)]
    [switch]$SkipStageArtifacts,
    [Parameter(Mandatory=$false)]
    [string]$UnityProjectBasePath = $PWD,
    [Parameter(Mandatory=$false)]
    [string]$VsSolutionBasePath = $PWD
)

function Import-RequiredModule
{
    [CmdletBinding()]
    param(
        [Object[]]$InternalModules
    )
    Import-Module UnitySetup -Scope Global

    foreach( $module in $InternalModules) {
        Import-Module @module -Scope Global
    }
}

function Edit-NugetConfigToken {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSPossibleIncorrectComparisonWithNull', "", Scope="Function")]
    [CmdletBinding()]
    param (
        [System.IO.FileInfo[]]$NugetConfigs,
        [String[]]$NugetSourceNames,
        [SecureString[]]$NugetSourceAuthTokens
    )

    $packageSourceCredentialsNodeString = "<packageSourceCredentials />"
    $packageSourceNameNodeFormatString = "<{0} />"
    $packageSourceTokenNodeFormatString = "<add key=""clearTextPassword"" value=""{0}"" />"

    $NugetConfigs | ForEach-Object {
        Write-Host ("  Injecting Nuget Source Auth Tokens to configuration file " + $_.FullName)

        [xml]$nugetConfigXml = Get-Content -Path $_.FullName
        if ($nugetConfigXml.configuration -eq $null) {
            throw ("Nuget for Unity configuration file " + $_.FullName + " is missing the 'configuration' xml node. See https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior")
        }

        if ($nugetConfigXml.configuration.packageSourceCredentials -eq $null) {
            $newNode = $nugetConfigXml.ImportNode(([xml]$packageSourceCredentialsNodeString).FirstChild, $true)
            $nugetConfigXml.ChildNodes.Where({$_.Name -eq "configuration"}).AppendChild($newNode) | Out-Null
        }

        for ($i = 0; $i -lt $NugetSourceNames.Length; $i++) {
            $nugetSourceName = $NugetSourceNames[$i]
            if ($nugetConfigXml.configuration.packageSourceCredentials.$nugetSourceName -eq $null) {
                [xml]$nugetSourceNameNode = [string]::Format($packageSourceNameNodeFormatString, $nugetSourceName)
                $newNode = $nugetConfigXml.ImportNode($nugetSourceNameNode.FirstChild, $true)
                $nugetConfigXml.configuration.ChildNodes.Where({$_.Name -eq "PackageSourceCredentials"}).AppendChild($newNode) | Out-Null
            }

            $nugetSourceToken = $NugetSourceAuthTokens[$i]
            $plainTextNugetSourceToken = (New-Object PSCredential "user", $nugetSourceToken).GetNetworkCredential().Password
            [xml]$nugetSourceTokenNode = [string]::Format($packageSourceTokenNodeFormatString, $plainTextNugetSourceToken)
            $newNode = $nugetConfigXml.ImportNode($nugetSourceTokenNode.FirstChild, $true)

            $packageSourceNode = $nugetConfigXml.configuration.packageSourceCredentials.ChildNodes.Where({$_.Name -eq $nugetSourceName})
            $packageSourceNode.RemoveAll()
            $packageSourceNode.AppendChild($newNode) | Out-Null
        }

        $nugetConfigXml.Save($_.FullName)
    }
}

function Grant-NugetAccessViaCredential
{
    [CmdletBinding()]
    param(
        [string[]] $UnityProjects
    )

    # Inject command line credential, if one is provided
    if ($NugetSourceNames.Length -ne $NugetSourceAuthTokens.Length) {
        throw New-Object System.ArgumentException "You must specify an equal number of NugetSourceNames and NugetSourceAuthTokens"
    }

    if( $NugetSourceNames.Length -gt 0 ) {
        Write-Host "  Collecting Nuget.config files for all Unity projects"
        $nugetConfigs = $null
        $UnityProjects | ForEach-Object {
            $nugetConfigs = $nugetConfigs + (Get-ChildItem "*\Nuget.config" -Recurse)
        }

        Edit-NugetConfigToken $nugetConfigs $NugetSourceNames $NugetSourceAuthTokens
    }
}

function Invoke-UnityProjectBuild
{
    [CmdletBinding()]
    param(
        [PSTypeName("UnityProjectInstance")]$UnityProjects
    )

    $UnityProjects | ForEach-Object {
        # Skip projects if the are in the Artifacts staging directory
        [string] $pathAsString = $_.Path
        if ($pathAsString.StartsWith((Join-Path -Path (Get-Location) -ChildPath "Artifacts")))
        {
            return; # in ForEach-Object, this behaves as a continue
        }

        Write-Host ("  Restoring Nuget packages for Unity project {0}" -f $_.Path)
        $logFile = Join-Path $_.Path "Unity-Restore.log"
        Start-UnityEditor -Project $_.Path -LogFile $logFile -AcceptAPIUpdate -BatchMode -Quit -Wait

        Write-Host ("  Building Unity project " + $_.Path)
        $logFile = Join-Path $_.Path "Unity-WSAPlayer.log"
        Start-UnityEditor -Project $_.Path -LogFile $logFile -AcceptAPIUpdate -BatchMode -Quit -Wait -ExecuteMethod "Build.Invoke" -BuildTarget WSAPlayer
    }
}

function Edit-AppxManifestRevision
{
    # Modify the appxmanifests with the revision number
    Get-ChildItem "*.appxmanifest" -Recurse | ForEach-Object {
        [xml]$xml = Get-Content $_
        $version = [System.Version]$xml.Package.Identity.Version
        if ($Revision -eq -1)
        {
            $Revision = $version.Revision + 1;
        }
        $xml.Package.Identity.Version = "$($version.Major).$($version.Minor).$($version.Build).$Revision"
        $xml.Save($_)
    }
}

function Get-VisualStudioSolution
{
    [CmdletBinding()]
    param(
        [string]$BasePath = $PWD
    )

    # Collecting the non-unity VS Solutions to build
    $vsSolutions = Get-ChildItem *.sln -Recurse -Path $BasePath | Where-Object {
        (Get-UnityProjectInstance -BasePath $_.DirectoryName).Count -eq 0
    }

    # Write the found Visual Studio Solutions to the pipeline
    Write-Output $vsSolutions
}

function Invoke-PreMSBuildCommandScript
{
    [CmdletBinding()]
    param(
        [string]$SolutionPath,
        [string]$Configuration,
        [string]$Platform
    )
    if (Test-Path(Join-Path -Path $PSScriptRoot -ChildPath "build.PreMSBuildSteps.ps1"))
    {
        & (Join-Path -Path $PSScriptRoot -ChildPath "build.PreMSBuildSteps.ps1") -Solution $SolutionPath -Configuration $Configuration -Platform $Platform
    }
}

function Invoke-MSBuildForVisualStudioSolution
{
    [CmdletBinding()]
    param(
        [System.IO.FileInfo[]]$VsSolutions
    )

    $VsSolutions | ForEach-Object {
        foreach( $platfo in $Platform) {
            foreach( $config in $Configuration ) {
                Invoke-PreMSBuildCommandScript -SolutionPath $_.FullName -Configuration $config -Platform $platfo
                Invoke-MSBuild -Path $_.FullName -Configuration $config -Platform $platfo
            }
        }
    }
}

function Publish-PackageSourceArtifact
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [bool]$UseGitCredentials,
        [Object[]]$InternalModules
    )

    if ($UseGitCredentials) {
        New-SourceSnapshot -Recurse -Path $IncludePackageSource -OutputPath ".\Artifacts\PackageSources\" -GitCredential $GitCredential
    }
    else {
        New-SourceSnapshot -Recurse -Path $IncludePackageSource -OutputPath ".\Artifacts\PackageSources\"
    }

    foreach($module in $InternalModules) {
        # Include the internal module source too.
        Get-Module $module.Name | Sort-Object -Property Version -Descending | Select-Object -First 1 -ExpandProperty ModuleBase | Get-Item | ForEach-Object {
            New-Item -ItemType Directory ".\Artifacts\Modules\$($_.Parent.Name)\$($_.Name)" -Force
            Copy-Item "$($_.FullName)\*" -Destination ".\Artifacts\Modules\$($_.Parent.Name)\$($_.Name)" -Recurse -Force
        }
    }
}

function Publish-BundleArtifact
{
    Get-ChildItem "*.appxbundle" -Recurse | ForEach-Object {
        Copy-Item $_.Directory -Destination ".\Artifacts\$($_.Directory.Name)" -Recurse
    }
    Get-ChildItem "*.msixbundle" -Recurse | ForEach-Object {
        Copy-Item $_.Directory -Destination ".\Artifacts\$($_.Directory.Name)" -Recurse
    }
}

function Main
{
    [CmdletBinding()]
    param(
        $Parameters
    )

    $internalModules = @(
        @{'Name' = 'Build'; 'MinimumVersion' = '4.0.7364' }
    )
    Import-RequiredModule -InternalModules $internalModules

    if (-not $SkipUnityBuild)
    {
        Write-Host "Collecting the unity projects" -ForegroundColor Green
        $unityProjects = Get-UnityProjectInstance -Recurse -BasePath $UnityProjectBasePath

        Write-Host "  Injecting credentials" -ForegroundColor Green
        Grant-NugetAccessViaCredential -UnityProjects $unityProjects

        Write-Host "  Building Unity projects" -ForegroundColor Green
        Invoke-UnityProjectBuild -UnityProjects $unityProjects

        Write-Host "  Updating AppxManifest Version Revision" -ForegroundColor Green
        Edit-AppxManifestRevision
    }

    if (-not $SkipMSBuild)
    {
        Write-Host "Collecting and Building non-Unity Visual Studio Solutions" -ForegroundColor Green
        $visualStudioSolutions = Get-VisualStudioSolution -BasePath $VsSolutionBasePath
        Invoke-MSBuildForVisualStudioSolution -VsSolutions $visualStudioSolutions
    }

    if (-not $SkipStageArtifacts)
    {
        Write-Host "Moving all the artifacts to a single uploadable directory" -ForegroundColor Green
        Publish-BundleArtifact
    }

    if ($Parameters.ContainsKey('IncludePackageSource'))
    {
        Write-Host "Taking snapshots of your packages' source" -ForegroundColor Green
        Publish-PackageSourceArtifact -UseGitCredentials ($Parameters.ContainsKey('GitCredential')) -InternalModules $internalModules
    }
}

# Issue warning if renamed parameters are detected
$AliasUsed = $MyInvocation.MyCommand.Parameters['SkipMSBuild'].Aliases | Where-Object {$MyInvocation.Line -match "\s-$([regex]::Escape($_))\s*"}
if ($AliasUsed)
{
    Write-Warning("The -SkipVisualStudioBuild parameter has been replaced by -SkipMSBuild; Consider updating your script");
}

Main -Parameters $PSBoundParameters