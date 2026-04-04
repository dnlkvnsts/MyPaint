using Paint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PolygonPlugin
{
    public  class PolygonShape : IShape
    {
        public List<Point> Points { get; set; } = new List<Point>();
        public Brush StrokeColor { get; set; } = Brushes.Black;
        public double StrokeThickness { get; set; } = 2;
        public Brush FillColor { get; set; } = Brushes.Transparent;
        public double Angle { get; set; } = 0;
        public bool IsSelected { get; set; } = false;
        public int OrderIndex { get; set; } = 0;

        public int SideCount { get; set; } = 5;



        public void Draw(Canvas canvas)
        {
            if (Points.Count < 2) return;

            Point center = Points[0];
            Point edge = Points[1];

            double radius = Math.Sqrt(Math.Pow(edge.X - center.X, 2) + Math.Pow(edge.Y - center.Y, 2));

           
            double startAngle = Math.Atan2(edge.Y - center.Y, edge.X - center.X);

            PointCollection polygonPoints = new PointCollection();

            for (int i = 0; i < SideCount; i++)
            {
               
                double angle = startAngle + (i * 2.0 * Math.PI / SideCount);
                double x = center.X + Math.Cos(angle) * radius;
                double y = center.Y + Math.Sin(angle) * radius;
                polygonPoints.Add(new Point(x, y));
            }

            Polygon polygon = new Polygon
            {
                Stroke = StrokeColor,
                StrokeThickness = StrokeThickness,
                Fill = FillColor,
                Points = polygonPoints,
                IsHitTestVisible = false
            };


            canvas.Children.Add(polygon);
        }




        public IShape Clone()
        {
            return new PolygonShape
            {

                Points = new List<Point>(this.Points),
                StrokeColor = this.StrokeColor,
                StrokeThickness = this.StrokeThickness,
                FillColor = this.FillColor,
                Angle = this.Angle,
                IsSelected = this.IsSelected,
                OrderIndex = this.OrderIndex,
                SideCount = this.SideCount
            };
        }
    }
}
