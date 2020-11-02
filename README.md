# EuNet - Starter

This is a project to easily get started with `EuNet`.

Several clients can connect to the server and move them using the `W, A, S, D` buttons.

All clients are synchronized using `P2P`.

It is automatically `HolePunching` or `Relay` to operate.

## Screenshot
![image](https://github.com/zestylife/EuNet-Starter/blob/master/Starter.gif)

## How to use

### Run the `Server`
* Open `Start.sln` file.
* Right-click on the `Server` project in Solution Explorer and click `Set as Startup Project`.
* Press F5 to `Start debugging` (Server will run)
![image](https://github.com/zestylife/EuNet-Starter/blob/master/Doc/ServerConsole.png)

### Run the `Client`
* Launch `Unity Hub`
* Click `Add` in `Unity Hub` to add the `Client` folder
* Set Unity Version to 2019.4 or higher.
* Click the project name to open the project.
* Open the `Login` scene.
* Enter the `Server Address`
  * If running locally, enter `127.0.0.1`.
  * To access from outside, enter the server's `Public IP`. How do I ask for a Google search.
![image](https://github.com/zestylife/EuNet-Starter/blob/master/Doc/SetupAddress.png)
* Play in `Unity`