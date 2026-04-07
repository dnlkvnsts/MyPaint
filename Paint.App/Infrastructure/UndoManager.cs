using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint.App.Infrastructure
{
    public class UndoManager
    {
       
        private readonly Stack<Paint.Core.ICommand> _undoStack = new Stack<Paint.Core.ICommand>();

        
        private readonly Stack<Paint.Core.ICommand> _redoStack = new Stack<Paint.Core.ICommand>();

        
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        
        public void Execute(Paint.Core.ICommand command)
        {
           
            command.Execute();

           
            _undoStack.Push(command);

           
            _redoStack.Clear();
        }

       
        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
              
                var command = _undoStack.Pop();

               
                command.Unexecute();

               
                _redoStack.Push(command);
            }
        }

        
        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                
                var command = _redoStack.Pop();

               
                command.Execute();

               
                _undoStack.Push(command);
            }
        }

        // Очистка истории (полезно при создании нового файла/очистке холста)
        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
