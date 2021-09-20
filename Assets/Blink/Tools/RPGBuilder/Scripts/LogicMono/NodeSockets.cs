using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSockets : MonoBehaviour
{
    [System.Serializable]
    public class NodeSocket
    {
        public string socketName;
        public Transform socketTransform;
    }

    public List<NodeSocket> sockets = new List<NodeSocket>();

    public Transform GetSocketByName(string socketName)
    {
        foreach (var socket in sockets)
        {
            if(socket.socketName != socketName) continue;
            return socket.socketTransform;
        }

        return null;
    }
}
