using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;      // Singleton instance
    private int score;                              // Score
    private TextMeshProUGUI scoreText;              // Text component to show score
    public bool rotating;                           // Indicates if the grouped hexagons rotating or not.
    public bool noBomb;                             // Disables the bombs.

    #region Unity Callbacks

    void Awake()
    {
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        // Make the game portrait only
        Screen.orientation = ScreenOrientation.Portrait;
    }

    // Initialization
    void Start()
    {
        scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();
        GridManager.instance.FillGrid();
        rotating = false;
        noBomb = false;
    }
    #endregion

    // To add score
    public void AddScore(int toBeAdded)
    {
        score += toBeAdded;
        if (score % 1000 == 0 && noBomb == false)
        {
            GridManager.instance.bombIsReady = true;
        }
        scoreText.text = score.ToString();
        StartCoroutine("GlowScore");

    }
    // Rotate 3 times to check matching hexagons
    public void RotateCheck3(bool isClockwise)
    {
        if (rotating) { return; }       

        rotating = true;
        
        StartCoroutine("RotateCheck", isClockwise);

        if(!(Bomb.instance == null))
        {
            print("counting");
            Bomb.instance.GetComponent<Bomb>().CountDown();
        }
    }
    IEnumerator RotateCheck(bool isClockwise)
    {

        for (int i = 0; i < 3; i++)
        {
            GridManager.instance.RotateSelected(isClockwise);
            if (GridManager.instance.CheckForMatches())
            {
                Selector.instance.Deselect();
                rotating = false;
                yield break;
            }
            yield return new WaitForSeconds(.5f);
        }
        Selector.instance.Deselect();
        rotating = false;

       GridManager.instance.CheckIfGameOver();

    }

    public void GameOver()
    {
        scoreText.text = "Game Over!";
        Time.timeScale = 0;
    }

    //Glow the score label
    IEnumerator GlowScore()
    {
        float dilationCoefficient = 0;

        while (dilationCoefficient < .3f)
        {
            dilationCoefficient = Mathf.MoveTowards(dilationCoefficient, .3f, Time.deltaTime * .5f);

            scoreText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, dilationCoefficient);
            scoreText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, dilationCoefficient);
            yield return null;
        }

        while (dilationCoefficient > 0)
        {
            dilationCoefficient = Mathf.MoveTowards(dilationCoefficient, 0, Time.deltaTime * .5f);
            scoreText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, dilationCoefficient);
            scoreText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, dilationCoefficient);
            yield return null;
        }

    }
}