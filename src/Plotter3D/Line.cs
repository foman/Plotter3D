using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace Thorlabs.WPF.Plotter3D
{
    public class Line
    {
        public Point3D startPoint;

        public Point3D endPoint;

        public double Thickness = 0.02;

        public Model3D Create()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            var halfThickness = Thickness / 2;
            mesh.Positions.Add(new Point3D(1, 0, 0));
            mesh.Positions.Add(new Point3D(1 + halfThickness, 0, 0));
            mesh.Positions.Add(new Point3D(1 + halfThickness, 2.5, 0));
            mesh.Positions.Add(new Point3D(1 + halfThickness, 2.5, halfThickness));

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(1);

            var normal = MathHelper.CalculateNormal(mesh.Positions[0], mesh.Positions[1], mesh.Positions[2]);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            GeometryModel3D model = new GeometryModel3D();
            model.Geometry = mesh;
            //model.Material = new EmissiveMaterial(new SolidColorBrush(Colors.Blue));
            //model.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Blue)); ;

            var mg = new MaterialGroup();
            mg.Children.Add(new DiffuseMaterial(Brushes.Black));
            mg.Children.Add(new EmissiveMaterial(new SolidColorBrush(Colors.Blue)));
            mg.Freeze();

            model.Material = mg;
            model.BackMaterial = mg;
            return model;
        }


    }
}
