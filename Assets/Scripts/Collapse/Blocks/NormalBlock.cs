using DG.Tweening;
using UnityEngine;

namespace Collapse.Blocks {
    /**
     * Normal Block specific behavior
     */
    public class NormalBlock : Block {
        
        public override void Trigger(float delay) {
            if (IsTriggered) return;
            IsTriggered = true;

            // Clear grid from this block place
            BoardManager.Instance.ClearBlockFromGrid(this);

            // Tween effect to make the block rotate and shrink
            transform.DORotate(new Vector3(0, 0, -360), 1, RotateMode.FastBeyond360).SetDelay(delay);
            transform.DOScale(0, 1).SetDelay(delay).onComplete += () => {
                transform.DOKill();
                Destroy(gameObject);
            };
        }
    }
}
