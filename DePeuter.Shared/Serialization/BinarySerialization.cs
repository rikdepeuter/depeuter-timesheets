//using System.IO;

//namespace DePeuter.Shared.Serialization
//{
//    public class BinarySerialization
//    {
//        public static void Serialize<T>(T data, string filename)
//        {
//            using(var file = File.Create(filename))
//            {
//                ProtoBuf.Serializer.Serialize(file, data);
//            }
//        }
//        public static byte[] Serialize<T>(T data)
//        {
//            using(var stream = new MemoryStream())
//            {
//                ProtoBuf.Serializer.Serialize(stream, data);

//                stream.Position = 0;
//                return stream.ToArray();
//            }
//        }

//        public static T Deserialize<T>(string filename)
//        {
//            using(var file = File.OpenRead(filename))
//            {
//                return ProtoBuf.Serializer.Deserialize<T>(file);
//            }
//        }
//        public static T Deserialize<T>(byte[] serializedData)
//        {
//            using(var stream = new MemoryStream(serializedData))
//            {
//                stream.Position = 0;
//                return ProtoBuf.Serializer.Deserialize<T>(stream);
//            }
//        }
//    }
//}
