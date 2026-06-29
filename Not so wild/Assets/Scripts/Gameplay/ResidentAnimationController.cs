using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class ResidentAnimationController : MonoBehaviour
    {
        static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
        static readonly int IsShootingHash = Animator.StringToHash("isShooting");

        Animator _animator;

        void Awake()
        {
            CacheAnimator();
        }

        public void SetWalking(bool walking)
        {
            CacheAnimator();
            if (_animator != null)
            {
                _animator.SetBool(IsWalkingHash, walking);
            }
        }

        public void SetShooting(bool shooting)
        {
            CacheAnimator();
            if (_animator == null)
            {
                return;
            }

            if (shooting)
            {
                _animator.SetBool(IsWalkingHash, false);
            }

            _animator.SetBool(IsShootingHash, shooting);
        }

        void CacheAnimator()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }
    }
}
