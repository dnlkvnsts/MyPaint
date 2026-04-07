using Paint.App.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint.App.Commands
{
    public class LayerCollectionCommand : Paint.Core.ICommand
    {
        private readonly ObservableCollection<Layer> _layers;
        private readonly Layer _layer;
        private readonly int _index;
        private readonly bool _isAdding;
        private readonly Action _redraw;

        public LayerCollectionCommand(ObservableCollection<Layer> layers, Layer layer, bool isAdding, Action redraw)
        {
            _layers = layers;
            _layer = layer;
            _isAdding = isAdding;
            _redraw = redraw;
            // Запоминаем индекс, чтобы при Undo вернуть слой на то же место
            _index = layers.IndexOf(layer);
            if (_index < 0) _index = layers.Count;
        }

        public void Execute()
        {
            if (_isAdding) _layers.Add(_layer);
            else _layers.Remove(_layer);
            _redraw();
        }

        public void Unexecute()
        {
            if (_isAdding) _layers.Remove(_layer);
            else _layers.Insert(Math.Min(_index, _layers.Count), _layer);
            _redraw();
        }
    }
}
