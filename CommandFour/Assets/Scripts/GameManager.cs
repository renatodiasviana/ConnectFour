using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GAME_DIFFICULTY
    {
        EASY = 0,
        MEDIUM = 1,
        HARD = 2
    };

    public enum CELL_MARKING
    {
        NONE,
        PLAYER,
        AI
    };

    enum GAME_STATE
    {
        PLAYER_MOVE,
        AI_MOVE,
        RESETING
    };

    // Used to save cell data for algorithm use
    public struct CELL_DATA
    {
        public CELL_MARKING Marking;
        public Transform Object;
    };

    // This game manager is Singleton, then it's instance is global use
    public static GameManager Instance = null;

    // Attribute used to check if there are more moves to be done
    int MovesDone = 0;

    public GAME_DIFFICULTY GameDifficulty = GAME_DIFFICULTY.EASY;

    // Pieces prefabs
    public GameObject PlayerPiecePrefab;
    public GameObject AIPiecePrefab;

    // HUD Objects
    public Color MaskMoveColor;
    public Transform Buttons;
    public UnityEngine.UI.Button ResetButton;
    public UnityEngine.UI.Image PlayerImage;
    public UnityEngine.UI.Image AIImage;
    public Transform ResultDialog;

    // This value is the difference between cells in it's line. It's used to put the piece at right place
    float DifferenceBetweenLineCells = 1.315f;

    CELL_DATA [,] Cells = null;

    GAME_STATE CurrentState = GAME_STATE.PLAYER_MOVE;

    void CreateGameTable()
    {
        Cells = new CELL_DATA[6, 7];

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                Cells[i, j].Marking = CELL_MARKING.NONE;
                Cells[i, j].Object = null;
            }
        }
    }

    void EnableAllColumnsButtons(bool enabled)
    {
        for (int j = 0; j < 7; j++)
        {
            UnityEngine.UI.Button button = Buttons.GetChild(j).GetComponent<UnityEngine.UI.Button>();
            button.interactable = (Cells[0, j].Marking != CELL_MARKING.NONE) ? false : enabled;
        }
    }

    void StartNewGame()
    {
        MovesDone = 0;

        CurrentState = GAME_STATE.PLAYER_MOVE;

        CreateGameTable();

        // Update buttons
        EnableAllColumnsButtons(true);
        ResetButton.interactable = false;

        PlayerImage.color = Color.white;
        AIImage.color = MaskMoveColor;
    }

    void ResetGame()
    {
        ResetButton.interactable = true;

        // Destroy old objects
        for (int i = 0; i < 7; i++)
        {
            Transform columnObj = transform.GetChild(i);
            for (int j = 0; j < columnObj.childCount; j++)
            {
                Transform obj = columnObj.GetChild(j);
                Destroy(obj.gameObject);
            }
        }

        StartNewGame();
    }

    CELL_MARKING CheckHorizontalVictory(int currentI, int currentJ, CELL_MARKING currentMarking,
                                        int count, CELL_DATA[,] currentCells)
    {
        // Stop Condition
        if (currentJ > 6)
            return CELL_MARKING.NONE;

        if (currentCells[currentI, currentJ].Marking == CELL_MARKING.NONE)
            return CheckHorizontalVictory(currentI, currentJ + 1, CELL_MARKING.NONE, 0, currentCells);

        if (currentCells[currentI, currentJ].Marking != currentMarking)
            return CheckHorizontalVictory(currentI, currentJ + 1, currentCells[currentI, currentJ].Marking, 1, currentCells);

        count++;

        if (count == 4)
        {
            // Marking the winning pieces to be used in win animation
            for (int j = 0; j < 4; j++)
            {
                Transform piece = currentCells[currentI, currentJ - j].Object;

                if (piece != null)
                    piece.GetComponent<Piece>().IsWinnerPiece = true;
            }

            return currentMarking;
        }

        return CheckHorizontalVictory(currentI, currentJ + 1, currentMarking, count, currentCells);
    }

    CELL_MARKING CheckVerticalVictory(int currentI, int currentJ, CELL_MARKING currentMarking,
                                        int count, CELL_DATA[,] currentCells)
    {
        // Stop Condition
        if (currentI > 5)
            return CELL_MARKING.NONE;

        if (currentCells[currentI, currentJ].Marking == CELL_MARKING.NONE)
            return CheckVerticalVictory(currentI + 1, currentJ, CELL_MARKING.NONE, 0, currentCells);

        if (currentCells[currentI, currentJ].Marking != currentMarking)
            return CheckVerticalVictory(currentI + 1, currentJ, currentCells[currentI, currentJ].Marking, 1, currentCells);

        count++;

        if (count == 4)
        {
            // Marking the winning pieces to be used in win animation
            for (int i = 0; i < 4; i++)
            {
                Transform piece = currentCells[currentI - i, currentJ].Object;

                if (piece != null)
                    piece.GetComponent<Piece>().IsWinnerPiece = true;
            }

            return currentMarking;
        }

        return CheckVerticalVictory(currentI + 1, currentJ, currentMarking, count, currentCells);
    }

    CELL_MARKING CheckLeftDiagonalVictory(int currentI, int currentJ, CELL_MARKING currentMarking,
                                          int count, CELL_DATA [,] currentCells)
    {
        // Stop Condition
        if (currentI > 5 || currentJ > 6)
            return CELL_MARKING.NONE;

        if (currentCells[currentI, currentJ].Marking == CELL_MARKING.NONE)
            return CheckLeftDiagonalVictory(currentI + 1, currentJ + 1, CELL_MARKING.NONE, 0, currentCells);

        if (currentCells[currentI, currentJ].Marking != currentMarking)
            return CheckLeftDiagonalVictory(currentI + 1, currentJ + 1, currentCells[currentI, currentJ].Marking, 1, currentCells);

        count++;

        if (count == 4)
        {
            for (int i = 0; i < 4; i++)
            {
                Transform piece = currentCells[currentI - i, currentJ - i].Object;

                if (piece != null)
                    piece.GetComponent<Piece>().IsWinnerPiece = true;
            }

            return currentMarking;
        }

        return CheckLeftDiagonalVictory(currentI + 1, currentJ + 1, currentMarking, count, currentCells);
    }

    CELL_MARKING CheckRightDiagonalVictory(int currentI, int currentJ, CELL_MARKING currentMarking,
                                           int count, CELL_DATA[,] currentCells)
    {
        // Stop Condition
        if (currentI > 5 || currentJ < 0)
            return CELL_MARKING.NONE;

        if (currentCells[currentI, currentJ].Marking == CELL_MARKING.NONE)
            return CheckRightDiagonalVictory(currentI + 1, currentJ - 1, CELL_MARKING.NONE, 0, currentCells);

        if (currentCells[currentI, currentJ].Marking != currentMarking)
            return CheckRightDiagonalVictory(currentI + 1, currentJ - 1, currentCells[currentI, currentJ].Marking, 1, currentCells);

        count++;

        if (count == 4)
        {
            for (int i = 0; i < 4; i++)
            {
                Transform piece = currentCells[currentI - i, currentJ + i].Object;

                if (piece != null)
                    piece.GetComponent<Piece>().IsWinnerPiece = true;
            }

            return currentMarking;
        }

        return CheckRightDiagonalVictory(currentI + 1, currentJ - 1, currentMarking, count, currentCells);
    }

    public CELL_MARKING CheckVictory(CELL_DATA [,] currentCells)
    {
        // Check horizontal victory
        for (int i = 0; i < 6; i++)
        {
            CELL_MARKING horizontalResult = CheckHorizontalVictory(i, 0, CELL_MARKING.NONE, 0, currentCells);

            if (horizontalResult != CELL_MARKING.NONE)
                return horizontalResult;
        }

        // Check vertical victory
        for (int j = 0; j < 7; j++)
        {
            CELL_MARKING verticalResult = CheckVerticalVictory(0, j, CELL_MARKING.NONE, 0, currentCells);

            if (verticalResult != CELL_MARKING.NONE)
                return verticalResult;
        }

        // Check left diagonal victory
        CELL_MARKING leftDiagonalResult = CELL_MARKING.NONE;
        for (int i = 0; i < 3; i++)
        {
            leftDiagonalResult = CheckLeftDiagonalVictory(i, 0, CELL_MARKING.NONE, 0, currentCells);
            if (leftDiagonalResult != CELL_MARKING.NONE)
                return leftDiagonalResult;
        }

        for (int j = 0; j < 4; j++)
        {
            leftDiagonalResult = CheckLeftDiagonalVictory(0, j, CELL_MARKING.NONE, 0, currentCells);
            if (leftDiagonalResult != CELL_MARKING.NONE)
                return leftDiagonalResult;
        }

        // Check right diagonal victory
        CELL_MARKING rightDiagonalResult = CELL_MARKING.NONE;
        for (int i = 0; i < 3; i++)
        {
            rightDiagonalResult = CheckRightDiagonalVictory(i, 6, CELL_MARKING.NONE, 0, currentCells);
            if (rightDiagonalResult != CELL_MARKING.NONE)
                return rightDiagonalResult;
        }

        for (int j = 6; j > 2; j--)
        {
            rightDiagonalResult = CheckRightDiagonalVictory(0, j, CELL_MARKING.NONE, 0, currentCells);
            if (rightDiagonalResult != CELL_MARKING.NONE)
                return rightDiagonalResult;
        }

        return CELL_MARKING.NONE;
    }

    Vector3 GetNewPieceSpotPosition(int column)
    {
        int i = 0;
        while (Cells[i, column].Marking == CELL_MARKING.NONE)
        {
            i++;

            // Validating the table limit
            if (i == 6)
                break;
        }

        // Calculating end piece position
        float yMoviment = (float) i * DifferenceBetweenLineCells;
        return new Vector3(0.0f, yMoviment * -1.0f, 0.0f);
    }

    int GetSpotLine(int column)
    {
        int line = 0;
        while (Cells[line, column].Marking == CELL_MARKING.NONE)
        {
            line++;

            // Checking limit
            if (line == 6)
                break;
        }

        line--;
        return line;
    }

    void DoAiMove(int column)
    {
        // Create the new piece
        GameObject piece = Instantiate(AIPiecePrefab);
        piece.transform.parent = transform.GetChild(column);
        piece.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        // Setting callbacks
        piece.transform.GetComponent<Piece>().OnFallAnimationEnded = OnPieceFallAnimationEnded;
        piece.transform.GetComponent<Piece>().OnWinAnimationEnded = OnPieceWinAnimationEnded;
        piece.transform.GetComponent<Piece>().OnResetAnimationEnded = OnPieceResetAnimationEnded;

        // Starting falling animation
        Vector3 endPosition = GetNewPieceSpotPosition(column);
        float animationTime = Vector3.Distance(piece.transform.localPosition, endPosition) / DifferenceBetweenLineCells;
        piece.transform.GetComponent<Piece>().StartFallAnimation(endPosition, animationTime);

        // Marking AI move
        int line = GetSpotLine(column);
        Cells[line, column].Marking = CELL_MARKING.AI;
        Cells[line, column].Object = piece.transform;
        MovesDone++;
    }

    void DoMainMaxMove()
    {
        MinMaxNode bestMoveNode = MinMaxNode.CalculateBestMove(Cells);

        DoAiMove(bestMoveNode.GetSelectedColumn());
    }

    void DoRandomAIMove()
    {
        int column = 0;
        bool sortNewNumber = true;

        /* Sort new column for AI random play
           (If we choose a random full column, we must try again until find a free column */
        while (sortNewNumber)
        {
            column = Random.Range(0, 7);
            if (Cells[0, column].Marking == CELL_MARKING.NONE)
            {
                sortNewNumber = false;
                break;
            }
        }

        DoAiMove(column);
    }

    void ShowWinnerDialog(CELL_MARKING winner)
    {
        PlayerImage.color = MaskMoveColor;
        AIImage.color = MaskMoveColor;

        ResetButton.interactable = false;

        EnableAllColumnsButtons(false);

        ResultDialog.gameObject.SetActive(true);

        // Starting the winning animation
        for (int j = 0; j < 7; j++)
        {
            Transform columnObj = transform.GetChild(j);
            for (int i = 0; i < columnObj.childCount; i++)
                columnObj.GetChild(i).GetComponent<Piece>().StartWinAnimation();
        }

        string winnerTxt = "";

        switch (winner)
        {
            case CELL_MARKING.NONE:
                winnerTxt = "THERE IS NO WINNER";
            break;
            case CELL_MARKING.PLAYER:
                winnerTxt = "PLAYER WINS";
            break;
            case CELL_MARKING.AI:
                winnerTxt = "AI WINS";
            break;
        };

        ResultDialog.GetComponent<ResultDialog>().OnDialogFinished = OnResultDialogFinished;
        ResultDialog.GetComponent<ResultDialog>().ShowDialog(winnerTxt);
    }

    void OnResultDialogFinished()
    {
        ResetGame();
    }

    void OnPieceFallAnimationEnded(Transform currentTransform)
    {
        // Check victory
        CELL_MARKING victory = CheckVictory(Cells);
        if (victory != CELL_MARKING.NONE)
        {
            ShowWinnerDialog(victory);
            return;
        }

        // Checking if it's end of game (Line plus column) - This was a no winner game
        if (MovesDone == 42)
        {
            ShowWinnerDialog(CELL_MARKING.NONE);
            return;
        }

        if (CurrentState == GAME_STATE.PLAYER_MOVE)
        {
            CurrentState = GAME_STATE.AI_MOVE;

            PlayerImage.color = MaskMoveColor;
            AIImage.color = Color.white;

            // Create AI Level (Easy - Random, Medium - MinMax with 3 levels tree, Hard - MinMax with 5 levels tree)
            if (GameDifficulty == GAME_DIFFICULTY.EASY)
                DoRandomAIMove();
            else
                DoMainMaxMove();

            // If it's the first move we must enable the reset button
            ResetButton.interactable = true;
        }
        else if (CurrentState == GAME_STATE.AI_MOVE)
        {
            EnableAllColumnsButtons(true);

            CurrentState = GAME_STATE.PLAYER_MOVE;

            PlayerImage.color = Color.white;
            AIImage.color = MaskMoveColor;
        }
    }

    void OnPieceWinAnimationEnded(Transform currentTransform)
    {
        // Do nothing
    }

    void OnPieceResetAnimationEnded(Transform currentTransform)
    {
        // When the first callback is called then we reset the game
        ResetGame();
    }

    public void ChangeGameDifficulty(int level)
    {
        GameDifficulty = (GAME_DIFFICULTY) level;
    }

    public int GetMinMaxTreeLevel()
    {
        if (GameDifficulty == GAME_DIFFICULTY.EASY)
            return 0;

        return (GameDifficulty == GAME_DIFFICULTY.MEDIUM) ? 3 : 5;
    }

    public void OnColumnPressed(int column)
    {
        EnableAllColumnsButtons(false);

        // Create the new piece
        GameObject piece = Instantiate(PlayerPiecePrefab);
        piece.transform.parent = transform.GetChild(column);
        piece.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        // Setting callbacks
        piece.transform.GetComponent<Piece>().OnFallAnimationEnded = OnPieceFallAnimationEnded;
        piece.transform.GetComponent<Piece>().OnWinAnimationEnded = OnPieceWinAnimationEnded;
        piece.transform.GetComponent<Piece>().OnResetAnimationEnded = OnPieceResetAnimationEnded;

        // Starting falling animation
        Vector3 endPosition = GetNewPieceSpotPosition(column);
        float animationTime = Vector3.Distance(piece.transform.localPosition, endPosition) / DifferenceBetweenLineCells;
        piece.transform.GetComponent<Piece>().StartFallAnimation(endPosition, animationTime);

        // Do player move
        int line = GetSpotLine(column);

        Cells[line, column].Marking = CELL_MARKING.PLAYER;
        Cells[line, column].Object = piece.transform;
        MovesDone++;
    }

    public void OnResetButtonPressed()
    {
        EnableAllColumnsButtons(false);
        ResetButton.interactable = false;

        for (int j = 0; j < 7; j++)
        {
            Transform columnObj = transform.GetChild(j);
            for (int i = 0; i < columnObj.childCount; i++)
            {
                columnObj.GetChild(i).GetComponent<Piece>().StartResetAnimation();
            }
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        StartNewGame();
    }
}