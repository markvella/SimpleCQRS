using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCQRS.Contracts
{
    public interface ISerializer
    {
        Task<T> Deserialize<T>(byte[] content);
        Task<object> Deserialize(Type tp,byte[] content);
        Task<byte[]> Serialize<T>(T obj);
    }
}
