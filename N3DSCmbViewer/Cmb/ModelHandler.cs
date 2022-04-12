using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace N3DSCmbViewer.Cmb
{
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class ModelHandler : IDisposable
    {
        public bool Disposed { get; private set; }

        const int vertBoneTexUnit = 7;

        bool ready;

        /* Shared shader */
        int vertexObject;

        /* For normal rendering */
        int fragmentObject, program;
        int tex0Location, tex1Location, tex2Location;
        int materialColorLocation;
        int skinningModeLocation, boneIdLocation;
        int vertBoneBufferId, vertBoneTexId;
        int vertBoneSamplerLocation;

        /* Component scaling */
        int vertexScaleLocation;
        int texCoordScaleLocation;
        int normalScaleLocation;

        /* Misc stuffs */
        int disableAlphaLocation;
        int enableLightingLocation;
        int enableSkeletalStuffLocation;
        int emptyTexture;

        /* For wireframe overlay */
        int fragmentObjectOverlay, programOverlay;
        int skinningModeLocationOverlay, boneIdLocationOverlay;
        int vertBoneSamplerLocationOverlay;
        int vertexScaleLocationOverlay;
        int enableSkeletalStuffLocationOverlay;

        /* Buffer caches */
        Dictionary<SepdChunk, int> vertexBufferObjects, normalBufferObjects, colorBufferObjects, texCoordBufferObjects;
        Dictionary<PrmsChunk, int> elementBufferObjects;

        public string Filename { get; set; }
        public byte[] Data { get; private set; }

        public CmbChunk Root { get; private set; }

        public ModelHandler(string filename)
        {
            Filename = filename;
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
            Data = new byte[reader.BaseStream.Length];
            reader.Read(Data, 0, Data.Length);
            reader.Close();

            Load();
        }

        public ModelHandler(byte[] data, int offset, int length)
        {
            Filename = string.Empty;
            Data = new byte[length];
            System.Buffer.BlockCopy(data, offset, Data, 0, Data.Length);

            Load();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0} - Model log for {1} - {2}\n", Program.Description, Path.GetFileName(Filename), DateTime.Now);
            sb.AppendLine();
            sb.Append(Root.ToString());

            return sb.ToString();
        }

        ~ModelHandler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Root.TexChunk.Dispose();

                    if (GL.IsProgram(program)) GL.DeleteProgram(program);
                    if (GL.IsProgram(programOverlay)) GL.DeleteProgram(programOverlay);

                    if (GL.IsShader(fragmentObject)) GL.DeleteShader(fragmentObject);
                    if (GL.IsShader(fragmentObjectOverlay)) GL.DeleteShader(fragmentObjectOverlay);

                    if (GL.IsShader(vertexObject)) GL.DeleteShader(vertexObject);

                    if (GL.IsTexture(emptyTexture)) GL.DeleteTexture(emptyTexture);

                    if (GL.IsBuffer(vertBoneBufferId)) GL.DeleteBuffer(vertBoneBufferId);
                    if (GL.IsTexture(vertBoneTexId)) GL.DeleteTexture(vertBoneTexId);

                    if (vertexBufferObjects != null)
                        foreach (KeyValuePair<SepdChunk, int> bufferObject in vertexBufferObjects.Where(x => GL.IsBuffer(x.Value)))
                            GL.DeleteBuffer(bufferObject.Value);
                    if (normalBufferObjects != null)
                        foreach (KeyValuePair<SepdChunk, int> bufferObject in normalBufferObjects.Where(x => GL.IsBuffer(x.Value)))
                            GL.DeleteBuffer(bufferObject.Value);
                    if (colorBufferObjects != null)
                        foreach (KeyValuePair<SepdChunk, int> bufferObject in colorBufferObjects.Where(x => GL.IsBuffer(x.Value)))
                            GL.DeleteBuffer(bufferObject.Value);
                    if (texCoordBufferObjects != null)
                        foreach (KeyValuePair<SepdChunk, int> bufferObject in texCoordBufferObjects.Where(x => GL.IsBuffer(x.Value)))
                            GL.DeleteBuffer(bufferObject.Value);
                    if (elementBufferObjects != null)
                        foreach (KeyValuePair<PrmsChunk, int> bufferObject in elementBufferObjects.Where(x => GL.IsBuffer(x.Value)))
                            GL.DeleteBuffer(bufferObject.Value);
                }

                Disposed = true;
            }
        }

        public void Load()
        {
            Disposed = ready = false;

            fragmentObject = vertexObject = program = -1;
            tex0Location = tex1Location = tex2Location = -1;
            materialColorLocation = -1;

            skinningModeLocation = boneIdLocation = -1;
            vertBoneBufferId = vertBoneTexId = -1;
            vertBoneSamplerLocation = -1;

            vertexScaleLocation = -1;
            texCoordScaleLocation = -1;
            normalScaleLocation = -1;

            disableAlphaLocation = -1;
            enableLightingLocation = -1;
            enableSkeletalStuffLocation = -1;

            skinningModeLocationOverlay = boneIdLocationOverlay = -1;
            vertBoneSamplerLocationOverlay = -1;
            vertexScaleLocationOverlay = -1;
            enableSkeletalStuffLocationOverlay = -1;

            Root = new CmbChunk(Data, 0, null);
        }

        public void PrepareGL()
        {
            if (!Root.TexChunk.AreTexturesLoaded) Root.TexChunk.LoadTextures();

            if (!GL.IsTexture(emptyTexture))
            {
                emptyTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, emptyTexture);
                byte[] empty = new byte[]
                {
                    255, 255, 255, 255,
                    255, 255, 255, 255,
                    255, 255, 255, 255,
                    255, 255, 255, 255
                };
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 2, 2, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, empty);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }

            if (!GL.IsProgram(program))
            {
                string fsOverlay =
                    "#version 110\n" +
                    "\n" +
                    "void main()\n" +
                    "{\n" +
                    "    gl_FragColor = vec4(1.0, 1.0, 1.0, 0.5);\n" +
                    "}\n";

                Aglex.GLSL.CreateFragmentShader(ref fragmentObject, File.Open("general-fs.glsl", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                Aglex.GLSL.CreateFragmentShader(ref fragmentObjectOverlay, fsOverlay);
                Aglex.GLSL.CreateVertexShader(ref vertexObject, File.Open("general-vs.glsl", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));

                Aglex.GLSL.CreateProgram(ref program, fragmentObject, vertexObject);

                skinningModeLocation = GL.GetUniformLocation(program, "skinningMode");
                boneIdLocation = GL.GetUniformLocation(program, "boneId");
                vertBoneSamplerLocation = GL.GetUniformLocation(program, "vertBoneSampler");

                tex0Location = GL.GetUniformLocation(program, "tex0");
                tex1Location = GL.GetUniformLocation(program, "tex1");
                tex2Location = GL.GetUniformLocation(program, "tex2");

                materialColorLocation = GL.GetUniformLocation(program, "materialColor");

                vertexScaleLocation = GL.GetUniformLocation(program, "vertexScale");
                texCoordScaleLocation = GL.GetUniformLocation(program, "texCoordScale");
                normalScaleLocation = GL.GetUniformLocation(program, "normalScale");
                disableAlphaLocation = GL.GetUniformLocation(program, "disableAlpha");
                enableLightingLocation = GL.GetUniformLocation(program, "enableLighting");
                enableSkeletalStuffLocation = GL.GetUniformLocation(program, "enableSkeletalStuff");

                Aglex.GLSL.CreateProgram(ref programOverlay, fragmentObjectOverlay, vertexObject);
                vertexScaleLocationOverlay = GL.GetUniformLocation(programOverlay, "vertexScale");
                skinningModeLocationOverlay = GL.GetUniformLocation(programOverlay, "skinningMode");
                boneIdLocationOverlay = GL.GetUniformLocation(programOverlay, "boneId");
                vertBoneSamplerLocationOverlay = GL.GetUniformLocation(programOverlay, "vertBoneSampler");
                enableSkeletalStuffLocationOverlay = GL.GetUniformLocation(programOverlay, "enableSkeletalStuff");
            }

            if (vertBoneBufferId == -1)
            {
                vertBoneBufferId = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.TextureBuffer, vertBoneBufferId);
                GL.BufferData(BufferTarget.TextureBuffer, new IntPtr(0x10000), IntPtr.Zero, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.TextureBuffer, 0);
            }

            if (vertBoneTexId == -1)
            {
                vertBoneTexId = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture0 + vertBoneTexUnit);
                GL.BindTexture(TextureTarget.TextureBuffer, vertBoneTexId);
                GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32ui, vertBoneBufferId);
                GL.BindTexture(TextureTarget.TextureBuffer, 0);
            }

            if (vertexBufferObjects == null) PrepareBuffers();

            ready = true;
        }

        private void PrepareBuffers()
        {
            vertexBufferObjects = new Dictionary<SepdChunk, int>();
            normalBufferObjects = new Dictionary<SepdChunk, int>();
            colorBufferObjects = new Dictionary<SepdChunk, int>();
            texCoordBufferObjects = new Dictionary<SepdChunk, int>();
            elementBufferObjects = new Dictionary<PrmsChunk, int>();

            foreach (MshsChunk.Mesh mesh in Root.SklmChunk.MshsChunk.Meshes)
            {
                int size, expectedSize;

                SepdChunk sepd = Root.SklmChunk.ShpChunk.SepdChunks[mesh.SepdID];

                if (Root.VatrChunk.Vertices.Length > 0)
                {
                    int newVertexBuffer = GL.GenBuffer();

                    GL.BindBuffer(BufferTarget.ArrayBuffer, newVertexBuffer);
                    if (sepd.VertexArrayDataType == Constants.PicaDataType.Byte || sepd.VertexArrayDataType == Constants.PicaDataType.UnsignedByte)
                    {
                        short[] converted = new short[Root.VatrChunk.Vertices.Length];
                        expectedSize = (converted.Length * sizeof(ushort));
                        for (int i = 0; i < Root.VatrChunk.Vertices.Length; i++) converted[i] = Root.VatrChunk.Vertices[i];
                        GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(expectedSize), ref converted[sepd.VertexArrayOffset], BufferUsageHint.StaticDraw);
                    }
                    else
                    {
                        expectedSize = (int)(Root.VatrChunk.Vertices.Length - sepd.VertexArrayOffset);
                        GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(expectedSize), ref Root.VatrChunk.Vertices[sepd.VertexArrayOffset], BufferUsageHint.StaticDraw);
                    }

                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
                    if (size != expectedSize) throw new ApplicationException(string.Format("Problem uploading vertices to VBO; tried {0} bytes, uploaded {1}", expectedSize, size));

                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                    vertexBufferObjects.Add(sepd, newVertexBuffer);
                }

                if (Root.VatrChunk.Normals.Length > 0)
                {
                    int newNormalBuffer = GL.GenBuffer();

                    expectedSize = (int)(Root.VatrChunk.Normals.Length - sepd.NormalArrayOffset);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, newNormalBuffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(expectedSize), ref Root.VatrChunk.Normals[sepd.NormalArrayOffset], BufferUsageHint.StaticDraw);

                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
                    if (size != expectedSize) throw new ApplicationException(string.Format("Problem uploading normals to VBO; tried {0} bytes, uploaded {1}", expectedSize, size));

                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                    normalBufferObjects.Add(sepd, newNormalBuffer);
                }

                if (Root.VatrChunk.Colors.Length > 0)
                {
                    int newColorBuffer = GL.GenBuffer();

                    expectedSize = (int)(Root.VatrChunk.Colors.Length - sepd.ColorArrayOffset);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, newColorBuffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(expectedSize), ref Root.VatrChunk.Colors[sepd.ColorArrayOffset], BufferUsageHint.StaticDraw);

                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
                    if (size != expectedSize) throw new ApplicationException(string.Format("Problem uploading colors to VBO; tried {0} bytes, uploaded {1}", expectedSize, size));

                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                    colorBufferObjects.Add(sepd, newColorBuffer);
                }

                if (Root.VatrChunk.TextureCoords.Length > 0)
                {
                    int newTexCoordBuffer = GL.GenBuffer();

                    GL.BindBuffer(BufferTarget.ArrayBuffer, newTexCoordBuffer);
                    if (sepd.TextureCoordArrayDataType == Constants.PicaDataType.Byte || sepd.TextureCoordArrayDataType == Constants.PicaDataType.UnsignedByte)
                    {
                        short[] converted = new short[Root.VatrChunk.TextureCoords.Length];
                        expectedSize = (converted.Length * sizeof(ushort));
                        for (int i = 0; i < Root.VatrChunk.TextureCoords.Length; i++) converted[i] = Root.VatrChunk.TextureCoords[i];
                        GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(expectedSize), ref converted[sepd.TextureCoordArrayOffset], BufferUsageHint.StaticDraw);
                    }
                    else
                    {
                        expectedSize = (int)(Root.VatrChunk.TextureCoords.Length - sepd.TextureCoordArrayOffset);
                        GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(expectedSize), ref Root.VatrChunk.TextureCoords[sepd.TextureCoordArrayOffset], BufferUsageHint.StaticDraw);
                    }

                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
                    if (size != expectedSize) throw new ApplicationException(string.Format("Problem uploading texcoords to VBO; tried {0} bytes, uploaded {1}", expectedSize, size));

                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                    texCoordBufferObjects.Add(sepd, newTexCoordBuffer);
                }

                foreach (PrmsChunk prms in sepd.PrmsChunks)
                {
                    int newElementBuffer = GL.GenBuffer();

                    expectedSize = (prms.PrmChunk.NumberOfIndices * prms.PrmChunk.ElementSize);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, newElementBuffer);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(expectedSize), ref Root.Indices[prms.PrmChunk.FirstIndex * sizeof(ushort)], BufferUsageHint.StaticDraw);

                    GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
                    if (size != expectedSize) throw new ApplicationException(string.Format("Problem uploading indices to VBO; tried {0} bytes, uploaded {1}", expectedSize, size));

                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                    elementBufferObjects.Add(prms, newElementBuffer);
                }
            }
        }

        public void Render(short meshToRender, Csab.AnimHandler animHandler = null)
        {
            if (Disposed) throw new ObjectDisposedException(string.Format("{0} was disposed", this.GetType().Name));

            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.PushClientAttrib(ClientAttribMask.ClientAllAttribBits);

            /* If we haven't yet, prepare some OGL stuff (shaders etc) */
            if (!ready) PrepareGL();

            /* Shaders or no shaders? */
            if (Properties.Settings.Default.DisableAllShaders)
                GL.UseProgram(0);
            else
                GL.UseProgram(program);

            /* Some setup if we want the wireframe overlay... */
            if (Properties.Settings.Default.AddWireframeOverlay)
            {
                GL.Enable(EnableCap.PolygonOffsetFill);
                GL.PolygonOffset(2.0f, 2.0f);
            }
            else
                GL.Disable(EnableCap.PolygonOffsetFill);

            /* Determine what to render... */
            List<MshsChunk.Mesh> renderList = new List<MshsChunk.Mesh>();
            if (meshToRender != -1)
            {
                renderList.Add(Root.SklmChunk.MshsChunk.Meshes[meshToRender]);
            }
            else
            {
                renderList.AddRange(Root.SklmChunk.MshsChunk.Meshes);
                renderList = renderList.OrderBy(x => x.Unknown).ToList();
            }

            /* Render meshes normally */
            foreach (MshsChunk.Mesh mesh in renderList)
            {
                if (mesh.SepdID > Root.SklmChunk.ShpChunk.SepdChunks.Length) continue;
                if (mesh.MaterialID > Root.MatsChunk.Materials.Length) continue;
                if (mesh.MaterialID > Root.MatsChunk.TextureEnvSettings.Length) continue;

                SepdChunk sepd = Root.SklmChunk.ShpChunk.SepdChunks[mesh.SepdID];
                MatsChunk.Material mat = Root.MatsChunk.Materials[mesh.MaterialID];
                MatsChunk.TextureEnvSetting tenv = Root.MatsChunk.TextureEnvSettings[mesh.MaterialID];

                /* Blend, Alphatest, etc (likely incorrect) */
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc((BlendingFactor)mat.BlendingFactorSrc, (BlendingFactor)mat.BlendingFactorDest);
                GL.BlendColor(1.0f, 1.0f, 1.0f, mat.BlendColorA);
                if (mat.AlphaTestEnable) GL.Enable(EnableCap.AlphaTest);
                else GL.Disable(EnableCap.AlphaTest);
                GL.AlphaFunc(mat.AlphaFunction, mat.AlphaReference);

                /* Texenv stuff testing, tho it doesn't work w/ shaders anyway, I dunno */
                /* Probably needs seperate shader per material, where the texenv stuff gets simulated... */
                /* And yes, I know this doesn't map 100% to standard OpenGL anyway */
                if (false)
                {
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Combine);

                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb, (int)tenv.CombineRgb);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb, (int)tenv.SourceRgb[0]); // OpenTK, Y U name this one differently?
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Rgb, (int)tenv.SourceRgb[1]);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src2Rgb, (int)tenv.SourceRgb[2]);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Rgb, (int)tenv.OperandRgb[0]);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand1Rgb, (int)tenv.OperandRgb[1]);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand2Rgb, (int)tenv.OperandRgb[2]);

                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineAlpha, (int)tenv.CombineAlpha);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src0Alpha, (int)tenv.SourceAlpha[0]);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Alpha, (int)tenv.SourceAlpha[1]);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src2Alpha, (int)tenv.SourceAlpha[2]);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Alpha, (int)tenv.OperandAlpha[0]);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand1Alpha, (int)tenv.OperandAlpha[1]);
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand2Alpha, (int)tenv.OperandAlpha[2]);
                }

                /* Apply textures :P */
                ApplyTextures(mat);

                /* Setup component arrays */
                GL.DisableClientState(ArrayCap.NormalArray);
                GL.DisableClientState(ArrayCap.ColorArray);
                GL.DisableClientState(ArrayCap.TextureCoordArray);
                GL.DisableClientState(ArrayCap.VertexArray);

                SetupNormalArray(sepd);
                SetupColorArray(sepd);
                SetupTextureCoordArray(sepd);
                SetupVertexArray(sepd);

                /* Send stuff to shader */
                GL.Uniform1(boneIdLocation, 0);

                GL.Uniform1(vertBoneSamplerLocation, vertBoneTexUnit);

                GL.Uniform1(tex0Location, 0);
                GL.Uniform1(tex1Location, 1);
                GL.Uniform1(tex2Location, 2);

                //GL.Uniform4(materialColorLocation, 1.0f, 1.0f, 1.0f, mat.Float158);
                GL.Uniform4(materialColorLocation, 1.0f, 1.0f, 1.0f, 1.0f);

                GL.Uniform1(vertexScaleLocation, sepd.VertexArrayScale);
                GL.Uniform1(texCoordScaleLocation, sepd.TextureCoordArrayScale);
                GL.Uniform1(normalScaleLocation, sepd.NormalArrayScale);

                GL.Uniform1(disableAlphaLocation, Convert.ToInt16(Properties.Settings.Default.DisableAlpha));
                GL.Uniform1(enableLightingLocation, Convert.ToInt16(Properties.Settings.Default.EnableLighting));
                GL.Uniform1(enableSkeletalStuffLocation, Convert.ToInt16(Properties.Settings.Default.EnableSkeletalStuff));

                /* Render each prms chunk */
                foreach (PrmsChunk prms in sepd.PrmsChunks)
                {
                    PrepareBoneInformation(sepd, prms);
                    GL.Uniform1(skinningModeLocation, Convert.ToInt16(prms.SkinningMode));
                    RenderBuffer(prms);
                }

                /* Clean up arrays */
                GL.DisableClientState(ArrayCap.NormalArray);
                GL.DisableClientState(ArrayCap.ColorArray);
                GL.DisableClientState(ArrayCap.TextureCoordArray);
                GL.DisableClientState(ArrayCap.VertexArray);
            }

            /* Render wireframe overlay */
            if (Properties.Settings.Default.AddWireframeOverlay && !Properties.Settings.Default.DisableAllShaders)
            {
                GL.UseProgram(programOverlay);

                foreach (MshsChunk.Mesh mesh in renderList)
                {
                    GL.Disable(EnableCap.PolygonOffsetFill);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);

                    SepdChunk sepd = Root.SklmChunk.ShpChunk.SepdChunks[mesh.SepdID];
                    GL.Uniform1(vertexScaleLocationOverlay, sepd.VertexArrayScale);
                    GL.Uniform1(enableSkeletalStuffLocationOverlay, Convert.ToInt16(Properties.Settings.Default.EnableSkeletalStuff));

                    SetupVertexArray(sepd);

                    GL.Uniform1(boneIdLocationOverlay, 0);
                    GL.Uniform1(vertBoneSamplerLocationOverlay, vertBoneTexUnit);

                    foreach (PrmsChunk prms in sepd.PrmsChunks)
                    {
                        PrepareBoneInformation(sepd, prms);
                        GL.Uniform1(skinningModeLocationOverlay, Convert.ToInt16(prms.SkinningMode));
                        RenderBuffer(prms);
                    }

                    GL.DisableClientState(ArrayCap.NormalArray);
                    GL.DisableClientState(ArrayCap.ColorArray);
                    GL.DisableClientState(ArrayCap.TextureCoordArray);
                    GL.DisableClientState(ArrayCap.VertexArray);
                }
            }

            /* Reset stuff */
            GL.Disable(EnableCap.AlphaTest);
            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            /* Render skeleton */
            //if (false)
            {
                // TODO  get rid of immediate mode
                GL.PushAttrib(AttribMask.AllAttribBits);
                GL.Disable(EnableCap.Texture2D);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Lighting);
                GL.PointSize(10.0f);
                GL.LineWidth(5.0f);
                GL.UseProgram(0);
                GL.Color4(Color4.Red);
                GL.Begin(PrimitiveType.Points);
                foreach (SklChunk.Bone bone in Root.SklChunk.Bones) GL.Vertex4(Vector4.Transform(Vector4.One, bone.GetMatrix(true)));
                GL.End();
                GL.Color4(Color4.Blue);
                GL.Begin(PrimitiveType.Lines);
                foreach (SklChunk.Bone bone in Root.SklChunk.Bones.Where(x => x.ParentBone != null))
                {
                    GL.Vertex4(Vector4.Transform(Vector4.One, bone.GetMatrix(true)));
                    GL.Vertex4(Vector4.Transform(Vector4.One, bone.ParentBone.GetMatrix(true)));
                }
                GL.End();
                GL.PopAttrib();
                // END TODO
            }

            GL.PopClientAttrib();
            GL.PopAttrib();
        }

        private void ApplyTextures(MatsChunk.Material mat)
        {
            /* Texture stages */
            for (int i = 0; i < 3; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                if (Properties.Settings.Default.DisableAllShaders)
                    GL.Enable(EnableCap.Texture2D);
                else
                    GL.Disable(EnableCap.Texture2D);

                if (Properties.Settings.Default.EnableTextures && Root.TexChunk.Textures.Length > 0 && mat.TextureIDs[i] != -1)
                {
                    /* Bind texture & set parameters */
                    GL.BindTexture(TextureTarget.Texture2D, Root.TexChunk.Textures[mat.TextureIDs[i]].GLID);

                    if (mat.TextureMinFilters[i] != TextureMinFilter.Linear && mat.TextureMinFilters[i] != TextureMinFilter.Nearest)
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    else
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)mat.TextureMinFilters[i]);

                    if (mat.TextureMagFilters[i] != TextureMagFilter.Linear && mat.TextureMagFilters[i] != TextureMagFilter.Nearest)
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)mat.TextureMagFilters[i]);
                    else
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)mat.TextureMagFilters[i]);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)mat.TextureWrapModeSs[i]);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)mat.TextureWrapModeTs[i]);
                }
                else
                    GL.BindTexture(TextureTarget.Texture2D, emptyTexture);
            }
        }

        private void PrepareBoneInformation(SepdChunk sepd, PrmsChunk prms)
        {
            /* TODO!! B/C HURR DURR, DON'T KNOW  https://www.the-gcn.com/topic/2859-oot-3d-3ds-model-format-discussion/page-3#entry46121 */
            if (prms.SkinningMode == PrmsChunk.SkinningModes.PerVertex)
            {
                uint[] lookupInts = new uint[(int)(Root.VatrChunk.BoneIndexLookup.Length - sepd.BoneIndexLookupArrayOffset) / sepd.BoneIndexLookupSize];
                for (int i = 0; i < lookupInts.Length; i++)
                {
                    switch (sepd.BoneIndexLookupArrayDataType)
                    {
                        case Constants.PicaDataType.Byte:
                        case Constants.PicaDataType.UnsignedByte:
                            lookupInts[i] = (uint)Root.VatrChunk.BoneIndexLookup[sepd.BoneIndexLookupArrayOffset + (i * sizeof(byte))];
                            break;
                        case Constants.PicaDataType.Short:
                        case Constants.PicaDataType.UnsignedShort:
                            lookupInts[i] = (uint)BitConverter.ToUInt16(Root.VatrChunk.BoneIndexLookup, (int)(sepd.BoneIndexLookupArrayOffset + (i * sizeof(ushort))));
                            break;
                        case Constants.PicaDataType.Int:
                        case Constants.PicaDataType.UnsignedInt:
                            lookupInts[i] = (uint)BitConverter.ToUInt32(Root.VatrChunk.BoneIndexLookup, (int)(sepd.BoneIndexLookupArrayOffset + (i * sizeof(uint))));
                            break;
                        case Constants.PicaDataType.Float:
                            lookupInts[i] = (uint)BitConverter.ToSingle(Root.VatrChunk.BoneIndexLookup, (int)(sepd.BoneIndexLookupArrayOffset + (i * sizeof(float))));
                            break;
                    }
                }

                GL.ActiveTexture(TextureUnit.Texture0 + vertBoneTexUnit);
                GL.BindTexture(TextureTarget.TextureBuffer, vertBoneTexId);

                GL.BindBuffer(BufferTarget.TextureBuffer, vertBoneBufferId);
                GL.BufferData<uint>(BufferTarget.TextureBuffer, new IntPtr(lookupInts.Length * sizeof(uint)), lookupInts, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.TextureBuffer, 0);
            }

            /* Maaaaaaybe? I dunno... not using this yet either */
            Vector2[] weights = new Vector2[(int)(Root.VatrChunk.BoneWeights.Length - sepd.BoneWeightArrayOffset) / 2];
            for (int i = 0, j = 0; i < weights.Length; i++, j += 2)
            {
                weights[i] = new Vector2(
                    Root.VatrChunk.BoneWeights[sepd.BoneWeightArrayOffset + j],
                    Root.VatrChunk.BoneWeights[sepd.BoneWeightArrayOffset + (j + 1)]);
            }

            for (int i = 0; i < prms.BoneIndexCount; i++)
            {
                Matrix4 matrix = Matrix4.Identity;

                SklChunk.Bone bone = Root.SklChunk.Bones.FirstOrDefault(x => x.BoneID == prms.BoneIndices[i]);
                if (bone != null) matrix = bone.GetMatrix(prms.SkinningMode != PrmsChunk.SkinningModes.PerVertexNoTrans);

                GL.UniformMatrix4(GL.GetUniformLocation(program, string.Format("boneMatrix[{0}]", i)), false, ref matrix);
            }
        }

        private void SetupNormalArray(SepdChunk sepd)
        {
            if (!normalBufferObjects.ContainsKey(sepd)) return;

            GL.EnableClientState(ArrayCap.NormalArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalBufferObjects[sepd]);
            GL.NormalPointer(sepd.NormalPointerType, 0, IntPtr.Zero);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void SetupColorArray(SepdChunk sepd)
        {
            if (!colorBufferObjects.ContainsKey(sepd)) return;

            GL.EnableClientState(ArrayCap.ColorArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBufferObjects[sepd]);
            GL.ColorPointer(4, sepd.ColorPointerType, 0, IntPtr.Zero);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void SetupTextureCoordArray(SepdChunk sepd)
        {
            if (!texCoordBufferObjects.ContainsKey(sepd)) return;

            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, texCoordBufferObjects[sepd]);
            GL.TexCoordPointer(2, sepd.TexCoordPointerType, 0, IntPtr.Zero);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void SetupVertexArray(SepdChunk sepd)
        {
            if (!vertexBufferObjects.ContainsKey(sepd)) return;

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObjects[sepd]);
            GL.VertexPointer(3, sepd.VertexPointerType, 0, IntPtr.Zero);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void RenderBuffer(PrmsChunk prms)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObjects[prms]);
            GL.DrawElements(PrimitiveType.Triangles, prms.PrmChunk.NumberOfIndices, prms.PrmChunk.DrawElementsType, IntPtr.Zero);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
