using System;
using System.Collections.Generic;
using System.Linq;

///<summary>Tiles for Uncanny Maze.</summary>
public class UncannyMazeTile
{
    ///<value>The letters of each row.</value>
    private static readonly string coordLetters = "ABCDEF";
    ///<value>The orders to submit letters for the 5×5 maze submission process.</value>
    public static readonly char[][][] submit5x5Mazes = new char[][][]{
        new char[][]{
            new char[]{'E', 'D', 'C', 'B', 'A'},
            new char[]{'J', 'I', 'H', 'G', 'F'},
            new char[]{'O', 'N', 'M', 'L', 'K'},
            new char[]{'T', 'S', 'R', 'Q', 'P'},
            new char[]{'Y', 'X', 'W', 'V', 'U'}
        },
        new char[][]{
            new char[]{'A', 'B', 'C', 'D', 'E'},
            new char[]{'F', 'G', 'H', 'I', 'J'},
            new char[]{'K', 'L', 'M', 'N', 'O'},
            new char[]{'P', 'Q', 'R', 'S', 'T'},
            new char[]{'U', 'V', 'W', 'X', 'Y'}
        },

        new char[][]{
            new char[]{'U', 'V', 'W', 'X', 'Y'},
            new char[]{'P', 'Q', 'R', 'S', 'T'},
            new char[]{'K', 'L', 'M', 'N', 'O'},
            new char[]{'F', 'G', 'H', 'I', 'J'},
            new char[]{'A', 'B', 'C', 'D', 'E'}
        },

        new char[][]{
            new char[]{'Y', 'X', 'W', 'V', 'U'},
            new char[]{'T', 'S', 'R', 'Q', 'P'},
            new char[]{'O', 'N', 'M', 'L', 'K'},
            new char[]{'J', 'I', 'H', 'G', 'F'},
            new char[]{'E', 'D', 'C', 'B', 'A'}
        },

        new char[][]{
            new char[]{'A', 'F', 'K', 'P', 'U'},
            new char[]{'B', 'G', 'L', 'Q', 'V'},
            new char[]{'C', 'H', 'M', 'R', 'W'},
            new char[]{'D', 'I', 'N', 'S', 'X'},
            new char[]{'E', 'J', 'O', 'T', 'Y'}
        },

        new char[][]{
            new char[]{'U', 'P', 'K', 'F', 'A'},
            new char[]{'V', 'Q', 'L', 'G', 'B'},
            new char[]{'W', 'R', 'M', 'H', 'C'},
            new char[]{'X', 'S', 'N', 'I', 'D'},
            new char[]{'Y', 'T', 'O', 'J', 'E'}
        },

        new char[][]{
            new char[]{'E', 'J', 'O', 'T', 'Y'},
            new char[]{'D', 'I', 'N', 'S', 'X'},
            new char[]{'C', 'H', 'M', 'R', 'W'},
            new char[]{'B', 'G', 'L', 'Q', 'V'},
            new char[]{'A', 'F', 'K', 'P', 'U'}
        },

        new char[][]{
            new char[]{'Y', 'T', 'O', 'J', 'E'},
            new char[]{'X', 'S', 'N', 'I', 'D'},
            new char[]{'W', 'R', 'M', 'H', 'C'},
            new char[]{'V', 'Q', 'L', 'G', 'B'},
            new char[]{'U', 'P', 'K', 'F', 'A'}
        },

        new char[][]{
            new char[]{'A', 'B', 'C', 'D', 'E'},
            new char[]{'J', 'I', 'H', 'G', 'F'},
            new char[]{'K', 'L', 'M', 'N', 'O'},
            new char[]{'T', 'S', 'R', 'Q', 'P'},
            new char[]{'U', 'V', 'W', 'X', 'Y'}
        },

        new char[][]{
            new char[]{'E', 'D', 'C', 'B', 'A'},
            new char[]{'F', 'G', 'H', 'I', 'J'},
            new char[]{'O', 'N', 'M', 'L', 'K'},
            new char[]{'P', 'Q', 'R', 'S', 'T'},
            new char[]{'Y', 'X', 'W', 'V', 'U'}
        }
    };
    ///<value>The different types of mazes.</value>
    public enum MazeTypes
    {
        GOAL,
        CENTER,
        TOTAL,
        CORNERS,
        BORDER
    }

    public char[][] chosenMazeFor5x5 = new char[][]{
        new char[]{'A', 'B', 'C', 'D', 'E'},
        new char[]{'F', 'G', 'H', 'I', 'J'},
        new char[]{'K', 'L', 'M', 'N', 'O'},
        new char[]{'P', 'Q', 'R', 'S', 'T'},
        new char[]{'U', 'V', 'W', 'X', 'Y'}
    };

    public static string[] playfairWords = new string[]{
        "SUPRGMNG",
        "UNCANXNY",
        "TRACTORX",
        "INCREDBL",
        "ONLYOHIO",
        "SKIBDITL",
        "JMBOJOSH",
        "WHISTLNG",
        "FORGORXD",
        "TROLFACE"
    };

    public string[] ValidDirections { get; set; }
    public string PlayfairWord { get; private set; }
    ///<value>The maze type associated with this tile.</value>
    public MazeTypes MazeType { get; private set; }
    ///<value>The value of this tile, from 0 to 9.</value>
    public int UncannyValue { get; private set; }
    ///<value>The x-coordinate of this tile in the maze.</value>
    public int Xcoordinate { get; private set; }
    ///<value>The y-coordinate of this tile in the maze.</value>
    public int Ycoordinate { get; private set; }
    ///<value>The x-coordinate of this tile in the maze as a letter.</value>
    public char LetterCoord { get; private set; }
    ///<value>The y-coordinate of this tile in the maze, plus 1 to match with letter-number coordinates.</value>
    public char NumberCoord { get; private set; }
    ///<value>The character associated with this tile for the 5×5 and 6×6 mazes.</value>
    public char? Character { get; set; }
    ///<value>The character associated with this tile for the 5×5 maze submission process.</value>
    public char? Submit5x5Character { get; set; }

    ///<summary>Creates a new tile.</summary>
    ///<param name="xCoord">The x-coordinate.</param>
    ///<param name="yCoord">The y-coordinate.</param>
    ///<param name="value">The value of this tile, from 0 to 9.</param>
    ///<param name="dimensions">The dimensions of the maze, used to validate the numbers.</param>
    ///<exception cref="ArgumentOutOfRangeException">Thrown if any value is of an invalid range.</exception>
    ///<returns>A new tile.</returns>
    public UncannyMazeTile(int xCoord, int yCoord, int value, int dimensions)
    {
        if (xCoord < 0 || xCoord > dimensions - 1)
        {
            throw new ArgumentOutOfRangeException("x-coordinate should be from 0 to " + (dimensions - 1) + ".");
        }
        if (yCoord < 0 || yCoord > dimensions - 1)
        {
            throw new ArgumentOutOfRangeException("y-coordinate should be from 0 to " + (dimensions - 1) + ".");
        }
        Xcoordinate = xCoord;
        Ycoordinate = yCoord;
        LetterCoord = coordLetters[xCoord];
        NumberCoord = (char)(yCoord + 49);
        if (value < 0 || value > 9)
        {
            throw new ArgumentOutOfRangeException("Tile should be from 0 to 9.");
        }
        UncannyValue = value;
        PlayfairWord = playfairWords[value];
        MazeType = (MazeTypes)(value / 2);
        Character = null;
        Submit5x5Character = null;
        ValidDirections = new string[] { };
    }

    public override string ToString()
    {
        return "" + LetterCoord + NumberCoord + ": " + UncannyValue;
    }

    public override bool Equals(object obj)
    {
        UncannyMazeTile tile = obj as UncannyMazeTile;
        return !ReferenceEquals(tile, null) &&
               EqualityComparer<char[][]>.Default.Equals(chosenMazeFor5x5, tile.chosenMazeFor5x5) &&
               PlayfairWord == tile.PlayfairWord &&
               MazeType == tile.MazeType &&
               UncannyValue == tile.UncannyValue &&
               Xcoordinate == tile.Xcoordinate &&
               Ycoordinate == tile.Ycoordinate &&
               LetterCoord == tile.LetterCoord &&
               NumberCoord == tile.NumberCoord &&
               Character == tile.Character &&
               Submit5x5Character == tile.Submit5x5Character &&
               ValidDirections.SequenceEqual(tile.ValidDirections);
    }

    public override int GetHashCode()
    {
        int hashCode = -1359825686;
        hashCode = (hashCode * -1521134295) + EqualityComparer<char[][]>.Default.GetHashCode(chosenMazeFor5x5);
        hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(PlayfairWord);
        hashCode = (hashCode * -1521134295) + MazeType.GetHashCode();
        hashCode = (hashCode * -1521134295) + UncannyValue.GetHashCode();
        hashCode = (hashCode * -1521134295) + Xcoordinate.GetHashCode();
        hashCode = (hashCode * -1521134295) + Ycoordinate.GetHashCode();
        hashCode = (hashCode * -1521134295) + LetterCoord.GetHashCode();
        hashCode = (hashCode * -1521134295) + NumberCoord.GetHashCode();
        hashCode = (hashCode * -1521134295) + Character.GetHashCode();
        hashCode = (hashCode * -1521134295) + Submit5x5Character.GetHashCode();
        hashCode = (hashCode * -1521134295) + ValidDirections.GetHashCode();
        return hashCode;
    }

    // i just let my LSP simplify these conditional statements to make it not bug me about them
    // basically, if either is null, they're not equal
    // if neither is null, compare their x and y coords
    // same x and y coords? same tile
    public static bool operator ==(UncannyMazeTile first, UncannyMazeTile second)
    {
        return (ReferenceEquals(first, null) && ReferenceEquals(second, null))
|| (!ReferenceEquals(first, null) && !ReferenceEquals(second, null) && first.Xcoordinate == second.Xcoordinate && first.Ycoordinate == second.Ycoordinate);
    }
    public static bool operator !=(UncannyMazeTile first, UncannyMazeTile second)
    {
        return (!ReferenceEquals(first, null) || !ReferenceEquals(second, null))
&& (ReferenceEquals(first, null) || ReferenceEquals(second, null) || first.Xcoordinate != second.Xcoordinate || first.Ycoordinate != second.Ycoordinate);
    }
}
