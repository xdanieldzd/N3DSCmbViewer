using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace N3DSCmbViewer.Cmb
{
    static class ExportCollada
    {
        public static void Export(string daeFilename, CmbChunk cmbRoot)
        {
            XmlTextWriter xw = new XmlTextWriter(daeFilename, Encoding.UTF8);

            xw.Formatting = Formatting.Indented;
            xw.Indentation = 4;
            xw.WriteStartDocument(false);
            xw.WriteStartElement("COLLADA");
            /*xw.WriteAttributeString("xmlns", "http://www.collada.org/2008/03/COLLADASchema");
            xw.WriteAttributeString("version", "1.5.0");*/
            xw.WriteAttributeString("xmlns", "http://www.collada.org/2005/11/COLLADASchema");
            xw.WriteAttributeString("version", "1.4.1");
            {
                WriteSectionAsset(xw, cmbRoot);

                if (cmbRoot.TexChunk != null && cmbRoot.TexChunk.Textures.Length > 0)
                {
                    WriteSectionLibraryImages(xw, cmbRoot);
                    WriteSectionLibraryEffects(xw, cmbRoot);
                    WriteSectionLibraryMaterials(xw, cmbRoot);
                }

                WriteSectionLibraryGeometry(xw, cmbRoot);
                WriteSectionLibraryVisualScenes(xw, cmbRoot);
                WriteSectionScene(xw, cmbRoot);
            }
            xw.WriteEndElement();

            xw.Close();
        }

        private static void WriteSectionAsset(XmlTextWriter xw, CmbChunk cmbRoot)
        {
            xw.WriteStartElement("asset");
            {
                xw.WriteStartElement("contributor");
                xw.WriteElementString("authoring_tool", System.Windows.Forms.Application.ProductName + " - " + Program.Description);
                xw.WriteEndElement();

                xw.WriteStartElement("created");
                xw.WriteString(DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z");
                xw.WriteEndElement();

                xw.WriteStartElement("modified");
                xw.WriteString(DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z");
                xw.WriteEndElement();

                xw.WriteStartElement("unit");
                xw.WriteAttributeString("meter", "0.01");
                xw.WriteAttributeString("name", "centimeter");
                xw.WriteEndElement();

                xw.WriteStartElement("up_axis");
                xw.WriteString("Y_UP");
                xw.WriteEndElement();
            }
            xw.WriteEndElement();
        }

        private static void WriteSectionLibraryImages(XmlTextWriter xw, CmbChunk cmbRoot)
        {
            xw.WriteStartElement("library_images");
            {
                foreach (TexChunk.Texture tex in cmbRoot.TexChunk.Textures)
                {
                    string texId = string.Format("image-{0}-{1:X8}", tex.Name, tex.GetHashCode());
                    xw.WriteStartElement("image");
                    xw.WriteAttributeString("id", texId);
                    xw.WriteAttributeString("name", texId);
                    {
                        xw.WriteStartElement("init_from");
                        xw.WriteString(string.Format("{0}_{1:X}.png", tex.Name, tex.DataOffset));
                        /*{
                            xw.WriteElementString("ref", string.Format("{0}-{1:X}.png", tex.Name, tex.DataOffset));
                        }*/
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                }
            }
            xw.WriteEndElement();
        }

        private static void WriteSectionLibraryEffects(XmlTextWriter xw, CmbChunk cmbRoot)
        {
            xw.WriteStartElement("library_effects");
            {
                string defaultID = "effect-default";

                xw.WriteStartElement("effect");
                xw.WriteAttributeString("id", defaultID);
                xw.WriteAttributeString("name", defaultID);
                {
                    xw.WriteStartElement("profile_COMMON");
                    {
                        xw.WriteStartElement("technique");
                        xw.WriteAttributeString("sid", "COMMON");
                        {
                            xw.WriteStartElement("phong");
                            {
                                xw.WriteStartElement("diffuse");
                                {
                                    xw.WriteStartElement("color");
                                    xw.WriteString("1.0 1.0 1.0 1.0");
                                    xw.WriteEndElement();
                                }
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();
                        }
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                }
                xw.WriteEndElement();

                foreach (TexChunk.Texture tex in cmbRoot.TexChunk.Textures)
                {
                    string effectID = string.Format("effect-{0}-{1:X8}", tex.Name, tex.GetHashCode());

                    xw.WriteStartElement("effect");
                    xw.WriteAttributeString("id", effectID);
                    xw.WriteAttributeString("name", effectID);
                    {
                        xw.WriteStartElement("profile_COMMON");
                        {
                            xw.WriteStartElement("newparam");
                            xw.WriteAttributeString("sid", string.Format("surface-{0}-{1:X8}", tex.Name, tex.GetHashCode()));
                            {
                                xw.WriteStartElement("surface");
                                xw.WriteAttributeString("type", "2D");
                                {
                                    xw.WriteStartElement("init_from");
                                    xw.WriteString(string.Format("image-{0}-{1:X8}", tex.Name, tex.GetHashCode()));
                                    /*{
                                        xw.WriteElementString("ref", string.Format("image-{0}-{1:X8}", tex.Name, tex.GetHashCode()));
                                    }*/
                                    xw.WriteEndElement();
                                }
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();

                            xw.WriteStartElement("newparam");
                            xw.WriteAttributeString("sid", string.Format("sampler-{0}-{1:X8}", tex.Name, tex.GetHashCode()));
                            {
                                xw.WriteStartElement("sampler2D");
                                {
                                    xw.WriteElementString("source", string.Format("surface-{0}-{1:X8}", tex.Name, tex.GetHashCode()));
                                    /*xw.WriteStartElement("instance_image");
                                    xw.WriteAttributeString("url", string.Format("#image-{0}-{1:X8}", tex.Name, tex.GetHashCode()));
                                    xw.WriteEndElement();*/
                                    xw.WriteElementString("wrap_s", "WRAP");
                                    xw.WriteElementString("wrap_t", "WRAP");
                                    xw.WriteElementString("minfilter", "LINEAR");
                                    xw.WriteElementString("magfilter", "LINEAR");
                                }
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();

                            xw.WriteStartElement("technique");
                            xw.WriteAttributeString("sid", "COMMON");
                            {
                                xw.WriteStartElement("phong");
                                {
                                    xw.WriteStartElement("diffuse");
                                    {
                                        xw.WriteStartElement("texture");
                                        xw.WriteAttributeString("texture", string.Format("sampler-{0}-{1:X8}", tex.Name, tex.GetHashCode()));
                                        xw.WriteAttributeString("texcoord", "TEXCOORD0");
                                        xw.WriteEndElement();
                                    }
                                    xw.WriteEndElement();
                                    /*
                                    xw.WriteStartElement("specular");
                                    {
                                        xw.WriteStartElement("color");
                                        xw.WriteString("0.2 0.2 0.2 1.0");
                                        xw.WriteEndElement();
                                    }
                                    xw.WriteEndElement();

                                    xw.WriteStartElement("shininess");
                                    {
                                        xw.WriteStartElement("float");
                                        xw.WriteString("0.5");
                                        xw.WriteEndElement();
                                    }
                                    xw.WriteEndElement();*/
                                }
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();
                        }
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                }
            }
            xw.WriteEndElement();
        }

        private static void WriteSectionLibraryMaterials(XmlTextWriter xw, CmbChunk cmbRoot)
        {
            xw.WriteStartElement("library_materials");
            {
                foreach (MatsChunk.Material mat in cmbRoot.MatsChunk.Materials)
                {
                    xw.WriteStartElement("material");
                    xw.WriteAttributeString("id", string.Format("material-{0:X8}", mat.GetHashCode()));
                    {
                        xw.WriteStartElement("instance_effect");
                        if (mat.TextureIDs[0] != -1)
                        {
                            TexChunk.Texture tex = cmbRoot.TexChunk.Textures[mat.TextureIDs[0]];
                            xw.WriteAttributeString("url", string.Format("#effect-{0}-{1:X8}", tex.Name, tex.GetHashCode()));
                        }
                        else
                            xw.WriteAttributeString("url", "#effect-default");

                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                }
            }
            xw.WriteEndElement();
        }

        private static void WriteSectionLibraryGeometry(XmlTextWriter xw, CmbChunk cmbRoot)
        {
            xw.WriteStartElement("library_geometries");
            {
                foreach (MshsChunk.Mesh mesh in cmbRoot.SklmChunk.MshsChunk.Meshes)
                {
                    SepdChunk sepd = cmbRoot.SklmChunk.ShpChunk.SepdChunks[mesh.SepdID];
                    MatsChunk.Material mat = cmbRoot.MatsChunk.Materials[mesh.MaterialID];

                    string meshId = string.Format("geom-{0:X8}", mesh.GetHashCode());
                    xw.WriteStartElement("geometry");
                    xw.WriteAttributeString("id", meshId);
                    xw.WriteAttributeString("name", meshId);
                    {
                        xw.WriteStartElement("mesh");
                        {
                            /* Vertices */
                            xw.WriteStartElement("source");
                            xw.WriteAttributeString("id", string.Format("{0}-pos", meshId));
                            {
                                float[] vtxData = ConvertToFloatArray(sepd.VertexArrayDataType, cmbRoot.VatrChunk.Vertices, sepd.VertexArrayOffset, sepd.VertexArrayScale);

                                xw.WriteStartElement("float_array");
                                xw.WriteAttributeString("id", string.Format("{0}-pos-array", meshId));
                                xw.WriteAttributeString("count", string.Format("{0}", vtxData.Length));
                                {
                                    for (int i = 0; i < vtxData.Length; i++)
                                    {
                                        xw.WriteString(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} ", vtxData[i]));
                                    }
                                }
                                xw.WriteEndElement();

                                xw.WriteStartElement("technique_common");
                                {
                                    xw.WriteStartElement("accessor");
                                    xw.WriteAttributeString("source", string.Format("#{0}-pos-array", meshId));
                                    xw.WriteAttributeString("count", string.Format("{0}", vtxData.Length / 3));
                                    xw.WriteAttributeString("stride", "3");
                                    {
                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "X");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();

                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "Y");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();

                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "Z");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();
                                    }
                                    xw.WriteEndElement();
                                }
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();

                            /* Texcoords */
                            xw.WriteStartElement("source");
                            xw.WriteAttributeString("id", string.Format("{0}-texcoord", meshId));
                            {
                                float[] texCoordData = ConvertToFloatArray(sepd.TextureCoordArrayDataType, cmbRoot.VatrChunk.TextureCoords, sepd.TextureCoordArrayOffset, sepd.TextureCoordArrayScale);

                                xw.WriteStartElement("float_array");
                                xw.WriteAttributeString("id", string.Format("{0}-texcoord-array", meshId));
                                xw.WriteAttributeString("count", string.Format("{0}", texCoordData.Length));
                                {
                                    for (int i = 0; i < texCoordData.Length; i++)
                                    {
                                        xw.WriteString(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} ", texCoordData[i]));
                                    }
                                }
                                xw.WriteEndElement();

                                xw.WriteStartElement("technique_common");
                                {
                                    xw.WriteStartElement("accessor");
                                    xw.WriteAttributeString("source", string.Format("#{0}-texcoord-array", meshId));
                                    xw.WriteAttributeString("count", string.Format("{0}", texCoordData.Length / 2));
                                    xw.WriteAttributeString("stride", "2");
                                    {
                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "S");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();

                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "T");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();
                                    }
                                    xw.WriteEndElement();
                                }
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();

                            /* Colors */
                            xw.WriteStartElement("source");
                            xw.WriteAttributeString("id", string.Format("{0}-colors", meshId));
                            {
                                float[] colorData = ConvertToFloatArray(sepd.ColorArrayDataType, cmbRoot.VatrChunk.Colors, sepd.ColorArrayOffset, sepd.ColorArrayScale);

                                xw.WriteStartElement("float_array");
                                xw.WriteAttributeString("id", string.Format("{0}-colors-array", meshId));
                                xw.WriteAttributeString("count", string.Format("{0}", colorData.Length));
                                {
                                    for (int i = 0; i < colorData.Length; i++)
                                    {
                                        xw.WriteString(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} ", colorData[i]));
                                    }
                                }
                                xw.WriteEndElement();

                                xw.WriteStartElement("technique_common");
                                {
                                    xw.WriteStartElement("accessor");
                                    xw.WriteAttributeString("source", string.Format("#{0}-colors-array", meshId));
                                    xw.WriteAttributeString("count", string.Format("{0}", colorData.Length / 4));
                                    xw.WriteAttributeString("stride", "4");
                                    {
                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "R");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();

                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "G");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();

                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "B");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();

                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "A");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();
                                    }
                                    xw.WriteEndElement();
                                }
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();

                            /* Normals */
                            /*xw.WriteStartElement("source");
                            xw.WriteAttributeString("id", string.Format("{0}-norm", meshId));
                            {
                                float[] normData = ConvertToFloatArray(sepd.NormalArrayFormat, cmbRoot.VatrChunk.Normals, sepd.NormalArrayDisplacement, sepd.NormalArrayScale);

                                xw.WriteStartElement("float_array");
                                xw.WriteAttributeString("id", string.Format("{0}-norm-array", meshId));
                                xw.WriteAttributeString("count", string.Format("{0}", normData.Length));
                                {
                                    for (int i = 0; i < normData.Length; i += 3)
                                    {
                                        xw.WriteString(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} {1:0.00} {2:0.00} ", normData[i], normData[i + 1], normData[i + 2]));
                                    }
                                }
                                xw.WriteEndElement();

                                xw.WriteStartElement("technique_common");
                                {
                                    xw.WriteStartElement("accessor");
                                    xw.WriteAttributeString("source", string.Format("#{0}-norm-array", meshId));
                                    xw.WriteAttributeString("count", string.Format("{0}", normData.Length / 3));
                                    xw.WriteAttributeString("stride", "3");
                                    {
                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "X");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();

                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "Y");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();

                                        xw.WriteStartElement("param");
                                        xw.WriteAttributeString("name", "Z");
                                        xw.WriteAttributeString("type", "float");
                                        xw.WriteEndElement();
                                    }
                                    xw.WriteEndElement();
                                }
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();
                            */
                            xw.WriteStartElement("vertices");
                            xw.WriteAttributeString("id", string.Format("{0}-vtx", meshId));
                            {
                                xw.WriteStartElement("input");
                                xw.WriteAttributeString("semantic", "POSITION");
                                xw.WriteAttributeString("source", string.Format("#{0}-pos", meshId));
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();

                            foreach (PrmsChunk prms in sepd.PrmsChunks)
                            {
                                xw.WriteStartElement("triangles");
                                xw.WriteAttributeString("count", string.Format("{0}", prms.PrmChunk.NumberOfIndices));
                                xw.WriteAttributeString("material", string.Format("material-{0:X8}-symbol", mat.GetHashCode()));
                                {
                                    xw.WriteStartElement("input");
                                    xw.WriteAttributeString("semantic", "VERTEX");
                                    xw.WriteAttributeString("source", string.Format("#{0}-vtx", meshId));
                                    xw.WriteAttributeString("offset", "0");
                                    xw.WriteEndElement();

                                    xw.WriteStartElement("input");
                                    xw.WriteAttributeString("semantic", "TEXCOORD");
                                    xw.WriteAttributeString("source", string.Format("#{0}-texcoord", meshId));
                                    xw.WriteAttributeString("offset", "0");
                                    xw.WriteEndElement();

                                    xw.WriteStartElement("input");
                                    xw.WriteAttributeString("semantic", "COLOR");
                                    xw.WriteAttributeString("source", string.Format("#{0}-colors", meshId));
                                    xw.WriteAttributeString("offset", "0");
                                    xw.WriteEndElement();
                                    /*
                                    xw.WriteStartElement("input");
                                    xw.WriteAttributeString("semantic", "NORMAL");
                                    xw.WriteAttributeString("source", string.Format("#{0}-norm", meshId));
                                    xw.WriteAttributeString("offset", "0");
                                    xw.WriteEndElement();
                                    */
                                    uint[] idx = new uint[prms.PrmChunk.NumberOfIndices];
                                    switch (prms.PrmChunk.DataType)
                                    {
                                        case Constants.DataTypes.UnsignedByte:
                                            for (int i = 0; i < prms.PrmChunk.NumberOfIndices; i++)
                                                idx[i] = (uint)cmbRoot.Indices[(prms.PrmChunk.FirstIndex * sizeof(ushort)) + (i * prms.PrmChunk.ElementSize)];
                                            break;
                                        case Constants.DataTypes.UnsignedShort:
                                            for (int i = 0; i < prms.PrmChunk.NumberOfIndices; i++)
                                                idx[i] = (uint)BitConverter.ToUInt16(cmbRoot.Indices, (prms.PrmChunk.FirstIndex * sizeof(ushort)) + (i * prms.PrmChunk.ElementSize));
                                            break;
                                        case Constants.DataTypes.UnsignedInt:
                                            for (int i = 0; i < prms.PrmChunk.NumberOfIndices; i++)
                                                idx[i] = BitConverter.ToUInt32(cmbRoot.Indices, (prms.PrmChunk.FirstIndex * sizeof(ushort)) + (i * prms.PrmChunk.ElementSize));
                                            break;
                                    }

                                    xw.WriteStartElement("p");
                                    {
                                        for (int i = 0; i < idx.Length; i++) xw.WriteString(string.Format("{0} ", idx[i]));
                                    }
                                    xw.WriteEndElement();
                                }
                                xw.WriteEndElement();
                            }
                        }
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                }
            }
            xw.WriteEndElement();
        }

        private static float[] ConvertToFloatArray(Constants.DataTypes format, byte[] data, uint offset, float scale)
        {
            float[] dataOut = new float[2];

            switch (format)
            {
                case Constants.DataTypes.Byte:
                case Constants.DataTypes.UnsignedByte:
                    {
                        byte[] temp = new byte[(data.Length - offset) / sizeof(byte)];
                        Buffer.BlockCopy(data, (int)offset, temp, 0, temp.Length * sizeof(byte));
                        dataOut = new float[temp.Length];
                        for (int i = 0; i < temp.Length; i++) dataOut[i] = Convert.ToSingle(temp[i]) * scale;
                    }
                    break;

                case Constants.DataTypes.Short:
                case Constants.DataTypes.UnsignedShort:
                    {
                        short[] temp = new short[(data.Length - offset) / sizeof(short)];
                        Buffer.BlockCopy(data, (int)offset, temp, 0, temp.Length * sizeof(short));
                        dataOut = new float[temp.Length];
                        for (int i = 0; i < temp.Length; i++) dataOut[i] = Convert.ToSingle(temp[i]) * scale;
                    }
                    break;

                case Constants.DataTypes.Float:
                    {
                        int texCoordDataLength = (int)(data.Length - offset);
                        dataOut = new float[texCoordDataLength / sizeof(float)];
                        Buffer.BlockCopy(data, (int)offset, dataOut, 0, texCoordDataLength);
                        for (int i = 0; i < dataOut.Length; i++) dataOut[i] *= scale;
                    }
                    break;
            }

            return dataOut;
        }

        private static void WriteSectionLibraryVisualScenes(XmlTextWriter xw, CmbChunk cmbRoot)
        {
            xw.WriteStartElement("library_visual_scenes");
            {
                xw.WriteStartElement("visual_scene");
                xw.WriteAttributeString("id", "default");
                {
                    foreach (MshsChunk.Mesh mesh in cmbRoot.SklmChunk.MshsChunk.Meshes)
                    {
                        SepdChunk sepd = cmbRoot.SklmChunk.ShpChunk.SepdChunks[mesh.SepdID];
                        MatsChunk.Material mat = cmbRoot.MatsChunk.Materials[mesh.MaterialID];

                        string nodeId = string.Format("node-{0:X8}", mesh.GetHashCode());
                        xw.WriteStartElement("node");
                        xw.WriteAttributeString("id", nodeId);
                        xw.WriteAttributeString("name", nodeId);
                        {
                            xw.WriteStartElement("translate");
                            xw.WriteString("0.0 0.0 0.0");
                            xw.WriteEndElement();

                            xw.WriteStartElement("rotate");
                            xw.WriteString("0.0 0.0 1.0 0.0");
                            xw.WriteEndElement();

                            xw.WriteStartElement("rotate");
                            xw.WriteString("0.0 1.0 0.0 0.0");
                            xw.WriteEndElement();

                            xw.WriteStartElement("rotate");
                            xw.WriteString("1.0 0.0 0.0 0.0");
                            xw.WriteEndElement();

                            xw.WriteStartElement("scale");
                            xw.WriteString("1.0 1.0 1.0");
                            xw.WriteEndElement();

                            xw.WriteStartElement("instance_geometry");
                            xw.WriteAttributeString("url", string.Format("#geom-{0:X8}", mesh.GetHashCode()));
                            {
                                xw.WriteStartElement("bind_material");
                                {
                                    xw.WriteStartElement("technique_common");
                                    {
                                        xw.WriteStartElement("instance_material");
                                        xw.WriteAttributeString("symbol", string.Format("material-{0:X8}-symbol", mat.GetHashCode()));
                                        xw.WriteAttributeString("target", string.Format("#material-{0:X8}", mat.GetHashCode()));
                                        xw.WriteEndElement();
                                    }
                                    xw.WriteEndElement();
                                }
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();
                        }
                        xw.WriteEndElement();
                    }
                }
                xw.WriteEndElement();
            }
            xw.WriteEndElement();
        }

        private static void WriteSectionScene(XmlTextWriter xw, CmbChunk cmbRoot)
        {
            xw.WriteStartElement("scene");
            {
                xw.WriteStartElement("instance_visual_scene");
                xw.WriteAttributeString("url", "#default");
                xw.WriteEndElement();
            }
            xw.WriteEndElement();
        }
    }
}
