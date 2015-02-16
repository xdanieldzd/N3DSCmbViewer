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

        bool ready;

        /* Shared shader */
        int vertexObject;

        /* For normal rendering */
        int fragmentObject, program;
        int materialColorLocation;
        int skinningModeLocation, boneIdLocation;
        int vertBoneBufferId, vertBoneTexId;
        int vertBoneSamplerLocation;
        int elementArrayBufferId;

        /* Component scaling */
        int vertexScaleLocation;
        int texCoordScaleLocation;
        int normalScaleLocation;

        /* Misc stuffs */
        int disableAlphaLocation;
        int enableLightingLocation;
        int emptyTexture;

        /* For wireframe overlay */
        int fragmentObjectOverlay, programOverlay;
        int skinningModeLocationOverlay, boneIdLocationOverlay;
        int vertBoneSamplerLocationOverlay;
        int vertexScaleLocationOverlay;

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
            Buffer.BlockCopy(data, offset, Data, 0, Data.Length);

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

                    if (GL.IsBuffer(elementArrayBufferId)) GL.DeleteBuffer(elementArrayBufferId);
                }

                Disposed = true;
            }
        }

        public void Load()
        {
            Disposed = ready = false;

            fragmentObject = vertexObject = program = -1;
            materialColorLocation = -1;

            skinningModeLocation = boneIdLocation = -1;
            vertBoneBufferId = vertBoneTexId = -1;
            vertBoneSamplerLocation = -1;

            vertexScaleLocation = -1;
            texCoordScaleLocation = -1;
            normalScaleLocation = -1;

            disableAlphaLocation = -1;
            enableLightingLocation = -1;

            skinningModeLocationOverlay = boneIdLocationOverlay = -1;
            vertBoneSamplerLocationOverlay = -1;
            vertexScaleLocationOverlay = -1;

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

                materialColorLocation = GL.GetUniformLocation(program, "materialColor");

                vertexScaleLocation = GL.GetUniformLocation(program, "vertexScale");
                texCoordScaleLocation = GL.GetUniformLocation(program, "texCoordScale");
                normalScaleLocation = GL.GetUniformLocation(program, "normalScale");
                disableAlphaLocation = GL.GetUniformLocation(program, "disableAlpha");
                enableLightingLocation = GL.GetUniformLocation(program, "enableLighting");

                Aglex.GLSL.CreateProgram(ref programOverlay, fragmentObjectOverlay, vertexObject);
                vertexScaleLocationOverlay = GL.GetUniformLocation(programOverlay, "vertexScale");
                skinningModeLocationOverlay = GL.GetUniformLocation(programOverlay, "skinningMode");
                boneIdLocationOverlay = GL.GetUniformLocation(programOverlay, "boneId");
                vertBoneSamplerLocationOverlay = GL.GetUniformLocation(programOverlay, "vertBoneSampler");
            }

            if (vertBoneBufferId == -1)
            {
                vertBoneBufferId = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.TextureBuffer, vertBoneBufferId);
                GL.BufferData(BufferTarget.TextureBuffer, new IntPtr(0x10000), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }

            if (vertBoneTexId == -1)
            {
                vertBoneTexId = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture8);
                GL.BindTexture(TextureTarget.TextureBuffer, vertBoneTexId);
                GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32ui, vertBoneBufferId);
                GL.BindBuffer(BufferTarget.TextureBuffer, 0);
            }

            elementArrayBufferId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementArrayBufferId);

            ready = true;
        }

        /*
         * The function where the "magic" happens... or alternatively, where any programmer with actual experience will shake their collective heads!! :DD
         * No seriously, if anyone decides to improve upon this project, you better fix this piece of shit function here.
         * Part of that requires more research - array formats etc. -, part just not being a lazy fuck like me
         */
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

                SepdChunk sepd = Root.SklmChunk.ShpChunk.SepdChunks[mesh.SepdID];
                MatsChunk.Material mat = Root.MatsChunk.Materials[mesh.MaterialID];

                if (Properties.Settings.Default.EnableTextures && Root.TexChunk.Textures.Length > 0)
                {
                    /* 1st texture stage */
                    GL.ActiveTexture(TextureUnit.Texture0);
                    if (mat.Stg1TextureID != -1)
                        GL.BindTexture(TextureTarget.Texture2D, Root.TexChunk.Textures[mat.Stg1TextureID].GLID);
                    else
                        GL.BindTexture(TextureTarget.Texture2D, emptyTexture);

                    switch (mat.Stg1TextureMagFilter)
                    {
                        case Constants.TextureMinMagFilter.GL_NEAREST: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest); break;
                        case Constants.TextureMinMagFilter.GL_LINEAR: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear); break;
                    }

                    switch (mat.Stg1TextureMinFilter)
                    {
                        case Constants.TextureMinMagFilter.GL_NEAREST:
                        case Constants.TextureMinMagFilter.GL_NEAREST_MIPMAP_LINEAR:
                        case Constants.TextureMinMagFilter.GL_NEAREST_MIPMAP_NEAREST:
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                            break;
                        case Constants.TextureMinMagFilter.GL_LINEAR:
                        case Constants.TextureMinMagFilter.GL_LINEAR_MIPMAP_LINEAR:
                        case Constants.TextureMinMagFilter.GL_LINEAR_MIPMAP_NEAREST:
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                            break;
                    }

                    // EXPERIMENTAL STUFF! -> wrap modes mess up stalfos; alphatest causes some missing textures...?
                    if (false)
                    {
                        switch (mat.Stg1TextureWrapModeS)
                        {
                            case Constants.TextureWrapMode.GL_REPEAT: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); break;
                            case Constants.TextureWrapMode.GL_CLAMP_TO_EDGE: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge); break;
                            case Constants.TextureWrapMode.GL_MIRRORED_REPEAT: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat); break;
                        }

                        switch (mat.Stg1TextureWrapModeT)
                        {
                            case Constants.TextureWrapMode.GL_REPEAT: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat); break;
                            case Constants.TextureWrapMode.GL_CLAMP_TO_EDGE: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge); break;
                            case Constants.TextureWrapMode.GL_MIRRORED_REPEAT: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat); break;
                        }

                        GL.AlphaFunc(AlphaFunction.Greater, 0.9f);
                        GL.Enable(EnableCap.AlphaTest);
                        GL.Enable(EnableCap.Blend);
                    }

                    /* 2nd texture stage (unimplemented) */
                    GL.ActiveTexture(TextureUnit.Texture1);
                    if (mat.MaybeStg2TextureID != -1)
                        GL.BindTexture(TextureTarget.Texture2D, Root.TexChunk.Textures[mat.MaybeStg2TextureID].GLID);
                    else
                        GL.BindTexture(TextureTarget.Texture2D, emptyTexture);
                }
                else
                    GL.BindTexture(TextureTarget.Texture2D, emptyTexture);

                foreach (PrmsChunk prms in sepd.PrmsChunks)
                {
                    // TODO TODO FIXME ->>> convert from vertex arrays to VBOs!!!


                    GL.DisableClientState(ArrayCap.NormalArray);
                    GL.DisableClientState(ArrayCap.ColorArray);
                    GL.DisableClientState(ArrayCap.TextureCoordArray);
                    GL.DisableClientState(ArrayCap.VertexArray);

                    PrepareBoneInformation(sepd, prms);

                    GL.Uniform1(skinningModeLocation, (int)prms.SkinningMode);
                    GL.Uniform1(boneIdLocation, 0);

                    GL.Uniform1(vertBoneSamplerLocation, 8);

                    //GL.Uniform4(materialColorLocation, 1.0f, 1.0f, 1.0f, mat.Float158);
                    GL.Uniform4(materialColorLocation, 1.0f, 1.0f, 1.0f, 1.0f);

                    GL.Uniform1(vertexScaleLocation, sepd.VertexArrayScale);
                    GL.Uniform1(texCoordScaleLocation, sepd.TextureCoordArrayScale);
                    GL.Uniform1(normalScaleLocation, sepd.NormalArrayScale);

                    GL.Uniform1(disableAlphaLocation, Convert.ToInt16(Properties.Settings.Default.DisableAlpha));
                    GL.Uniform1(enableLightingLocation, Convert.ToInt16(Properties.Settings.Default.EnableLighting));

                    SetupNormalArray(sepd);
                    SetupColorArray(sepd);
                    SetupTextureCoordArray(sepd);
                    SetupVertexArray(sepd);

                    RenderBuffer(prms);
                }
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
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                    GL.DisableClientState(ArrayCap.NormalArray);
                    GL.DisableClientState(ArrayCap.ColorArray);
                    GL.DisableClientState(ArrayCap.TextureCoordArray);

                    SepdChunk sepd = Root.SklmChunk.ShpChunk.SepdChunks[mesh.SepdID];
                    GL.Uniform1(vertexScaleLocationOverlay, sepd.VertexArrayScale);

                    foreach (PrmsChunk prms in sepd.PrmsChunks)
                    {
                        PrepareBoneInformation(sepd, prms);

                        GL.Uniform1(skinningModeLocationOverlay, (int)prms.SkinningMode);
                        GL.Uniform1(boneIdLocationOverlay, 0);

                        GL.Uniform1(vertBoneSamplerLocationOverlay, 8);

                        SetupVertexArray(sepd);

                        RenderBuffer(prms);
                    }
                }
            }

            /* Render skeleton */
            //if (false)
            {
                // TODO  get rid of immediate mode
                GL.PushAttrib(AttribMask.AllAttribBits);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Lighting);
                GL.PointSize(10.0f);
                GL.LineWidth(5.0f);
                GL.UseProgram(0);
                GL.Color4(Color4.Red);
                GL.Begin(PrimitiveType.Points);
                foreach (SklChunk.Bone bone in Root.SklChunk.Bones) GL.Vertex3(Vector3.Transform(Vector3.One, bone.GetMatrix()));
                GL.End();
                GL.Color4(Color4.Blue);
                GL.Begin(PrimitiveType.Lines);
                foreach (SklChunk.Bone bone in Root.SklChunk.Bones.Where(x => x.ParentBone != null))
                {
                    GL.Vertex3(Vector3.Transform(Vector3.One, bone.GetMatrix()));
                    GL.Vertex3(Vector3.Transform(Vector3.One, bone.ParentBone.GetMatrix()));
                }
                GL.End();
                GL.PopAttrib();
                // END TODO
            }

            GL.PopClientAttrib();
            GL.PopAttrib();
        }

        private void RenderBuffer(PrmsChunk prms)
        {
            /* S-L-O-W-! */
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementArrayBufferId);
            int size = (prms.PrmChunk.NumberOfIndices * prms.PrmChunk.ElementSize);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(size), ref Root.Indices[prms.PrmChunk.FirstIndex * prms.PrmChunk.ElementSize], BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(size), ref Root.Indices[prms.PrmChunk.FirstIndex * sizeof(ushort)], BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            GL.DrawElements(PrimitiveType.Triangles, size / prms.PrmChunk.ElementSize, prms.PrmChunk.DrawElementsType, IntPtr.Zero);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //GL.DrawElements(PrimitiveType.Triangles, prms.PrmChunk.NumberOfIndices, prms.PrmChunk.DrawElementsType, ref Root.Indices[prms.PrmChunk.FirstIndex * prms.PrmChunk.ElementSize]);
            //GL.DrawElements(PrimitiveType.Triangles, prms.PrmChunk.NumberOfIndices, prms.PrmChunk.DrawElementsType, ref Root.Indices[prms.PrmChunk.FirstIndex * sizeof(ushort)]);
        }

        private void PrepareBoneInformation(SepdChunk sepd, PrmsChunk prms)
        {
            /* TODO!! B/C HURR DURR, DON'T KNOW  https://www.the-gcn.com/topic/2859-oot-3d-3ds-model-format-discussion/page-3#entry46121 */
            if (prms.SkinningMode != PrmsChunk.SkinningModes.SingleBone)
            {
                uint[] lookupInts = new uint[(int)(Root.VatrChunk.BoneIndexLookupSize - sepd.BoneIndexLookupArrayOffset)];
                for (int i = 0; i < lookupInts.Length; i++)
                {
                    switch (sepd.BoneIndexLookupArrayDataType)
                    {
                        case Constants.DataTypes.GL_BYTE:
                        case Constants.DataTypes.GL_UNSIGNED_BYTE:
                            lookupInts[i] = (uint)Root.VatrChunk.BoneIndexLookup[sepd.BoneIndexLookupArrayOffset + (i * sizeof(byte))];
                            break;
                        case Constants.DataTypes.GL_SHORT:
                        case Constants.DataTypes.GL_UNSIGNED_SHORT:
                            lookupInts[i] = (uint)BitConverter.ToUInt16(Root.VatrChunk.BoneIndexLookup, (int)(sepd.BoneIndexLookupArrayOffset + (i * sizeof(ushort))));
                            break;
                        case Constants.DataTypes.GL_INT:
                        case Constants.DataTypes.GL_UNSIGNED_INT:
                            lookupInts[i] = (uint)BitConverter.ToUInt32(Root.VatrChunk.BoneIndexLookup, (int)(sepd.BoneIndexLookupArrayOffset + (i * sizeof(uint))));
                            break;
                        case Constants.DataTypes.GL_FLOAT:
                            lookupInts[i] = (uint)BitConverter.ToSingle(Root.VatrChunk.BoneIndexLookup, (int)(sepd.BoneIndexLookupArrayOffset + (i * sizeof(float))));
                            break;
                    }
                }

                GL.ActiveTexture(TextureUnit.Texture8);
                GL.BindTexture(TextureTarget.TextureBuffer, vertBoneTexId);

                GL.BindBuffer(BufferTarget.TextureBuffer, vertBoneBufferId);
                GL.BufferSubData<uint>(BufferTarget.TextureBuffer, IntPtr.Zero, new IntPtr(lookupInts.Length * sizeof(uint)), lookupInts);
            }

            for (int i = 0; i < prms.BoneIndexCount; i++)
            {
                Matrix4 matrix = Root.SklChunk.Bones[prms.BoneIndices[i]].GetMatrix();

                //TODO FIXME
                if (prms.SkinningMode == PrmsChunk.SkinningModes.PerVertexNoTrans) matrix = matrix.ClearTranslation();

                GL.UniformMatrix4(GL.GetUniformLocation(program, string.Format("boneMatrix[{0}]", i)), false, ref matrix);
            }
        }

        private void SetupNormalArray(SepdChunk sepd)
        {
            if (Root.VatrChunk.Normals.Length > 0)
            {
                GL.EnableClientState(ArrayCap.NormalArray);
                GL.NormalPointer(sepd.NormalPointerType, 0, ref Root.VatrChunk.Normals[sepd.NormalArrayOffset]);
            }
        }

        private void SetupColorArray(SepdChunk sepd)
        {
            if (Root.VatrChunk.Colors.Length > 0)
            {
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.ColorPointer(4, sepd.ColorPointerType, 0, ref Root.VatrChunk.Colors[sepd.ColorArrayOffset]);
            }
        }

        private void SetupTextureCoordArray(SepdChunk sepd)
        {
            if (Root.VatrChunk.TextureCoords.Length > 0)
            {
                GL.EnableClientState(ArrayCap.TextureCoordArray);
                if (sepd.TextureCoordArrayDataType == Constants.DataTypes.GL_BYTE || sepd.TextureCoordArrayDataType == Constants.DataTypes.GL_UNSIGNED_BYTE)
                {
                    short[] converted = new short[Root.VatrChunk.TextureCoords.Length];
                    for (int i = 0; i < Root.VatrChunk.TextureCoords.Length; i++) converted[i] = Root.VatrChunk.TextureCoords[i];
                    GL.TexCoordPointer(2, sepd.TexCoordPointerType, 0, ref converted[sepd.TextureCoordArrayOffset]);
                }
                else
                    GL.TexCoordPointer(2, sepd.TexCoordPointerType, 0, ref Root.VatrChunk.TextureCoords[sepd.TextureCoordArrayOffset]);
            }
        }

        private void SetupVertexArray(SepdChunk sepd)
        {
            if (Root.VatrChunk.Vertices.Length > 0)
            {
                GL.EnableClientState(ArrayCap.VertexArray);
                if (sepd.VertexArrayDataType == Constants.DataTypes.GL_BYTE || sepd.VertexArrayDataType == Constants.DataTypes.GL_UNSIGNED_BYTE)
                {
                    short[] converted = new short[Root.VatrChunk.Vertices.Length];
                    for (int i = 0; i < Root.VatrChunk.Vertices.Length; i++) converted[i] = Root.VatrChunk.Vertices[i];
                    GL.VertexPointer(3, sepd.VertexPointerType, 0, ref converted[sepd.VertexArrayOffset]);
                }
                else
                    GL.VertexPointer(3, sepd.VertexPointerType, 0, ref Root.VatrChunk.Vertices[sepd.VertexArrayOffset]);
            }
        }
    }
}
