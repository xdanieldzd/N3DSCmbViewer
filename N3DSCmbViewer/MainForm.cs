using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using OpenTK.Graphics.OpenGL;

using Aglex;

using N3DSCmbViewer.Cmb;
using N3DSCmbViewer.Csab;
using N3DSCmbViewer.ZSI;

namespace N3DSCmbViewer
{
    /* As usual with my programs, the GUI code is a bloody mess! :DDD
     * My sincerest apologies.
     */

    partial class MainForm : Form
    {
        ImageList iconImageListSmall;

        //Shaders.DVLB shaderBinary;
        ArchiveFile archiveFile;
        ModelHandler modelFile;
        AnimHandler animFile;
        ZSIHandler zsiFile;
        short meshToRender;
        bool renderError;

        TextPrinter print;

        HudPages hudPage;
        Color hudBackColor;

        enum HudPages
        {
            [Description("Cmb Model Stats")]
            ModelStats = 0,
            [Description("Csab Anim Stats")]
            AnimStats,
            [Description("System Stats")]
            SystemStats
        };

        enum FileTypes
        {
            Unknown, ZSI, ZAR, cmb, csab, shbin, GAR, LzS
        };

        Dictionary<string, FileTypes> fileTypeDict = new Dictionary<string, FileTypes>()
        {
            { "ZSI\x01", FileTypes.ZSI },
            { "ZAR\x01", FileTypes.ZAR },
            { "cmb ", FileTypes.cmb },
            { "csab", FileTypes.csab },
            { "shbin", FileTypes.shbin },

            { "GAR\x02", FileTypes.GAR },
            { "ZSI\t", FileTypes.ZSI },

            { "LzS\x01", FileTypes.LzS },
        };

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            foreach (Control control in this.Controls)
            {
                if (control.Font == Form.DefaultFont) control.Font = SystemFonts.MessageBoxFont;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            print = new TextPrinter(new Font("Verdana", 9.0f, FontStyle.Bold));
            hudPage = HudPages.ModelStats;
            hudBackColor = Color.FromArgb(96, Color.Black);

            glControl1.VSync = Properties.Settings.Default.VSync;
            enableHUDToolStripMenuItem.Checked = Properties.Settings.Default.EnableHUD;
            disableAllShadersToolStripMenuItem.Checked = Properties.Settings.Default.DisableAllShaders;
            enableSkeletalRenderingToolStripMenuItem.Checked = Properties.Settings.Default.EnableSkeletalStuff;

            iconImageListSmall = new ImageList() { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };
            iconImageListSmall.Images.Add("folderc", Win32.GetFolderIcon(Win32.IconSize.Small, Win32.FolderType.Closed));
            iconImageListSmall.Images.Add("foldero", Win32.GetFolderIcon(Win32.IconSize.Small, Win32.FolderType.Open));
            iconImageListSmall.Images.Add("default", Win32.GetDllIcon("shell32.dll", 72, Win32.IconSize.Small));

            treeViewEx1.ImageList = iconImageListSmall;
            treeViewEx1.ImageKey = "folderc";
            treeViewEx1.SelectedImageKey = "foldero";

            meshToRender = -1;
            renderError = false;

            this.Text = Program.Description;
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            // STUFF ALL THIS IN!!! MWAHAHAHAHAHAAA!!!
            GL.Enable(EnableCap.PointSmooth);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.PointSize(5.0f);

            GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.25f, 0.25f, 0.25f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 0.75f, 0.75f, 0.75f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);

            //whatever
            /*MessageBox.Show(
                "This is a buggy test build; the main reason for it to exist is to let #zelda play around with the MM3D support.\n\nThere WILL be glitches, broken functionality, etc., etc.\n\nYou have been warned.",
                "Hi #zelda", MessageBoxButtons.OK, MessageBoxIcon.Warning);*/
        }

        private void glControl1_Render(object sender, EventArgs e)
        {
            GLControl senderControl = (sender as GLControl);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Scale(0.005, 0.005, 0.005);

            //if (shaderBinary == null) shaderBinary = new Shaders.DVLB(@"E:\- 3DS OoT Hacking -\romfs\CmbVShader.shbin");

            if (modelFile != null && !modelFile.Disposed && !renderError)
            {
                try
                {
                    modelFile.Render(meshToRender, animFile);
                }
                catch (AglexException aex)
                {
                    MessageBox.Show(aex.ToString());
                    renderError = true;
                }
            }

            if (zsiFile != null && !zsiFile.Disposed && !renderError)
            {
                if (zsiFile.SelectedSetup != null) zsiFile.SelectedSetup.RenderActors();
            }

            if (Properties.Settings.Default.EnableHUD)
            {
                GL.AlphaFunc(AlphaFunction.Always, 0.0f);

                print.Begin(senderControl);
                print.Print(new OpenTK.Vector2d(-16.0, -16.0), hudBackColor, string.Format("F1: {0} ({1}/{2})", hudPage.DescriptionAttr(), (int)(hudPage + 1), Enum.GetValues(typeof(HudPages)).Length));
                print.Print(new OpenTK.Vector2d(-16.0, 16.0), hudBackColor, string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} FPS", senderControl.FPS));

                print.Print(new OpenTK.Vector2d(16.0, -16.0), hudBackColor,
                    string.Format("F5: Textures (<color:{0}>{1}<color:>)\nF6: Vsync (<color:{2}>{3}<color:>)\nF7: Wireframe overlay (<color:{4}>{5}<color:>)\nF8: Lighting (<color:{6}>{7}<color:>)\nF9: Texture alpha (<color:{8}>{9}<color:>)",
                    (Properties.Settings.Default.EnableTextures ? "AA,205,50" : "255,165,0"), (Properties.Settings.Default.EnableTextures ? "ON" : "OFF"),
                    (senderControl.VSync ? "AA,205,50" : "255,165,0"), (senderControl.VSync ? "ON" : "OFF"),
                    (Properties.Settings.Default.AddWireframeOverlay ? "AA,205,50" : "255,165,0"), (Properties.Settings.Default.AddWireframeOverlay ? "ON" : "OFF"),
                    (Properties.Settings.Default.EnableLighting ? "AA,205,50" : "255,165,0"), (Properties.Settings.Default.EnableLighting ? "ON" : "OFF"),
                    (!Properties.Settings.Default.DisableAlpha ? "AA,205,50" : "255,165,0"), (!Properties.Settings.Default.DisableAlpha ? "ON" : "OFF")));

                if (renderError)
                {
                    print.Print(new OpenTK.Vector2d(16.0, -128.0), hudBackColor, "<color:255,96,96>-- Model render error! Rendering disabled! --");
                }

                switch (hudPage)
                {
                    case HudPages.ModelStats:
                        if (modelFile != null && !modelFile.Disposed)
                        {
                            print.Print(new OpenTK.Vector2d(16.0, 16.0), hudBackColor,
                                string.Format("Filename: {0}\nModel name: {1}\nBones: {2}, Materials: {3}, Textures: {4}, Primitives: {5}\nVertices: {6}, Normals: {7}, Colors: {8}, Texture coords: {9}",
                                Path.GetFileName(modelFile.Filename), modelFile.Root.CmbName,
                                modelFile.Root.SklChunk.BoneCount, modelFile.Root.MatsChunk.MaterialCount, modelFile.Root.TexChunk.TextureCount, modelFile.Root.TotalPrimitives,
                                modelFile.Root.TotalVertices, modelFile.Root.TotalNormals, modelFile.Root.TotalColors, modelFile.Root.TotalTexCoords));
                        }
                        else
                            print.Print(new OpenTK.Vector2d(16.0, 16.0), hudBackColor, "No file loaded");
                        break;

                    case HudPages.AnimStats:
                        print.Print(new OpenTK.Vector2d(16.0, 16.0), hudBackColor, "Sorry, almost completely unimplemented!\nNo animations and stuff parsed yet, just some structural data...");
                        break;

                    case HudPages.SystemStats:
                        print.Print(new OpenTK.Vector2d(16.0, 16.0), hudBackColor, string.Format("OpenGL version: {0}\nVendor & renderer: {1}, {2}\n\nOpenTK version: {3}\n\nHelper ident: {4}",
                            Toolkit.VersionString, Toolkit.VendorString, Toolkit.RendererString, Toolkit.OpenTKVersion, Toolkit.GetVersion()));
                        break;
                }

                print.Flush();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();

            System.Reflection.FieldInfo[] fields = this.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (System.Reflection.FieldInfo info in fields.Where(x => x.FieldType.GetInterfaces().Contains(typeof(IDisposable))))
            {
                object obj = info.GetValue(this);
                if (obj != null) (obj as IDisposable).Dispose();
            }
        }

        private void glControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                hudPage++;
                if ((int)hudPage >= Enum.GetValues(typeof(HudPages)).Length) hudPage = (HudPages)0;
            }
            else if (e.KeyCode == Keys.F5)
                Properties.Settings.Default.EnableTextures = !Properties.Settings.Default.EnableTextures;
            else if (e.KeyCode == Keys.F6)
            {
                GLControl senderControl = (sender as GLControl);
                Properties.Settings.Default.VSync = senderControl.VSync = !senderControl.VSync;
            }
            else if (e.KeyCode == Keys.F7)
                Properties.Settings.Default.AddWireframeOverlay = !Properties.Settings.Default.AddWireframeOverlay;
            else if (e.KeyCode == Keys.F8)
                Properties.Settings.Default.EnableLighting = !Properties.Settings.Default.EnableLighting;
            else if (e.KeyCode == Keys.F9)
                Properties.Settings.Default.DisableAlpha = !Properties.Settings.Default.DisableAlpha;

            // TEST TEMP TODO FIXME HACK
            else if (e.KeyCode == Keys.Back)
            {
                // TEST TEMP TODO FIXME HACK
                modelFile.Root.SklChunk.Bones[0].Rotation = new OpenTK.Vector3(
                    modelFile.Root.SklChunk.Bones[0].Rotation.X + 0.05f,
                    modelFile.Root.SklChunk.Bones[0].Rotation.Y,
                    modelFile.Root.SklChunk.Bones[0].Rotation.Z);
            }
            // TEST TEMP TODO FIXME HACK
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.LastFile != string.Empty)
            {
                ofdOpenFile.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.LastFile);
                ofdOpenFile.FileName = Path.GetFileName(Properties.Settings.Default.LastFile);
            }

            if (ofdOpenFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.LastFile = ofdOpenFile.FileName;

                meshToRender = -1;
                renderError = false;

                if (modelFile != null) modelFile.Dispose();
                if (animFile != null) animFile.Dispose();
                if (zsiFile != null) zsiFile.Dispose();

                if (archiveFile != null) archiveFile = null;
                treeViewEx1.Nodes.Clear();

                byte[] fileData = File.ReadAllBytes(Properties.Settings.Default.LastFile);

                PerformFileAction(fileData);

                archiveToolStripMenuItem.Enabled = (archiveFile != null);
                dumpToolStripMenuItem.Enabled = (modelFile != null);

                this.Text = string.Format("{0} - [{1}]", Program.Description, Path.GetFileName(Properties.Settings.Default.LastFile));
            }
        }

        private FileTypes IdentifyFile(string fn)
        {
            return IdentifyFile(File.ReadAllBytes(fn));
        }

        private FileTypes IdentifyFile(byte[] data)
        {
            string id = Encoding.ASCII.GetString(data, 0, 4);
            if (fileTypeDict.ContainsKey(id))
                return fileTypeDict[id];
            else
                return FileTypes.Unknown;
        }

        private void PerformFileAction(byte[] fileData)
        {
            FileTypes fileType = IdentifyFile(fileData);
            switch (fileType)
            {
                case FileTypes.cmb:
                    modelFile = new ModelHandler(fileData, 0, fileData.Length);
                    treeViewEx1.Nodes.Add(new TreeNode(Path.GetFileName(Properties.Settings.Default.LastFile)) { ImageKey = "default" });
                    AddSepdChunksToTree(treeViewEx1, treeViewEx1.TopNode);
                    break;

                case FileTypes.ZAR:
                case FileTypes.GAR:
                    archiveFile = new ArchiveFile(fileData);
                    treeViewEx1.Enabled = true;
                    CreateArchiveTree();
                    break;

                case FileTypes.LzS:
                    byte[] decData = LZSS.Decompress(fileData);
                    string outfile = @"H:\temp\mm3d-test\" + Path.GetFileName(Properties.Settings.Default.LastFile) + ".dec";
                    //File.WriteAllBytes(outfile, decData);
                    PerformFileAction(decData);
                    break;

                case FileTypes.ZSI:
                    treeViewEx1.Nodes.Add(new TreeNode(Path.GetFileName(Properties.Settings.Default.LastFile)) { ImageKey = "default" });
                    zsiFile = new ZSIHandler(fileData, 0, fileData.Length);
                    if (zsiFile.SelectedSetup != null) modelFile = zsiFile.SelectedSetup.Model;
                    AddZsiDataToTree(treeViewEx1, treeViewEx1.TopNode);
                    break;
            }
        }

        private void CreateArchiveTree()
        {
            treeViewEx1.BeginUpdate();
            treeViewEx1.Nodes.Clear();

            treeViewEx1.PathSeparator = @"/";
            PopulateTreeView(treeViewEx1, archiveFile.FileInfos, new char[] { '/', '\\' });

            treeViewEx1.Sort();

            treeViewEx1.EndUpdate();
        }

        // Based on http://stackoverflow.com/a/19332770
        private static void PopulateTreeView(TreeView treeView, ArchiveFile.FileInfo[] fileInfos, char[] pathSeparator)
        {
            TreeNode rootNode = new TreeNode(Path.GetFileName(Properties.Settings.Default.LastFile));

            TreeNode lastNode = null;
            string subPathAgg;
            foreach (ArchiveFile.FileInfo fileInfo in fileInfos)
            {
                subPathAgg = string.Empty;
                foreach (string subPath in fileInfo.FilePath.Split(pathSeparator))
                {
                    subPathAgg += subPath + pathSeparator[0];
                    TreeNode[] nodes = rootNode.Nodes.Find(subPathAgg, true);
                    if (nodes.Length == 0)
                    {
                        if (lastNode == null)
                            lastNode = rootNode.Nodes.Add(subPathAgg, subPath);
                        else
                            lastNode = lastNode.Nodes.Add(subPathAgg, subPath);
                    }
                    else
                        lastNode = nodes[0];
                }

                lastNode.Tag = (uint)Array.IndexOf(fileInfos, fileInfo);
                lastNode.ImageKey = "default";
                lastNode.SelectedImageKey = "default";

                lastNode = null;
            }

            rootNode.Expand();
            treeView.Nodes.Add(rootNode);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void enableHUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableHUD = ((sender as ToolStripMenuItem).Checked);
        }

        private void enableSkeletalRenderingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableSkeletalStuff = ((sender as ToolStripMenuItem).Checked);
        }

        private void saveScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdScreenshot.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Bitmap bmp = new Bitmap(glControl1.ClientRectangle.Width, glControl1.ClientRectangle.Height);

                System.Drawing.Imaging.BitmapData data = bmp.LockBits(glControl1.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                GL.ReadPixels(0, 0, glControl1.ClientRectangle.Width, glControl1.ClientRectangle.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                bmp.UnlockBits(data);

                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                bmp.Save(sfdScreenshot.FileName);
            }
        }

        static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string assemblyCopyright = System.Reflection.Assembly.GetExecutingAssembly().GetAttribute<System.Reflection.AssemblyCopyrightAttribute>().Copyright;
            string mainAboutString = string.Format("{0} v{1} - {2}{3}{4}", Application.ProductName, new Version(Application.ProductVersion).ToString(3), Program.Description, Environment.NewLine, assemblyCopyright);

            DateTime linkerTimestamp = RetrieveLinkerTimestamp();
            string buildString = string.Format("Build {0}", linkerTimestamp.ToString("MM/dd/yyyy HH:mm:ss UTCzzz", System.Globalization.CultureInfo.InvariantCulture));

            string acknString =
                "* ETC1 support based on rg_etc1 by Rich Geldreich (http://code.google.com/p/rg-etc1/)" + Environment.NewLine +
                "* ZAR archive format reversed & documented by Twili" + Environment.NewLine +
                "* Various additional research by Twili" + Environment.NewLine +
                "* COLLADA exporter written with a lot of help from Peardian" + Environment.NewLine +
                "* LZSS decompression code adapted from C++ code by ShimmerFairy (https://github.com/lue/MM3D/)" + Environment.NewLine +
                "* Additional modifications by NishaWolfe (https://nishawolfe.com)" + Environment.NewLine +
                 Environment.NewLine +
                "* Greetings to TCRF and #zelda on BadnikNET";

            string title = "About";

            MessageBox.Show(string.Format("{0}{1}{1}{2}{1}{1}{3}", mainAboutString, Environment.NewLine, acknString, buildString), title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void treeViewEx1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is uint)
            {
                meshToRender = -1;

                uint fileno = (uint)e.Node.Tag;
                ArchiveFile.FileInfo fi = archiveFile.FileInfos[fileno];
                string ext = Path.GetExtension(fi.FilePath);

                byte[] data = archiveFile.GetFile(fileno);

                switch (ext)
                {
                    case ".cmb":
                        renderError = false;
                        if (modelFile != null) modelFile.Dispose();
                        modelFile = new ModelHandler(data, 0, data.Length);
                        modelFile.Filename = Path.GetFileName(fi.FilePath);
                        this.Text = string.Format("{0} - [{1} - ({2})]", Program.Description, Path.GetFileName(Properties.Settings.Default.LastFile), Path.GetFileName(fi.FilePath));
                        dumpToolStripMenuItem.Enabled = (modelFile != null);

                        AddSepdChunksToTree(treeViewEx1, e.Node);
                        break;

                    case ".csab":
                        if (animFile != null) animFile.Dispose();
                        animFile = new AnimHandler(data, 0, data.Length);
                        animFile.Filename = Path.GetFileName(fi.FilePath);

                        // TODO more??
                        break;
                }
            }
            else if (e.Node.Tag is short)
            {
                // assume mesh number
                meshToRender = (short)e.Node.Tag;
            }
            else if (e.Node.Tag is Setup)
            {
                zsiFile.SelectedSetup = (e.Node.Tag as Setup);
            }
            else if (e.Node.Tag is Actor)
            {
                if (zsiFile.SelectedSetup != null)
                    zsiFile.SelectedSetup.SelectedActor = (e.Node.Tag as Actor);
            }
            else if (e.Node.Tag == null)
            {
                meshToRender = -1;
                if (zsiFile != null) zsiFile.SelectedSetup = null;
            }
        }

        private void AddSepdChunksToTree(TreeView treeView, TreeNode cmbNode)
        {
            List<TreeNode> allNodes = treeView.FlattenTree().ToList();
            foreach (TreeNode existingNode in allNodes) if (existingNode.Tag is short) treeView.RemoveNested(existingNode);

            if (cmbNode.Nodes.Count == 0)
            {
                for (int i = 0; i < modelFile.Root.SklmChunk.MshsChunk.Meshes.Length; i++)
                {
                    MshsChunk.Mesh mesh = modelFile.Root.SklmChunk.MshsChunk.Meshes[i];
                    cmbNode.Nodes.Add(new TreeNode(string.Format("Mesh {0:D3}: S:{1}, M:{2}, U:{3}", i, mesh.SepdID, mesh.MaterialID, mesh.Unknown)) { Tag = (short)i, ImageKey = "default", SelectedImageKey = "default" });
                }
                cmbNode.Expand();
            }
        }

        private void AddZsiDataToTree(TreeView treeView, TreeNode zsiNode)
        {
            TreeNode setupsNode = new TreeNode("Room Setups");
            for (int i = 0; i < zsiFile.Setups.Count; i++)
            {
                Setup setup = zsiFile.Setups[i];
                TreeNode setupNode = new TreeNode(string.Format("Setup #{0} (0x{1:X6})", i, setup.Offset)) { Tag = setup, ImageKey = "default", SelectedImageKey = "default" };

                if (setup.Actors.Count != 0)
                {
                    TreeNode actorsNode = new TreeNode("Room Actors") { Tag = setup };
                    for (int j = 0; j < setup.Actors.Count; j++)
                    {
                        //These numbers are adjusted to match the collada export.
                        actorsNode.Nodes.Add(new TreeNode(string.Format("Actor #{0} (0x{1:X4}), Pos({3},{4},{5}) Rot({6},{7},{8}) Var({9})",
                            j,
                            setup.Actors[j].Number,
                            setup.Actors[j].Number & 0x0FFF,
                            (float)setup.Actors[j].PositionX / 100f,
                            -(float)setup.Actors[j].PositionZ / 100f,
                            (float)setup.Actors[j].PositionY / 100f,
                            (float)setup.Actors[j].RotationX / 100f,
                            (float)setup.Actors[j].RotationZ / 100f,
                            (float)setup.Actors[j].RotationY / 100f,
                            setup.Actors[j].Variable
                            )) { Tag = setup.Actors[j], ImageKey = "default", SelectedImageKey = "default" });
                    }
                    setupNode.Nodes.Add(actorsNode);
                }

                setupsNode.Nodes.Add(setupNode);
            }
            setupsNode.Expand();

            zsiNode.Nodes.Add(setupsNode);
            zsiNode.Expand();
        }

        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (archiveFile != null)
            {
                if (fbdArchiveExtractAll.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = fbdArchiveExtractAll.SelectedPath;

                    ExtractFiles(path, treeViewEx1.TopNode);
                }
            }
        }

        private void ExtractFiles(string path, TreeNode node)
        {
            if (node.Tag is uint)
            {
                uint fileno = (uint)node.Tag;
                byte[] data = archiveFile.GetFile(fileno);
                string fn = Path.Combine(path, node.FullPath);
                Directory.CreateDirectory(Path.GetDirectoryName(fn));
                BinaryWriter bw = new BinaryWriter(File.Create(fn));
                bw.Write(data);
                bw.Close();
            }

            foreach (TreeNode child in node.Nodes) ExtractFiles(path, child);
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewEx1.SelectedNode == null || !(treeViewEx1.SelectedNode.Tag is uint)) return;

            uint fileno = (uint)treeViewEx1.SelectedNode.Tag;

            sfdExtractFile.FileName = Path.GetFileName(archiveFile.FileInfos[fileno].FilePath);
            if (sfdExtractFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                byte[] data = archiveFile.GetFile(fileno);
                BinaryWriter bw = new BinaryWriter(File.Create(sfdExtractFile.FileName));
                bw.Write(data);
                bw.Close();
            }
        }

        private void treeViewEx1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                TreeViewEx tvEx = (sender as TreeViewEx);
                tvEx.SelectedNode = tvEx.GetNodeAt(e.X, e.Y);

                if (tvEx.SelectedNode != null && tvEx.SelectedNode.Tag is uint) cmsTreeFile.Show(tvEx, e.Location);
            }
        }

        private void resetCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            glControl1.ResetCamera();
        }

        private void dumpModelInfosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sfdLogFile.FileName = string.Format("{0}.txt", modelFile.Root.CmbName);

            if (sfdLogFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter log = new StreamWriter(File.Create(sfdLogFile.FileName), Encoding.Unicode);
                log.WriteLine(modelFile.ToString());
                log.Flush();
                log.Close();
            }
        }

        //TODO: Move this to a seperate function, perhaps in the modelFile function
        private void dumpSceneActorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (zsiFile != null)
            {
                sfdLogFile.FileName = string.Format("{0}.txt", modelFile.Root.CmbName + "_actors");

                if (sfdLogFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    StreamWriter log = new StreamWriter(File.Create(sfdLogFile.FileName), Encoding.Unicode);

                    for (int i = 0; i < zsiFile.Setups.Count; i++)
                    {
                        Setup setup = zsiFile.Setups[i];
                        log.WriteLine(string.Format("Setup #{0} (0x{1:X6})\n", i, setup.Offset));

                        if (setup.Actors.Count != 0)
                        {
                            TreeNode actorsNode = new TreeNode("Room Actors") { Tag = setup };
                            for (int j = 0; j < setup.Actors.Count; j++)
                            {
                                log.WriteLine(

                                    //These numbers are adjusted to match the collada export.
                                    string.Format("Actor #{0}\n Hex ID: 0x{1:X4}\n Shortened Hex ID: 0x{2:X4}\n Literal ID: {2}\n Position:\n   x: {3}\n   y: {4}\n   z: {5}\n Rotation:\n   x: {6}\n   y: {7}\n   z: {8}\n Actor Variable: {9}\n\n",
                                    j,
                                    setup.Actors[j].Number,
                                    setup.Actors[j].Number & 0x0FFF,
                                    (float)setup.Actors[j].PositionX / 100f,
                                    -(float)setup.Actors[j].PositionZ / 100f,
                                    (float)setup.Actors[j].PositionY / 100f,
                                    (float)setup.Actors[j].RotationX / 100f,
                                    (float)setup.Actors[j].RotationZ / 100f,
                                    (float)setup.Actors[j].RotationY / 100f,
                                    setup.Actors[j].Variable)

                                );
                            }
                        }
                    }
                    log.Flush();
                    log.Close();
                }
            }
            else
            {
                string title = "Not a Scene!";
                string boxText = "No scene data was found.\nThis probably means this is an actor file.";
                MessageBox.Show(boxText, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void disableAllShadersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisableAllShaders = (sender as ToolStripMenuItem).Checked;
        }

        private void dumpCOLLADAFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sfdColladaDae.FileName = string.Format("{0}.dae", modelFile.Root.CmbName);

            if (sfdColladaDae.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExportCollada.Export(sfdColladaDae.FileName, modelFile.Root);
                foreach (TexChunk.Texture tex in modelFile.Root.TexChunk.Textures)
                {
                    string texPath = Path.Combine(Path.GetDirectoryName(sfdColladaDae.FileName), string.Format("{0}_{1:X}.png", tex.Name, tex.DataOffset));
                    tex.TexImage.Save(texPath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }
    }
}
