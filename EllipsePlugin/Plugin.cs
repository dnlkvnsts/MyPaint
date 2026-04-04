using Paint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EllipsePlugin
{
    public class Plugin : IPlugin
    {
        public string Name => "Эллипс";

        public IShape CreateInstance() => new EllipseShape();
    }
}
