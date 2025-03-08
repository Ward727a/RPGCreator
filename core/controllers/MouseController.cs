using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using RPGCreator.core.types;
using RPGCreator.core.types.Math.Transform;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.controllers
{
    static class MouseController
    {
        public struct DragResult
        {
            public bool success = false;
            public Position StartAt;
            public Position StopAt;
            public GameObject DraggedObject;
            public bool IsDroppedOnAnother;
            public GameObject DroppedOn;

            public DragResult()
            {
            }
        }

        static readonly Stack<GameObject> _InObject = [];
        static private DragResult _Dragging = new();

        static public void InObject(GameObject obj)
        {
            Log.Logger.Verbose($"[{_InObject.Count+1}]Added object {obj}");
            _InObject.Push(obj);
        }

        static public GameObject LastObject()
        {
            if(_InObject.Count == 0)
            {
                return null;
            }
            return _InObject.Peek();
        }

        static public GameObject OutObject()
        {
            return _InObject.Pop();
        }

        static public GameObject OutObject(GameObject ObjectAwaited)
        {
            if(_InObject.Peek() == ObjectAwaited)
            {
                Log.Logger.Verbose($"[{_InObject.Count}]Removed object {_InObject.Peek()}");
                return _InObject.Pop();
            } else
            {
                Log.Logger.Error($"Awaited object {ObjectAwaited} but getted {_InObject.Peek()} when trying to OutObject from MouseHover object list.");
                return null;
            }
        }

        static public bool CanDrag()
        {
            return _Dragging.DraggedObject == null;
        }

        static public void StartDrag(GameObject obj)
        {
            if(!CanDrag())
            {
                Log.Logger.Error($"Trying to drag a new object ({obj}) but already dragging another object ({_Dragging}).");
                return;
            }

            DragResult result = new();
            result.StartAt = new Position(MouseExtended.GetState().Position);
            result.DraggedObject = obj;

            _Dragging = result;
        }

        static public DragResult StopDragging()
        {
            if(CanDrag())
            {
                Log.Logger.Error("Trying to stop dragging an object but there is no object currently being dragged.");
                return new();
            }

            if(LastObject() != _Dragging.DraggedObject) {
                _Dragging.IsDroppedOnAnother = true;
                _Dragging.DraggedObject = LastObject();
            }

            _Dragging.StopAt = new(MouseExtended.GetState().Position);
            _Dragging.success = true;
            DragResult oldResult = _Dragging;

            _Dragging = new();
            return oldResult;
        }

        static public GameObject GetDraggedObject()
        {
            return _Dragging.DraggedObject;
        }

        static public Position GetDragStartPosition()
        {
            return _Dragging.StartAt;
        }
    }
}
