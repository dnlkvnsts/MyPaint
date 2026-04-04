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

namespace RectanglePlugin
{
    public class RectangleShape : IShape
    {
        public List<Point> Points { get; set; } = new List<Point>();
        public Brush StrokeColor { get; set; } = Brushes.Black;
        public double StrokeThickness { get; set; } = 2;
        public Brush FillColor { get; set; } = Brushes.Transparent; // Заливка
        public double Angle { get; set; } = 0;                     // Угол поворота
        public bool IsSelected { get; set; } = false;              // Выделена ли фигура
        public int OrderIndex { get; set; } = 0;



        public void Draw(Canvas canvas)
        {
            if(Points.Count < 2) return;

            Point start = Points[0];
            Point end = Points[1];

            double left = Math.Min(start.X, end.X);
            double top = Math.Min(start.Y, end.Y);
            double width = Math.Abs(start.X - end.X);
            double height = Math.Abs(start.Y - end.Y);
    

            Rectangle rectangle = new Rectangle
            {
                Stroke = StrokeColor,
                StrokeThickness = StrokeThickness,
                Fill = FillColor,
                Width = width,
                Height = height,
                Uid = Guid.NewGuid().ToString()
            };

            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);

            canvas.Children.Add(rectangle);

        }



        public IShape Clone()
        {
            return new RectangleShape
            {
                // Создаем новый список точек, чтобы не было ссылки на старый
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
