using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace csgeom_test {
    public enum meshComponent {
        normals = 1 << 0,
        uvs = 1 << 1,
        colors = 1 << 2
    }

    public class mesh {
        readonly List<vec3> positions;
        public float[] positionData => positions.rawData();
        readonly List<vec3> normals;
        public float[] normalData => hasNormals ? normals.rawData() : null;
        readonly List<vec2> uvs;
        public float[] uvData => hasUVs ? uvs.rawData() : null;
        readonly List<vec3> colors;
        public float[] colorData => hasColors ? colors.rawData() : null;
        readonly List<int> indices;
        public int[] indexData => indices.rawData();

        public readonly meshComponent components;

        public bool hasNormals => (components & meshComponent.normals) != 0;
        public bool hasUVs => (components & meshComponent.uvs) != 0;
        public bool hasColors => (components & meshComponent.colors) != 0;

        private int findVertex(vec3 p, vec3 n, vec2 uv, vec3 c) {
            for(int i = 0; i < positions.Count; i++) {
                bool matchingPosition = meshExtensions.compareVec(p, positions[i]);
                if (!matchingPosition) break;
                bool matchingNormal = hasNormals ? meshExtensions.compareVec(n, normals[i]) : true;
                if (!matchingNormal) break;
                bool matchingUVs = hasUVs ? meshExtensions.compareVec(uv, uvs[i]) : true;
                if (!matchingUVs) break;
                bool matchingColors = hasColors ? meshExtensions.compareVec(c, colors[i]) : true;
                if (!matchingColors) break;

                return i;
            }
            return -1;
        }

        private void addTriangle(vec3 p0, vec3 p1, vec3 p2, vec3 n0, vec3 n1, vec3 n2, vec2 uv0, vec2 uv1, vec2 uv2, vec3 c0, vec3 c1, vec3 c2) {
            int index0 = findVertex(p0, n0, uv0, vec3.Zero);
            int index1 = findVertex(p1, n1, uv1, vec3.Zero);
            int index2 = findVertex(p2, n2, uv2, vec3.Zero);

            if (index0 == -1) {
                index0 = positions.Count;
                positions.Add(p0);
                if(hasNormals) normals.Add(n0);
                if(hasUVs) uvs.Add(uv0);
                if(hasColors) colors.Add(c0);
            }
            if (index1 == -1) {
                index1 = positions.Count;
                positions.Add(p1);
                if (hasNormals) normals.Add(n1);
                if (hasUVs) uvs.Add(uv1);
                if (hasColors) colors.Add(c1);
            }
            if (index2 == -1) {
                index2 = positions.Count;
                positions.Add(p2);
                if (hasNormals) normals.Add(n2);
                if (hasUVs) uvs.Add(uv2);
                if (hasColors) colors.Add(c2);
            }

            indices.Add(index0);
            indices.Add(index1);
            indices.Add(index2);
        }
        public void addColorOnlyTriangle(vec3 p0, vec3 p1, vec3 p2, vec3 c0, vec3 c1, vec3 c2) {
            addTriangle(
                p0,         p1,         p2,
                vec3.Zero,  vec3.Zero,  vec3.Zero,
                vec2.Zero,  vec2.Zero,  vec2.Zero,
                c0,         c1,         c2
            );
        }
        public void addNormalOnlyTriangle(vec3 p0, vec3 p1, vec3 p2, vec3 n0, vec3 n1, vec3 n2) {
            addTriangle(
                p0, p1, p2,
                n0, n1, n2,
                vec2.Zero, vec2.Zero, vec2.Zero,
                vec3.Zero, vec3.Zero, vec3.Zero
            );
        }
        public void addUVOnlyTriangle(vec3 p0, vec3 p1, vec3 p2, vec2 uv0, vec2 uv1, vec2 uv2) {
            addTriangle(
                p0, p1, p2,
                vec3.Zero, vec3.Zero, vec3.Zero,
                uv0, uv1, uv2,
                vec3.Zero, vec3.Zero, vec3.Zero
            );
        }
        
        public void loadObj(string path) {
            List<vec3> verts = new List<vec3>();
            List<vec3> normals = new List<vec3>();
            List<vec2> uvs = new List<vec2>();
            List<int[]> indices = new List<int[]>();

            string[] lines = System.IO.File.ReadAllLines(path).Select(line => line.Trim()).ToArray();
            for (int outerIndex = 0; outerIndex < lines.Length; outerIndex++) {
                string line = lines[outerIndex];
                if (line.StartsWith("v ")) {
                    vec3 vals = new vec3();

                    string trimmed = line.Substring(2);
                    string[] sections = trimmed.Split(' ');
                    if (sections.Length != 3) throw new Exception("3 components of position attribute must be separated by a single space");

                    for (int i = 0; i < sections.Length; i++) {
                        try {
                            vals[i] = float.Parse(sections[i]);
                        } catch {
                            throw new Exception("Couldn't parse v value " + vals[i]);
                        }
                    }

                    verts.Add(vals);
                } else if (line.StartsWith("vt ")) {
                    vec2 vals = new vec2();

                    string trimmed = line.Substring(3);
                    string[] sections = trimmed.Split(' ');
                    if (sections.Length != 2) throw new Exception("2 components of uv attribute must be separated by a single space");

                    for (int i = 0; i < sections.Length; i++) {
                        try {
                            vals[i] = float.Parse(sections[i]);
                        } catch {
                            throw new Exception("Couldn't parse vt value " + vals[i]);
                        }
                    }

                    uvs.Add(vals);
                } else if (line.StartsWith("vn ")) {
                    vec3 vals = new vec3();

                    string trimmed = line.Substring(3);
                    string[] sections = trimmed.Split(' ');
                    if (sections.Length != 3) throw new Exception("3 components of position attribute must be separated by a single space");

                    for (int i = 0; i < sections.Length; i++) {
                        try {
                            vals[i] = float.Parse(sections[i]);
                        } catch {
                            throw new Exception("Couldn't parse vn value " + vals[i]);
                        }
                    }

                    normals.Add(vals);
                } else if (line.StartsWith("f ")) {
                    int[] vals = new int[9];

                    string trimmed = line.Substring(2);
                    string[] sections = trimmed.Split(' ');
                    if (sections.Length != 3) throw new Exception("3 components of face attribute must be separated by a single space");

                    for (int i = 0; i < sections.Length; i++) {
                        try {
                            string section = sections[i];

                            string[] inds = section.Split('/');
                            if (inds.Length != 3) throw new Exception("3 indices of face component must be separated by a single /");

                            for (int u = 0; u < inds.Length; u++) {
                                try {
                                    vals[i * 3 + u] = int.Parse(inds[u]) - 1;
                                } catch {
                                    throw new Exception("couldn't parse f index value " + inds[u]);
                                }
                            }
                        } catch {
                            throw new Exception("Couldn't parse f value " + vals[i]);
                        }
                    }

                    indices.Add(vals);
                }
            }

            foreach (int[] attributeIndices in indices) {
                addTriangle(
                    verts[attributeIndices[0]], verts[attributeIndices[3]], verts[attributeIndices[6]],
                    normals[attributeIndices[2]], normals[attributeIndices[5]], normals[attributeIndices[8]],
                    uvs[attributeIndices[1]], uvs[attributeIndices[4]], uvs[attributeIndices[7]],
                    vec3.Zero, vec3.Zero, vec3.Zero
                );
            }
        }

        public void randomColoredTriangles(List<vec2[]> _verts) {
            Random r = new Random();
            for (int i = 0; i < _verts.Count; i++) {
                indices.Add(positions.Count + 0);
                indices.Add(positions.Count + 1);
                indices.Add(positions.Count + 2);

                positions.Add(new vec3(_verts[i][0]));
                positions.Add(new vec3(_verts[i][1]));
                positions.Add(new vec3(_verts[i][2]));

                colors.Add(new vec3((float)r.NextDouble() * 0.2f + 0.8f, (float)r.NextDouble() * 0.2f + 0.5f, (float)r.NextDouble() * 0.2f + 0.5f));
                colors.Add(new vec3((float)r.NextDouble() * 0.2f + 0.8f, (float)r.NextDouble() * 0.2f + 0.5f, (float)r.NextDouble() * 0.2f + 0.5f));
                colors.Add(new vec3((float)r.NextDouble() * 0.2f + 0.8f, (float)r.NextDouble() * 0.2f + 0.5f, (float)r.NextDouble() * 0.2f + 0.5f));
            }
        }

        /// <summary>
        ///     Makes a rectangle with a certain size and uv in cartesian space
        /// </summary>
        /// <param name="p0">Lower left corner of the rect</param>
        /// <param name="p1">Upper right corner of the rect</param>
        /// <param name="uv0">Lower left corner's uv</param>
        /// <param name="uv1">Upper right corner's uv</param>
        public static mesh rect(vec2 p0, vec2 p1, vec2 uv0, vec2 uv1) {
            mesh res = new mesh(meshComponent.uvs);
            res.addUVOnlyTriangle(new vec3(p0), new vec3(p1), new vec3(p0.x, p1.y, 0),
                uv0, uv1, new vec2(uv0.x, uv1.y)
            );
            res.addUVOnlyTriangle(new vec3(p0), new vec3(p1.x, p0.y, 0), new vec3(p1),
                uv0, new vec2(uv1.x, uv0.y), uv1
            );
            return res;
        }
        public static mesh text(string text, float x, float y, float scale = 0.2f, float charWidth = 0.5f) {
            mesh res = new mesh(meshComponent.uvs);
            
            float xFactor = scale * charWidth;

            vec2 c0 = new vec2(x, y);
            vec2 c1 = c0 + new vec2(xFactor, scale);
            
            for(int i = 0; i < text.Length; i++) {
                ivec2 pos = new ivec2((text[i] - 16) % 16, (text[i] - 16) / 16);

                vec2 uv0 = new vec2(pos.x / 16.0f, pos.y / 16.0f);
                vec2 uv1 = uv0 + new vec2(1.0f / 16.0f * charWidth, -1.0f / 16.0f);

                res.addUVOnlyTriangle(
                    new vec3(c0), new vec3(c1), new vec3(c0.x, c1.y, 0),
                    uv0, uv1, new vec2(uv0.x, uv1.y)
                );

                res.addUVOnlyTriangle(
                    new vec3(c0), new vec3(c1.x, c0.y, 0), new vec3(c1),
                    uv0, new vec2(uv1.x, uv0.y), uv1
                );

                c0.x += xFactor;
                c1.x += xFactor;
            }

            return res;
        }
        public static mesh coloredRectangle(vec2 size, vec3 color) {
            mesh res = new mesh(meshComponent.colors);
            res.addColorOnlyTriangle(vec3.Zero, new vec3(size.x, 0, 0), new vec3(size.x, size.y, 0), color, color, color);
            res.addColorOnlyTriangle(vec3.Zero, new vec3(size.x, size.y, 0), new vec3(0, size.y, 0), color, color, color);
            return res;
        }
        
        public mesh() : this(meshComponent.normals | meshComponent.uvs) {
        }
        public mesh(meshComponent components) {
            positions = new List<vec3>();
            indices = new List<int>();
            this.components = components;

            if ((components & meshComponent.normals) != 0) normals = new List<vec3>();
            if ((components & meshComponent.uvs) != 0) uvs = new List<vec2>();
            if ((components & meshComponent.colors) != 0) colors = new List<vec3>();
        }
    }

    public static class meshExtensions {
        public static float[] rawData(this List<vec2> list) {
            float[] res = new float[list.Count * 2];
            for (int i = 0; i < list.Count; i++) {
                res[i * 2 + 0] = list[i].x;
                res[i * 2 + 1] = list[i].y;
            }
            return res;
        }
        public static float[] rawData(this List<vec3> list) {
            float[] res = new float[list.Count * 3];
            for (int i = 0; i < list.Count; i++) {
                res[i * 3 + 0] = list[i].x;
                res[i * 3 + 1] = list[i].y;
                res[i * 3 + 2] = list[i].z;
            }
            return res;
        }
        public static int[] rawData(this List<int> list) {
            return list.ToArray();
        }

        public static float[] rawData(this vec2[] list) {
            float[] res = new float[list.Length * 2];
            for (int i = 0; i < list.Length; i++) {
                res[i * 2 + 0] = list[i].x;
                res[i * 2 + 1] = list[i].y;
            }
            return res;
        }
        public static float[] rawData(this vec3[] list) {
            float[] res = new float[list.Length * 3];
            for (int i = 0; i < list.Length; i++) {
                res[i * 3 + 0] = list[i].x;
                res[i * 3 + 1] = list[i].y;
                res[i * 3 + 2] = list[i].z;
            }
            return res;
        }

        public static bool compareVec(vec2 a, vec2 b) {
            float eps = 0.00001f;
            return Math.Abs(a.x - b.x) < eps && Math.Abs(a.y - b.y) < eps;
        }
        public static bool compareVec(vec3 a, vec3 b) {
            float eps = 0.00001f;
            return Math.Abs(a.x - b.x) < eps && Math.Abs(a.y - b.y) < eps && Math.Abs(a.z - b.z) < eps;
        }
    }


}
