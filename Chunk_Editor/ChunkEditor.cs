using OpenTK;
using OpenTK.Graphics.OpenGL;
using nbtj;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Chunk_Editor
{
    class ChunkEditor : GameWindow
    {
        Camera camera;
        public ChunkEditor(int width, int height)
            : base(width, height)
        {
            camera = new Camera(Vector2.Zero, new Vector2(ClientRectangle.Width, ClientRectangle.Height));
            camera.Position = new Vector2(0, 1f);
            AnvilParser.FromFile(@"Resources/r.0.0.mca");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            camera.Update();

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Color.CornflowerBlue);

            Matrix4 viewMatrix = camera.GetViewMatrix();
            GL.LoadMatrix(ref viewMatrix);


            this.SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(this.ClientRectangle);
            camera.Size = new Vector2(ClientRectangle.Width, ClientRectangle.Height);
        }
    }
}
