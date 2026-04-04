using Paint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolylinePlugin
{
    public class Plugin : IPlugin
    {
        public string Name => "Ломаная";

        public IShape CreateInstance() => new PolylineShape();

    }
}
