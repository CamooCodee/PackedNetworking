using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PackedNetworking.Packets;
using PackedNetworking.Util;
using UnityEngine;

namespace PackedNetworking
{
    public class AutoPacketSupporter : MonoBehaviour
    {
        private bool _logFoundPacketTypes;

        private void FindPackets()
        {
            var myAssembly = Assembly.GetAssembly(typeof(Packet));
            if (myAssembly == null)
            {
                NetworkingLogs.LogFatal("Failed to auto-support packets: There was no assembly loaded with the Packet type.");
                return;
            }
            
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblies = allAssemblies.Where(a => a == myAssembly || a.GetReferencedAssemblies().Any(n => n.Name == myAssembly.GetName().Name));
            
            var packetTypes = new List<Type>();
            
            foreach (var assembly in assemblies)
                packetTypes.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Packet)) && !t.IsAbstract));
            
            foreach (var type in packetTypes)
            {
                var id = GetIdByPacketType(type);
                if(id == -1) continue;
                if(_logFoundPacketTypes)
                    NetworkingLogs.LogInfo($"Automatically supported packet type {type.Name} with the id {id}.");
                NetworkBehaviour.AddSupportedPacketType(type, id);
            }
        }
        
        private void Start()
        {
            Destroy(this);
        }

        static int GetIdByPacketType(Type type)
        {
            var values =
                from fieldInfo in type.GetFields()
                where (fieldInfo.Attributes & FieldAttributes.Literal) != 0 && fieldInfo.Name == "ID"
                select (int)fieldInfo.GetRawConstantValue();

            var array = values as int[] ?? values.ToArray();
            
            if (!array.Any())
                NetworkingLogs.LogError(
                    $"Failed to support packet '{type.Name}'. A public constant for the ID is required.");
            else
                return array[0];

            return -1;
        }

        public void SetValues(bool active, bool printPacketClasses)
        {
            _logFoundPacketTypes = printPacketClasses;
            if(active)
                FindPackets();
        }
    }
}