using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMaxNode : TreeNode
{
    public enum TYPE
    {
        MIN,
        MAX
    };

    public GameManager.CELL_DATA[,] GameTable = null;

    TYPE MoveType;

    int CurrentLevel = 0;

    int IndexI = 0;

    int IndexJ = 0;

    int MaxTreeLevel = 3;

    float MoveResult = 0.0f;

    bool IsGameTableFull(GameManager.CELL_DATA[,] currentTable)
    {
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (currentTable[i, j].Marking == GameManager.CELL_MARKING.NONE)
                    return false;
            }
        }

        return true;
    }

    public int GetSelectedColumn()
    {
        return IndexJ;
    }

    public void SetMaxTreeLevel(int level)
    {
        MaxTreeLevel = level;
    }

    public void StartMinMaxNode(GameManager.CELL_DATA[,] currentTable, int level = 0,
                                TYPE type = TYPE.MAX, int indexI = 0, int indexJ = 0,
                                GameManager.CELL_MARKING marking = GameManager.CELL_MARKING.PLAYER)
    {
        CurrentLevel = level;
        MoveType = type;

        // Copying table
        GameTable = new GameManager.CELL_DATA[6, 7];
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 7; j++)
                GameTable[i, j].Marking = currentTable[i, j].Marking;
        }


        if (CurrentLevel > 0)
        {
            IndexI = indexI;
            IndexJ = indexJ;
            GameTable[IndexI, IndexJ].Marking = marking;

            // Check if there are more children (only if game is not finished)
            GameManager.CELL_MARKING victory = GameManager.Instance.CheckVictory(GameTable);
            if (victory == GameManager.CELL_MARKING.AI)
            {
                MoveResult = 10.0f - (float) CurrentLevel;
                return;
            }
            else if (victory == GameManager.CELL_MARKING.PLAYER)
            {
                MoveResult = -1.0f * (10.0f - (float) CurrentLevel);
                return;
            }
            else if (IsGameTableFull(GameTable))
                return;
        }

        // Checking if it's possible to go down on tree
        int maxTreeLevel = GameManager.Instance.GetMinMaxTreeLevel();
        if (CurrentLevel + 1 <= maxTreeLevel)
        {
            GameManager.CELL_MARKING newMarking = (marking == GameManager.CELL_MARKING.PLAYER) ?
                                                   GameManager.CELL_MARKING.AI : GameManager.CELL_MARKING.PLAYER;

            TYPE newType = (MoveType == TYPE.MAX) ? TYPE.MIN : TYPE.MAX;

            for (int j = 0; j < 7; j++)
            {
                if (GameTable[0, j].Marking != GameManager.CELL_MARKING.NONE)
                    continue;

                int i = 1;
                while (GameTable[i, j].Marking == GameManager.CELL_MARKING.NONE)
                {
                    i++;
                    if (i == 6)
                        break;
                }

                i--;

                // Creating new min max move
                MinMaxNode node = new MinMaxNode();

                AddTreeChild(node);

                node.StartMinMaxNode(GameTable, CurrentLevel + 1, newType, i, j, newMarking);
            }
        }
    }

    static public MinMaxNode CalculateBestMove(GameManager.CELL_DATA[,] currentTable)
    {
        // Treating the special situations
        MinMaxNode aiFirstMoveNode = new MinMaxNode();
        aiFirstMoveNode.StartMinMaxNode(currentTable);

        MinMaxNode bestMove = aiFirstMoveNode.CalculateInternalBestMove();
        return bestMove;
    }

    MinMaxNode CalculateInternalBestMove()
    {
        if (GetTreeChildrenCount() == 0)
            return this;

        int ChoosedChild = 0;
        float resultValue = (MoveType == TYPE.MAX) ? -20.0f : 20.0f;
        int resultIIndex = 0;

        for (int i = 0; i < GetTreeChildrenCount(); i++)
        {
            MinMaxNode node = (MinMaxNode) GetTreeChild(i);

            if (node.MoveResult == 0.0f)
                node.CalculateInternalBestMove();

            if (MoveType == TYPE.MAX && node.MoveResult > resultValue)
            {
                ChoosedChild = i;
                resultValue = node.MoveResult;
                resultIIndex = node.IndexI;
            }
            else if (MoveType == TYPE.MIN && node.MoveResult < resultValue)
            {
                ChoosedChild = i;
                resultValue = node.MoveResult;
                resultIIndex = node.IndexI;
            }
            else if (node.MoveResult == resultValue && node.IndexI > resultIIndex)
            {
                ChoosedChild = i;
                resultValue = node.MoveResult;
                resultIIndex = node.IndexI;
            }
            else if (node.MoveResult == resultValue && Random.value > 0.5f)
            {
                ChoosedChild = i;
                resultValue = node.MoveResult;
                resultIIndex = node.IndexI;
            }
        }

        MoveResult = resultValue;
        return (MinMaxNode) GetTreeChild(ChoosedChild);
    }
}