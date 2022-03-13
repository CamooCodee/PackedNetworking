namespace PackedNetworking.Packets
{
    /// <summary>Identical to the packet ids. Used by server and client.</summary>
    public enum PacketTypes
    {
        Handshake = 0,
        HandshakeReceived,
        UdpTest,
        UdpTestReceived,
    }
}