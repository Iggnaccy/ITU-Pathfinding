using UnityEngine;
using DG.Tweening;
using System;

public class UnitMovementAnimator : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    private Unit player, enemy;
    private float tileSize;
    

    public void Initialize(Unit player, Unit enemy, float tileSize)
    {
        this.player = player;
        this.enemy = enemy;
        this.tileSize = tileSize;
    }

    public void AnimatePath(Tile[] path, float durationPerSegment, Action callback = null)
    {
        AnimateObjectOnPath(player.gameObject, path, durationPerSegment, () => {
            player.x = path[^1].x;
            player.y = path[^1].y;
            callback?.Invoke();
        });
    }

    public void AnimateObjectOnPath(GameObject obj, Tile[] path, float durationPerSegment, Action callback = null)
    {
        if (path.Length < 1) return;
        TileEvents.EnableClick = false;
        Sequence movementSequence = DOTween.Sequence();
        Vector3 directionToFirst = new Vector3(path[0].x * tileSize, 0, path[0].y * tileSize) - obj.transform.position;
        Quaternion lastRotation = obj.transform.rotation;
        if(directionToFirst != Vector3.zero)
        {
            Quaternion initialRotation = Quaternion.LookRotation(directionToFirst.normalized);
            if (!(Quaternion.Angle(initialRotation, lastRotation) < 1f))
            {
                movementSequence.Append(obj.transform.DORotateQuaternion(initialRotation, 0.2f));
                lastRotation = initialRotation;
            }
        }
        movementSequence.Append(obj.transform.DOMove(new Vector3(path[0].x * tileSize, 0, path[0].y * tileSize), durationPerSegment));
        for (int i = 1; i < path.Length; i++)
        {
            Vector3 startPoint = new Vector3(path[i - 1].x * tileSize, 0, path[i - 1].y * tileSize);
            Vector3 endPoint = new Vector3(path[i].x * tileSize, 0, path[i].y * tileSize);
            Vector3 direction = (endPoint - startPoint).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Debug.Log($"Target Rotation: {targetRotation.eulerAngles}, Current Rotation: {lastRotation.eulerAngles}, Angle Difference: {Quaternion.Angle(targetRotation, obj.transform.rotation)}");
            if (!(Quaternion.Angle(targetRotation, lastRotation) < 1f))
            {
                movementSequence.Append(obj.transform.DORotateQuaternion(targetRotation, 0.2f));
                lastRotation = targetRotation;
            }
            movementSequence.Append(obj.transform.DOMove(endPoint, durationPerSegment).SetEase(Ease.Linear));
        }
        movementSequence.OnComplete(() => {
            TileEvents.EnableClick = true;
            callback?.Invoke();
        });
        movementSequence.Play();
    }

    public void RespawnEnemy(int newPositionX, int newPositionY)
    {
        enemy.transform.position = new Vector3(newPositionX * tileSize, 2, newPositionY * tileSize);
        enemy.transform.DOMove(new Vector3(newPositionX * tileSize, 0, newPositionY * tileSize), 0.3f).SetEase(Ease.OutBounce);
        enemy.x = newPositionX;
        enemy.y = newPositionY;
    }

    public void Attack(Tile[] path, float durationPerSegment, Action callback = null)
    {
        var direction = new Vector3(path[0].x * tileSize, 0, path[0].y * tileSize) - player.transform.position;
        var initialRotation = Quaternion.LookRotation(direction.normalized);
        var bullet = Instantiate(bulletPrefab, player.transform.position, initialRotation);
        AnimateObjectOnPath(bullet, path, durationPerSegment, () => {
            Destroy(bullet);
            callback?.Invoke();
        });
    }
}
