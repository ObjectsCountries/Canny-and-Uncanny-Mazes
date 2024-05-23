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
    public Texture2D[] blurred;
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
    private int blur = 2;
    private bool logging = false;
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
    private List<UncannyMazeTile> mustAppend = new List<UncannyMazeTile>();
    private List<UncannyMazeTile> sequence = new List<UncannyMazeTile>();
    private Config<UncannyMazeSettings> umSettings;
    private string sequenceCharacters = "";
    public TextureGeneratorUncanny t;
    List<string> canGo = new List<string>();

    [Serializable]
    public sealed class UncannyMazeSettings
    {
        public int animationSpeed = 30;
        public bool playMusicOnSolve = true;
        public int blurThreshold = 2;
    }
    public static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]{
        new Dictionary<string,object>{
            {"Filename","uncannymaze-settings.json"},
            {"Name","Uncanny Maze"},
            {"Listings",new List<Dictionary<string,object>>{
                new Dictionary<string,object>{
                    {"Key","animationSpeed"},
                    {"Text","Animation Speed"},
                    {"Description","Set the speed of the module's moving animation in frames.\nShould be from 10 to 60. Set to 2 to forgo moving animation."}
                },
                new Dictionary<string, object>{
                    {"Key","playMusicOnSolve"},
                    {"Text","Play Music On Solve"},
                    {"Description","If streaming, disable this to avoid copyright claims."}
                },
                new Dictionary<string,object>{
                    {"Key","blurThreshold"},
                    {"Text","Blur Threshold"},
                    {"Description","Blur all images from this number onward. Numbers outside 0 to 9 will leave all images unblurred."}
                }
            }}
        }
    };
    void Start()
    {
        umSettings = new Config<UncannyMazeSettings>();
        animSpeed = umSettings.Read().animationSpeed;
        if (umSettings.Read().animationSpeed != 2)
            animSpeed = Mathf.Clamp(umSettings.Read().animationSpeed, 10, 60);
        music = umSettings.Read().playMusicOnSolve;
        blur = umSettings.Read().blurThreshold;
        if (blur >= 0 && blur <= 9)
        {
            for (int i = blur; i < 10; i++)
            {
                t.textures[i] = blurred[i];
            }
        }
        umSettings.Write("{\"animationSpeed\":" + animSpeed + ",\"playMusicOnSolve\":" + music.ToString().ToLowerInvariant() + ",\"blurThreshold\":" + blur + "}");
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
            if (tookTooLong)
                Solve("Solved by pressing the Numbers button after generation took too long.");
            else if (mazeGenerated && viewingWholeMaze && !Status.IsSolved)
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
        resetButton.Set(onInteract: () =>
        {
            if (tookTooLong)
                Solve("Solved by pressing the Reset button after generation took too long.");
            else if (mazeGenerated && !Status.IsSolved && !viewingWholeMaze)
            {
                Log("Reset the maze.");
                StartCoroutine(Moving("reset", 2));
            }
            Shake(resetButton, .75f, Sound.BigButtonPress);
        });
        appendButton.Set(onInteract: () =>
        {
            if (tookTooLong)
                Solve("Solved by pressing the Append button after generation took too long.");
            else if (mazeGenerated && !Status.IsSolved && !viewingWholeMaze)
            {
                StartCoroutine(Moving("append", 2));
            }
            Shake(resetButton, .75f, Sound.BigButtonPress);

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
        try
        {
            Setup();
        }
        catch (IndexOutOfRangeException e)
        {
            string[] exceptionLines = e.ToString().Split('\n');
            Log("Ran into an IndexOutOfRangeException. Regenerating… The following is the content of the exception:");
            foreach (string line in exceptionLines)
                Log(line);
            t.Awake();
            mustAppend.Clear();
            StartCoroutine(Initialization());
            yield break;
        }
        catch (InvalidOperationException e)
        {
            string[] exceptionLines = e.ToString().Split('\n');
            Log("Ran into an InvalidOperationException. Regenerating… The following is the content of the exception:");
            foreach (string line in exceptionLines)
                Log(line);
            t.Awake();
            correctPath.Clear();
            mustAppend.Clear();
            StartCoroutine(Initialization());
            yield break;
        }
        int appendIndex = 0;
        sequence.Clear();
        while (UncannyMazeTile.current != mustAppend[appendIndex] && attempts < 3)
        {
            if (canGo.Count == 0)
            {
                break;
            }
            else
            {
                index = UnityEngine.Random.Range(0, canGo.Count);
                correctPath.Add(canGo.ElementAt(index));
                try
                {
                    yield return StartCoroutine(Moving(canGo.ElementAt(index), 2));
                }
                catch (IndexOutOfRangeException e)
                {
                    string[] exceptionLines = e.ToString().Split('\n');
                    Log("Ran into an IndexOutOfRangeException. Regenerating… The following is the content of the exception:");
                    foreach (string line in exceptionLines)
                        Log(line);
                    correctPath.Clear();
                    t.Awake();
                    StartCoroutine(Initialization());
                    yield break;
                }
                finally { }//needed for the yield return StartCoroutine in the try block to function properly
                movements++;
                if (movements >= 50)
                {
                    yield return StartCoroutine(Moving("reset", 2));
                    movements = 0;
                    correctPath.Clear();
                    attempts++;
                }
            }
            if (UncannyMazeTile.current == mustAppend[appendIndex])
            {
                if (appendIndex == mustAppend.Count - 1)
                {
                    break;
                }
                else
                {
                    correctPath.Add("append");
                    appendIndex++;
                }
            }
        }
        if (attempts != 3 && canGo.Count != 0)
        {
            logging = true;
            sequence.Clear();
            mustAppend.Clear();
            Log("A possible route is: " + string.Join(", ", correctPath.ToArray()));
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
            mustAppend.Clear();
            correctPath.Clear();
            StartCoroutine(Initialization());
            yield break;
        }
    }

    private void Setup()
    {
        map = t.layout;
        UncannyMazeTile.setStartAndGoal(map[(dims - yStart - 1), xStart], map[(dims - yGoal - 1), xGoal]);
        UncannyMazeTile.current = map[(dims - yCoords - 1), xCoords];
        UncannyMazeTile.chosenMazeFor5x5 = UncannyMazeTile.submit5x5Mazes[UncannyMazeTile.start.uncannyValue];
        currentBox.transform.localScale = new Vector3(7f / dims, 1, 7f / dims);
        if (logging)
        {
            string[] outputLines = output.Split('\n');
            Log("Your maze layout is:");
            foreach (string line in outputLines)
            {
                Log(line);
            }
        }
        if (dims == 4)
        {
            sequenceCharacters = sum4x4();
            if (logging)
                Log("Your unsigned long is: " + sequenceCharacters);
        }
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
            char[][] playfairMaze = UncannyMazeTile.chosenMazeFor5x5;
            string outputPlayfairSubmit = "";
            for (int r = 0; r < dims; r++)
            {
                for (int c = 0; c < dims; c++)
                {
                    outputPlayfair += "" + map[r, c].character;
                    outputPlayfairSubmit += "" + playfairMaze[r][c];
                    if (c != dims - 1)
                    {
                        outputPlayfair += " ";
                        outputPlayfairSubmit += " ";
                    }
                }
                if (r != dims - 1)
                {
                    outputPlayfair += "\n";
                    outputPlayfairSubmit += "\n";
                }
            }
            string[] outputPlayfairLines = outputPlayfair.Split('\n');
            string[] outputPlayfairSubmitLines = outputPlayfairSubmit.Split('\n');
            if (logging)
            {
                Log("Your playfair cipher key is:");
                foreach (string line in outputPlayfairLines)
                {
                    Log(line);
                }
                Log("Your playfair cipher submitting grid is:");
                foreach (string line in outputPlayfairSubmitLines)
                {
                    Log(line);
                }
            }
            sequenceCharacters = "";
            for (int i = 0; i < 8; i += 2)
            {
                UncannyMazeTile firstTile = (from UncannyMazeTile u in map where u.character == UncannyMazeTile.goal.playfairWord[i] select u).ToArray()[0];
                UncannyMazeTile secondTile = (from UncannyMazeTile u in map where u.character == UncannyMazeTile.goal.playfairWord[i + 1] select u).ToArray()[0];
                if (firstTile.x == secondTile.x)
                {
                    sequenceCharacters += map[(1 + firstTile.y) % 5, firstTile.x].character;
                    sequenceCharacters += map[(1 + secondTile.y) % 5, secondTile.x].character;
                }
                else if (firstTile.y == secondTile.y)
                {
                    sequenceCharacters += map[firstTile.y, (1 + firstTile.x) % 5].character;
                    sequenceCharacters += map[secondTile.y, (1 + secondTile.x) % 5].character;
                }
                else
                {
                    sequenceCharacters += map[firstTile.y, secondTile.x].character;
                    sequenceCharacters += map[secondTile.y, firstTile.x].character;
                }
            }
            if (logging)
                Log("Your encrypted word is: " + sequenceCharacters);
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
            sequenceCharacters = sum6x6();
            if (logging)
                Log("Your base-36 sum is: " + sequenceCharacters);

        }
        if (dims == 4)
        {
            foreach (char character in sequenceCharacters)
            {
                UncannyMazeTile[] allTilesWithValue = (from UncannyMazeTile u in map where u.uncannyValue == int.Parse(character.ToString()) select u).ToArray();
                if (allTilesWithValue.Length == 0)
                {
                    throw new InvalidOperationException("This 4×4 maze does not contain the number " + character + " and may not be solvable.");
                }
                else
                {
                    List<int> manhattanDistances = new List<int>();
                    if (mustAppend.Count == 0)
                    {
                        for (int i = 0; i < allTilesWithValue.Length; i++)
                        {
                            UncannyMazeTile tile = allTilesWithValue[i];
                            if (i > 0 && tile == allTilesWithValue[i - 1])
                            {
                                manhattanDistances.Add(-1);
                            }
                            manhattanDistances.Add(Math.Abs(tile.x - UncannyMazeTile.start.x) + Math.Abs(tile.y - UncannyMazeTile.start.y));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < allTilesWithValue.Length; i++)
                        {
                            UncannyMazeTile tile = allTilesWithValue[i];
                            if (i > 0 && tile == allTilesWithValue[i - 1])
                            {
                                manhattanDistances.Add(-1);
                            }
                            manhattanDistances.Add(Math.Abs(tile.x - mustAppend[mustAppend.Count - 1].x) + Math.Abs(tile.y - mustAppend[mustAppend.Count - 1].y));
                        }
                    }
                    mustAppend.Add(allTilesWithValue[manhattanDistances.IndexOf(manhattanDistances.Where(x => x != -1).Min())]);
                }
            }
        }
        else if (dims == 5)
        {
            foreach (char character in sequenceCharacters)
            {
                mustAppend.Add((from UncannyMazeTile u in map where u.submit5x5Character == character select u).ToArray()[0]);
            }
        }
        else if (dims == 6)
        {
            foreach (char character in sequenceCharacters)
            {
                mustAppend.Add((from UncannyMazeTile u in map where u.character == character select u).ToArray()[0]);
            }
        }
        mustAppend.Add(UncannyMazeTile.goal);
        if (logging)
        {
            Log("Your starting tile is: " + UncannyMazeTile.start);
            Log("Your current tile is: " + UncannyMazeTile.current);
            Log("Your goal tile is: " + UncannyMazeTile.goal);
            Log("Your must append is: " + string.Join(", ", mustAppend.Select(u => "" + u.letterCoord + u.numberCoord).ToArray()));
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
        if (logging)
            Log("Your sum of maze modulo 10 is: " + totalMazeTotal);
        leftSum = 0;
        rightSum = 0;
        aboveSum = 0;
        belowSum = 0;
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
        StartCoroutine(Moving("reset", 2));
    }

    private float f(float x)
    {
        return (x * -2 / dims) - (-2 / dims);
    }

    private string sum4x4()
    {
        if (dims != 4)
            return null;
        string startingQuadrant, goalQuadrant;
        if (UncannyMazeTile.start == map[0, 0] || UncannyMazeTile.start == map[0, 1] || UncannyMazeTile.start == map[1, 0] || UncannyMazeTile.start == map[1, 1])
        {
            startingQuadrant = "1";
        }
        else if (UncannyMazeTile.start == map[0, 2] || UncannyMazeTile.start == map[0, 3] || UncannyMazeTile.start == map[1, 2] || UncannyMazeTile.start == map[1, 3])
        {
            startingQuadrant = "2";
        }
        else if (UncannyMazeTile.start == map[2, 0] || UncannyMazeTile.start == map[2, 1] || UncannyMazeTile.start == map[3, 0] || UncannyMazeTile.start == map[3, 1])
        {
            startingQuadrant = "3";
        }
        else
        {
            startingQuadrant = "4";
        }
        if (UncannyMazeTile.goal == map[0, 0] || UncannyMazeTile.goal == map[0, 1] || UncannyMazeTile.goal == map[1, 0] || UncannyMazeTile.goal == map[1, 1])
        {
            goalQuadrant = "1";
        }
        else if (UncannyMazeTile.goal == map[0, 2] || UncannyMazeTile.start == map[0, 3] || UncannyMazeTile.start == map[1, 2] || UncannyMazeTile.start == map[1, 3])
        {
            goalQuadrant = "2";
        }
        else if (UncannyMazeTile.goal == map[2, 0] || UncannyMazeTile.goal == map[2, 1] || UncannyMazeTile.goal == map[3, 0] || UncannyMazeTile.goal == map[3, 1])
        {
            goalQuadrant = "3";
        }
        else
        {
            goalQuadrant = "4";
        }
        string firstQuadrant = Convert.ToString(ushort.Parse("" + map[0, 0].uncannyValue + map[0, 1].uncannyValue + map[1, 0].uncannyValue + map[1, 1].uncannyValue), 2);
        int firstLength = firstQuadrant.Length;
        if (firstQuadrant == "0")
        {
            firstQuadrant = "00000000000000";
        }
        else
        {
            for (int i = 0; i < 14 - firstLength; i++)
            {
                firstQuadrant = "0" + firstQuadrant;
            }
        }
        firstQuadrant = (startingQuadrant == "1" ? "1" : "0") + (goalQuadrant == "1" ? "1" : "0") + firstQuadrant;
        string secondQuadrant = Convert.ToString(ushort.Parse("" + map[0, 2].uncannyValue + map[0, 3].uncannyValue + map[1, 2].uncannyValue + map[1, 3].uncannyValue), 2);
        int secondLength = secondQuadrant.Length;
        if (secondQuadrant == "0")
        {
            secondQuadrant = "00000000000000";
        }
        else
        {
            for (int i = 0; i < 14 - secondLength; i++)
            {
                secondQuadrant = "0" + secondQuadrant;
            }
        }
        secondQuadrant = (startingQuadrant == "2" ? "1" : "0") + (goalQuadrant == "2" ? "1" : "0") + secondQuadrant;
        string thirdQuadrant = Convert.ToString(ushort.Parse("" + map[2, 0].uncannyValue + map[2, 1].uncannyValue + map[3, 0].uncannyValue + map[3, 1].uncannyValue), 2);
        int thirdLength = thirdQuadrant.Length;
        if (thirdQuadrant == "0")
        {
            thirdQuadrant = "00000000000000";
        }
        else
        {
            for (int i = 0; i < 14 - thirdLength; i++)
            {
                thirdQuadrant = "0" + thirdQuadrant;
            }
        }
        thirdQuadrant = (startingQuadrant == "3" ? "1" : "0") + (goalQuadrant == "3" ? "1" : "0") + thirdQuadrant;
        string fourthQuadrant = Convert.ToString(ushort.Parse("" + map[2, 2].uncannyValue + map[2, 3].uncannyValue + map[3, 2].uncannyValue + map[3, 3].uncannyValue), 2);
        int fourthLength = fourthQuadrant.Length;
        if (fourthQuadrant == "0")
        {
            fourthQuadrant = "00000000000000";
        }
        else
        {
            for (int i = 0; i < 14 - fourthLength; i++)
            {
                fourthQuadrant = "0" + fourthQuadrant;
            }
        }
        fourthQuadrant = (startingQuadrant == "4" ? "1" : "0") + (goalQuadrant == "4" ? "1" : "0") + fourthQuadrant;
        if (logging)
            Log("The binary sequence is " + firstQuadrant + secondQuadrant + thirdQuadrant + fourthQuadrant + ".");
        return Convert.ToUInt64(firstQuadrant + secondQuadrant + thirdQuadrant + fourthQuadrant, 2).ToString();
    }

    private string sum6x6()
    {
        if (dims != 6)
            return null;
        int firstQuadrant = 1;
        int secondQuadrant = 1;
        int thirdQuadrant = 1;
        int fourthQuadrant = 1;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (map[i, j].uncannyValue != 0)
                {
                    firstQuadrant *= map[i, j].uncannyValue;
                }
            }
        }
        for (int i = 0; i < 3; i++)
        {
            for (int j = 3; j < 6; j++)
            {
                if (map[i, j].uncannyValue != 0)
                {
                    secondQuadrant *= map[i, j].uncannyValue;
                }
            }
        }
        for (int i = 3; i < 6; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (map[i, j].uncannyValue != 0)
                {
                    thirdQuadrant *= map[i, j].uncannyValue;
                }
            }
        }
        for (int i = 3; i < 6; i++)
        {
            for (int j = 3; j < 6; j++)
            {
                if (map[i, j].uncannyValue != 0)
                {
                    fourthQuadrant *= map[i, j].uncannyValue;
                }
            }
        }
        int sumOfQuadrants = firstQuadrant + secondQuadrant + thirdQuadrant + fourthQuadrant;
        char[] clistarr = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        string result = "";
        while (sumOfQuadrants != 0)
        {
            result = clistarr[sumOfQuadrants % 36] + result;
            sumOfQuadrants /= 36;
        }
        return result;
    }

    private IEnumerator Moving(string direction, int n)
    {
        map = t.layout;
        if (viewingWholeMaze || currentlyMoving
        || (xCoords == 0 && direction == "left")
        || (xCoords == dims - 1 && direction == "right")
        || (yCoords == dims - 1 && direction == "up")
        || (yCoords == 0 && direction == "down"))
            yield break;
        if (direction != "reset" && direction != "append" && !canGo.Contains(direction) && logging)
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
            case "append":
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
        else if (direction == "append")
        {
            if (UncannyMazeTile.current == mustAppend[sequence.Count])
            {
                Log("Appended " + UncannyMazeTile.current.letterCoord + UncannyMazeTile.current.numberCoord + ".");
                sequence.Add(UncannyMazeTile.current);
            }
            else
            {
                Strike("Tried to append " + UncannyMazeTile.current.letterCoord + UncannyMazeTile.current.numberCoord + ", was supposed to append" + mustAppend[sequence.Count].letterCoord + mustAppend[sequence.Count].numberCoord + ".");
            }
            if (UncannyMazeTile.current == UncannyMazeTile.goal && sequence.SequenceEqual(mustAppend) && logging)
            {
                Solve("Your module is: solved module");
                yield break;
            }
        }
        else
        {
            if (logging)
                Log("Pressed " + direction + ", going to " + UncannyMazeTile.current.letterCoord + UncannyMazeTile.current.numberCoord + ".");
        }
        coordsText.GetComponent<TextMesh>().text = "CURRENT: " + UncannyMazeTile.current.letterCoord + UncannyMazeTile.current.numberCoord + "\nGOAL: " + UncannyMazeTile.goal.letterCoord + UncannyMazeTile.goal.numberCoord;
        if (logging)
        {
            Log("Your current tile is: " + UncannyMazeTile.current);
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
                if (logging)
                    Log("Your maze is: Goal Maze");
                possibleDirections = ClosestInValue(UncannyMazeTile.goal.uncannyValue, false, leftTile, rightTile, aboveTile, belowTile);
                break;
            case UncannyMazeTile.mazeTypes.TOTAL:
                if (logging)
                    Log("Your maze is: Total Maze");
                possibleDirections = ClosestInValue(totalMazeTotal, false, leftTile, rightTile, aboveTile, belowTile);
                break;
            case UncannyMazeTile.mazeTypes.CROSS:
                if (logging)
                    Log("Your maze is: Cross Maze");
                possibleDirections = CrossMaze(UncannyMazeTile.current.y, UncannyMazeTile.current.x, leftTile, rightTile, aboveTile, belowTile);
                break;
            case UncannyMazeTile.mazeTypes.BORDER:
                if (logging)
                    Log("Your maze is: Border Maze");
                possibleDirections = ClosestInValue(0, true, leftTile, rightTile, aboveTile, belowTile);
                break;
            case UncannyMazeTile.mazeTypes.CORNERS:
                if (logging)
                    Log("Your maze is: Corners Maze");
                possibleDirections = CornersMaze();
                break;
        }
        canGo.Clear();
        foreach (UncannyMazeTile tile in possibleDirections)
        {
            if (tile != null && directions.ContainsValue(tile))
            {
                canGo.Add(directions.FirstOrDefault(x => x.Value == tile).Key);
            }
        }
        canGo = canGo.Distinct().ToList();
        if (logging)
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
                if (differences[i] != -1 && differences[i] == differences.Where(n => n != -1).Min())
                {
                    result.Add(adjacent[i]);
                }
            }
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
            allCombinations.AddRange((from UncannyMazeTile u in map where u.y == combination.x && u.x == combination.y select u).ToList());
            allCombinations.AddRange((from UncannyMazeTile u in map where u.x == combination.x && u.y == combination.y select u).ToList());
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
            if (totaltime == 13 && !mazeGenerated)
            {
                gm.GetComponent<TextMesh>().fontSize = 27;
                gm.GetComponent<TextMesh>().text = "SORRY THE MAZE\nTOOK SO LONG TO\nLOAD, PRESS ANY\nOF THE THREE WHITE\nBUTTONS TO\nSOLVE IMMEDIATELY.";
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
