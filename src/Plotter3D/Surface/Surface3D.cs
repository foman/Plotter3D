using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using HelixToolkit.Wpf;
using Plotter3D.Common;

namespace Plotter3D
{
    /// <summary>
    /// The surface created by position of points.
    /// </summary>
    public class Surface3D : ModelVisual3D
    {
        public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(Point3D[,]), typeof(Surface3D), new UIPropertyMetadata(null, new PropertyChangedCallback(Surface3D.GeometryChanged)));

        public static readonly DependencyProperty TextureBuilderProperty = DependencyProperty.Register("TextureBuilder", typeof(SurfaceTextureBuilder), typeof(Surface3D), new UIPropertyMetadata(null, new PropertyChangedCallback(Surface3D.GeometryChanged)));

        private double zRangeWidth = 1.0;

        private Range<double> _zRange;

        private GeometryModel3D _model3D;

        private MeshGeometry3D _mesh;

        public Range<double> ZRange
        {
            get
            {
                return _zRange;
            }
            set
            {
                _zRange = value;
                this.zRangeWidth = value.Max - value.Min;
            }
        }

        public Point3D[,] Points
        {
            get
            {
                return (Point3D[,])base.GetValue(Surface3D.PointsProperty);
            }
            set
            {
                base.SetValue(Surface3D.PointsProperty, value);
            }
        }

        public double BorderThickness
        {
            get;
            set;
        }

        public SurfaceTextureBuilder TextureBuilder
        {
            get
            {
                return (SurfaceTextureBuilder)base.GetValue(Surface3D.TextureBuilderProperty);
            }
            set
            {
                base.SetValue(Surface3D.TextureBuilderProperty, value);
            }
        }

        public Surface3D()
        {
            _mesh = new MeshGeometry3D();
            _model3D = new GeometryModel3D();
            _model3D.Geometry = _mesh;
            base.Content = _model3D;
        }

        protected static void GeometryChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((Surface3D)sender).UpdateGeometry();
        }

        private void UpdateGeometry()
        {
            this.CreateSurface();
        }

        private void CreateSurface()
        {
            if (this.Points != null && this.TextureBuilder != null)
            {
                Material texture = this.TextureBuilder.CreateTexture();
                int xWidth = this.Points.GetLength(0);
                int yWidth = this.Points.GetLength(1);
                int capacity = (xWidth - 1) * (yWidth - 1);
                Point3DCollection positions = new Point3DCollection(capacity);
                Int32Collection indices = new Int32Collection(capacity);
                PointCollection texCoords = new PointCollection(capacity);
                Vector3DCollection normals = new Vector3DCollection(capacity);
                int indiceCount = 0;
                for (int ix = 0; ix < xWidth - 1; ix++)
                {
                    if (!double.IsNaN(this.Points[ix, 0].Z))
                    {
                        if (!double.IsNaN(this.Points[ix + 1, 0].Z))
                        {
                            for (int iy = 0; iy < yWidth - 1; iy++)
                            {
                                // V0-----V3
                                // |       |
                                // |       |
                                // V1-----V2  

                                //Add Triangle V0--V1--V2

                                positions.Add(this.Points[ix, iy]);
                                positions.Add(this.Points[ix + 1, iy]);
                                positions.Add(this.Points[ix + 1, iy + 1]);
                                double middleZ = (this.Points[ix, iy].Z + this.Points[ix + 1, iy].Z + this.Points[ix + 1, iy + 1].Z + this.Points[ix, iy + 1].Z) / 4.0;
                                Point texturePt = this.TextureBuilder.GetTextureMapping(middleZ);
                                texCoords.Add(texturePt);
                                texCoords.Add(texturePt);
                                texCoords.Add(texturePt);
                                indices.Add(indiceCount++);
                                indices.Add(indiceCount++);
                                indices.Add(indiceCount++);
                                Vector3D normal = MathHelper.CalculateNormal(this.Points[ix + 1, iy + 1], this.Points[ix + 1, iy], this.Points[ix, iy]);
                                normals.Add(normal);
                                normals.Add(normal);
                                normals.Add(normal);

                                //Add Triangle V2--V3-V0

                                positions.Add(this.Points[ix + 1, iy + 1]);
                                positions.Add(this.Points[ix, iy + 1]);
                                positions.Add(this.Points[ix, iy]);
                                texCoords.Add(texturePt);
                                texCoords.Add(texturePt);
                                texCoords.Add(texturePt);
                                indices.Add(indiceCount++);
                                indices.Add(indiceCount++);
                                indices.Add(indiceCount++);
                                Vector3D normal2 = MathHelper.CalculateNormal(this.Points[ix, iy], this.Points[ix, iy + 1], this.Points[ix + 1, iy + 1]);
                                normals.Add(normal2);
                                normals.Add(normal2);
                                normals.Add(normal2);
                            }
                        }
                    }
                }
                positions.Freeze();
                _mesh.Positions = positions;
                indices.Freeze();
                _mesh.TriangleIndices = indices;
                texCoords.Freeze();
                _mesh.TextureCoordinates = texCoords;
                normals.Freeze();
                _mesh.Normals = normals;
                _model3D.Material = texture;
                _model3D.BackMaterial = texture;
            }
        }

        /// <summary>
        /// Get the nearest point
        /// V0-----V3------V5------V7
        /// |       |       |       |
        /// |       |       |       |
        /// V1-----V2------v4------v6
        /// |       |       |       |
        /// |       |       |       |
        /// V8-----V9------v10-----v11
        /// |       |       |       |
        /// |       |       |       |
        ///V12-----V13-----v14-----v15
        /// Every cell contains 2 triangles,each triangle we use 3 positions to represent,
        /// so each cell contians 6 positions.
        /// </summary>
        /// <param name="pt">The point on the surface</param>
        /// <param name="verIndex1">The first index of triangle in Mesh Positions</param>
        /// <returns></returns>
        public Point3D GetNearestPoint(Point3D pt, int verIndex1)
        {
            double minimum = double.MaxValue;
            Point3D nearestPt = _mesh.Positions[verIndex1];

            var colCount = Points.GetLength(1);
            var rowCount = Points.GetLength(0);

            //Get the row index in Points
            var row = verIndex1 / 6 / (colCount - 1);

            //Get the column index in Points
            var col = verIndex1 / 6 % (colCount - 1);

            //Refering the table above,If the indexes are v2-v4-v10,
            //Its row index is 1,col index is 1,so we shoud iterate
            //points' row index from 0 to 3,column index from 0 to 3.

            row = row > 0 ? row - 1 : row;
            col = col > 0 ? col - 1 : col;
            var p1 = this.Transform.Transform(pt);
            for (int i = row; i < row + 4 && i < rowCount; i++)
                for (int j = col; j < col + 4 && j < colCount; j++)
                {
                    var p2 = this.Transform.Transform(Points[i, j]);

                    var lengthSquared = (p1 - p2).LengthSquared;
                    if (lengthSquared < minimum)
                    {
                        nearestPt = Points[i, j];
                        minimum = lengthSquared;
                    }
                }

            return nearestPt;
        }
    }
}
