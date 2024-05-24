using System;

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
    public enum mazeTypes
    {
        GOAL,
        CENTER,
        TOTAL,
        CROSS,
        BORDER
    }

    public static char[][] chosenMazeFor5x5 = new char[][]{
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

    public string playfairWord { get; private set; }

    ///<value>The starting tile.</value>
    public static UncannyMazeTile start { get; private set; }
    ///<value>The current tile.</value>
    public static UncannyMazeTile current { get; set; }
    ///<value>The goal tile.</value>
    public static UncannyMazeTile goal { get; private set; }
    ///<value>The maze type associated with this tile.</value>
    public mazeTypes mazeType { get; }
    ///<value>The value of this tile, from 0 to 9.</value>
    public int uncannyValue { get; }
    ///<value>The x-coordinate of this tile in the maze.</value>
    public int x { get; }
    ///<value>The y-coordinate of this tile in the maze.</value>
    public int y { get; }
    ///<value>The x-coordinate of this tile in the maze as a letter.</value>
    public char letterCoord { get; }
    ///<value>The y-coordinate of this tile in the maze, plus 1 to match with letter-number coordinates.</value>
    public char numberCoord { get; }
    ///<value>The character associated with this tile for the 5×5 and 6×6 mazes.</value>
    public char? character { get; set; }
    ///<value>The character associated with this tile for the 5×5 maze submission process.</value>
    public char? submit5x5Character { get; set; }

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
        x = xCoord;
        y = yCoord;
        letterCoord = coordLetters[xCoord];
        numberCoord = (char)(yCoord + 49);
        if (value < 0 || value > 9)
        {
            throw new ArgumentOutOfRangeException("Tile should be from 0 to 9.");
        }
        uncannyValue = value;
        playfairWord = playfairWords[value];
        mazeType = (mazeTypes)((int)(value / 2));
        character = null;
        submit5x5Character = null;
    }

    public override string ToString()
    {
        return "" + letterCoord + numberCoord + ": " + uncannyValue;
    }

    public static void setStartAndGoal(UncannyMazeTile startPosition, UncannyMazeTile goalPosition)
    {
        start = startPosition;
        goal = goalPosition;
    }
    public static bool operator ==(UncannyMazeTile first, UncannyMazeTile second)
    {
        if (Object.ReferenceEquals(first, null) && Object.ReferenceEquals(second, null))
        {
            return true;
        }
        if (Object.ReferenceEquals(first, null) || Object.ReferenceEquals(second, null))
        {
            return false;
        }
        return first.x == second.x && first.y == second.y;
    }
    public static bool operator !=(UncannyMazeTile first, UncannyMazeTile second)
    {
        if (Object.ReferenceEquals(first, null) && Object.ReferenceEquals(second, null))
        {
            return false;
        }
        if (Object.ReferenceEquals(first, null) || Object.ReferenceEquals(second, null))
        {
            return true;
        }
        return first.x != second.x || first.y != second.y;
    }
}
