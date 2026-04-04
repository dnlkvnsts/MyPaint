using Paint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonPlugin
{
    public class Plugin : IPlugin
    {
        public string Name => "Многоугольник";

        public IShape CreateInstance() => new PolygonShape();

    }
}
