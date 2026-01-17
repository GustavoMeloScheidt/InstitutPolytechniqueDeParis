using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1; //vai pra cima se for branca e pra baixo se for preta

        //One in front
        if (board[currentX, currentY + direction] == null)
            r.Add(new Vector2Int(currentX, currentY + direction));

        //Two in front
        if (board[currentX, currentY + direction] == null)
        {
            //White team
            if(team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            //Black team
            if(team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
        }

        //Diagonal (kill move)
        if (currentX != tileCountX-1) //ter certeza que nao estamos na borda do tabuleiro
            if(board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
                r.Add(new Vector2Int(currentX + 1, currentY + direction));

        if (currentX != 0) 
            if(board[currentX -1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
                r.Add(new Vector2Int(currentX - 1, currentY + direction));
        return r; 
    }
    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team ==0) ? 1 : -1;
        
        //Promotion
        if((team == 0) && currentY == 6 || (team == 1 && currentY == 1))
            return SpecialMove.Promotion;
        
        //En Passant
        if(moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if(board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn)
            {
                if(Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2) //usa o Mathf.Abs pra dar certo com as pretas tambem (que fica -2)
                {
                    if (board[lastMove[1].x, lastMove[1].y].team != team) //If the move was from the other team
                    {
                        if (lastMove[1].y == currentY) //If both pawns are on the same Y
                        {
                            if (lastMove[1].x == currentX - 1) //Landed Left
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                            if (lastMove[1].x == currentX + 1) //Landed right
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }

        return SpecialMove.None;
    }
}
