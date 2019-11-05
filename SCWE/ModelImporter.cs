using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SCWE
{
    public static class ModelImporter
    {
        public static Mesh ImportPly(string fileName)
        {
            using (Stream s = File.OpenRead(fileName))
            {
                return ImportPly(s);
            }
        }

        // import mesh from a ply file
        // only support mesh consists of triangles
        public static Mesh ImportPly(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, false);
            var header = ReadHeader(reader);

            int vertexCount = header.FindElement("vertex").count;
            int faceCount = header.FindElement("face").count;

            ModelLoader loader = new ModelLoader(header);
            var vertices = new Vector3[vertexCount];
            loader.AssignPropertyLoader("vertex", "x", (i, obj) =>
            {
                vertices[i].x = (float)obj;
            });
            loader.AssignPropertyLoader("vertex", "y", (i, obj) =>
            {
                vertices[i].y = (float)obj;
            });
            loader.AssignPropertyLoader("vertex", "z", (i, obj) =>
            {
                vertices[i].z = (float)obj;
            });
            var triangles = new uint[faceCount * 3];
            string indices = header.FindElement("face").ContainsProperty("vertex_index") ? "vertex_index" : "vertex_indices";
            loader.AssignPropertyLoader("face", indices, (i, obj) =>
            {
                object[] tri = (object[])obj;
                if (tri.Length != 3)
                    throw new Exception("does not support non triangle faces");
                i *= 3;
                triangles[i] = (uint)tri[0];
                triangles[i + 1] = (uint)tri[1];
                triangles[i + 2] = (uint)tri[2];
            });
            Color[] colors;
            if (header.FindElement("vertex").ContainsProperties("red", "green", "blue"))
            {
                colors = new Color[vertexCount];
                loader.AssignPropertyLoader("vertex", "red", (i, obj) =>
                {
                    colors[i].r = (byte)obj;
                });
                loader.AssignPropertyLoader("vertex", "green", (i, obj) =>
                {
                    colors[i].g = (byte)obj;
                });
                loader.AssignPropertyLoader("vertex", "blue", (i, obj) =>
                {
                    colors[i].b = (byte)obj;
                });
            }
            else
            {
                colors = new Color[0];
            }
            Vector2[] uv;
            if (header.FindElement("vertex").ContainsProperties("s", "t"))
            {
                uv = new Vector2[vertexCount];
                loader.AssignPropertyLoader("vertex", "s", (i, obj) =>
                {
                    uv[i].x = (float)obj;
                });
                loader.AssignPropertyLoader("vertex", "t", (i, obj) =>
                {
                    uv[i].y = (float)obj;
                });
            }
            else
            {
                uv = new Vector2[0];
            }

            loader.LoadAll(reader);

            return new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv,
                colors = colors
            };
        }

        class ModelLoader
        {
            PlyDefinition plyDef;
            // list of list of loaders for each property
            Action<int, object>[][] loaders;

            public ModelLoader(PlyDefinition ply)
            {
                plyDef = ply;
                loaders = new Action<int, object>[ply.elements.Length][];
                for (int i = 0; i < loaders.Length; i++)
                {
                    loaders[i] = new Action<int, object>[ply.elements[i].properties.Length];
                }
            }

            // assign a loader for a property of an element
            // ex. AssignElementLoader("vertex", x, ...)
            // the loader takes in the index of the element and the value of the property
            public void AssignPropertyLoader(string elemName, string propertyName, Action<int, object> loader)
            {
                int elemIndex = plyDef.FindElement(elemName, out ElementDefinition eDef);
                int propIndex = eDef.FindProperty(propertyName, out _);
                if (loaders[elemIndex][propIndex] != null)
                {
                    loaders[elemIndex][propIndex] += loader;
                }
                else
                {
                    loaders[elemIndex][propIndex] = loader;
                }
            }

            public void LoadAll(BinaryReader reader)
            {
                if (plyDef.format == PlyFormat.binary_little_endian)
                {
                    for (int i = 0; i < plyDef.elements.Length; i++)
                    {
                        var elem = plyDef.elements[i];
                        var activeLoaders = loaders[i];
                        int propCount = activeLoaders.Length;
                        for (int k = 0; k < elem.count; k++)
                        {
                            var props = ReadElement(elem, reader);
                            for (int j = 0; j < props.Length; j++)
                            {
                                activeLoaders[j]?.Invoke(k, props[j]);
                            }
                        }
                    }
                }
                else if (plyDef.format == PlyFormat.ascii)
                {
                    for (int i = 0; i < plyDef.elements.Length; i++)
                    {
                        var elem = plyDef.elements[i];
                        var activeLoaders = loaders[i];
                        int propCount = activeLoaders.Length;
                        for (int k = 0; k < elem.count; k++)
                        {
                            string line = ReadLine(reader);
                            var props = ReadElement(elem, new StringReader(line));
                            for (int j = 0; j < props.Length; j++)
                            {
                                activeLoaders[j]?.Invoke(k, props[j]);
                            }
                        }
                    }
                }
            }
        }

        // read an element from a TextReader
        // returns a list of property values
        static object[] ReadElement(ElementDefinition elemDef, TextReader reader)
        {
            string line = reader.ReadLine();
            object[] result = new object[elemDef.properties.Length];
            StringReader r = new StringReader(line);
            for (int k = 0; k < elemDef.properties.Length; k++)
            {
                var p = elemDef.properties[k];
                if (p.type == PropertyType.List)
                {
                    int count = Convert.ToInt32(ReadValue(p.indexType, r));
                    object[] list = new object[count];
                    for (int j = 0; j < count; j++)
                    {
                        list[j] = ReadValue(p.itemType, r);
                    }
                    result[k] = list;
                }
                else
                {
                    result[k] = ReadValue(p.type, r);
                }
            }
            return result;
        }

        // read an element from a BinaryReader
        // returns a list of property values
        static object[] ReadElement(ElementDefinition elemDef, BinaryReader r)
        {
            object[] result = new object[elemDef.properties.Length];
            for (int k = 0; k < elemDef.properties.Length; k++)
            {
                var p = elemDef.properties[k];
                if (p.type == PropertyType.List)
                {
                    int count = Convert.ToInt32(ReadValue(p.indexType, r));
                    object[] list = new object[count];
                    for (int j = 0; j < count; j++)
                    {
                        list[j] = ReadValue(p.itemType, r);
                    }
                    result[k] = list;
                }
                else
                {
                    result[k] = ReadValue(p.type, r);
                }
            }
            return result;
        }

        // enumerate through all the elements stored in a ply file
        // does not support big endian
        //static IEnumerable<PlyElement> EnumerateElements(PlyDefinition def, BinaryReader reader)
        //{
        //    if (def.format == PlyFormat.ascii)
        //    {
        //        Dictionary<string, object> props = new Dictionary<string, object>();
        //        foreach (var elemDef in def.elements)
        //        {
        //            for (int i = 0; i < elemDef.count; i++)
        //            {
        //                props.Clear();
        //                string line = ReadLine(reader);
        //                StringReader r = new StringReader(line);
        //                for (int k = 0; k < elemDef.properties.Length; k++)
        //                {
        //                    var p = elemDef.properties[k];
        //                    if (p.type == PropertyType.List)
        //                    {
        //                        int count = Convert.ToInt32(ReadValue(p.indexType, r));
        //                        object[] list = new object[count];
        //                        for (int j = 0; j < count; j++)
        //                        {
        //                            list[j] = ReadValue(p.itemType, r);
        //                        }
        //                        props[p.name] = list;
        //                    }
        //                    else
        //                    {
        //                        props[p.name] = ReadValue(p.type, r);
        //                    }
        //                }
        //                yield return new PlyElement
        //                {
        //                    name = elemDef.name,
        //                    properties = props,
        //                    index = i
        //                };
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (def.format == PlyFormat.binary_big_endian)
        //            throw new NotImplementedException("does not support big endian");

        //        Dictionary<string, object> props = new Dictionary<string, object>();
        //        foreach (var elemDef in def.elements)
        //        {
        //            for (int i = 0; i < elemDef.count; i++)
        //            {
        //                props.Clear();
        //                for (int k = 0; k < elemDef.properties.Length; k++)
        //                {
        //                    var p = elemDef.properties[k];
        //                    if (p.type == PropertyType.List)
        //                    {
        //                        int count = Convert.ToInt32(ReadValue(p.indexType, reader));
        //                        object[] list = new object[count];
        //                        for (int j = 0; j < count; j++)
        //                        {
        //                            list[j] = ReadValue(p.itemType, reader);
        //                        }
        //                        props[p.name] = list;
        //                    }
        //                    else
        //                    {
        //                        props[p.name] = ReadValue(p.type, reader);
        //                    }
        //                }
        //                yield return new PlyElement
        //                {
        //                    name = elemDef.name,
        //                    properties = props,
        //                    index = i
        //                };
        //            }
        //        }
        //    }
        //}

        static object ReadValue(PropertyType type, BinaryReader r)
        {
            switch (type)
            {
                case PropertyType.Char:
                    return r.ReadSByte();
                case PropertyType.Uchar:
                    return r.ReadByte();
                case PropertyType.Short:
                    return r.ReadInt16();
                case PropertyType.Ushort:
                    return r.ReadUInt16();
                case PropertyType.Int:
                    return r.ReadInt32();
                case PropertyType.Uint:
                    return r.ReadUInt32();
                case PropertyType.Float:
                    return r.ReadSingle();
                case PropertyType.Double:
                    return r.ReadDouble();
                case PropertyType.List:
                    throw new Exception();
            }
            return null;
        }

        static object ReadValue(PropertyType type, TextReader r)
        {
            switch (type)
            {
                case PropertyType.Char:
                    return sbyte.Parse(ReadToken(r));
                case PropertyType.Uchar:
                    var tok = ReadToken(r);
                    return byte.Parse(tok);
                case PropertyType.Short:
                    return short.Parse(ReadToken(r));
                case PropertyType.Ushort:
                    return ushort.Parse(ReadToken(r));
                case PropertyType.Int:
                    return int.Parse(ReadToken(r));
                case PropertyType.Uint:
                    return uint.Parse(ReadToken(r));
                case PropertyType.Float:
                    var f = ReadToken(r);
                    return float.Parse(f);
                case PropertyType.Double:
                    return double.Parse(ReadToken(r));
                case PropertyType.List:
                    throw new Exception();
            }
            return null;
        }

        static PlyDefinition ReadHeader(BinaryReader reader)
        {
            string line = ReadLine(reader);
            PlyDefinition def = new PlyDefinition();
            List<ElementDefinition> elems = new List<ElementDefinition>();
            string elemName = "";
            int elemCount = 0;
            List<PropertyDefinition> props = new List<PropertyDefinition>();

            if (line != "ply") throw new Exception("uncorrect format for ply file");
            while ((line = ReadLine(reader)) != null)
            {
                StringReader strReader = new StringReader(line);
                var type = ReadToken(strReader);
                switch (type)
                {
                    case "format":
                        def.format = ReadFormat(strReader);
                        break;
                    case "element":
                        if (elemCount != 0)
                        {
                            elems.Add(new ElementDefinition
                            {
                                name = elemName,
                                count = elemCount,
                                properties = props.ToArray()
                            });
                            props.Clear();
                        }
                        elemName = ReadToken(strReader);
                        elemCount = int.Parse(ReadToken(strReader));
                        break;
                    case "property":
                        props.Add(ReadProperty(strReader));
                        break;
                    case "end_header":
                        elems.Add(new ElementDefinition
                        {
                            name = elemName,
                            count = elemCount,
                            properties = props.ToArray()
                        });
                        def.elements = elems.ToArray();
                        return def;
                }
            }
            throw new Exception("cannot find end_header");
        }

        static PlyFormat ReadFormat(TextReader reader)
        {
            var f = ReadToken(reader);
            switch (f)
            {
                case "ascii":
                    return PlyFormat.ascii;
                case "binary_little_endian":
                    return PlyFormat.binary_little_endian;
                case "binary_big_endian":
                    return PlyFormat.binary_big_endian;
            }
            throw new Exception("unsupported ply format: " + f);
        }

        static PropertyDefinition ReadProperty(TextReader reader)
        {
            var result = new PropertyDefinition();
            result.type = ReadPropertyType(reader);
            if (result.type == PropertyType.List)
            {
                result.indexType = ReadPropertyType(reader);
                result.itemType = ReadPropertyType(reader);
            }
            result.name = ReadToken(reader);
            return result;
        }

        static PropertyType ReadPropertyType(TextReader reader)
        {
            var tok = ReadToken(reader);
            switch (tok)
            {
                case "char":
                    return PropertyType.Char;
                case "uchar":
                    return PropertyType.Uchar;
                case "short":
                    return PropertyType.Short;
                case "ushort":
                    return PropertyType.Ushort;
                case "int":
                    return PropertyType.Int;
                case "uint":
                    return PropertyType.Uint;
                case "float":
                case "float32":
                    return PropertyType.Float;
                case "double":
                    return PropertyType.Double;
                case "list":
                    return PropertyType.List;
            }
            throw new Exception("cannot read property type: " + tok);
        }

        static string ReadToken(TextReader reader)
        {
            StringBuilder builder = new StringBuilder();
            int i;
            while ((i = reader.Read()) != -1)
            {
                if (i == ' ') break;
                builder.Append((char)i);
            }
            return builder.ToString();
        }

        static string ReadLine(BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();
            int i;
            while ((i = reader.Read()) != -1)
            {
                if (i == '\n' || i == '\r')
                {
                    var next = reader.PeekChar();
                    if (next != '\n' && next != '\r')
                        break;
                    continue;
                }
                builder.Append((char)i);
            }
            return builder.ToString();
        }

        struct PlyDefinition
        {
            public ElementDefinition[] elements;
            public PlyFormat format;

            // find an element in the definition, if not found, an exception will be thrown
            public ElementDefinition FindElement(string name)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    var e = elements[i];
                    if (e.name == name)
                    {
                        return e;
                    }
                }
                throw new Exception("cannot find ply property: " + name);
            }

            // find an element in the definition, if not found, will return -1
            // returns the index of the element
            public int FindElement(string name, out ElementDefinition def)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    var e = elements[i];
                    if (e.name == name)
                    {
                        def = e;
                        return i;
                    }
                }
                def = default;
                return -1;
            }
        }

        enum PlyFormat
        {
            binary_little_endian,
            binary_big_endian,
            ascii
        }

        struct ElementDefinition
        {
            public PropertyDefinition[] properties;
            public int count;
            public string name;

            // find an property in the definition, if not found, an exception will be thrown
            // returns the index of the property
            public int FindProperty(string name, out PropertyDefinition def)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var e = properties[i];
                    if (e.name == name)
                    {
                        def = e;
                        return i;
                    }
                }
                throw new Exception("cannot find ply property: " + name);
            }

            public bool ContainsProperty(string name)
            {
                foreach (var p in properties)
                {
                    if (p.name == name)
                        return true;
                }
                return false;
            }

            public bool ContainsProperties(params string[] names)
            {
                foreach (var n in names)
                {
                    if (!ContainsProperty(n))
                        return false;
                }
                return true;
            }
        }

        struct PropertyDefinition
        {
            public PropertyType type;
            public PropertyType indexType;
            public PropertyType itemType;
            public string name;
        }

        enum PropertyType
        {
            Char,
            Uchar,
            Short,
            Ushort,
            Int,
            Uint,
            Float,
            Double,
            List
        }
    }
}
