using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Plane dragPlane;
    private Vector3 offset;
    private bool isDragging = false;

    public GameObject dragEffectPrefab; // Sürükleme efekti
    private GameObject dragEffectInstance;

    public GameObject dropEffectPrefab; // Bırakma efekti
    private bool droppedOnce = false; // Efektin sadece bir kez çalıştığını kontrol eder

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnMouseDown()
    {
        if (rb != null)
            rb.isKinematic = true;

        dragPlane = new Plane(Vector3.up, Vector3.zero);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (dragPlane.Raycast(ray, out distance))
        {
            offset = transform.position - ray.GetPoint(distance);
        }

        if (dragEffectPrefab != null)
        {
            dragEffectInstance = Instantiate(dragEffectPrefab, transform.position, Quaternion.identity);
        }

        isDragging = true;
        droppedOnce = false; // Yeni bir sürükleme başlatıldığında sıfırla
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance;

            if (dragPlane.Raycast(ray, out distance))
            {
                transform.position = ray.GetPoint(distance) + offset;

                if (dragEffectInstance != null)
                {
                    dragEffectInstance.transform.position = transform.position;
                }
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        if (dragEffectInstance != null)
        {
            Destroy(dragEffectInstance);
        }

        if (!droppedOnce && dropEffectPrefab != null)
        {
            // Bırakma efektini oluştur ve 1 saniye sonra yok et
            GameObject dropEffectInstance = Instantiate(dropEffectPrefab, transform.position, Quaternion.identity);
            Destroy(dropEffectInstance, 1f); // 1 saniye sonra efekti yok et

            droppedOnce = true;
        }

        if (rb != null)
            rb.isKinematic = false;
    }
}
