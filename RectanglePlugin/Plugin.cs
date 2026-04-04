using Paint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RectanglePlugin
{
    public class Plugin : IPlugin
    {
        public string Name => "Прямоугольник";

        public IShape CreateInstance() => new RectangleShape();

    }
}
