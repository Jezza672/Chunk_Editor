using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Chunk_Editor
{
    class Program
    {
        static void Main(string[] args)
        {
            ChunkEditor game = new ChunkEditor(800, 600);
            game.Run();
        }
    }
}
