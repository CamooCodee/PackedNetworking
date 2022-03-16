# PackedNetworking
A networking solution for Unity allowing low level server and client communication. Server and Client are in the same Unity project. 
This library is based off of the old [C# Networking series by Tom Weiland](https://www.youtube.com/playlist?list=PLXkn83W0QkfnqsK8I0RAz5AbUxfg3bOQ5)

Packed Networking is in very early stages and has only been tested within a closed network.

The goal of this library is to offer a simple and easy to implement multiplayer system for small and medium sized games. Additionally it shouldn't overwhelm the user with a long feature list which makes it difficult to get started. Speaking of getting started...

## How To Get Started

### 1. The Networking Manager

The first scene of your game should contain a game object with the `NetowrkingManager` component attaced to it.
Make sure to specify a port which isn't used by another application and if you want to test client and server on the same machine, use `127.0.0.1` as the ip-address. This is a loop-back ip address, meaning the machinge refers to itself.

### 2. The Server Scene

If you want to, you can have a seperate server scene. This scene will be loaded when starting the game as a server. This will either be done when building your application in batch-mode or when you tick the `Force Server Build` boolean in the inspector of the `NetworkingManager`. To specify what server scene to load, enter the scene name into the `Server Scene Name` field. You don't have to work with an additional server scene.
Additionally, you don't have to worry about switching scenes on the client or server side. The networking manager is part of the `DontDestroyOnLoad` scene. Meaning once it exits, it will not be destroyed when a new scene is beeing loaded.

### 3. Testing The Setup

That's all the setup you need. Any sort of handshake for both, udp and tcp are handled by the library. In order to test, build the server or client side. If you want to build the server side, make sure you tick the `Force Server Build` box on the `NetworkingManager`. Also ensure the scene with the `NetworkingManager` is being loaded first. Once the build is completed, start up the server (the .exe that was built). Once the server is running, start the client in the Unity Editor. Make sure to untick `Force Server Build`.

### 4. Sending Custom Packets

## Basic Networking Knowledge

This is on a very basic level and the bare minimum you need to work with this framework.

### UDP

Udp is an internet protocol, which stands for `User Datagram Protocol`. Udp isn't very reliable meaning data might get lost or reach the targeted machine in a different order than it was sent. Use this for data which is send continously, for example user input. Udp is used due to performance benefits.

### TCP

Tcp is another internet protocol, which stands for `Transmission Control Protocol`. It's much more reliable and basically guarantees the target to receive the data how it was intended. Use this when you send data with a single packet, for example the username or user inputs which occur less frequently like a jump.
