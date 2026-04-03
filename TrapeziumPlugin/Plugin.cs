using Paint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrapeziumPlugin
{
    public  class Plugin : IPlugin
    {
        public string Name  => "Трапеция";

        public IShape CreateInstance () => new TrapeziumShape ();
    }
}
