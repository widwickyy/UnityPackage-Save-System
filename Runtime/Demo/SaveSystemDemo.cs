using UnityEngine;

namespace Widwickyy.SaveSystem
{
    [System.Serializable]
    public class DemoPlayerData
    {
        public string playerName;
        public int level;
        public float health;
    }

    public class SaveSystemDemo : MonoBehaviour
    {
        [SerializeField] private string _saveKey = "sample-player";
        [SerializeField] private string _encryptionKey = "sample-encryption-key";

        private void Start()
        {
            SaveManager.Initialize(version: 1, encryptionKey: _encryptionKey);

            var dataToSave = new DemoPlayerData
            {
                playerName = "Hero",
                level = 8,
                health = 92.5f
            };

            SaveManager.Save(_saveKey, dataToSave);

            var loadedData = SaveManager.Load<DemoPlayerData>(_saveKey);
            if (loadedData == null)
                return;

            Debug.Log($"Loaded: {loadedData.playerName} Lv.{loadedData.level} HP:{loadedData.health}");
        }
    }
}
