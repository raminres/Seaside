using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private int levelIndex;

    public void SelectThisLevel()
    {
        GameManager.Instance.SelectLevel(levelIndex);
    }
}