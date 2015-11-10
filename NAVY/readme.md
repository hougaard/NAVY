NAVY documentation
------------------

NAVY is a package manager for Microsoft Dynamics NAV 2015 and 2016. It has only been tested on NAV2016.

NAVY can build packages, install, uninstall, suspend and restore packages.

The basic idea is to package FOB files with DELTA files that hold changes to standard objects and 
be able to install and uninstall them automatic.

NAVY can also "suspend" a package. Suspending a package will remove changes from standard objects and save field values in a special table. 
The restore command performs a reverse suspend, installs the changes to standard objects and restores the field values.

A binary version is available to download from http://www.hougaard.com

Limitation currently:
* Not widely tested :)
* Not all field types are currently supported - check codeunit.cs
* Nested packages should be possible, but not tested

Check NAVY.exe.config for setting the basic setup.

THIS IS PRELEASE SOFTWARE, SO DO NOT USE IN PRODUCTION !


Parameters for NAVY.exe

Create package command:

build Name="<name>" Version="<version>" FOB=<fob file> DELTAFILES=DELTA\

Install package command:

install DatabaseName=NAV9_CLEAN Name="<name>"

Uninstall package command:

uninstall DatabaseName=NAV9_CLEAN Name="<name>"

Suspend package command:

suspend DatabaseName=NAV9_CLEAN Name="<name>"

Restore package command:

restore DatabaseName=NAV9_CLEAN Name="<name>"
