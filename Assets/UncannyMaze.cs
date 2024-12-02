using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using wawa.Modules;
using wawa.Extensions;
using wawa.IO;
using wawa.Schemas;

public class UncannyMaze : ModdedModule
{
    private readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public KMSelectable arrowleft, arrowright, arrowup, arrowdown, maze, numbersButton, resetButton, appendButton;
    public Texture2D[] unblurred;
    internal bool viewingWholeMaze = false;
    internal int animSpeed = 30;
    private int leftSum, rightSum, aboveSum, belowSum;
    private List<int> borderSums = new List<int>();
    private float m, b;
    internal bool mazeGenerated = false;
    internal bool currentlyMoving = false;
    internal bool tookTooLong = false;
    private bool generatingMazeIdleCurrentlyRunning = false;
    internal bool music = false;
    internal int blur = 0;
    private bool logging = false;
    private Vector2 currentPosition, startingPosition;
    private int xStart, yStart, xCoords, yCoords, xGoal, yGoal;
    public GameObject numbers, gm, currentBox, goalBox, anchor, coordsText;
    private int dims;
    private int totalMazeTotal;
    private int centerMazeSum;
    private int cornersMazeSum;
    private string output, outputFiller;
    private UncannyMazeTile[,] map;
    private List<UncannyMazeTile> mustAppend = new List<UncannyMazeTile>();
    private List<UncannyMazeTile> sequence = new List<UncannyMazeTile>();
    private Config<UncannyMazeSettings> umSettings;
    private string sequenceCharacters = "";
    public TextureGeneratorUncanny t;
    private bool failed = false;
    private int movements, attempts, appendIndex;
    private string chosenDirection;
    internal List<string> correctPath = new List<string>();
    private UncannyMazeTile start, goal;
    internal UncannyMazeTile current;
    private char[][] chosenMazeFor5x5;
    private string outputPlayfair, outputPlayfairSubmit, outputBase36;

    [Serializable]
    public sealed class UncannyMazeSettings
    {
        [TweaksSetting.Number("Set the speed of the module's moving animation in frames.\nShould be from 10 to 60. Set to 2 to forgo moving animation.", "Animation Speed")]
        public int uncannyAnimationSpeed = 30;
        [TweaksSetting.Checkbox("If streaming, disable this to avoid copyright claims.", "Play Music on Solve")]
        public bool uncannyPlayMusicOnSolve = false;
        [TweaksSetting.Number("Blur all images from this number onward.\nNumbers outside 0 to 9 will leave all images unblurred.", "Blur Threshold")]
        public int uncannyBlurThreshold = 2;

        public UncannyMazeSettings(){}

        public UncannyMazeSettings(int speed){
            if (speed == 2){
                uncannyAnimationSpeed = 2;
            }
            else {
                uncannyAnimationSpeed = Mathf.Clamp(speed, 10, 60);
            }
        }
    }

    static readonly TweaksEditorSettings TweaksEditorSettings = TweaksEditorSettings.CreateListing("Uncanny Maze", "uncannymaze").Register<UncannyMazeSettings>().BuildAndClear();

    protected override void Awake()
    {
        umSettings = new Config<UncannyMazeSettings>("uncannymaze-settings.json");
        blur = umSettings.Read().uncannyBlurThreshold;
        if (blur > 0 && blur <= 9)
        {
            for (int i = 0; i < blur; i++)
            {
                t.textures[i] = unblurred[i];
            }
        }
        animSpeed = umSettings.Read().uncannyAnimationSpeed;
        if (umSettings.Read().uncannyAnimationSpeed != 2)
        {
            animSpeed = Mathf.Clamp(umSettings.Read().uncannyAnimationSpeed, 10, 60);
        }
        music = umSettings.Read().uncannyPlayMusicOnSolve;
        umSettings.Write(new UncannyMazeSettings(animSpeed));
    }


    /*
    public static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]{
        new Dictionary<string,object>{
            {"Filename","uncannymaze-settings.json"},
            {"Name","Uncanny Maze"},
            {"Listings",new List<Dictionary<string,object>>{
                new Dictionary<string,object>{
                    {"Key","uncannyAnimationSpeed"},
                    {"Text","Animation Speed"},
                    {"Description","Set the speed of the module's moving animation in frames.\nShould be from 10 to 60. Set to 2 to forgo moving animation."}
                },
                new Dictionary<string, object>{
                    {"Key","uncannyPlayMusicOnSolve"},
                    {"Text","Play Music On Solve"},
                    {"Description","If streaming, disable this to avoid copyright claims."}
                },
                new Dictionary<string,object>{
                    {"Key","uncannyBlurThreshold"},
                    {"Text","Blur Threshold"},
                    {"Description","Blur all images from this number onward.\nNumbers outside 0 to 9 will leave all images unblurred."}
                }
            }}
        }
    };
    */

    public void Start()
    {
        StartCoroutine(Initialization());
        arrowleft.Set(onInteract: () =>
        {
            if (mazeGenerated && !Status.IsSolved)
            {
                StartCoroutine(Moving("left", animSpeed));
            }
            Shake(arrowleft, 1, Sound.BigButtonPress);
        });
        arrowright.Set(onInteract: () =>
        {
            if (mazeGenerated && !Status.IsSolved)
            {
                StartCoroutine(Moving("right", animSpeed));
            }
            Shake(arrowright, 1, Sound.BigButtonPress);
        });
        arrowup.Set(onInteract: () =>
        {
            if (mazeGenerated && !Status.IsSolved)
            {
                StartCoroutine(Moving("up", animSpeed));
            }
            Shake(arrowup, 1, Sound.BigButtonPress);
        });
        arrowdown.Set(onInteract: () =>
        {
            if (mazeGenerated && !Status.IsSolved)
            {
                StartCoroutine(Moving("down", animSpeed));
            }
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
            {
                Solve("Solved by pressing the Numbers button after generation took too long.");
            }
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
            {
                Solve("Solved by pressing the Reset button after generation took too long.");
            }
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
            {
                Solve("Solved by pressing the Append button after generation took too long.");
            }
            else if (mazeGenerated && !Status.IsSolved && !viewingWholeMaze)
            {
                StartCoroutine(Moving("append", 2));
            }
            Shake(resetButton, .75f, Sound.BigButtonPress);

        });
    }

    private IEnumerator Initialization()
    {
        totalMazeTotal = 0;
        centerMazeSum = 0;
        leftSum = 0;
        rightSum = 0;
        aboveSum = 0;
        belowSum = 0;
        t.changeTexture(t.whiteBG);
        output = "";
        outputFiller = "";
        dims = t.gridDimensions;
        int max = t.layout.Select(l => l).Max(l => l.Count());
        map = new UncannyMazeTile[t.layout.Count, max];
        for (int i = 0; i < t.layout.Count; i++)
        {
            for (int j = 0; j < t.layout[i].Count(); j++)
            {
                map[i, j] = t.layout[i][j];
            }
        }
        totalMazeTotal %= 10;
        numbers.GetComponent<TextMesh>().text = output;
        currentPosition = new Vector2((float)UnityEngine.Random.Range(0, dims) / dims, (float)UnityEngine.Random.Range(0, dims) / dims);
        startingPosition = currentPosition;
        maze.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1f / dims, 1f / dims);
        maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
        if (tookTooLong)
        {
            yield break;
        }
        if (!generatingMazeIdleCurrentlyRunning)
        {
            StartCoroutine(GeneratingMazeIdle());
        }
        xStart = (int)((startingPosition.x * dims) + .01f);
        yStart = (int)((startingPosition.y * dims) + .01f);
        xCoords = (int)((currentPosition.x * dims) + .01f);
        yCoords = (int)((currentPosition.y * dims) + .01f);
        do
        {
            xGoal = UnityEngine.Random.Range(0, dims);
            yGoal = UnityEngine.Random.Range(0, dims);
        } while (map[dims - yCoords - 1, xCoords] == map[dims - yGoal - 1, xGoal]);
        movements = 0;
        attempts = 0;
        chosenDirection = "";
        start = map[dims - yStart - 1, xStart];
        goal = map[dims - yGoal - 1, xGoal];
        current = map[dims - yCoords - 1, xCoords];
        try
        {
            Setup();
        }
        catch (IndexOutOfRangeException)
        {
            t.Awake();
            mustAppend.Clear();
            correctPath.Clear();
            StartCoroutine(Initialization());
            yield break;
        }
        catch (InvalidOperationException e)
        {
            Log("Ran into an InvalidOperationException during setup. Regenerating… The following is the content of the exception:");
            string[] exceptionLines = e.ToString().Split('\n');
            foreach (string line in exceptionLines)
            {
                Log(line);
            }
            t.Awake();
            mustAppend.Clear();
            correctPath.Clear();
            StartCoroutine(Initialization());
            yield break;
        }
        appendIndex = 0;
        sequence.Clear();
        do
        {
            try
            {
                if (current.ValidDirections.Length == 0)
                {
                    throw new IndexOutOfRangeException("erm what the sigma (dead end)");
                }
                chosenDirection = current.ValidDirections.PickRandom();
                correctPath.Add(chosenDirection);
                switch (chosenDirection)
                {
                    case "left":
                        xCoords--;
                        break;
                    case "right":
                        xCoords++;
                        break;
                    case "up":
                        yCoords++;
                        break;
                    case "down":
                        yCoords--;
                        break;
                    default:
                        throw new InvalidOperationException("erm what the sigma (direction switch statement)");
                }
                current = map[dims - yCoords - 1, xCoords];
            }
            catch (InvalidOperationException e)
            {
                Log("Ran into an InvalidOperationException while generating a solvable maze. Regenerating… The following is the content of the exception:");
                string[] exceptionLines = e.ToString().Split('\n');
                foreach (string line in exceptionLines)
                {
                    Log(line);
                }
                mustAppend.Clear();
                correctPath.Clear();
                t.Awake();
                StartCoroutine(Initialization());
                yield break;
            }
            catch (IndexOutOfRangeException e)
            {
                Log("Ran into an IndexOutOfRangeException while generating a solvable maze. Regenerating… The following is the content of the exception:");
                string[] exceptionLines = e.ToString().Split('\n');
                foreach (string line in exceptionLines)
                {
                    Log(line);
                }
                mustAppend.Clear();
                correctPath.Clear();
                t.Awake();
                StartCoroutine(Initialization());
                yield break;
            }
            movements++;
            if (movements >= 100)
            {
                movements = 0;
                attempts++;
                if (attempts < 3)
                {
                    continue;
                }
                else
                {
                    failed = true;
                    break;
                }
            }
            if (current == mustAppend[appendIndex])
            {
                if (appendIndex >= mustAppend.Count - 1)
                {
                    correctPath.Add("append");
                    attempts = 3;
                    failed = false;
                    break;
                }
                else
                {
                    int notiar = NumberOfTimesInARow(mustAppend, appendIndex);
                    for (int i = 0; i < notiar; i++)
                    {
                        correctPath.Add("append");
                    }
                    appendIndex += notiar;
                }
            }
        } while (appendIndex < mustAppend.Count && current != mustAppend[appendIndex] && attempts < 3);
        if (attempts >= 3 && current.ValidDirections.Length != 0 && !failed)
        {
            logging = true;
            sequence.Clear();
            current = map[dims - yStart - 1, xStart];
            chosenMazeFor5x5 = UncannyMazeTile.submit5x5Mazes[start.UncannyValue];
            currentBox.transform.localScale = new Vector3(7f / dims, 1, 7f / dims);
            string filler = "\n       \n";
            if (dims == 5)
            {
                filler = "\n         \n";
            }
            else if (dims == 6)
            {
                filler = "\n           \n";
            }
            output = "";
            outputFiller = "";
            totalMazeTotal = 0;
            for (int r = 0; r < dims; r++)
            {
                for (int c = 0; c < dims; c++)
                {
                    output += "" + map[r, c].UncannyValue;
                    outputFiller += "" + map[r, c].UncannyValue;
                    totalMazeTotal += map[r, c].UncannyValue;
                    if (c != dims - 1)
                    {
                        output += " ";
                        outputFiller += " ";
                    }
                }
                if (r != dims - 1)
                {
                    output += "\n";
                    outputFiller += filler;
                }
            }
            totalMazeTotal %= 10;
            if (logging)
            {
                for (int i = 0; i < dims; i++)
                {
                    for (int j = 0; j < dims; j++)
                    {
                        if (map[i, j].ValidDirections.Contains("left"))
                        {
                            outputFiller = outputFiller[(4 * dims * i) + (2 * j) - 1] == '>'
                                ? outputFiller.Remove((4 * dims * i) + (2 * j) - 1, 1).Insert((4 * dims * i) + (2 * j) - 1, "x")
                                : outputFiller.Remove((4 * dims * i) + (2 * j) - 1, 1).Insert((4 * dims * i) + (2 * j) - 1, "<");
                        }
                        if (map[i, j].ValidDirections.Contains("right"))
                        {
                            outputFiller = outputFiller.Remove((4 * dims * i) + (2 * j) + 1, 1).Insert((4 * dims * i) + (2 * j) + 1, ">");
                        }
                        if (map[i, j].ValidDirections.Contains("up"))
                        {
                            outputFiller = outputFiller[(4 * dims * i) + (2 * j) - (2 * dims)] == 'V'
                                ? outputFiller.Remove((4 * dims * i) + (2 * j) - (2 * dims), 1).Insert((4 * dims * i) + (2 * j) - (2 * dims), "X")
                                : outputFiller.Remove((4 * dims * i) + (2 * j) - (2 * dims), 1).Insert((4 * dims * i) + (2 * j) - (2 * dims), "^");
                        }
                        if (map[i, j].ValidDirections.Contains("down"))
                        {
                            outputFiller = outputFiller.Remove((4 * dims * i) + (2 * j) + (2 * dims), 1).Insert((4 * dims * i) + (2 * j) + (2 * dims), "V");
                        }
                    }
                }
                string[] outputLines = outputFiller.Split('\n');
                Log("Your maze layout is:");
                foreach (string line in outputLines)
                {
                    Log(line);
                }
            }
            if (dims == 4)
            {
                sequenceCharacters = Sum4x4();
                Log("Your unsigned long is: " + sequenceCharacters);
            }
            if (dims == 5)
            {
                string[] outputPlayfairLines = outputPlayfair.Split('\n');
                string[] outputPlayfairSubmitLines = outputPlayfairSubmit.Split('\n');
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
                Log("Your encrypted word is: " + sequenceCharacters);
            }
            else if (dims == 6)
            {
                centerMazeSum = map[2, 2].UncannyValue + map[2, 3].UncannyValue + map[3, 2].UncannyValue + map[3, 3].UncannyValue;
                int currentIndex = 0;
                UncannyMazeTile[] tilesWithValue;
                int currentNumberIndex = 0;
                for (int i = 0; i < 10; i++)
                {
                    tilesWithValue = (from UncannyMazeTile u in map where u.UncannyValue == i select u).ToArray();
                    if (tilesWithValue.Length == 0)
                    {
                        currentNumberIndex++;
                        continue;
                    }
                    tilesWithValue[0].Character = (char)(i + 48);
                    for (int j = 1; j < tilesWithValue.Length; j++)
                    {
                        tilesWithValue[j].Character = alphabet[currentIndex % 26];
                        currentIndex++;
                    }
                    currentNumberIndex++;
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
                sequenceCharacters = Sum6x6();
                if (logging)
                {
                    Log("Your base-36 sum is: " + sequenceCharacters);
                }
            }
            if (logging)
            {
                Log("Your starting tile is: " + start);
                Log("Your goal tile is: " + goal);
                Log("You must append: " + string.Join(", ", mustAppend.Select(u => "" + u.LetterCoord + u.NumberCoord).ToArray()));
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
                default:
                    throw new InvalidOperationException("erm what the sigma (dimensions)");
            }
            goalBox.transform.localScale = new Vector3(7f / dims, 1, 7f / dims);
            goalBox.transform.localPosition = new Vector3((m * xGoal) - b, -.01f, (-m * yGoal) + b);
            cornersMazeSum = (map[0, 0].UncannyValue + map[0, dims - 1].UncannyValue + map[dims - 1, 0].UncannyValue + map[dims - 1, dims - 1].UncannyValue) % 10;
            centerMazeSum %= 10;
            Log("Your sum of maze modulo 10 is: " + totalMazeTotal);
            Log("Your sum of center modulo 10 is: " + centerMazeSum);
            Log("Your sum of corners modulo 10 is: " + cornersMazeSum);
            Log("The sum of the left border is: " + leftSum);
            Log("The sum of the right border is: " + rightSum);
            Log("The sum of the top border is: " + aboveSum);
            Log("The sum of the bottom border is: " + belowSum);
            correctPath = RemoveOpposites(correctPath);
            correctPath = RemoveGoingInCircles(correctPath);
            Log("A possible path is: " + string.Join(", ", correctPath.ToArray()));
            mazeGenerated = true;
            gm.SetActive(false);
            t.changeTexture(t.finalTexture);
            maze.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1f / dims, 1f / dims);
            maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
            xCoords = xStart;
            yCoords = yStart;
            current = start;
        }
        else
        {
            failed = false;
            t.Awake();
            mustAppend.Clear();
            correctPath.Clear();
            StartCoroutine(Initialization());
        }
    }

    private List<string> RemoveOpposites(List<string> path)
    {
        List<string> pathCopy = new List<string>(path);
        bool stillHasOpposites;
        do
        {
            stillHasOpposites = false;
            for (int i = pathCopy.Count - 2; i >= 0; i--)
            {
                switch (pathCopy[i])
                {
                    case "left":
                        if (pathCopy[i + 1] == "right")
                        {
                            pathCopy.RemoveAt(i + 1);
                            pathCopy.RemoveAt(i);
                            stillHasOpposites = true;
                        }
                        break;
                    case "right":
                        if (pathCopy[i + 1] == "left")
                        {
                            pathCopy.RemoveAt(i + 1);
                            pathCopy.RemoveAt(i);
                            stillHasOpposites = true;
                        }
                        break;
                    case "up":
                        if (pathCopy[i + 1] == "down")
                        {
                            pathCopy.RemoveAt(i + 1);
                            pathCopy.RemoveAt(i);
                            stillHasOpposites = true;
                        }
                        break;
                    case "down":
                        if (pathCopy[i + 1] == "up")
                        {
                            pathCopy.RemoveAt(i + 1);
                            pathCopy.RemoveAt(i);
                            stillHasOpposites = true;
                        }
                        break;
                    case "append":
                    default:
                        break;
                }
            }
        } while (stillHasOpposites);
        return pathCopy;
    }

    private List<string> RemoveGoingInCircles(List<string> path)
    {
        List<string> pathCopy = new List<string>(path);
        bool stillHasCircles;
        do
        {
            stillHasCircles = false;
            for (int i = pathCopy.Count - 4; i >= 0; i--)
            {
                switch (pathCopy[i])
                {
                    case "left":
                        if (pathCopy[i + 2] == "right" &&
                          ((pathCopy[i + 1] == "down" && pathCopy[i + 3] == "up")
                        || (pathCopy[i + 1] == "up" && pathCopy[i + 3] == "down")))
                        {
                            pathCopy.RemoveAt(i + 3);
                            pathCopy.RemoveAt(i + 2);
                            pathCopy.RemoveAt(i + 1);
                            pathCopy.RemoveAt(i);
                            stillHasCircles = true;
                        }
                        break;
                    case "right":
                        if (pathCopy[i + 2] == "left" &&
                          ((pathCopy[i + 1] == "down" && pathCopy[i + 3] == "up")
                        || (pathCopy[i + 1] == "up" && pathCopy[i + 3] == "down")))
                        {
                            pathCopy.RemoveAt(i + 3);
                            pathCopy.RemoveAt(i + 2);
                            pathCopy.RemoveAt(i + 1);
                            pathCopy.RemoveAt(i);
                            stillHasCircles = true;
                        }
                        break;
                    case "up":
                        if (pathCopy[i + 2] == "down" &&
                          ((pathCopy[i + 1] == "left" && pathCopy[i + 3] == "right")
                        || (pathCopy[i + 1] == "right" && pathCopy[i + 3] == "left")))
                        {
                            pathCopy.RemoveAt(i + 3);
                            pathCopy.RemoveAt(i + 2);
                            pathCopy.RemoveAt(i + 1);
                            pathCopy.RemoveAt(i);
                            stillHasCircles = true;
                        }
                        break;
                    case "down":
                        if (pathCopy[i + 2] == "up" &&
                          ((pathCopy[i + 1] == "left" && pathCopy[i + 3] == "right")
                        || (pathCopy[i + 1] == "right" && pathCopy[i + 3] == "left")))
                        {
                            pathCopy.RemoveAt(i + 3);
                            pathCopy.RemoveAt(i + 2);
                            pathCopy.RemoveAt(i + 1);
                            pathCopy.RemoveAt(i);
                            stillHasCircles = true;
                        }
                        break;
                    case "append":
                    default:
                        break;
                }
            }
        } while (stillHasCircles);
        return pathCopy;
    }

    private int NumberOfTimesInARow(List<UncannyMazeTile> list, int index)
    {
        int repeat = 0;
        for (int i = index; i < list.Count; i++)
        {
            if (list[i] == list[index])
            {
                repeat++;
            }
            else
            {
                return repeat;
            }
        }
        return repeat;
    }

    private void Setup()
    {
        start = map[dims - yStart - 1, xStart];
        goal = map[dims - yGoal - 1, xGoal];
        current = map[dims - yCoords - 1, xCoords];
        chosenMazeFor5x5 = UncannyMazeTile.submit5x5Mazes[start.UncannyValue];
        currentBox.transform.localScale = new Vector3(7f / dims, 1, 7f / dims);
        string filler = "\n       \n";
        if (dims == 5)
        {
            filler = "\n         \n";
        }
        else if (dims == 6)
        {
            filler = "\n           \n";
        }
        output = "";
        outputFiller = "";
        totalMazeTotal = 0;
        for (int r = 0; r < dims; r++)
        {
            for (int c = 0; c < dims; c++)
            {
                output += "" + map[r, c].UncannyValue;
                outputFiller += "" + map[r, c].UncannyValue;
                totalMazeTotal += map[r, c].UncannyValue;
                if (c != dims - 1)
                {
                    output += " ";
                    outputFiller += " ";
                }
            }
            if (r != dims - 1)
            {
                output += "\n";
                outputFiller += filler;
            }
        }
        totalMazeTotal %= 10;
        Dictionary<string, UncannyMazeTile> tempDirs = new Dictionary<string, UncannyMazeTile>
            {
                { "left", null },
                { "right", null },
                { "up", null },
                { "down", null }
            };
        for (int i = 0; i < dims; i++)
        {
            leftSum += map[i, 0].UncannyValue;
            rightSum += map[i, dims - 1].UncannyValue;
            aboveSum += map[0, i].UncannyValue;
            belowSum += map[dims - 1, i].UncannyValue;
        }
        borderSums.Clear();
        borderSums.Add(leftSum);
        borderSums.Add(rightSum);
        borderSums.Add(aboveSum);
        borderSums.Add(belowSum);
        for (int i = 0; i < dims; i++)
        {
            for (int j = 0; j < dims; j++)
            {
                tempDirs["left"] = j == 0 ? null : map[i, j - 1];
                tempDirs["right"] = j == dims - 1 ? null : map[i, j + 1];
                tempDirs["up"] = i == 0 ? null : map[i - 1, j];
                tempDirs["down"] = i == dims - 1 ? null : map[i + 1, j];
                map[i, j].ValidDirections = DirectionsAvailable(map[i, j], tempDirs);
            }
        }
        if (logging)
        {
            for (int i = 0; i < dims; i++)
            {
                for (int j = 0; j < dims; j++)
                {
                    if (map[i, j].ValidDirections.Contains("left"))
                    {
                        outputFiller = outputFiller[(4 * dims * i) + (2 * j) - 1] == '>'
                            ? outputFiller.Remove((4 * dims * i) + (2 * j) - 1, 1).Insert((4 * dims * i) + (2 * j) - 1, "x")
                            : outputFiller.Remove((4 * dims * i) + (2 * j) - 1, 1).Insert((4 * dims * i) + (2 * j) - 1, "<");
                    }
                    if (map[i, j].ValidDirections.Contains("right"))
                    {
                        outputFiller = outputFiller.Remove((4 * dims * i) + (2 * j) + 1, 1).Insert((4 * dims * i) + (2 * j) + 1, ">");
                    }
                    if (map[i, j].ValidDirections.Contains("up"))
                    {
                        outputFiller = outputFiller[(4 * dims * i) + (2 * j) - (2 * dims)] == 'V'
                            ? outputFiller.Remove((4 * dims * i) + (2 * j) - (2 * dims), 1).Insert((4 * dims * i) + (2 * j) - (2 * dims), "X")
                            : outputFiller.Remove((4 * dims * i) + (2 * j) - (2 * dims), 1).Insert((4 * dims * i) + (2 * j) - (2 * dims), "^");
                    }
                    if (map[i, j].ValidDirections.Contains("down"))
                    {
                        outputFiller = outputFiller.Remove((4 * dims * i) + (2 * j) + (2 * dims), 1).Insert((4 * dims * i) + (2 * j) + (2 * dims), "V");
                    }
                }
            }
            string[] outputLines = outputFiller.Split('\n');
            Log("Your maze layout is:");
            foreach (string line in outputLines)
            {
                Log(line);
            }
        }
        if (dims == 4)
        {
            centerMazeSum = map[1, 1].UncannyValue + map[1, 2].UncannyValue + map[2, 1].UncannyValue + map[2, 2].UncannyValue;
            sequenceCharacters = Sum4x4();
            if (logging)
            {
                Log("Your unsigned long is: " + sequenceCharacters);
            }
            foreach (char character in sequenceCharacters)
            {
                UncannyMazeTile[] allTilesWithValue = (from UncannyMazeTile u in map where u.UncannyValue == int.Parse(character.ToString()) select u).ToArray();
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
                            manhattanDistances.Add(Math.Abs(tile.Xcoordinate - start.Xcoordinate) + Math.Abs(tile.Ycoordinate - start.Ycoordinate));
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
                            manhattanDistances.Add(Math.Abs(tile.Xcoordinate - mustAppend[mustAppend.Count - 1].Xcoordinate) + Math.Abs(tile.Ycoordinate - mustAppend[mustAppend.Count - 1].Ycoordinate));
                        }
                    }
                    mustAppend.Add(allTilesWithValue[manhattanDistances.IndexOf(manhattanDistances.Where(x => x != -1).Min())]);
                }
            }

        }
        if (dims == 5)
        {
            centerMazeSum = map[2, 2].UncannyValue;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    map[i, j].Submit5x5Character = chosenMazeFor5x5[i][j];
                }
            }
            int currentIndex = 0;
            UncannyMazeTile[] tilesWithValue;
            for (int i = 0; i < 10; i++)
            {
                tilesWithValue = (from UncannyMazeTile u in map where u.UncannyValue == i select u).ToArray();
                for (int j = 0; j < t.amountOfEachNumber[i]; j++)
                {
                    tilesWithValue[j].Character = alphabet[currentIndex];
                    currentIndex++;
                }
            }
            outputPlayfair = "";
            char[][] playfairMaze = chosenMazeFor5x5;
            outputPlayfairSubmit = "";
            for (int r = 0; r < dims; r++)
            {
                for (int c = 0; c < dims; c++)
                {
                    outputPlayfair += "" + map[r, c].Character;
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
                UncannyMazeTile firstTile = (from UncannyMazeTile u in map where u.Character == goal.PlayfairWord[i] select u).ToArray()[0];
                UncannyMazeTile secondTile = (from UncannyMazeTile u in map where u.Character == goal.PlayfairWord[i + 1] select u).ToArray()[0];
                if (firstTile.Xcoordinate == secondTile.Xcoordinate)
                {
                    sequenceCharacters += map[(1 + firstTile.Ycoordinate) % 5, firstTile.Xcoordinate].Character;
                    sequenceCharacters += map[(1 + secondTile.Ycoordinate) % 5, secondTile.Xcoordinate].Character;
                }
                else if (firstTile.Ycoordinate == secondTile.Ycoordinate)
                {
                    sequenceCharacters += map[firstTile.Ycoordinate, (1 + firstTile.Xcoordinate) % 5].Character;
                    sequenceCharacters += map[secondTile.Ycoordinate, (1 + secondTile.Xcoordinate) % 5].Character;
                }
                else
                {
                    sequenceCharacters += map[firstTile.Ycoordinate, secondTile.Xcoordinate].Character;
                    sequenceCharacters += map[secondTile.Ycoordinate, firstTile.Xcoordinate].Character;
                }
            }
            if (logging)
            {
                Log("Your encrypted word is: " + sequenceCharacters);
            }
            foreach (char character in sequenceCharacters)
            {
                mustAppend.Add((from UncannyMazeTile u in map where u.Submit5x5Character == character select u).ToArray()[0]);
            }
        }
        else if (dims == 6)
        {
            centerMazeSum = map[2, 2].UncannyValue + map[2, 3].UncannyValue + map[3, 2].UncannyValue + map[3, 3].UncannyValue;
            int currentIndex = 0;
            UncannyMazeTile[] tilesWithValue;
            int currentNumberIndex = 0;
            for (int i = 0; i < 10; i++)
            {
                tilesWithValue = (from UncannyMazeTile u in map where u.UncannyValue == i select u).ToArray();
                if (tilesWithValue.Length == 0)
                {
                    currentNumberIndex++;
                    continue;
                }
                tilesWithValue[0].Character = (char)(i + 48);
                for (int j = 1; j < tilesWithValue.Length; j++)
                {
                    tilesWithValue[j].Character = alphabet[currentIndex % 26];
                    currentIndex++;
                }
                currentNumberIndex++;
            }
            outputBase36 = "";
            for (int r = 0; r < dims; r++)
            {
                for (int c = 0; c < dims; c++)
                {
                    outputBase36 += "" + map[r, c].Character;
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
            sequenceCharacters = Sum6x6();
            if (logging)
            {
                Log("Your base-36 sum is: " + sequenceCharacters);
            }
            foreach (char character in sequenceCharacters)
            {
                mustAppend.Add((from UncannyMazeTile u in map where u.Character == character select u).ToArray()[0]);
            }
        }
        centerMazeSum %= 10;
        mustAppend.Add(goal);
        if (logging)
        {
            Log("Your starting tile is: " + start);
            Log("Your goal tile is: " + goal);
            Log("You must append: " + string.Join(", ", mustAppend.Select(u => "" + u.LetterCoord + u.NumberCoord).ToArray()));
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
            default:
                throw new InvalidOperationException("erm what the sigma (dimensions)");
        }
        goalBox.transform.localScale = new Vector3(7f / dims, 1, 7f / dims);
        goalBox.transform.localPosition = new Vector3((m * xGoal) - b, -.01f, (-m * yGoal) + b);
        if (logging)
        {
            Log("Your sum of maze modulo 10 is: " + totalMazeTotal);
            Log("The sum of the left border is: " + leftSum);
            Log("The sum of the right border is: " + rightSum);
            Log("The sum of the top border is: " + aboveSum);
            Log("The sum of the bottom border is: " + belowSum);
        }
        StartCoroutine(Moving("reset", 2));
    }

    private float MovementFunction(float x)
    {
        return (x * -2 / dims) - (-2 / dims);
    }

    private string Sum4x4()
    {
        if (dims != 4)
        {
            return null;
        }
        string startingQuadrant, goalQuadrant;
        startingQuadrant = start == map[0, 0] || start == map[0, 1] || start == map[1, 0] || start == map[1, 1]
            ? "1"
            : start == map[0, 2] || start == map[0, 3] || start == map[1, 2] || start == map[1, 3]
                ? "2"
                : start == map[2, 0] || start == map[2, 1] || start == map[3, 0] || start == map[3, 1] ? "3" : "4";
        goalQuadrant = goal == map[0, 0] || goal == map[0, 1] || goal == map[1, 0] || goal == map[1, 1]
            ? "1"
            : goal == map[0, 2] || start == map[0, 3] || start == map[1, 2] || start == map[1, 3]
                ? "2"
                : goal == map[2, 0] || goal == map[2, 1] || goal == map[3, 0] || goal == map[3, 1] ? "3" : "4";
        string firstQuadrant = Convert.ToString(ushort.Parse("" + map[0, 0].UncannyValue + map[0, 1].UncannyValue + map[1, 0].UncannyValue + map[1, 1].UncannyValue), 2);
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
        string secondQuadrant = Convert.ToString(ushort.Parse("" + map[0, 2].UncannyValue + map[0, 3].UncannyValue + map[1, 2].UncannyValue + map[1, 3].UncannyValue), 2);
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
        string thirdQuadrant = Convert.ToString(ushort.Parse("" + map[2, 0].UncannyValue + map[2, 1].UncannyValue + map[3, 0].UncannyValue + map[3, 1].UncannyValue), 2);
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
        string fourthQuadrant = Convert.ToString(ushort.Parse("" + map[2, 2].UncannyValue + map[2, 3].UncannyValue + map[3, 2].UncannyValue + map[3, 3].UncannyValue), 2);
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
        {
            Log("The binary sequence is " + firstQuadrant + secondQuadrant + thirdQuadrant + fourthQuadrant + ".");
        }
        return Convert.ToUInt64(firstQuadrant + secondQuadrant + thirdQuadrant + fourthQuadrant, 2).ToString();
    }

    private string Sum6x6()
    {
        if (dims != 6)
        {
            return null;
        }
        int firstQuadrant = 1;
        int secondQuadrant = 1;
        int thirdQuadrant = 1;
        int fourthQuadrant = 1;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (map[i, j].UncannyValue != 0)
                {
                    firstQuadrant *= map[i, j].UncannyValue;
                }
            }
        }
        for (int i = 0; i < 3; i++)
        {
            for (int j = 3; j < 6; j++)
            {
                if (map[i, j].UncannyValue != 0)
                {
                    secondQuadrant *= map[i, j].UncannyValue;
                }
            }
        }
        for (int i = 3; i < 6; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (map[i, j].UncannyValue != 0)
                {
                    thirdQuadrant *= map[i, j].UncannyValue;
                }
            }
        }
        for (int i = 3; i < 6; i++)
        {
            for (int j = 3; j < 6; j++)
            {
                if (map[i, j].UncannyValue != 0)
                {
                    fourthQuadrant *= map[i, j].UncannyValue;
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
        if (viewingWholeMaze || (currentlyMoving && logging)
        || (xCoords == 0 && direction == "left")
        || (xCoords == dims - 1 && direction == "right")
        || (yCoords == dims - 1 && direction == "up")
        || (yCoords == 0 && direction == "down"))
        {
            yield break;
        }
        if (direction != "reset" && direction != "append" && !current.ValidDirections.Contains(direction))
        {
            if (logging)
            {
                Strike("Tried to move " + direction + ", not allowed.");
            }
            yield break;
        }
        switch (direction)
        {
            case "up":
                if (currentPosition.y + .01f >= ((dims - 1f) / dims))
                {
                    yield break;
                }
                else
                {
                    currentlyMoving = true;
                    currentPosition.y += 1f / (n * dims); // Difference between the integral of movement and the Riemann sum
                    for (int i = n - 1; i > 0; i--)
                    {
                        currentPosition.y -= MovementFunction(i * 1f / n) / n; // Riemann sum
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
                        yield return null; // necessary for the animation to play
                    }
                    currentlyMoving = false;
                }
                break;
            case "down":
                if (currentPosition.y <= .01f)
                {
                    yield break;
                }
                else
                {
                    currentlyMoving = true;
                    currentPosition.y -= 1f / (n * dims);
                    for (int i = n - 1; i > 0; i--)
                    {
                        currentPosition.y += MovementFunction(i * 1f / n) / n;
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
                        yield return null;
                    }
                    currentlyMoving = false;
                }
                break;
            case "left":
                if (currentPosition.x <= .01f)
                {
                    yield break;
                }
                else
                {
                    currentlyMoving = true;
                    currentPosition.x -= 1f / (n * dims);
                    for (int i = n - 1; i > 0; i--)
                    {
                        currentPosition.x += MovementFunction(i * 1f / n) / n;
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset = currentPosition;
                        yield return null;
                    }
                    currentlyMoving = false;
                }
                break;
            case "right":
                if (currentPosition.x + .01f >= ((dims - 1f) / dims))
                {
                    yield break;
                }
                else
                {
                    currentlyMoving = true;
                    currentPosition.x += 1f / (n * dims);
                    for (int i = n - 1; i > 0; i--)
                    {
                        currentPosition.x -= MovementFunction(i * 1f / n) / n;
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
            default:
                break;
        }
        currentPosition = maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
        xCoords = (int)((currentPosition.x * dims) + .01f);
        yCoords = (int)((currentPosition.y * dims) + .01f);
        current = map[dims - yCoords - 1, xCoords];
        currentBox.transform.localPosition = new Vector3((m * xCoords) - b, -.01f, (-m * yCoords) + b);
        numbers.GetComponent<TextMesh>().text = output;
        if (((dims - yCoords - 1) * 2 * dims) + (xCoords * 2) + 1 > ((dims - yGoal - 1) * 2 * dims) + (xGoal * 2) + 1)
        {
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert(((dims - yCoords - 1) * 2 * dims) + (xCoords * 2) + 1, "</color>");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert(((dims - yCoords - 1) * 2 * dims) + (xCoords * 2), "<color=\"blue\">");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert(((dims - yGoal - 1) * 2 * dims) + (xGoal * 2) + 1, "</color>");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert(((dims - yGoal - 1) * 2 * dims) + (xGoal * 2), "<color=\"red\">");
        }
        else if (((dims - yCoords - 1) * 2 * dims) + (xCoords * 2) + 1 < ((dims - yGoal - 1) * 2 * dims) + (xGoal * 2) + 1)
        {
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert(((dims - yGoal - 1) * 2 * dims) + (xGoal * 2) + 1, "</color>");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert(((dims - yGoal - 1) * 2 * dims) + (xGoal * 2), "<color=\"red\">");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert(((dims - yCoords - 1) * 2 * dims) + (xCoords * 2) + 1, "</color>");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert(((dims - yCoords - 1) * 2 * dims) + (xCoords * 2), "<color=\"blue\">");
        }
        else
        {
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert(((dims - yGoal - 1) * 2 * dims) + (xGoal * 2) + 1, "</color>");
            numbers.GetComponent<TextMesh>().text = numbers.GetComponent<TextMesh>().text.Insert(((dims - yGoal - 1) * 2 * dims) + (xGoal * 2), "<color=\"purple\">");

        }
        if (logging)
        {
            Log("---"); // makes the log a bit easier to read
        }

        if (direction == "reset")
        {
            sequence.Clear();
        }
        else if (direction == "append")
        {
            if (current == mustAppend[sequence.Count])
            {
                Log("Appended " + current.LetterCoord + current.NumberCoord + ".");
                sequence.Add(current);
            }
            else
            {
                Strike("Tried to append " + current.LetterCoord + current.NumberCoord + ", was supposed to append " + mustAppend[sequence.Count].LetterCoord + mustAppend[sequence.Count].NumberCoord + ".");
            }
            if (current == goal && sequence.SequenceEqual(mustAppend) && logging)
            {
                Solve("Your module is: solved module");
                yield break;
            }
        }
        else
        {
            if (logging)
            {
                Log("Pressed " + direction + ", going to " + current.ToString() + ".");
            }
        }
        coordsText.GetComponent<TextMesh>().text = "CURRENT: " + current.LetterCoord + current.NumberCoord + "\nGOAL: " + goal.LetterCoord + goal.NumberCoord;
        if (logging && direction != "append")
        {
            switch (current.MazeType)
            {
                case UncannyMazeTile.MazeTypes.GOAL:
                    Log("Your maze is: Goal Maze (goal is " + goal.UncannyValue + ")");
                    break;
                case UncannyMazeTile.MazeTypes.CENTER:
                    Log("Your maze is: Center Maze (center sum is " + centerMazeSum + ")");
                    break;
                case UncannyMazeTile.MazeTypes.TOTAL:
                    Log("Your maze is: Total Maze (total is " + totalMazeTotal + ")");
                    break;
                case UncannyMazeTile.MazeTypes.CORNERS:
                    Log("Your maze is: Corners Maze (corners sum is " + cornersMazeSum + ")");
                    break;
                case UncannyMazeTile.MazeTypes.BORDER:
                    Log("Your maze is: Border Maze");
                    break;
                default:
                    throw new InvalidOperationException("erm what the sigma (enum) (this should NEVER happen; if it somehow does, ping or dm @objectscountries on discord)");

            }
            Log("Possible directions are: " + string.Join(", ", current.ValidDirections));
        }
    }

    private string[] DirectionsAvailable(UncannyMazeTile tile, Dictionary<string, UncannyMazeTile> dict)
    {
        List<UncannyMazeTile> temp;
        switch (tile.MazeType)
        {
            case UncannyMazeTile.MazeTypes.GOAL:
                temp = ClosestAndFurthestInValue(goal.UncannyValue, false, dict["left"], dict["right"], dict["up"], dict["down"]);
                break;
            case UncannyMazeTile.MazeTypes.CENTER:
                temp = ClosestAndFurthestInValue(centerMazeSum, false, dict["left"], dict["right"], dict["up"], dict["down"]);
                break;
            case UncannyMazeTile.MazeTypes.TOTAL:
                temp = ClosestAndFurthestInValue(totalMazeTotal, false, dict["left"], dict["right"], dict["up"], dict["down"]);
                break;
            case UncannyMazeTile.MazeTypes.CORNERS:
                temp = ClosestAndFurthestInValue(cornersMazeSum, false, dict["left"], dict["right"], dict["up"], dict["down"]);
                break;
            case UncannyMazeTile.MazeTypes.BORDER:
                temp = ClosestAndFurthestInValue(0, true, dict["left"], dict["right"], dict["up"], dict["down"]);
                break;
            default:
                throw new InvalidOperationException("erm what the sigma (enum) (this should NEVER happen; if it somehow does, ping or dm @objectscountries on discord)");
        }
        return dict.Where(x => x.Value != null && temp.Contains(x.Value)).Select(x => x.Key).Distinct().ToArray();
    }

    private List<UncannyMazeTile> ClosestAndFurthestInValue(int compare, bool border, params UncannyMazeTile[] adjacent)
    {
        List<UncannyMazeTile> result = new List<UncannyMazeTile>();
        if (border)
        {
            for (int i = 0; i < 4; i++)
            {
                if (borderSums[i] == borderSums.Max() || borderSums[i] == borderSums.Min())
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
                differences.Add(tile == null ? -1 : Math.Abs(tile.UncannyValue - compare));
            }
            for (int i = 0; i < differences.Count; i++)
            {
                if (differences[i] != -1 && (differences[i] == differences.Where(n => n != -1).Min() || differences[i] == differences.Where(n => n != -1).Max()))
                {
                    result.Add(adjacent[i]);
                }
            }
        }
        return result;
    }

    private IEnumerator GeneratingMazeIdle()
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
                gm.GetComponent<TextMesh>().text = "SORRY THE MAZE\nTOOK SO LONG TO\nLOAD. PRESS ANY\nOF THE THREE WHITE\nBUTTONS TO\nSOLVE IMMEDIATELY.";
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
