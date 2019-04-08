using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using zlib;

namespace nbtj
{
    public static class AnvilParser
    {
        public static void FromBytes(byte[] bytes)
        {
            //Console.WriteLine(BytesToHex(bytes.Take(8).ToArray()));
            for(int x = 0; x < 32; x++)
            {
                for (int z = 0; z < 32; z++)
                {
                    int headerOffset = 4 * ((x & 31) + (z & 31) * 32);
                    //Console.Write(headerOffset.ToString() + "   ");
                    byte[] chunkOffsetBytes = new byte[4];
                    chunkOffsetBytes[0] = 0;
                    chunkOffsetBytes[1] = bytes[headerOffset];
                    chunkOffsetBytes[2] = bytes[headerOffset + 1];
                    chunkOffsetBytes[3] = bytes[headerOffset + 2];

                    chunkOffsetBytes = chunkOffsetBytes.Reverse().ToArray();

                    uint chunkoffset = BitConverter.ToUInt32(chunkOffsetBytes, 0);
                    /*Console.WriteLine("{0}--> ({1:N0}); len: {2}", BytesToHex(chunkOffsetBytes.Reverse()),
                                      chunkoffset, bytes[headerOffset + 3]);*/


                }
            }

            byte[] buffer = bytes.Skip(4096 * 2).Take(4096).ToArray(); //get first chunk in data
            byte[] lengthBytes = buffer.Take(4).Reverse().ToArray();  // turn first 4 bytes into int
            uint compressedLen = BitConverter.ToUInt32(lengthBytes, 0) - 1; // this number is the length of the compressed data (-1 for compression scheme byte)

            byte[] dataBytes = buffer.Skip(5).Take((int)compressedLen).ToArray(); // put data bytes into a buffer

            byte[] decompressedBytes;
            DecompressData(dataBytes, out decompressedBytes); //decompress bytes and place in new buffer

            Console.WriteLine("Orig: {0}, Data: {1}, Deco: {2}", bytes.Length, dataBytes.Length, decompressedBytes.Length);

            Chunk chunk = new Chunk(decompressedBytes);

        }

        public static void FromFile(string path)
        {
            FromBytes(File.ReadAllBytes(path));
        }

        /// <summary>
        /// Turns a byte array into a string of hexes.
        /// </summary>
        /// <param name="bytes">The enumerable of bytes to format.</param>
        /// <returns>A list of each byte as a hex number, seperated by spaces.</returns>
        public static string BytesToHex(IEnumerable<byte> bytes)
        {
            string value = "";
            foreach (var byt in bytes)
                value += String.Format("{0:X2} ", byt);

            return value;
        }

        /// <summary>
        /// Compress a buffer using zlib.
        /// </summary>
        /// <param name="inData">The input buffer.</param>
        /// <param name="outData">The output buffer.</param>
        public static void CompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_DEFAULT_COMPRESSION))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        /// <summary>
        /// Decompress a buffer using zlib.
        /// </summary>
        /// <param name="inData">The input buffer.</param>
        /// <param name="outData">The output buffer.</param>
        public static void DecompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deep copy a stream to another stream
        /// </summary>
        /// <param name="inData">The input stream.</param>
        /// <param name="outData">The output stream.</param>
        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }
    }
}
