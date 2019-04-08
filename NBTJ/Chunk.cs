using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using zlib;
using nbtj;

namespace nbtj
{
    class Chunk
    {
        public NBTTag Tag;
        string[,] BlockMap;

        /// <summary>
        /// Create a chunk from a decompressed NBT byte buffer.
        /// </summary>
        /// <param name="bytes"></param>
        public Chunk(byte[] bytes)
        {
            int position = 0;
            Tag = NBTJ.Parse(bytes, ref position)[0];
            BlockMap = GetBlockMap();
            File.WriteAllText("nbtout.txt", Tag.ToString());
        }

        /// <summary>
        /// Generate the blockmap - must be called aftter the Tag is set
        /// </summary>
        /// <returns>The generated blockmap</returns>
        public string[,] GetBlockMap()
        {
            if (Tag is null)
            {
                throw new NullReferenceException("The Tag of the chunk is not set!");
            }

            // get sections as a stack ordered by Y index
            Stack<NBTTag> sections = new Stack<NBTTag>(((List<NBTTag>)Tag.Search("Sections").Payload)
                                            .OrderBy(element => element.Search("Y").Payload)
                                            );
            // get the highest non-air section in the chunk

            // generate queue of all x,z coordinate paris in the chunk
            List<Tuple<int, int>> unresolved = new List<Tuple<int, int>>();
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    unresolved.Add(new Tuple<int, int>(x, z));
                }
            }
            string[,] blockMap = new string[16,16];

            // repeat this until all blocks are resolved or run out of sections.
            do
            {
                if (sections.Count <= 0)
                {
                    break;
                }
                // shallow copy unresolved coords to coordinate queue then reset unresolved queue
                List<Tuple<int, int>> coords = new List<Tuple<int, int>>(unresolved);
                unresolved = new List<Tuple<int, int>>();

                // Get next highest non-empty section of chunk
                NBTTag topSection = sections.Pop();
                while (((List<NBTTag>)topSection.Search("Palette").Payload).Count <= 1)
                {
                    topSection = sections.Pop();
                }

                Console.WriteLine(topSection);

                // Parse the pallete
                string[] pallete = ParsePallete(topSection.Search("Palette"));

                // get bytes containing block states and calculate bits per block
                byte[] blockStateBytes = (byte[])topSection.Search("BlockStates").Payload;
                int bitsPerBlock = NextPowerOfTwo(pallete.Length);
                //Console.WriteLine("Bits per block: {0}, {1}", bitsPerBlock, pallete.Length);

                for (int i = 0; i < coords.Count; i++)
                {
                    // for each x,z pair, find highest block in the section that isn't air
                    int y = 15;
                    string block = "";
                    do
                    {
                        if (y < 0) //if all block in column are empty, add to unersolved queue
                        {
                            unresolved.Add(coords[i]);
                            break;
                        }
                        block = pallete[NumFromBytes(bitsPerBlock, y * 256 + coords[i].Item2 * 16 + coords[i].Item1, blockStateBytes)];
                        y--;
                    } while (block == "minecraft:air");

                    // write block to the 2d array. if empty, air is written to it.
                    blockMap[coords[i].Item1, coords[i].Item2] = block;
                }
                Console.WriteLine("Unresolved: {0}", unresolved.Count);
            }
            while (unresolved.Count > 0);
            return blockMap;
        }

        /// <summary>
        /// Parse the block pallete from a pallete NBTTag
        /// </summary>
        /// <param name="palleteTag">The tag to parse fom</param>
        /// <returns>A list of the blocks in the pallete as strings</returns>
        private string[] ParsePallete(NBTTag palleteTag)
        {
            List<string> pallete = new List<string>();
            foreach(NBTTag item in (List<NBTTag>)palleteTag.Payload)
            {
                NBTTag stringTag = ((List<NBTTag>)item.Payload)[0];
                pallete.Add((string)stringTag.Payload);
            }
            return pallete.ToArray();
        }

        /// <summary>
        /// Gets the next lowest power of two above the given number
        /// </summary>
        /// <param name="n"></param>
        /// <returns>The next lowest power of two</returns>
        private int NextPowerOfTwo(int n)
        {
            int count = 0;
            while (n != 0)
            {
                n = n >> 1;
                count++;
            }
            return 1 << count;
        }

        /// <summary>
        /// Converts n bits to a number from a byte array.
        /// </summary>
        /// <param name="bits">How many bits to convert.</param>
        /// <param name="location">The location of the bits, i.e. how many n bit blocks in is the number.</param>
        /// <param name="bytes">The byte array to convert to a number.</param>
        /// <returns>The number from the byte array.</returns>
        public static int NumFromBytes(int bits, int location, byte[] bytes)
        {
            // calcualte which byte and bit address we will be using
            int bitsIn = location * bits;
            int byteAddr = bitsIn / 8;
            int bitAddr = bitsIn % 8;

            int result = 0;
            // used to get correct byte
            for (int bitsRead = 0; bitsRead < bits; byteAddr++)
            {
                // used to read individual bits, will not read past end of byte
                for (; bitsRead < bits && bitAddr < 8; bitsRead++, bitAddr++)
                {
                    // get bit, bugs may be here cos the first bit in a byte is 0 (google it, makes sense ish)
                    int bit = ByteAsArray(bitAddr, bytes[byteAddr]);
                    result += bit << (bits - bitsRead - 1);
                }

                // reset bit address
                bitAddr = 0;
            }

            //Console.WriteLine("{0}", result); // shows the final result for ya

            return result;
        }

        /// <summary>
        /// Access the bits of a byte by index.
        /// </summary>
        /// <param name="index">Index, starts at the most significant bit.</param>
        /// <param name="byt">The byte to access.</param>
        /// <param name="debug">Optionaly print debug info.</param>
        /// <returns>The specified bit as an int - either 1 or 0</returns>
        private static int ByteAsArray(int index, byte byt, bool debug = false)
        {
            int mask = 128 >> index;          
            int output = ((mask & byt) != 0) ? 1 : 0;
            if (debug)
            {
                Console.WriteLine("Byte: {0}", ToBin(byt));
                Console.WriteLine("Mask: {0}", ToBin(mask));
                Console.WriteLine("And:  {0}", ToBin(byt & mask));
                Console.WriteLine("Out: {0}\n", output);
            }
            return output;
        }

        /// <summary>
        /// convert an int to a string in binary format
        /// </summary>
        /// <param name="value">The integer to convert</param>
        /// <param name="len">Optional length of the binary string deafult 8 for one byte</param>
        /// <returns>The converted and formatted string</returns>
        private static string ToBin(int value, int len = 8)
        {
            return (len > 1 ? ToBin(value >> 1, len - 1) : null) + "01"[value & 1];
        }
    }
}
