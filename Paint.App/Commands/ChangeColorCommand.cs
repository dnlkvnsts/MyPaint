using Paint.Core;
using System;

using System.Windows.Media;

namespace Paint.App.Commands
{
    public class ChangeColorCommand : Paint.Core.ICommand
    {
        
        private List<(IShape shape, Brush oldBrush)> _oldStates = new();
        private Brush _newBrush;
        private bool _isStroke; 
        private Action _redraw;

        public ChangeColorCommand(List<IShape> shapes, Brush newBrush, bool isStroke, Action redraw)
        {
            _newBrush = newBrush;
            _isStroke = isStroke;
            _redraw = redraw;

            
            foreach (var s in shapes)
            {
                _oldStates.Add((s, isStroke ? s.StrokeColor : s.FillColor));
            }
        }

        public void Execute()
        {
            foreach (var state in _oldStates)
            {
                if (_isStroke) state.shape.StrokeColor = _newBrush;
                else state.shape.FillColor = _newBrush;
            }
            _redraw();
        }

        public void Unexecute()
        {
            foreach (var state in _oldStates)
            {
                if (_isStroke) state.shape.StrokeColor = state.oldBrush;
                else state.shape.FillColor = state.oldBrush;
            }
            _redraw();
        }
    }
}
