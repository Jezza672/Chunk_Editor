using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nbtj
{
    public static class NBTJ
    {
        public static List<NBTTag> Parse(byte[] bytes, ref int position)
        {
           
            List<NBTTag> tags = new List<NBTTag>();
            while (position < bytes.Length)
            {
                byte tagID = bytes[position];
                position += 1;
                if (tagID == 0)
                {
                    return tags;
                }
                else
                {
                    byte[] nameBytes = bytes.Skip(position).Take(2).Reverse().ToArray();
                    int nameLength = BitConverter.ToUInt16(nameBytes, 0);
                    position += 2;
                    string name;
                    if (nameLength > 0)
                    {
                        name = Encoding.UTF8.GetString(bytes, position, nameLength);
                    }
                    else
                    {
                        name = "";
                    }
                    position += nameLength;

                    tags.Add(ProcessPayload(tagID, bytes, ref position, name));
                }
            }
            return tags;
        }

        public static NBTTag ProcessPayload(int tagID, byte[] bytes, ref int position, string name = "")
        {
            NBTTag tag;
            switch (tagID)
            {
                case 1: // Signed Byte
                    {
                        tag = new NBTTag(name, (sbyte)bytes[position]);
                        position += 1;
                    }
                    break;
                case 2: // Signed Short
                    {
                        byte[] numBytes = bytes.Skip(position).Take(2).Reverse().ToArray();
                        tag = new NBTTag(name, BitConverter.ToInt16(numBytes, 0));
                        position += 2;
                    }
                    break;
                case 3: // Signed Int
                    {
                        byte[] numBytes = bytes.Skip(position).Take(4).Reverse().ToArray();
                        tag = new NBTTag(name, BitConverter.ToInt32(numBytes, 0));
                        position += 4;
                    }
                    break;
                case 4: // Signed Long
                    {
                        byte[] numBytes = bytes.Skip(position).Take(8).Reverse().ToArray();
                        tag = new NBTTag(name, BitConverter.ToInt64(numBytes, 0));
                        position += 8;
                    }
                    break;
                case 5: // Signed Float
                    {
                        byte[] numBytes = bytes.Skip(position).Take(4).Reverse().ToArray();
                        tag = new NBTTag(name, BitConverter.ToSingle(numBytes, 0));
                        position += 4;
                    }
                    break;
                case 6: // Signed Double
                    {
                        byte[] numBytes = bytes.Skip(position).Take(8).Reverse().ToArray();
                        tag = new NBTTag(name, BitConverter.ToDouble(numBytes, 0));
                        position += 8;
                    }
                    break;
                case 7: // Array of Signed Bytes
                    {
                        byte[] numBytes = bytes.Skip(position).Take(4).Reverse().ToArray();
                        int arrayLength = BitConverter.ToInt32(numBytes, 0);
                        position += 4;
                        sbyte[] sbyteArray = bytes.Skip(position).Take(arrayLength).Select(i => (sbyte)i).ToArray();
                        position += arrayLength;
                        tag = new NBTTag(name, sbyteArray);
                    }
                    break;
                case 8: // String
                    {
                        byte[] numBytes = bytes.Skip(position).Take(2).Reverse().ToArray();
                        int stringLength = BitConverter.ToInt16(numBytes, 0);
                        position += 2;
                        string str = Encoding.UTF8.GetString(bytes, position, stringLength);
                        position += stringLength;
                        tag = new NBTTag(name, str);
                    }
                    break;
                case 9: // Tag List -> recursively solve, same way as compound tags
                    {
                        byte childID = bytes[position];
                        position += 1;
                        byte[] numBytes = bytes.Skip(position).Take(4).Reverse().ToArray();
                        int numberOfTags = BitConverter.ToInt32(numBytes, 0);
                        position += 4;
                        List<NBTTag> children = new List<NBTTag>();
                        for (int i = 0; i < numberOfTags; i++)
                        {
                            children.Add(ProcessPayload(childID, bytes, ref position));
                        }
                        tag = new NBTTag(name, children);
                    }
                    break;
                case 10: // Compund Tag
                    {
                        List<NBTTag> children = Parse(bytes, ref position);
                        tag = new NBTTag(name, children);
                    }
                    break;
                case 11: // Int array
                    {
                        byte[] numBytes = bytes.Skip(position).Take(4).Reverse().ToArray();
                        int arrayLength = BitConverter.ToInt32(numBytes, 0);
                        position += 4;
                        int[] array = new int[arrayLength];
                        for (int i = 0; i < arrayLength; i++)
                        {
                            numBytes = bytes.Skip(position).Take(4).Reverse().ToArray();
                            array[i] = BitConverter.ToInt32(numBytes, 0);
                            position += 4;
                        }
                        tag = new NBTTag(name, array);
                    }
                    break;
                case 12:// Long Array
                    {
                        byte[] numBytes = bytes.Skip(position).Take(4).Reverse().ToArray();
                        int arrayLength = BitConverter.ToInt32(numBytes, 0);
                        position += 4;
                        if (name == "BlockStates")
                        {
                            byte[] array = bytes.Skip(position).Take(arrayLength * 8).ToArray();
                            tag = new NBTTag(name, array);
                            position += arrayLength * 8;
                        }
                        else
                        {
                            long[] array = new long[arrayLength];
                            for (int i = 0; i < arrayLength; i++)
                            {
                                numBytes = bytes.Skip(position).Take(8).Reverse().ToArray();
                                array[i] = BitConverter.ToInt64(numBytes, 0);
                                position += 8;
                            }
                            tag = new NBTTag(name, array);
                        }
                    }
                    break;
                default:
                    {
                        tag = new NBTTag(string.Format("{0} - not Recognised", tagID), null);
                    }
                    break;
            }
            return tag;
        }
    }
}
