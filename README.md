# ProjectTemplateUnity

Starter template for creating new Mixed Reality Studios Unity projects.

# How to Create a New Project

1. Create a new Git repository in VSTS.
1. Go to [ProjectTemplateUnity](https://wwhs.visualstudio.com/SharedTech/_git/ProjectTemplateUnity?version=GBmaster) and click the download button in the top-right, which looks like this: ![Download](Site\Download.JPG)
1. Click Open to open the downloaded zip file in File Explorer.
1. Copy the contents of the ProjectTemplateUnity folder into your repository.
1. (Optional) Rename the Apps/Unity folder to the name of your project and/or rename the app in Unity's ProjectSettings.
1. Edit this README.md to reflect the information and instructions for your project. You'll need to change some of the paths in *Setting up your development environment* to point to your project instead of to this template project. Delete this section (How to Create a New Project) because it is no longer needed.
1. Commit and push all your changes to the Git repository.

# Setting up your development environment

1. [Install Git for Windows](http://git-scm.com/downloads)
1. [Install Visual Studio 2017](http://go.microsoft.com/fwlink/?LinkId=309297&clcid=0x409&slcid=0x409)
1. Check [Apps/Unity/ProjectSettings/ProjectVersion.txt](Apps/Unity/ProjectSettings/ProjectVersion.txt) and install the matching version from the [Unity Download Archive](https://unity3d.com/get-unity/download/archive?_ga=2.260980869.940825270.1493940205-955438095.1489109829).
1. Clone the repo https://wwhs.visualstudio.com/SharedTech/_git/ProjectTemplateUnity using your preferred Git tool.
    - In Visual Studio, you can do this from the [VSTS project](https://wwhs.visualstudio.com/SharedTech/_git/ProjectTemplateUnity) by clicking Clone in the upper-right corner and choosing Clone in Visual Studio.

# How to Install Packages Into Your Project

Use [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) to install packages that you will support development of your Mixed Reality application.

1. Bring up the NuGet pane via NuGet > Manage NuGet Packages.
1. Browse list of projects and click the Install button for the packages that you want.
1. Commit and push your changes to the packages.config file.

![Packages](Site\NuGetWindow.JPG)

# How to Make a Build

Build artifacts are gathered into the `.\Artifacts` folder

In PowerShell run:
```powershell
# This will build 'Master' for 'x86'.
.\build.ps1
```

Go deep with easy multi configuration/platform support:
```powershell
# This will build the full matrix of platform/configuration options as revision 123.
.\build.ps1 -Configuration Master,Release,Debug -Platform x86,x64,ARM  -Revision 123
```

You can always get help via:
```powershell
help .\build.ps1 -Full
```

# Fixing Bugs and Adding Features to Packages

See [Contributing to Mixed Reality Studios Shared Technology](https://wwhs.visualstudio.com/_git/SharedTech?path=%2FContributing.md&version=GBdev%2FdocImprovePackages), which describes steps to take when contributing to packages. Access to this may currently only be available using internal Microsoft systems. If you are a Mixed Reality Studios customer that would like to contribute, please let us know and we'd love to discuss how this may be possible.

# How to Make a Build Including Package Sources

Mixed Reality Studios project uses [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) packages from an internal Microsoft feed. Following an engagment, customers who no longer have access to the  internal feed may want to continue development of the packages used in their application. The source code for all the packages used in a project can be downloaded in builds given to a customer.

In PowerShell run:
```powershell
# This will build 'Master' for 'x86' and then download the source code of all the packages used in a project.
.\build.ps1 -IncludePackageSource
```

After making this build the project sources can be delivered to a customer including:
* The restored packages inside the project (Apps/Unity/Assets/Packages)
* The package source code (Artifacts/PackageSources)
* The powershell modules required (Artifacts/Modules)

A customer receiving this project can make use of these packages in one of two ways:

### Option 1: **Check in Packages File As Part of Project**

This option is the simplest solution and it treats the project as a single unified codebase.

The packages used in a project are located at Apps/Unity/Assets/Packages. If `/Assets/Packages/` is removed from Apps/Unity/.gitignore, then the Packages folder can be checked in and treated as part of the project. Many packages provide code in raw .cs files. Some packages provide code inside .dll's. If .dll's need to be modified this can be accomplished by editing and rebuilding the corresponding Visual Studio solution inside Artifacts/PackageSources.

### Option 2: **Host A Package Feed**

This option is the best solution for organizations that will develop multiple Mixed Reality applications and want to maintain a library of helper packages that are used across different applications. This makes it possible to fix issues and make impovements to packages outside of the source code repository of an individual project and, when desired, to use [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) to pull those improved versions into multiple different Unity projects. This avoids merge issues because changes to a package are made outside of each  application.

Some work will be required to set up a feed of packages for this purpose. Mixed Reality Studios uses Azure DevOps with a repository and build pipline for each package. Builds use the `build.ps1` script in each package to generate nuget packages that are pushed to the [Azure DevOps Artifacts feed](https://azure.microsoft.com/en-us/services/devops/artifacts/). Any of these build scripts may depend on the powershell modules provided. The continuous integration build definitions for each package are included in the [.vsts-ci.yml](https://blogs.msdn.microsoft.com/devops/2017/11/15/pipeline-as-code-yaml-preview/) file inside each package. If you are using Azure DevOps, you may be able to change the `publishVstsFeed` id and DevOps will automatically create builds for your package repositories.

As part of an engagement with Mixed Reality Studio, please ask if you have questions or need advice about managing package development in your organization.
