#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;
using UnityEngine.InputSystem;

namespace Game.Scripts.ECS.Systems.DebugHelpers
{
    public class ObjectiveDebugSystem : IEcsInitSystem, IEcsRunSystem
    {
        private static readonly string[] ObjectiveKeys = { "ui.objective.brush_teeth", "ui.objective.check_mail" };
        private static readonly string[] ThoughtKeys = { "ui.thought.brush_teeth", "ui.thought.check_mail" };

        private readonly Queue<string> _issued = new Queue<string>();
        private EcsWorld _world;
        private int _next;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
        }

        public void Run(IEcsSystems systems)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.f4Key.wasPressedThisFrame)
            {
                int index = _next % ObjectiveKeys.Length;
                _next++;
                if (_issued.Contains(ObjectiveKeys[index])) return;

                _issued.Enqueue(ObjectiveKeys[index]);
                _world.ShowObjective(ObjectiveKeys[index], ThoughtKeys[index]);
            }

            if (keyboard.f5Key.wasPressedThisFrame && _issued.Count > 0)
                _world.CompleteObjective(_issued.Dequeue());
        }
    }
}
#endif
