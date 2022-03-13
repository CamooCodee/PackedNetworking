# PackedNetworking
A networking solution for Unity allowing low level server and client communication. Server and Client are in the same Unity project. 
This library is based off of the old [C# Networking series by Tom Weiland](https://www.youtube.com/playlist?list=PLXkn83W0QkfnqsK8I0RAz5AbUxfg3bOQ5)

Packed Networking is in very early stages and has only been tested within a closed network.

The goal of this library is to offer a simple and easy to implement multiplayer system for small and medium sized games. Additionally it shouldn't overwhelm the user with a long feature list which makes it difficult to get started. Speaking of getting started...

## How to get started

### 1. The Networking manager

The first scene of your game should contain a game object with the `NetowrkingManager` component attaced to it.
Make sure to specify a port which isn't used by another application and if you want to test client and server on the same machine, use `127.0.0.1` as the ip-address. This is a loop-back ip address, meaning the machinge refers to itself. `MaxClients` defines how many clients will be accepted to connect to the server.
