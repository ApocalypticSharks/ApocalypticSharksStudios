using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class RoadEntryPoint : MonoBehaviour
    {
        TownGrid _grid;

        public Vector3 Position => _grid != null ? _grid.EntryWorldPosition : transform.position;
        public Vector2 Direction => _grid != null ? _grid.EntryDirection : Vector2.left;

        public void Initialize(TownGrid grid)
        {
            _grid = grid;
            transform.position = grid.EntryWorldPosition;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)Direction * 1.5f);
        }
    }
}
