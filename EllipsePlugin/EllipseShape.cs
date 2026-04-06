
using Paint.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EllipsePlugin
{
    public class EllipseShape : IShape
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
            if (Points.Count < 2) return;

            Point start = Points[0];
            Point end = Points[1];

            double left = Math.Min(start.X, end.X);
            double top = Math.Min(start.Y, end.Y);
            double width = Math.Abs(start.X - end.X);
            double height = Math.Abs(start.Y - end.Y);


            Ellipse ellipse = new Ellipse
            {
                Width = width,
                Height = height,
                Stroke = StrokeColor,
                StrokeThickness = StrokeThickness,
                Fill = FillColor,
                Uid = Guid.NewGuid().ToString(),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(Angle)
            };  

            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);

            canvas.Children.Add(ellipse);
        }



        public IShape Clone()
        {
            return new EllipseShape
            {
                Points = new List<Point>(this.Points),
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
