using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Paint.Core
{
    public  interface IShape
    {
        List<Point> Points { get; set; }
        Brush StrokeColor { get; set; }
        double StrokeThickness { get; set; }

        Brush FillColor { get; set; }


        double Angle { get; set; }
        bool IsSelected { get; set; }

        int OrderIndex { get; set; }


        void Draw(Canvas canvas);
        IShape Clone();
    }
}
