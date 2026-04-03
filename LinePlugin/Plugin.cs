using Paint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinePlugin
{
    public class Plugin : IPlugin
    {
        public string Name => "Линия";

        public IShape CreateInstance() => new LineShape();

    }
}
