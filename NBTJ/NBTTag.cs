using System;
using System.Collections.Generic;

namespace nbtj
{
    public class NBTTag
    {
        public string Name;
        public object Payload;

        public NBTTag(string name, object payload)
        {
            Name = name;
            Payload = payload;
        }

        public string GenString(int depth = 0)
        {
            string str = "";
            str += Tabs(depth) + Name + ": ";
            switch (Payload) {
                case List<NBTTag> tags:
                    str += "\n";
                    foreach (NBTTag tag in tags)
                    {
                        str += tag.GenString(depth + 1);
                    }
                    break;
                case sbyte[] sbytes:
                    str += string.Format("<{0}>: [{1}]", sbytes.GetType(), string.Join(", ", sbytes));
                    break;
                case int[] ints:
                    str += string.Format("<{0}>: [{1}]", ints.GetType(), string.Join(", ", ints));
                    break;
                case long[] longs:
                    str += string.Format("<{0}>: [{1}]", longs.GetType(), string.Join(", ", longs));
                    break;
                default:
                    str += string.Format("<{0}>: {1}", Payload.GetType(), Payload);
                    break;
            }
            str += "\n";

            return str;
        }

        public override string ToString()
        {
            return GenString(0);
        }

        private static string Tabs(int depth)
        {
            return new String('\t', depth);
        }

        public NBTTag Search(string name)
        {
            if (Name == name)
            {
                return this;
            }
            else
            {
                if (Payload is List<NBTTag>)
                {
                    foreach (NBTTag tag in (List<NBTTag>)Payload)
                    {
                        if (tag.Search(name) != null)
                        {
                            return tag.Search(name);
                        }
                    }
                }
            }
            return null;
        }
    }
}
