using UnityEngine;
using UnityEngine.SceneManagement;

public class DestructionTable : MonoBehaviour
{
    private GameObject fruit1 = null; // İlk meyve
    private GameObject fruit2 = null; // İkinci meyve

    public GameObject destroyEffectPrefab; // Yok etme efekti
    public GameObject[] fruitPrefabs; // Tüm meyve prefab'ları (7 çift)
    public Transform[] spawnPoints; // Meyvelerin oluşturulacağı noktalar

    public float ejectForce = 15f; // Fırlatma kuvveti
    public Transform tableCenter; // Masanın merkezinin referansı
    private int score = 0; // Skor

    private int remainingFruits; // Kalan meyve sayısını takip eder
    private float remainingTime = 60f; // Geri sayım süresi
    private bool isGameOver = false; // Oyunun bitiş durumu
    private bool isExtraPointsActive = false; // Ekstra puan durumu
    private bool isTimePaused = false; // Süre durdurulduğunda aktif olur
    private float timePauseEnd = 0f; // Süre duraklamasının biteceği zaman

    private int matchCounter = 0; // Eşleşen meyve çiftleri sayacı

    // Buton kilit değişkenleri
    private bool isExtraPointsLocked = false;
    private float extraPointsUnlockTime = 0f;
    private bool isPauseTimeLocked = false;
    private float pauseTimeUnlockTime = 0f;

    private GUIStyle guiStyle = new GUIStyle(); // Skor yazı tipi stili
    private GUIStyle buttonStyle = new GUIStyle(); // Buton stili
    private GUIStyle timerStyle = new GUIStyle(); // Zaman yazı tipi stili
    private GUIStyle gameOverStyle = new GUIStyle(); // Oyun bitiş yazısı stili

    void Start()
    {
        // Skor yazı tipi ayarları
        guiStyle.fontSize = 40;
        guiStyle.normal.textColor = Color.blue;

        // Buton stili ayarları
        buttonStyle.fontSize = 30;
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.alignment = TextAnchor.MiddleCenter;

        // Zaman yazı tipi stili
        timerStyle.fontSize = 60;
        timerStyle.normal.textColor = Color.red;
        timerStyle.alignment = TextAnchor.UpperRight;

        // Oyun bitiş yazısı stili
        gameOverStyle.fontSize = 80;
        gameOverStyle.normal.textColor = Color.yellow;
        gameOverStyle.alignment = TextAnchor.MiddleCenter;

        SpawnFruits();
    }

    void Update()
    {
        if (!isGameOver && !isTimePaused)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
            {
                remainingTime = 0;
                GameOver();
            }
        }

        if (isTimePaused && Time.time >= timePauseEnd)
        {
            isTimePaused = false;
        }

        if (isExtraPointsLocked && Time.time >= extraPointsUnlockTime)
            isExtraPointsLocked = false;

        if (isPauseTimeLocked && Time.time >= pauseTimeUnlockTime)
            isPauseTimeLocked = false;

        // Sayaç 7 olduğunda mevcut meyveleri sıfırla ve yeniden spawn et
        if (matchCounter >= 7 && !isGameOver)
        {
            ResetFruits();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;

        if (fruit1 == null)
        {
            fruit1 = other.gameObject;
            Rigidbody rb = fruit1.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // İlk meyvenin fiziksel özelliklerini iptal et
            }
        }
        else if (fruit2 == null && fruit1 != other.gameObject)
        {
            fruit2 = other.gameObject;
            Rigidbody rb = fruit2.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // İkinci meyvenin fiziksel özelliklerini iptal et
            }

            CheckFruits();
        }
    }

    void CheckFruits()
    {
        if (fruit1 != null && fruit2 != null)
        {
            if (fruit1.tag == fruit2.tag)
            {
                Debug.Log("Eşleşen meyveler yok ediliyor!");
                PlayDestroyEffect(fruit1.transform.position);
                PlayDestroyEffect(fruit2.transform.position);

                if (isExtraPointsActive)
                {
                    score += 300;
                    isExtraPointsActive = false;
                }
                else
                {
                    score += 100;
                }

                Destroy(fruit1, 0.5f);
                Destroy(fruit2, 0.5f);
                fruit1 = null;
                fruit2 = null;

                remainingFruits -= 2;

                // Sayaç artır
                matchCounter++;
            }
            else
            {
                Debug.Log("Meyveler farklı! İkinci meyve ittiriliyor.");
                EjectObject(fruit2);
                fruit2 = null;
            }
        }
    }

    void PlayDestroyEffect(Vector3 position)
    {
        if (destroyEffectPrefab != null)
        {
            GameObject effect = Instantiate(destroyEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 1.5f);
        }
    }

    void EjectObject(GameObject fruit)
    {
        Vector3 ejectDirection = (fruit.transform.position - tableCenter.position).normalized;
        Rigidbody rb = fruit.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(ejectDirection * ejectForce, ForceMode.Impulse);
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Oyun bitti! Toplam skor: " + score);
    }

    void SpawnFruits()
    {
        remainingFruits = spawnPoints.Length;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject fruit = Instantiate(fruitPrefabs[i % fruitPrefabs.Length], spawnPoints[i].position, Quaternion.identity);
            fruit.tag = fruitPrefabs[i % fruitPrefabs.Length].tag;
        }
    }

    void ResetFruits()
    {
        // Mevcut meyveleri sil
        foreach (GameObject fruit in GameObject.FindGameObjectsWithTag("Fruit"))
        {
            Destroy(fruit);
        }

        // Yeni meyveleri spawn et
        SpawnFruits();
        matchCounter = 0; // Sayaç sıfırlanır
        Debug.Log("Meyveler sıfırlandı!");
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 40), "Score: " + score, guiStyle);
        GUI.Label(new Rect(Screen.width - 210, 10, 200, 40), "Süre: " + Mathf.Ceil(remainingTime).ToString() + "s", timerStyle);
        GUI.Label(new Rect(10, 60, 200, 40), "Sayaç: " + matchCounter, guiStyle); // Sayaç ekrana yazdır

        if (GUI.Button(new Rect(10, Screen.height - 70, 200, 50), "RESETLE", buttonStyle))
        {
            RestartGame();
        }

        buttonStyle.normal.background = MakeTexture(200, 50, isExtraPointsLocked ? Color.gray : new Color(1f, 0.4f, 0.8f));
        GUI.enabled = !isExtraPointsLocked;
        if (GUI.Button(new Rect(Screen.width - 220, Screen.height - 70, 200, 50), "EKSTRA PUAN", buttonStyle))
        {
            ActivateExtraPoints();
            isExtraPointsLocked = true;
            extraPointsUnlockTime = Time.time + 5f;
        }
        GUI.enabled = true;

        buttonStyle.normal.background = MakeTexture(200, 50, isPauseTimeLocked ? Color.gray : new Color(1f, 0.4f, 0.8f));
        GUI.enabled = !isPauseTimeLocked;
        if (GUI.Button(new Rect(Screen.width - 220, 70, 200, 50), "SÜREYİ DURDUR", buttonStyle))
        {
            PauseTime();
            isPauseTimeLocked = true;
            pauseTimeUnlockTime = Time.time + 5f;
        }
        GUI.enabled = true;

        if (isGameOver)
        {
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 50, 400, 100), "Oyun Bitti!\nSkor: " + score, gameOverStyle);
        }
    }

    void PauseTime()
    {
        isTimePaused = true;
        timePauseEnd = Time.time + 10f;
        Debug.Log("Süre durduruldu!");
    }

    void ActivateExtraPoints()
    {
        if (!isGameOver)
        {
            isExtraPointsActive = true;
            Debug.Log("Ekstra puan aktif! Bir sonraki eşleşmede 300 puan.");
        }
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    Texture2D MakeTexture(int width, int height, Color col)
    {
        Texture2D result = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
        result.SetPixels(pixels);
        result.Apply();
        return result;
    }
}
