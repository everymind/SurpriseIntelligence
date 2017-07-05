using Bonsai;
using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SurpriseIntelligence
{
    public class ShowImage : Sink<IplImage>
    {
        const string VertexShader = @"
#version 400
uniform vec2 scale = vec2(1, 1);
uniform vec2 shift;
layout(location = 0) in vec2 vp;
layout(location = 1) in vec2 vt;
out vec2 texCoord;

void main()
{
  gl_Position = vec4(vp * scale + shift, 0.0, 1.0);
  texCoord = vt;
}
";
        const string FragmentShader = @"
#version 400
uniform sampler2D tex;
in vec2 texCoord;
out vec4 fragColor;

void main()
{
  vec4 texel = texture(tex, texCoord);
  fragColor = texel;
}
";

        public WindowState WindowState { get; set; }

        public DisplayIndex Device { get; set; }

        static int CreateShader()
        {
            int status;
            int vertexShader = 0;
            int fragmentShader = 0;
            try
            {
                vertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertexShader, VertexShader);
                GL.CompileShader(vertexShader);
                GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);
                if (status == 0)
                {
                    var message = string.Format(
                        "Failed to compile vertex shader:\n{0}",
                        GL.GetShaderInfoLog(vertexShader));
                    throw new InvalidOperationException(message);
                }

                fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragmentShader, FragmentShader);
                GL.CompileShader(fragmentShader);
                GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);
                if (status == 0)
                {
                    var message = string.Format(
                        "Failed to compile fragment shader:\n{0}",
                        GL.GetShaderInfoLog(fragmentShader));
                    throw new InvalidOperationException(message);
                }

                var shaderProgram = GL.CreateProgram();
                GL.AttachShader(shaderProgram, vertexShader);
                GL.AttachShader(shaderProgram, fragmentShader);
                GL.LinkProgram(shaderProgram);
                GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out status);
                if (status == 0)
                {
                    var message = string.Format(
                        "Failed to link shader program:\n{0}",
                        GL.GetProgramInfoLog(shaderProgram));
                    throw new InvalidOperationException(message);
                }

                return shaderProgram;
            }
            finally
            {
                GL.DeleteShader(fragmentShader);
                GL.DeleteShader(vertexShader);
            }
        }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Create<IplImage>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    IplImage image = null;
                    using (var window = new GameWindow(640, 480, GraphicsMode.Default, null, GameWindowFlags.Default, DisplayDevice.GetDisplay(Device)))
                    using (var notification = cancellationToken.Register(window.Close))
                    using (var imageUpdate = source.Do(input => Interlocked.Exchange(ref image, input)).SubscribeSafe(observer))
                    {
                        int vbo = 0;
                        int vao = 0;
                        int vcount = 0;
                        int shader = 0;
                        int texture = 0;
                        window.WindowState = WindowState;
                        window.Load += (sender, e) =>
                        {
                            shader = CreateShader();
                            GL.GenBuffers(1, out vbo);
                            GL.GenVertexArrays(1, out vao);
                            GL.GenTextures(1, out texture);
                            GL.BindTexture(TextureTarget.Texture2D, texture);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                            vcount = VertexHelper.TexturedQuad(vbo, vao, false, true);
                            var samplerLocation = GL.GetUniformLocation(shader, "tex");
                            GL.Uniform1(samplerLocation, 0);
                        };
                        window.Resize += (sender, e) => GL.Viewport(0, 0, window.Width, window.Height);
                        window.UpdateFrame += (sender, e) =>
                        {
                            var update = Interlocked.Exchange(ref image, null);
                            if (update != null)
                            {
                                TextureHelper.UpdateTexture(texture, PixelInternalFormat.Rgba, update);
                            }
                        };
                        window.RenderFrame += (sender, e) =>
                        {
                            GL.UseProgram(shader);
                            GL.ClearColor(0, 0, 0, 1);
                            GL.Clear(ClearBufferMask.ColorBufferBit);

                            GL.BindVertexArray(vao);
                            GL.ActiveTexture(TextureUnit.Texture0);
                            GL.BindTexture(TextureTarget.Texture2D, texture);
                            GL.DrawArrays(PrimitiveType.Quads, 0, vcount);
                            window.SwapBuffers();
                        };
                        window.Unload += (sender, e) =>
                        {
                            GL.DeleteVertexArrays(1, ref vao);
                            GL.DeleteBuffers(1, ref vbo);
                            GL.DeleteTextures(1, ref texture);
                            GL.DeleteProgram(shader);
                        };
                        window.Run();
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }
    }
}
