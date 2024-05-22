using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wawa.Modules;
using Wawa.Extensions;
using Wawa.IO;

public class UncannyMaze : ModdedModule
{
    private readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public KMSelectable arrowleft, arrowright, arrowup, arrowdown, maze, numbersButton, resetButton, appendButton;
    internal bool viewingWholeMaze = false;
    internal int animSpeed = 30;
    private int leftSum, rightSum, aboveSum, belowSum;
    private int topLeft, topRight, bottomLeft, bottomRight;
    private List<UncannyMazeTile> possibleDirections;
    List<int> borderSums = new List<int>();
    private float m, b;
    private bool mazeGenerated = false;
    private bool currentlyMoving = false;
    private bool tookTooLong = false;
    private bool generatingMazeIdleCurrentlyRunning = false;
    private bool music = false;
    private Vector2 currentPosition, startingPosition;
    private Vector2[] cornerCombinations;
    private Dictionary<string, UncannyMazeTile> directions;
    private UncannyMazeTile leftTile, rightTile, aboveTile, belowTile;
    private int xStart, yStart, xCoords, yCoords, xGoal, yGoal;
    public GameObject numbers, gm, currentBox, goalBox, anchor, coordsText;
    private int dims;
    private int totalMazeTotal;
    private string output;
    private UncannyMazeTile[,] map;
    internal List<string> correctPath;
    private List<UncannyMazeTile> sequence = new List<UncannyMazeTile>();
    public TextureGeneratorUncanny t;
    List<string> canGo = new List<string>();

    void Start()
    {
        StartCoroutine(Initialization());
        arrowleft.Set(onInteract: () =>
        {
            if (mazeGenerated && !Status.IsSolved)
                StartCoroutine(Moving("left", animSpeed));
            Shake(arrowleft, 1, Sound.BigButtonPress);
        });
        arrowright.Set(onInteract: () =>
        {
            if (mazeGenerated && !Status.IsSolved)
                StartCoroutine(Moving("right", animSpeed));
            Shake(arrowright, 1, Sound.BigButtonPress);
        });
        arrowup.Set(onInteract: () =>
        {
            if (mazeGenerated && !Status.IsSolved)
                StartCoroutine(Moving("up", animSpeed));
            Shake(arrowup, 1, Sound.BigButtonPress);
        });
        arrowdown.Set(onInteract: () =>
        {
            if (mazeGenerated && !Status.IsSolved)
                StartCoroutine(Moving("down", animSpeed));
            Shake(arrowdown, 1, Sound.BigButtonPress);
        });
        maze.Set(onInteract: () =>
        {
            if (mazeGenerated && !currentlyMoving && !Status.IsSolved)
            {
                if (viewingWholeMaze)
                {
                    if (numbers.activeInHierarchy)
                    {
                        numbers.SetActive(false);
                        t.changeTexture(t.finalTexture);
                    }
                    currentBox.SetActive(false);
                    goalBox.SetActive(false);
                    coordsText.SetActive(false);
                    maze.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1f / dims, 1f / dims);
                    maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
                }
                else
                {
                    currentBox.SetActive(true);
                    goalBox.SetActive(true);
                    coordsText.SetActive(true);
                    maze.GetComponent<MeshRenderer>().material.mainTextureScale = Vector2.one;
                    maze.GetComponent<MeshRenderer>().material.mainTextureOffset = Vector2.zero;
                }
                viewingWholeMaze = !viewingWholeMaze;
            }
            Shake(maze, .75f, Sound.BigButtonPress);
        });
        numbersButton.Set(onInteract: () =>
        {
            if (mazeGenerated && viewingWholeMaze && !Status.IsSolved)
            {
                if (!numbers.activeInHierarchy)
                {
                    numbers.SetActive(true);
                    t.changeTexture(t.whiteBG);
                    currentBox.SetActive(false);
                    goalBox.SetActive(false);
                }
                else
                {
                    numbers.SetActive(false);
                    t.changeTexture(t.finalTexture);
                    currentBox.SetActive(true);
                    goalBox.SetActive(true);
                }
            }
            Shake(numbersButton, .75f, Sound.BigButtonPress);
        });
    }

    private IEnumerator Initialization()
    {
        t.changeTexture(t.whiteBG);
        output = "";
        dims = t.gridDimensions;
        map = t.layout;
        for (int r = 0; r < dims; r++)
        {
            for (int c = 0; c < dims; c++)
            {
                output += "" + map[r, c].uncannyValue;
                totalMazeTotal += map[r, c].uncannyValue;
                if (c != dims - 1)
                {
                    output += " ";
                }
            }
            if (r != dims - 1)
            {
                output += "\n";
            }
        }
        totalMazeTotal %= 10;
        numbers.GetComponent<TextMesh>().text = output;
        string[] outputLines = output.Split('\n');
        Log("Your maze layout is:");
        foreach (string line in outputLines)
        {
            Log(line);
        }
        currentPosition = new Vector2((float)UnityEngine.Random.Range(0, dims) / dims, (float)UnityEngine.Random.Range(0, dims) / dims);
        startingPosition = currentPosition;
        maze.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1f / dims, 1f / dims);
        maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
        if (tookTooLong)
            yield break;
        if (!generatingMazeIdleCurrentlyRunning)
            StartCoroutine(generatingMazeIdle());
        xStart = (int)(startingPosition.x * dims + .01f);
        yStart = (int)(startingPosition.y * dims + .01f);
        xCoords = (int)(currentPosition.x * dims + .01f);
        yCoords = (int)(currentPosition.y * dims + .01f);
        do
        {
            xGoal = UnityEngine.Random.Range(0, dims);
            yGoal = UnityEngine.Random.Range(0, dims);
        } while (map[(dims - yCoords - 1), xCoords] == map[(dims - yGoal - 1), xGoal]);
        int movements = 0;
        int attempts = 0;
        int index;
        UncannyMazeTile.setStartAndGoal(map[(dims - yStart - 1), xStart], map[(dims - yGoal - 1), xGoal]);
        UncannyMazeTile.current = map[(dims - yCoords - 1), xCoords];
        correctPath = new List<string>();
        Setup();
        while (UncannyMazeTile.current != UncannyMazeTile.goal && attempts < 3)
        {
            if (canGo.Count == 0)
                break;
            index = UnityEngine.Random.Range(0, canGo.Count);
            correctPath.Add(canGo.ElementAt(index));
            try
            {
                yield return StartCoroutine(Moving(canGo.ElementAt(index), 2, false));
            }
            catch (IndexOutOfRangeException e)
            {
                string[] exceptionLines = e.ToString().Split('\n');
                Log("Ran into an IndexOutOfRangeException. Regenerating… The following is the content of the exception:", LogType.Exception);
                foreach (string line in exceptionLines)
                    Log(line, LogType.Exception);
                t.Awake();
                StartCoroutine(Initialization());
                yield break;
            }
            finally { }//needed for the yield return StartCoroutine in the try block to function properly
            movements++;
            if (movements >= 20)
            {
                yield return StartCoroutine(Moving("reset", 2, false));
                movements = 0;
                correctPath.Clear();
                attempts++;
            }
        }
        if (attempts != 3 && canGo.Count != 0)
        {
            Setup();
            mazeGenerated = true;
            gm.SetActive(false);
            t.changeTexture(t.finalTexture);
            maze.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1f / dims, 1f / dims);
            maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
            yield break;
        }
        else
        {
            t.Awake();
            StartCoroutine(Initialization());
            yield break;
        }
    }

    private void Setup(bool logging = false)
    {
        UncannyMazeTile.setStartAndGoal(map[(dims - yStart - 1), xStart], map[(dims - yGoal - 1), xGoal]);
        UncannyMazeTile.current = map[(dims - yCoords - 1), xCoords];
        UncannyMazeTile.chosenMazeFor5x5 = UncannyMazeTile.submit5x5Mazes[UncannyMazeTile.start.uncannyValue];
        currentBox.transform.localScale = new Vector3(7f / dims, 1, 7f / dims);
        if (dims == 5)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    map[i, j].submit5x5Character = UncannyMazeTile.chosenMazeFor5x5[i][j];
                }
            }
            int currentIndex = 0;
            UncannyMazeTile[] tilesWithValue;
            for (int i = 0; i < 10; i++)
            {
                tilesWithValue = (from UncannyMazeTile u in map where u.uncannyValue == i select u).ToArray();
                for (int j = 0; j < t.amountOfEachNumber[i]; j++)
                {
                    tilesWithValue[j].character = alphabet[currentIndex];
                    currentIndex++;
                }
            }
            string outputPlayfair = "";
            for (int r = 0; r < dims; r++)
            {
                for (int c = 0; c < dims; c++)
                {
                    outputPlayfair += "" + map[r, c].character;
                    if (c != dims - 1)
                    {
                        outputPlayfair += " ";
                    }
                }
                if (r != dims - 1)
                {
                    outputPlayfair += "\n";
                }
            }
            string[] outputPlayfairLines = outputPlayfair.Split('\n');
            if (logging)
            {
                Log("Your playfair cipher key is:");
                foreach (string line in outputPlayfairLines)
                {
                    Log(line);
                }
            }
        }
        else if (dims == 6)
        {
            int currentIndex = 0;
            UncannyMazeTile[] tilesWithValue;
            int currentNumberIndex = 0;
            for (int i = 0; i < 10; i++)
            {
                tilesWithValue = (from UncannyMazeTile u in map where u.uncannyValue == i select u).ToArray();
                if (tilesWithValue.Length == 0)
                {
                    currentNumberIndex++;
                    continue;
                }
                tilesWithValue[0].character = (char)(i + 48);
                for (int j = 1; j < tilesWithValue.Length; j++)
                {
                    tilesWithValue[j].character = alphabet[(currentIndex % 26)];
                    currentIndex++;
                }
                currentNumberIndex++;
            }
            string outputBase36 = "";
            for (int r = 0; r < dims; r++)
            {
                for (int c = 0; c < dims; c++)
                {
                    outputBase36 += "" + map[r, c].character;
                    if (c != dims - 1)
                    {
                        outputBase36 += " ";
                    }
                }
                if (r != dims - 1)
                {
                    outputBase36 += "\n";
                }
            }
            string[] outputBase36Lines = outputBase36.Split('\n');
            if (logging)
            {
                Log("Your base 36 key is:");
                foreach (string line in outputBase36Lines)
                {
                    Log(line);
                }
            }
        }
        if (logging)
        {
            Log("Your starting tile is: " + UncannyMazeTile.start + ".");
            Log("Your current tile is: " + UncannyMazeTile.current + ".");
            Log("Your goal tile is: " + UncannyMazeTile.goal + ".");
        }
        switch (dims)
        {

            case 4:
                numbers.GetComponent<TextMesh>().fontSize = 35;
                m = .25f;
                b = 1.1226f;
                anchor.transform.localPosition = new Vector3(.75f, .75f, 0);
                break;
            case 5:
                numbers.GetComponent<TextMesh>().fontSize = 30;
                m = .2f;
                b = 1.1f;
                anchor.transform.localPosition = new Vector3(.7f, .7f, 0);
                break;
            case 6:
                numbers.GetComponent<TextMesh>().fontSize = 25;
                m = .167429f;
                b = 1.08143f;
                anchor.transform.localPosition = new Vector3(2 / 3f, 2 / 3f, 0);
                break;
        }
        goalBox.transform.localScale = new Vector3(7f / dims, 1, 7f / dims);
        goalBox.transform.localPosition = new Vector3(m * xGoal - b, -.01f, -m * yGoal + b);
        mazeGenerated = true;
        if (logging)
            Log("Your sum of maze modulo 10 is: " + totalMazeTotal);
        for (int i = 0; i < dims; i++)
        {
            leftSum += map[i, 0].uncannyValue;
            rightSum += map[i, dims - 1].uncannyValue;
            aboveSum += map[0, i].uncannyValue;
            belowSum += map[dims - 1, i].uncannyValue;
        }
        if (logging)
        {
            Log("The sum of the left border is: " + leftSum);
            Log("The sum of the right border is: " + rightSum);
            Log("The sum of the top border is: " + aboveSum);
            Log("The sum of the bottom border is: " + belowSum);
        }
        borderSums.Add(leftSum);
        borderSums.Add(rightSum);
        borderSums.Add(aboveSum);
        borderSums.Add(belowSum);
        topLeft = map[0, 0].uncannyValue % dims;
        topRight = map[0, dims - 1].uncannyValue % dims;
        bottomLeft = map[dims - 1, 0].uncannyValue % dims;
        bottomRight = map[dims - 1, dims - 1].uncannyValue % dims;
        cornerCombinations = new Vector2[]{
            new Vector2(topLeft, topRight),
            new Vector2(topLeft, bottomLeft),
            new Vector2(topLeft, bottomRight),
            new Vector2(topRight, bottomLeft),
            new Vector2(topRight, bottomRight),
            new Vector2(bottomLeft, bottomRight),
            new Vector2(topRight, topLeft),
            new Vector2(bottomLeft, topLeft),
            new Vector2(bottomRight, topLeft),
            new Vector2(bottomLeft, topRight),
            new Vector2(bottomRight, topRight),
            new Vector2(bottomRight, bottomLeft)
        };
        directions = new Dictionary<string, UncannyMazeTile>();
        directions.Add("left", null);
        directions.Add("right", null);
        directions.Add("up", null);
        directions.Add("down", null);
        StartCoroutine(Moving("reset", 2, logging));
    }

    private float f(float x)
    {
        return (x * -2 / dims) - (-2 / dims);
    }

    private IEnumerator Moving(string direction, int n, bool logging = true)
    {
        if (viewingWholeMaze || currentlyMoving
        || (xCoords == 0 && direction == "left")
        || (xCoords == dims - 1 && direction == "right")
        || (yCoords == dims - 1 && direction == "up")
        || (yCoords == 0 && direction == "down"))
            yield break;
        if (direction != "reset" && !possibleDirections.Contains(directions[direction]) && logging)
        {
            Strike("Tried to move " + direction + ", not allowed.");
            yield break;
        }
        switch (direction)
        {
            case "up":
                if (currentPosition.y + .01f >= ((dims - 1f) / dims))
                    yield break;
                else
                {
                    currentlyMoving = true;
                    currentPosition += new Vector2(0, 1f / (n * dims));//Difference between the integral of movement and the Riemann sum
                    for (int i = n - 1; i > 0; i--)
                    {
                        currentPosition -= new Vector2(0, f(i * 1f / n) * 1f / n);//Riemann sum
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
                        yield return null;//necessary for the animation to play
                    }
                    currentlyMoving = false;
                }
                break;
            case "down":
                if (currentPosition.y <= .01f)
                    yield break;
                else
                {
                    currentlyMoving = true;
                    currentPosition -= new Vector2(0, 1f / (n * dims));
                    for (int i = n - 1; i > 0; i--)
                    {
                        currentPosition += new Vector2(0, f(i * 1f / n) * 1f / n);
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
                        yield return null;
                    }
                    currentlyMoving = false;
                }
                break;
            case "left":
                if (currentPosition.x <= .01f)
                    yield break;
                else
                {
                    currentlyMoving = true;
                    currentPosition -= new Vector2(1f / (n * dims), 0);
                    for (int i = n - 1; i > 0; i--)
                    {
                        currentPosition += new Vector2(f(i * 1f / n) * 1f / n, 0);
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
                        yield return null;
                    }
                    currentlyMoving = false;
                }
                break;
            case "right":
                if (currentPosition.x + .01f >= ((dims - 1f) / dims))
                    yield break;
                else
                {
                    currentlyMoving = true;
                    currentPosition += new Vector2(1f / (n * dims), 0);
                    for (int i = n - 1; i > 0; i--)
                    {
                        currentPosition -= new Vector2(f(i * 1f / n) * 1f / n, 0);
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
                        yield return null;
                    }
                    currentlyMoving = false;
                }
                break;
            case "reset":
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset = startingPosition;
                break;

        }
        currentPosition = maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
        xCoords = (int)(currentPosition.x * dims + .01f);
        yCoords = (int)(currentPosition.y * dims + .01f);
        UncannyMazeTile.current = map[(dims - yCoords - 1), xCoords];
        currentBox.transform.localPosition = new Vector3(m * xCoords - b, -.01f, -m * yCoords + b);
        numbers.GetComponent<TextMesh>().text = output;
        if ((dims - yCoords - 1) * 2 * dims + (xCoords * 2) + 1 > (dims - yGoal - 1) * 2 * dims + (xGoal * 2) + 1)
        {
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert((dims - yCoords - 1) * 2 * dims + (xCoords * 2) + 1, "</color>");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert((dims - yCoords - 1) * 2 * dims + (xCoords * 2), "<color=\"blue\">");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert((dims - yGoal - 1) * 2 * dims + (xGoal * 2) + 1, "</color>");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert((dims - yGoal - 1) * 2 * dims + (xGoal * 2), "<color=\"red\">");
        }
        else
        {
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert((dims - yGoal - 1) * 2 * dims + (xGoal * 2) + 1, "</color>");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert((dims - yGoal - 1) * 2 * dims + (xGoal * 2), "<color=\"red\">");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert((dims - yCoords - 1) * 2 * dims + (xCoords * 2) + 1, "</color>");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert((dims - yCoords - 1) * 2 * dims + (xCoords * 2), "<color=\"blue\">");
        }

        if (logging)
            Log("---");//makes the log a bit easier to read
        if (direction == "reset")
        {
            sequence.Clear();
        }
        else
        {
            if (logging)
                Log("Pressed " + direction + ", going to " + UncannyMazeTile.current.letterCoord + UncannyMazeTile.current.numberCoord + ".");
        }
        coordsText.GetComponent<TextMesh>().text = "CURRENT: " + UncannyMazeTile.current.letterCoord + UncannyMazeTile.current.numberCoord + "\nGOAL: " + UncannyMazeTile.goal.letterCoord + UncannyMazeTile.goal.numberCoord;
        if (UncannyMazeTile.current == UncannyMazeTile.goal && logging)
        {
            Solve("Your module is: solved module");
            yield break;
        }
        if (logging)
        {
            Log("Your current tile is: " + map[(dims - yCoords - 1), xCoords]);
        }
        if (xCoords != 0)
        {
            leftTile = map[dims - yCoords - 1, xCoords - 1];
            if (logging)
                Log("Your left tile is: " + leftTile);
        }
        else leftTile = null;
        directions["left"] = leftTile;

        if (xCoords != dims - 1)
        {
            rightTile = map[dims - yCoords - 1, xCoords + 1];
            if (logging)
                Log("Your right tile is: " + rightTile);
        }
        else rightTile = null;
        directions["right"] = rightTile;

        if (yCoords != dims - 1)
        {
            aboveTile = map[dims - yCoords - 2, xCoords];
            if (logging)
                Log("Your above tile is: " + aboveTile);
        }
        else aboveTile = null;
        directions["up"] = aboveTile;

        if (yCoords != 0)
        {
            belowTile = map[dims - yCoords, xCoords];
            if (logging)
                Log("Your below tile is: " + belowTile);
        }
        else belowTile = null;
        directions["down"] = belowTile;

        switch (UncannyMazeTile.current.mazeType)
        {
            case UncannyMazeTile.mazeTypes.GOAL:
                Log("Your maze is: Goal Maze");
                possibleDirections = ClosestInValue(UncannyMazeTile.goal.uncannyValue, false, leftTile, rightTile, aboveTile, belowTile);
                break;
            case UncannyMazeTile.mazeTypes.TOTAL:
                Log("Your maze is: Total Maze");
                possibleDirections = ClosestInValue(totalMazeTotal, false, leftTile, rightTile, aboveTile, belowTile);
                break;
            case UncannyMazeTile.mazeTypes.CROSS:
                Log("Your maze is: Cross Maze");
                possibleDirections = CrossMaze(UncannyMazeTile.current.y, UncannyMazeTile.current.x, leftTile, rightTile, aboveTile, belowTile);
                break;
            case UncannyMazeTile.mazeTypes.BORDER:
                Log("Your maze is: Border Maze");
                possibleDirections = ClosestInValue(0, true, leftTile, rightTile, aboveTile, belowTile);
                break;
            case UncannyMazeTile.mazeTypes.CORNERS:
                Log("Your maze is: Corners Maze");
                possibleDirections = CornersMaze();
                break;
        }
        foreach (UncannyMazeTile tile in possibleDirections)
        {
            canGo.Add(directions.FirstOrDefault(x => x.Value == tile).Key);
        }
        canGo = canGo.Distinct().ToList();
        Log("Possible directions are: " + string.Join(", ", canGo.ToArray()));
    }

    private List<UncannyMazeTile> ClosestInValue(int compare, bool border, params UncannyMazeTile[] adjacent)
    {
        List<UncannyMazeTile> result = new List<UncannyMazeTile>();
        if (border)
        {
            for (int i = 0; i < 4; i++)
            {
                if (borderSums[i] == borderSums.Max())
                {
                    result.Add(adjacent[i] ?? adjacent[i % 2 == 0 ? i + 1 : i - 1]);
                }
            }
        }
        else
        {
            List<int> differences = new List<int>();
            foreach (UncannyMazeTile tile in adjacent)
            {
                differences.Add(tile == null ? -1 : Math.Abs(tile.uncannyValue - compare));
            }
            for (int i = 0; i < differences.Count; i++)
            {
                if (differences[i] != -1 && differences[i] == differences.Min())
                {
                    result.Add(adjacent[i]);
                }
            }
            Log("DEBUG: differences is " + string.Join(", ", differences.Select(d => d.ToString()).ToArray()) + ".");
        }
        return result;
    }

    private List<UncannyMazeTile> CrossMaze(int column, int row, params UncannyMazeTile[] adjacent)
    {
        int columnSum = (from UncannyMazeTile u in map where u.y == column select u.uncannyValue).Sum();
        int rowSum = (from UncannyMazeTile u in map where u.x == row select u.uncannyValue).Sum();
        int total = (columnSum * rowSum) % 10;
        return ClosestInValue(total, false, adjacent);
    }

    private List<UncannyMazeTile> CornersMaze()
    {
        List<UncannyMazeTile> allCombinations = new List<UncannyMazeTile>();
        foreach (Vector2 combination in cornerCombinations)
        {
            allCombinations.AddRange((from UncannyMazeTile u in map where u.y == combination.y && u.x == combination.x select u).ToList());
        }
        foreach (UncannyMazeTile tile in allCombinations)
        {
            Log("DEBUG: allCombinations has " + tile + ".");
        }
        return allCombinations.Distinct().ToList();
    }

    private IEnumerator generatingMazeIdle()
    {
        generatingMazeIdleCurrentlyRunning = true;
        gm.GetComponent<TextMesh>().fontSize = 45;
        string gen = "GENERATING\nMAZE.";
        int totaltime = 0;
        while (!mazeGenerated)
        {
            gm.GetComponent<TextMesh>().text = gen;
            yield return new WaitForSeconds(.75f);
            if (totaltime == 4 && !mazeGenerated)
            {
                gm.GetComponent<TextMesh>().fontSize = 27;
                gm.GetComponent<TextMesh>().text = "SORRY THE MODULE\nTOOK SO LONG TO\nLOAD. PRESS EITHER\nOF THE TWO RED\nBUTTONS BELOW TO\nSOLVE IMMEDIATELY.";
                tookTooLong = true;
                music = false;
                yield break;
            }
            gm.GetComponent<TextMesh>().text = gen + ".";
            yield return new WaitForSeconds(.75f);
            gm.GetComponent<TextMesh>().text = gen + "..";
            yield return new WaitForSeconds(.75f);
            totaltime++;
        }
        generatingMazeIdleCurrentlyRunning = false;
    }
}
