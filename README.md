# PackedNetworking
A networking solution for Unity allowing low level server and client communication. Server and Client are in the same Unity project. 
This library is based off of the old [C# Networking series by Tom Weiland](https://www.youtube.com/playlist?list=PLXkn83W0QkfnqsK8I0RAz5AbUxfg3bOQ5)

Packed Networking is in very early stages and has only been tested within a closed network.

The goal of this library is to offer a simple and easy to implement multiplayer system for small and medium sized games. Additionally it shouldn't overwhelm the user with a long feature list which makes it difficult to get started. Speaking of getting started...

## How To Get Started

#### 1. The Networking Manager

The first scene of your game should contain a game object with the `NetowrkingManager` component attaced to it.
Make sure to specify a port which isn't used by another application and if you want to test client and server on the same machine, use `127.0.0.1` as the ip-address. This is a loop-back ip address, meaning the machinge refers to itself.

#### 2. The Server Scene

If you want to, you can have a seperate server scene. This scene will be loaded when starting the game as a server. This will either be done when building your application in batch-mode or when you tick the `Force Server Build` boolean in the inspector of the `NetworkingManager`. To specify what server scene to load, enter the scene name into the `Server Scene Name` field and ensure the scene exits in the build settings. You don't _have_ to work with an additional server scene.
Additionally, you don't have to worry about switching scenes on the client or server side. The networking manager is part of the `DontDestroyOnLoad` scene. Meaning once it exits, it will not be destroyed when a new scene is beeing loaded.

#### 3. Testing The Setup

That's all the setup you need. Any sort of handshake for both, udp and tcp are handled by the library. In order to test, build the server or client side. If you want to build the server side, make sure you tick the `Force Server Build` box on the `NetworkingManager`. Also ensure the scene with the `NetworkingManager` is being loaded first. Once the build is completed, start up the server (the .exe that was built). Once the server is running, start the client in the Unity Editor. Make sure to untick `Force Server Build`. You might have to change focus between the `Unity Editor Game View` and the `.exe` that is running a few times, since by default Unity builds pause when they are running in the background. Eventually the Console in the Editor should say `Received Handshake: Handshake Message. Your client ID: n` (not the last message in the console). If this is the case, focuse the server build one more time. After that, the client and server are connected successfully.

#### 4. Sending Custom Packets

[See the 'Packed Networking Essentials' on how to do this!](#packed-networking-essentials)

## Basic Networking Knowledge

This is on a very basic level and the bare minimum you need to work with this framework.

#### UDP

Udp is an internet protocol, which stands for `User Datagram Protocol`. Udp isn't very reliable meaning data might get lost or reach the targeted machine in a different order than it was sent in. Use this for data which is send continously, for example user input. Udp is used due to performance benefits.

#### TCP

Tcp is another internet protocol, which stands for `Transmission Control Protocol`. It's much more reliable and basically guarantees the target to receive the data how it was intended. Use this when you send data with a single packet, for example the username or user inputs which occur less frequently like a jump.

## Packed Networking Essentials

This section will create an example program showing you how to send data from one end to the other.

Packets are messages you can send between the client and the server. There are three types of packets you can send. A `ServerPacket` can only be send by the server, a `ClientPacket` can only be send by the client and a `ClientServerPacket` can be send both ways. Packets are no `MonoBehaviours`, instead you have to inherit from one of the three packet types listed above. Make sure to add `using PackedNetworking.Packets;` Every packet you create needs a `public`, `constant` variable of type `int`, called `ID`. The ids `0` to `3` cannot be used since they are used by the framework. It is recommended to set up an enum similar to this:
```
public enum PacketTypes
{
    ExampleServerPacket = 4, // Start at 4 since 0 to 3 is already used by the framwork
    ExampleClientPacket, // Will be equivalent to 5
    BothWayPacket, // Will be equivalent to 6 and so on...
}
```
and then assign the `ID` like this:
```
public const int ID = (int)PacketTypes.ExampleServerPacket;
```
There are given constructors each PacketType requires.
The `ServerPacket` needs the following two constructors:
```
public ExampleServerPacket(int targetClient) : base(ID, targetClient) { }
public ExampleServerPacket(Packet packet) : base(ID) { }
```
First of all, the `ID` has to be passed into the base constructors. The `targetClient` variable takes the `client id` a specific packet is aimed at. You have the option to add a third constructor. You can also replace the first constructor.
```
public ExampleServerPacket() : base(ID) { }
```
This constructor will ensure the packet gets send to every client connected to the server.
The second constructor is used to build up the packet object from a low level `Packet` class.

Let's take a look at this in practice. We will use the `ExampleServerPacket` to send a highscore and the username history of a client. We will pretend that these things are saved somewhere on the server side and the client has requested these. Normally these two things probably wouln't be send within the same packet. They are pretty unrelated.

The username history will be an `array` of `strings` and the highscore a `float`. Add both of these as simple fields.
```
public readonly string[] usernameHistory;
public readonly float highscore;
```
Now initialize them in the (first and/or third) constructor. In this example, the packet will always target a specific client, so there are only two constructors. This is currently the first one:
```
public ExampleServerPacket(float highscore, int targetClient, params string[] usernameHistory) : base(ID,
    targetClient)
{
    this.highscore = highscore;
    this.usernameHistory = usernameHistory;
}
```
However, at the moment the data isn't really part of the packet. We have to write the data first. It's only the data that was written to the packet, that will be send. In order two write the highscore, we just need one simple call.
```
Write(highscore);
```
The history will be a bit more complex since it's an array. We have to write the length first. That way, when reading the packet, there is a way to know how many items have to be read.
```
Write(usernameHistory.Length);
foreach (var name in usernameHistory) Write(name);
```
Finally, we use the second constructor to read the data. The second constructor ***must*** take a single low end `Packet`, which we will use to read data from. The data ***must*** be read in the same order as it was written in.
```
public ExampleServerPacket(Packet packet) : base(ID)
{
    highscore = packet.ReadFloat();
    usernameHistory = new string[packet.ReadInt()];
    for (int i = 0; i < usernameHistory.Length; i++) 
        usernameHistory[i] = packet.ReadString();
}
```
The complete `ExampleServerPacket` class now looks like this:
```
public class ExampleServerPacket : ServerPacket
{
    public const int ID = (int)PacketTypes.ExampleServerPacket;

    public readonly string[] usernameHistory;
    public readonly float highscore;

    public ExampleServerPacket(float highscore, int targetClient, params string[] usernameHistory) : base(ID,
        targetClient)
    {
        this.highscore = highscore;
        this.usernameHistory = usernameHistory;
        
        Write(highscore);
        Write(usernameHistory.Length);
        foreach (var name in usernameHistory) Write(name);
    }

    public ExampleServerPacket(Packet packet) : base(ID)
    {
        highscore = packet.ReadFloat();
        usernameHistory = new string[packet.ReadInt()];
        for (int i = 0; i < usernameHistory.Length; i++) 
            usernameHistory[i] = packet.ReadString();
    }
    
    public void PrintUsernameHistory()
    {
        var builder = new StringBuilder();
        
        foreach (var name in usernameHistory) builder.Append($"{name}, ");
        builder.Remove(builder.Length - 2, 2);
        
        Debug.Log($"USERNAME HISTORY: {builder}");
    }
}
```
(I added a `PrintUsernameHistory` method for convenience)

The next step is sending the packet we just created. For that, you need a `ServerNetworkBehaviour`. Create a new `MonoBehaviour`, add `using PackedNetwoking.Server;` and instead of inheriting from `MonoBehaviour`, inherit from `ServerNetworkBehaviour`. Now, you can use the `SendTcpPacket` or `SendUdpPacket` methods to send a packet. For this example, the server will send the packet when `G` is pressed.
```
private void Update()
{
    if (Input.GetKeyDown(KeyCode.G))
    {
        SendTcpPacket(new ExampleServerPacket(103.98f, 1,
            "John", "Doe", "Jackson"));
    }
}
```
Now just make sure to add your `ServerNetworkBehaviour` to a game object to your scene. If you have a server scene, add it to the server scene. For now, the packet is targeted at the first client, this will give an error when there is no client with the `client id` `1`. So when testing, make sure you connect a client before you press G on the server.

Before we can test whether or not our packet reaches the client, we have to listen for the packet first. Create another `MonoBehaviour` but inherit from `ClientNetworkBehaviour` instead. Don't forget the `using PackedNetowking.Client;` and `using PackedNetwoking.Packets;` statements at the top. In theory, you can now call `ListenForPacket` in `Start` or `Awake`. The recommended way to go about this however is this:
```
private void OnEnable() => ListenForPacket<ExampleServerPacket>(OnExamplePacket);
private void OnDisable() => StopListeningForPacket<ExampleServerPacket>(OnExamplePacket);
```
The method listening for a packet needs this signature (the name doesn't matter of course):
```
void OnExamplePacket(Packet p) { }
```
Printing the received data is as simple as this:
```
void OnExamplePacket(Packet p)
{
    var packet = (ExampleServerPacket) p;
    Debug.Log($"Highscore: {packet.highscore}");
    packet.PrintUsernameHistory();
}
```
Now add a game object with this script to your client scene. We are now sending a custom packet from the server to the client where we are listeing for that packet and printing it's values.

Build the server and boot it up. Now connect the client using the UnityEditor. When pressing G on the server build, you should see the data we sent being printed to the console window.

Sending data from the client to the server is pretty much the same. The packet will look slightly different. There are always the exact same two constuctors required.
```
public class ExampleClientPacket : ClientPacket
{
    public const int ID = (int) PacketTypes.ExampleClientPacket;
    
    public ExampleClientPacket(int sendingClient) : base(ID, sendingClient) { }
    public ExampleClientPacket(Packet packet) : base(ID, packet) { }
}
```
We won't have any data in this packet, but it would work the same way as for the server packet.

Let's send the packet on the client side when `G` is pressed.
```
private void Update()
{
    if (Input.GetKeyDown(KeyCode.G)) 
        SendTcpPacket(new ExampleClientPacket(ClientId));
}
```
Instead of sending the packet on the server when G is pressed, we will send it when receiving the `ExampleClientPacket`. The server code for this looks like this:
```
private void OnEnable() => ListenForPacket<ExampleClientPacket>(OnExamplePacket);
private void OnDisable() => StopListeningForPacket<ExampleClientPacket>(OnExamplePacket);

void OnExamplePacket(Packet p)
{
    var packet = (ExampleClientPacket) p;
    SendTcpPacket(new ExampleServerPacket(103.98f, packet.sendingClient,
        "John", "Doe", "Jackson"));
}
```
Note that we are now targeting the packet at the client that send the `ExampleClientPacket`

Again, build and boot up the server. Ensure server and client are connected successfully. With the steps described [here](#3-testing-the-setup). Now press G on the client. The server will reveive the client packet and send tp the client the hightscore and username history. The console window should show the highscore and username history.

## More Useful Features

#### Client Server Packets

All the possible constuctors for a `ClientServerPacket` are these:
```
public MessagePacket(int actingClient) : base(ID, actingClient) { }
public MessagePacket(string message, int sendingClient) : base(ID)
{
    this.message = message;
    this.sendingClient = sendingClient;

    Write(message);
    Write(sendingClient);
}

public MessagePacket(Packet packet) : base(ID, packet)
{
    message = packet.ReadString();
    sendingClient = packet.ReadInt();
}
```
