using UnityEngine;
using UnityEngine.SceneManagement;

public class StageBounds2D : MonoBehaviour
{
    private static readonly Vector2 DefaultMin = new Vector2(-15.295f, -3.505f);
    private static readonly Vector2 DefaultMax = new Vector2(19.583f, -0.985f);

    [Header("Area jugable interna")]
    [SerializeField] private Vector2 min = DefaultMin;
    [SerializeField] private Vector2 max = DefaultMax;

    private static StageBounds2D current;
    private static Rect cachedArea;
    private static string cachedSceneKey;
    private static bool hasCachedArea;

    private Rect PlayableArea
    {
        get
        {
            float minX = Mathf.Min(min.x, max.x);
            float maxX = Mathf.Max(min.x, max.x);
            float minY = Mathf.Min(min.y, max.y);
            float maxY = Mathf.Max(min.y, max.y);
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }
    }

    private void OnEnable()
    {
        current = this;
        hasCachedArea = false;
    }

    private void OnDisable()
    {
        if (current == this)
        {
            current = null;
            hasCachedArea = false;
        }
    }

    public static Vector2 ClampPoint(Vector2 point)
    {
        return ClampPoint(point, ResolvePlayableArea());
    }

    public static Vector2 ClampRigidbodyTarget(Rigidbody2D body, Vector2 target)
    {
        Rect area = ResolvePlayableArea();
        if (area.width <= 0f || area.height <= 0f)
        {
            return target;
        }

        if (body == null)
        {
            return ClampPoint(target, area);
        }

        Collider2D[] bodyColliders = body.GetComponents<Collider2D>();
        if (bodyColliders == null || bodyColliders.Length == 0)
        {
            return ClampPoint(target, area);
        }

        Vector2 root = body.position;
        float minX = area.xMin;
        float maxX = area.xMax;
        float footOffsetY = 0f;
        bool hasSolidCollider = false;

        for (int i = 0; i < bodyColliders.Length; i++)
        {
            Collider2D bodyCollider = bodyColliders[i];
            if (bodyCollider == null || !bodyCollider.enabled || bodyCollider.isTrigger)
            {
                continue;
            }

            Bounds bounds = bodyCollider.bounds;
            minX = Mathf.Max(minX, area.xMin - (bounds.min.x - root.x));
            maxX = Mathf.Min(maxX, area.xMax - (bounds.max.x - root.x));

            float colliderFootOffsetY = bounds.min.y - root.y;
            footOffsetY = hasSolidCollider
                ? Mathf.Min(footOffsetY, colliderFootOffsetY)
                : colliderFootOffsetY;
            hasSolidCollider = true;
        }

        if (!hasSolidCollider)
        {
            return ClampPoint(target, area);
        }

        if (minX > maxX)
        {
            minX = maxX = (area.xMin + area.xMax) * 0.5f;
        }

        float minY = area.yMin - footOffsetY;
        float maxY = area.yMax - footOffsetY;
        if (minY > maxY)
        {
            minY = maxY = (area.yMin + area.yMax) * 0.5f;
        }

        return new Vector2(
            Mathf.Clamp(target.x, minX, maxX),
            Mathf.Clamp(target.y, minY, maxY)
        );
    }

    private static Vector2 ClampPoint(Vector2 point, Rect area)
    {
        return new Vector2(
            Mathf.Clamp(point.x, area.xMin, area.xMax),
            Mathf.Clamp(point.y, area.yMin, area.yMax)
        );
    }

    private static Rect ResolvePlayableArea()
    {
        if (current != null && current.isActiveAndEnabled)
        {
            return current.PlayableArea;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        string sceneKey = string.IsNullOrEmpty(activeScene.path) ? activeScene.name : activeScene.path;
        if (!hasCachedArea || cachedSceneKey != sceneKey)
        {
            cachedSceneKey = sceneKey;
            cachedArea = TryDetectAreaFromSceneWalls(out Rect detectedArea)
                ? detectedArea
                : Rect.MinMaxRect(DefaultMin.x, DefaultMin.y, DefaultMax.x, DefaultMax.y);
            hasCachedArea = true;
        }

        return cachedArea;
    }

    private static bool TryDetectAreaFromSceneWalls(out Rect area)
    {
        area = Rect.MinMaxRect(DefaultMin.x, DefaultMin.y, DefaultMax.x, DefaultMax.y);

        bool hasLeft = false;
        bool hasRight = false;
        bool hasBottom = false;
        bool hasTop = false;
        float minX = DefaultMin.x;
        float maxX = DefaultMax.x;
        float minY = DefaultMin.y;
        float maxY = DefaultMax.y;

        BoxCollider2D[] colliders = FindObjectsByType<BoxCollider2D>(FindObjectsInactive.Exclude);
        for (int i = 0; i < colliders.Length; i++)
        {
            BoxCollider2D wall = colliders[i];
            if (wall == null || !wall.enabled || wall.isTrigger)
            {
                continue;
            }

            Bounds bounds = wall.bounds;
            string wallName = wall.gameObject.name;
            if (wallName == "Muro_Izquierdo")
            {
                minX = bounds.max.x;
                hasLeft = true;
            }
            else if (wallName == "Muro_Derecho")
            {
                maxX = bounds.min.x;
                hasRight = true;
            }
            else if (wallName == "Muro_Inferior")
            {
                minY = bounds.max.y;
                hasBottom = true;
            }
            else if (wallName == "Muro_Superior")
            {
                maxY = bounds.min.y;
                hasTop = true;
            }
        }

        if (!hasLeft || !hasRight || !hasBottom || !hasTop || minX >= maxX || minY >= maxY)
        {
            return false;
        }

        area = Rect.MinMaxRect(minX, minY, maxX, maxY);
        return true;
    }
}
