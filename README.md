<h1 align="center">

<img src="https://raw.githubusercontent.com/SixLabors/Branding/master/icons/imagesharp.web/sixlabors.imagesharp.web.svg?sanitize=true" alt="SixLabors.ImageSharp.Web" width="256"/>
<br/>
SixLabors.ImageSharp.Web
</h1>

<div align="center">

[![Build Status](https://img.shields.io/github/workflow/status/SixLabors/ImageSharp.Web/Build/master)](https://github.com/SixLabors/ImageSharp.Web/actions)
[![Code coverage](https://codecov.io/gh/SixLabors/ImageSharp.Web/branch/master/graph/badge.svg)](https://codecov.io/gh/SixLabors/ImageSharp.Web)
[![License: Apache 2.0](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![GitHub issues](https://img.shields.io/github/issues/SixLabors/ImageSharp.Web.svg)](https://github.com/SixLabors/ImageSharp.Web/issues)

[![GitHub stars](https://img.shields.io/github/stars/SixLabors/ImageSharp.Web.svg)](https://github.com/SixLabors/ImageSharp.Web/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/SixLabors/ImageSharp.Web.svg)](https://github.com/SixLabors/ImageSharp.Web/network)
[![Twitter](https://img.shields.io/twitter/url/http/shields.io.svg?style=flat&logo=twitter)](https://twitter.com/intent/tweet?hashtags=imagesharp,dotnet,oss&text=ImageSharp.+A+new+cross-platform+2D+graphics+API+in+C%23&url=https%3a%2f%2fgithub.com%2fSixLabors%2fImageSharp&via=sixlabors)
</div>

### **ImageSharp.Web** is a new high-performance ASP.NET Core middleware leveraging the ImageSharp graphics library to allow on-the-fly image manipulation via URL based commands.

## License
  
- ImageSharp.Web is licensed under the [Apache License, Version 2.0](https://opensource.org/licenses/Apache-2.0)  
- An alternative Commercial Support License can be purchased **for projects and applications requiring support**.
Please visit https://sixlabors.com/pricing for details.

## Support Six Labors

Support the efforts of the development of the Six Labors projects. 
 - [Purchase a Commercial Support License :heart:](https://sixlabors.com/pricing/)
 - [Become a sponsor via GitHub Sponsors :heart:]( https://github.com/sponsors/SixLabors)
 - [Become a sponsor via Open Collective :heart:](https://opencollective.com/sixlabors)
## Documentation

- [Detailed documentation](https://sixlabors.github.io/docs/) for the ImageSharp.Web API is available. This includes additional conceptual documentation to help you get started.

## Questions

- Do you have questions? We are happy to help! Please [join our Discussions Forum](https://github.com/SixLabors/ImageSharp/discussions/category_choices), or ask them on [Stack Overflow](https://stackoverflow.com) using the `ImageSharp.Web` tag. Please do not open issues for questions.
- Please read our [Contribution Guide](https://github.com/SixLabors/ImageSharp.Web/blob/master/.github/CONTRIBUTING.md) before opening issues or pull requests!


## Code of Conduct  
This project has adopted the code of conduct defined by the [Contributor Covenant](https://contributor-covenant.org/) to clarify expected behavior in our community.
For more information, see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### Installation

Install stable releases via Nuget; development releases are available via MyGet.

| Package Name                   | Release (NuGet) | Nightly (MyGet) |
|--------------------------------|-----------------|-----------------|
| `SixLabors.ImageSharp.Web`         | [![NuGet](https://img.shields.io/nuget/v/SixLabors.ImageSharp.Web.svg)](https://www.nuget.org/packages/SixLabors.ImageSharp.Web/) | [![MyGet](https://img.shields.io/myget/sixlabors/vpre/SixLabors.ImageSharp.Web.svg)](https://www.myget.org/feed/sixlabors/package/nuget/SixLabors.ImageSharp.Web) |

## Manual build

If you prefer, you can compile ImageSharp.Web yourself (please do and help!)

- Using [Visual Studio 2019](https://visualstudio.microsoft.com/vs/)
  - Make sure you have the latest version installed
  - Make sure you have [the .NET Core 3.1 SDK](https://www.microsoft.com/net/core#windows) installed

Alternatively, you can work from command line and/or with a lightweight editor on **both Linux/Unix and Windows**:

- [Visual Studio Code](https://code.visualstudio.com/) with [C# Extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
- [.NET Core](https://www.microsoft.com/net/core#linuxubuntu)

To clone ImageSharp.Web locally, click the "Clone in [YOUR_OS]" button above or run the following git commands:

```bash
git clone https://github.com/SixLabors/ImageSharp.Web
```

If working with Windows please ensure that you have enabled log file paths in git (run as Administrator).

```bash
git config --system core.longpaths true
```

This repository contains [git submodules](https://blog.github.com/2016-02-01-working-with-submodules/). To add the submodules to the project, navigate to the repository root and type:

``` bash
git submodule update --init --recursive
```

#### Running the Tests

The unit tests require [Azurite Azure Storage Emulator](https://github.com/Azure/Azurite) and [s3rver](https://github.com/jamhall/s3rver) in order to run.

On Windows to install and run the server as a background process run the following command

```bash
npm install -g azurite
start /B azurite --loose

npm install -g s3rver
start /B s3rver -d .
```

On Linux

```bash
sudo npm install -g azurite
sudo azurite --loose &

sudo npm install -g s3rver
sudo s3rver -d . &
```

## How can you help?

Please... Spread the word, contribute algorithms, submit performance improvements, unit tests, no input is too little. Make sure to read our [Contribution Guide](https://github.com/SixLabors/ImageSharp.Web/blob/master/.github/CONTRIBUTING.md) before opening a PR.

## The ImageSharp.Web Team

- [James Jackson-South](https://github.com/jimbobsquarepants)
- [Dirk Lemstra](https://github.com/dlemstra)
- [Anton Firsov](https://github.com/antonfirsov)
- [Scott Williams](https://github.com/tocsoft)
- [Brian Popow](https://github.com/brianpopow)
