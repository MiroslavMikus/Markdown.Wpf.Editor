Dependency scanner from [chocolatey](https://chocolatey.org/packages/dependency-scanner)

  **Features**:

- Dependency scanner has how a plugin architecture (public API is coming soon)
- There is a new Settings concept build on top of LiteDB and Autofac
- Dependency scanner has now a new plugin: 'Working directory'
![WorkingDirectory](https://raw.githubusercontent.com/MiroslavMikus/DependencyScanner/master/DependencyScanner.Standalone/res/pic/2019-03-11_2.png)
- The user can now work with multiple working directories at once
- All working directories and their repositories will be stored
- The user can clone a new repository to the selected working directory
- The user can pull all repotitories in all working directories at once
![WorkingDirectoryProgress](https://raw.githubusercontent.com/MiroslavMikus/DependencyScanner/master/DependencyScanner.Standalone/res/pic/2019-03-11_3.gif)
- Dependency scanner has now an CI Build ![Azure build](https://dev.azure.com/DependencyScanner/DependencyScanner/_apis/build/status/CI-Master)
- **Added 'Donate' button :)**

![DonateButton](https://raw.githubusercontent.com/MiroslavMikus/DependencyScanner/master/DependencyScanner.Standalone/res/pic/2019-03-11_4.png)
- The user can not switch to the remote branches as well
![RemoteBranches](https://raw.githubusercontent.com/MiroslavMikus/DependencyScanner/master/DependencyScanner.Standalone/res/pic/2019-03-11_1.png)
- Dependency scanner can now recognize if there is no internet connection and therefore cut all remote calls

**Improvements**:

- 'Nuget dependency scan' plugin was temporarily removed due to the performance issues and incompatibility with the new PackageReference dependency management format
- Huge refactoring under the hood
- Getting git informations is now faster

**Bug-Fix**:
- [7#](https://github.com/MiroslavMikus/DependencyScanner/issues/7) Choco-updater internet problems

All release notes are [HERE](https://github.com/MiroslavMikus/DependencyScanner/blob/master/DependencyScanner.Standalone/res/Changeset.md).