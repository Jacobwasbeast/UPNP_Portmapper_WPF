## Port Mapper WPF Application using alanmcgovern/Mono.Nat
## Overview
The Port Mapper WPF Application is a straightforward tool designed to help users manage port mappings with an easy-to-use graphical interface. It allows you to set an IP address, add, edit, and remove port mappings, and save/load these settings from a JSON configuration file.

# Features
## Manage Port Mappings: Add, edit, and remove port mappings, including fields for Name, Internal Port, External Port, Protocol, and IP Address.
Set IP Address: Set and store the IP address used for the port mappings.
Persistent Configuration: Save and load the configuration, ensuring that all settings are retained between sessions.
Intuitive Interface: Simple and easy-to-use graphical interface built with WPF.
## Requirements
<p>.NET Framework 8.0/6.0 works too<br>
 Visual Studio 2019/Rider (for development)<br>
 Newtonsoft.Json NuGet package for JSON serialization <br></p>
 [Mono.nat]("https://github.com/alanmcgovern/Mono.Nat")
 
## Installation
Clone or download the repository to your local machine.
Open the solution in Visual Studio.
Restore NuGet packages (especially Newtonsoft.Json).
Build and run the application.
## Usage
Launch the Application: Open the executable or run the application from Visual Studio.
Add a Port Mapping: Click the "Add" button, fill in the details, and save.
Edit a Port Mapping: Select a port mapping from the list, click "Edit," modify the details, and save.
Remove a Port Mapping: Select a port mapping from the list, and click "Remove."
Set IP Address: Click the "Settings" button, enter the desired IP address, and save.
Save Configuration: Configuration is automatically saved after every change.
Load Configuration: The configuration is automatically loaded on application startup.
Configuration File
The configuration is saved in a config.json file located in the application's directory. This file contains all port mappings and the IP address, serialized in JSON format.

# Contributing
If you would like to contribute to the project, feel free to fork the repository and submit pull requests. Bug reports and feature requests are also welcome.

# License
This project is licensed under the MIT License. See the LICENSE file for details.

# Contact
For questions or support, please open an issue on the GitHub repository or contact the maintainer directly.

