using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Levels;
using Raylib_cs;
using MathExtras;
using Player;

namespace Engine
{
    public static class ActionHistory
    {
        private static List<EditAction> pastActions = new List<EditAction>();
        private static List<EditAction> futureActions = new List<EditAction>();

        public static void AddAction(EditAction action)
        {
            ClearFutureActions();
            pastActions.Insert(0, action);
        }

        public static void ClearFutureActions()
        {
            futureActions.Clear();
        }

        public static void UndoLastAction(Scene scene)
        {
            if (pastActions.Count < 1)
            {
                Console.WriteLine("Nothing to undo");
                return;
            }
            pastActions[0].Undo(scene);
            futureActions.Insert(0, pastActions[0]);
            pastActions.RemoveAt(0);
        }

        public static void RedoNextAction(Scene scene)
        {
            if (futureActions.Count < 1)
            {
                Console.WriteLine("Nothing to redo");
                return;
            }
            futureActions[0].Redo(scene);
            pastActions.Insert(0, futureActions[0]);
            futureActions.RemoveAt(0);
        }
    }
}