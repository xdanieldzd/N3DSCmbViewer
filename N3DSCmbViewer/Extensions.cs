using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;

namespace N3DSCmbViewer
{
    static class Extensions
    {
        public static string DescriptionAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }

        public static object DefaultValueAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());
            DefaultValueAttribute[] attributes = (DefaultValueAttribute[])fi.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            if (attributes != null && attributes.Length > 0) return attributes[0].Value;
            else return source;
        }

        public static uint Reverse(this uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static ulong Reverse(this ulong value)
        {
            return ((ulong)Reverse((uint)(value & 0xFFFFFFFF)) << 32 | (ulong)Reverse((uint)(value >> 32)));
        }

        public static float[] RGBAToFloats(this uint value)
        {
            float[] vals = new float[4];
            vals[0] = (float)(((value >> 24) & 0xFF) / 255.0f);
            vals[1] = (float)(((value >> 16) & 0xFF) / 255.0f);
            vals[2] = (float)(((value >> 8) & 0xFF) / 255.0f);
            vals[3] = (float)(((value >> 0) & 0xFF) / 255.0f);
            return vals;
        }

        public static T GetAttribute<T>(this ICustomAttributeProvider assembly, bool inherit = false) where T : Attribute
        {
            return assembly.GetCustomAttributes(typeof(T), inherit).OfType<T>().FirstOrDefault();
        }

        public static IEnumerable<TreeNode> FlattenTree(this TreeView tv)
        {
            return FlattenTree(tv.Nodes);
        }

        public static IEnumerable<TreeNode> FlattenTree(this TreeNodeCollection coll)
        {
            return coll.Cast<TreeNode>().Concat(coll.Cast<TreeNode>().SelectMany(x => FlattenTree(x.Nodes)));
        }

        public static void RemoveNested(this TreeView tv, TreeNode node)
        {
            tv.Nodes.Remove(node);
            RemoveNestedNode(tv.TopNode, node);
        }

        private static void RemoveNestedNode(TreeNode parent, TreeNode nodeToRemove)
        {
            if (parent == null) return;

            parent.Nodes.Remove(nodeToRemove);
            foreach (TreeNode checkNode in parent.Nodes)
            {
                RemoveNestedNode(checkNode, nodeToRemove);
            }
        }
    }
}
