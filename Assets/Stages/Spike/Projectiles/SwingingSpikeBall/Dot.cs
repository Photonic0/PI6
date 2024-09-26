using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot
{
    public Vector2 position;
    public Vector2 lastPosition;
    public bool isLocked;
    public readonly List<DotConnection> connections;
    public Dot(Vector2 initialPos, bool isLocked)
    {
        connections = new List<DotConnection>();
        position = initialPos;
        lastPosition = initialPos;
        this.isLocked = isLocked;
    }
    public static DotConnection Connect(Dot dotA, Dot dotB, float length = -1)
    {
        DotConnection connection = length < 0f 
            ? new DotConnection(dotA, dotB) 
            : new DotConnection(dotA, dotB, length);
        dotA.connections.Add(connection);
        dotB.connections.Add(connection);
   
        return connection;
    }
    public static void Disconnect(DotConnection connection)
    {
        List<DotConnection> dotAConnections = connection.dotA.connections;
        List<DotConnection> dotBConnections = connection.dotB.connections;
        if(dotAConnections.Contains(connection)) connection.dotA.connections.Remove(connection);
        if(dotBConnections.Contains(connection)) connection.dotB.connections.Remove(connection);
    }
}
