using System;
using DG.Tweening;
using UnityEngine;

namespace Collapse.Blocks {
    /**
     * Bomb specific behavior
     */
    public class Bomb : Block {
        [SerializeField]
        private Transform Sprite;

        [SerializeField]
        private Vector3 ShakeStrength;

        [SerializeField]
        private int ShakeVibrato;

        [SerializeField]
        private float ShakeDuration;

        private Vector3 origin;

        private Action<float> completedAction;

        private void Awake() {
            origin = Sprite.localPosition;
            completedAction = Trigger;
        }

        protected override void OnMouseUp() {
            if (!BoardManager.Instance.ActiveCombo)
            {
            Shake(completedAction);
            }
        }
        
        /**
         * Convenience for shake animation with callback in the end
         */
        private void Shake(Action<float> onComplete = null) {
            Sprite.DOKill();
            Sprite.localPosition = origin;
            Sprite.DOShakePosition(ShakeDuration, ShakeStrength, ShakeVibrato, fadeOut: false).onComplete += () => {
                onComplete?.Invoke(0);
            };
        }

        // Trigger Function
        public override void Trigger(float delay) {
            if (IsTriggered) return;
            IsTriggered = true;

            //Trigger the origin bombs
            if (delay == 0)
            BoardManager.Instance.TriggerBomb(this);

            //Make any bomb explode
            Explode(delay);
        }

        // Explode Effect with a delay
        public void Explode(float delay)
        {
            // Tween scale effect 
            transform.DOScale(1f, 0);
            transform.DOScale(1.1f, 0.3f).SetDelay(delay).onComplete += () => {
                transform.DOKill();
                Destroy(gameObject);
            };
        }
    }
}