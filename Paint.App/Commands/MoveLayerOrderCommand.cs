using Paint.App.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint.App.Commands
{
    public class MoveLayerOrderCommand : Paint.Core.ICommand
    {
        private readonly ObservableCollection<Layer> _layers;
        private readonly int _oldIndex;
        private readonly int _newIndex;
        private readonly Action _redraw;

        public MoveLayerOrderCommand(ObservableCollection<Layer> layers, int oldIdx, int newIdx, Action redraw)
        {
            _layers = layers;
            _oldIndex = oldIdx;
            _newIndex = newIdx;
            _redraw = redraw;
        }

        public void Execute()
        {
            _layers.Move(_oldIndex, _newIndex);
            _redraw();
        }

        public void Unexecute()
        {
            _layers.Move(_newIndex, _oldIndex);
            _redraw();
        }
    }
}
