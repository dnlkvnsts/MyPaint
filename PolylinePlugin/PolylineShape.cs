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

namespace PolylinePlugin
{
    public class PolylineShape : IShape
    {
        public List<Point> Points { get; set; } = new List<Point>();
        public Brush StrokeColor { get; set; } = Brushes.Black;
        public double StrokeThickness { get; set; } = 2;
        public Brush FillColor { get; set; } = Brushes.Transparent; 
        public double Angle { get; set; } = 0;                     
        public bool IsSelected { get; set; } = false;             
        public int OrderIndex { get; set; } = 0;



        public void Draw(Canvas canvas)
        {
            if (Points == null || Points.Count < 2) return;

            
            double minX = Points.Min(p => p.X);
            double maxX = Points.Max(p => p.X);
            double minY = Points.Min(p => p.Y);
            double maxY = Points.Max(p => p.Y);

           
            double centerX = (minX + maxX) / 2;
            double centerY = (minY + maxY) / 2;

            Polyline polyline = new Polyline
            {
                Stroke = StrokeColor,
                StrokeThickness = StrokeThickness,
                Fill = Brushes.Transparent,
                Points = new PointCollection(this.Points),
                Uid = Guid.NewGuid().ToString(),
                IsHitTestVisible = false
                
            };

            RotateTransform rotateTransform = new RotateTransform(Angle, centerX, centerY);
            polyline.RenderTransform = rotateTransform;

            canvas.Children.Add(polyline);
        }


        public IShape Clone()
        {
            return new PolylineShape
            {

                Points = new List<Point>(this.Points.Select(p => new Point(p.X, p.Y))),
                StrokeColor = this.StrokeColor,
                StrokeThickness = this.StrokeThickness,
                FillColor = this.FillColor,
                Angle = this.Angle,
                IsSelected = this.IsSelected,
                OrderIndex = this.OrderIndex
            };
        }
    }
}
