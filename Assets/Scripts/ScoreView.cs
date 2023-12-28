using UnityEngine;
using TMPro;
using DG.Tweening;

public class ScoreView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    private int _lastScore;
    private void OnEnable()
    {
        ScoreManager.onScoreChange += Render;
    }
    private void OnDisable()
    {
        ScoreManager.onScoreChange -= Render;
    }
    private void Render(int score)
    {
        _scoreText.text = $"Score: {score}";

        _scoreText.transform.DOKill();
        _scoreText.DOKill();
        _scoreText.transform.DOPunchScale(Vector3.one * .2f, .4f);

        if (score == 0) return;

        Color color = _lastScore > score ? Color.red : Color.green;
        _scoreText.DOColor(color, .2f)
            .OnComplete(() => _scoreText.DOColor(Color.white, .2f));

        _lastScore = score;
    }
}
