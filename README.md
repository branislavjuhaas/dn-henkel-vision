# DN Henkel Vision
![Version](https://img.shields.io/badge/Version-2024.0.0-blue) ![License](https://img.shields.io/badge/License-MIT-red) ![State](https://img.shields.io/badge/Development_State-Active-green) ![Framework](https://img.shields.io/badge/Framework-Windows_App_SDK_1.5-purple?link=https%3A%2F%2Flearn.microsoft.com%2Fen-us%2Fwindows%2Fapps%2Fwinui%2Fwinui3%2F)

# DN Henkel Vision
![DN Henkel Vision Editor](Readme/henkel-vision-editor.png)

# Description
DN Henkel Vision is a software application designed for fault registration at KraussMaffei Technologies, spol. s r.o. The project is partially funded by the same organization. The application provides support for exporting data to both Netstal and Autrag Microsoft Access Databases. One of the key features of the application is its integration with FelberAI, which predicts the classification of faults.

# Setup Instructions
To set up DN Henkel Vision, follow the standard setup instructions for a WinUI app:

1. **Prerequisites**: Make sure you have the following installed on your machine:
    - Windows 11, version 23H2 (build 22631), or later
    - Visual Studio 2022, version 17.9.4 or later, with the Universal Windows Platform development workload
    - The Windows 11 SDK, version 23H2 (build 22621), or later

2. **Clone the Repository**: Clone the DN Henkel Vision repository to your local machine.

3. **Open the Solution**: Open the `.sln` file in Visual Studio.

4. **Restore NuGet Packages**: Right-click on the solution in Solution Explorer and select `Restore NuGet Packages`.

5. **Create the Temporary Key**: Right-click on the `DN Henkel Vision` project in Solution Explorer and select `Properties`. Go to the `Package` tab and click on the `Generate Key` button to create a temporary key.

6. **Set the Startup Project**: Set the DN Henkel Vision project as the startup project.

7. **Build and Run the Application**: Press `F5` or use the `Debug > Start Debugging` menu command to build and run the application.

# Support
For any issues or queries, please contact the support team at juhas.branislaw@gmail.com.