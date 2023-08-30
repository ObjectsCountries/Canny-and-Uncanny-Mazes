using Wawa.Modules;
using Wawa.Extensions;
using Wawa.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using KModkit;

public class _cannymaze:ModdedModule{
    public KMSelectable arrowleft,arrowright,arrowup,arrowdown,maze,numbersButton,resetButton;
    internal bool viewingWholeMaze=false;
    private Vector2 currentPosition,startingPosition;
    private string coordLetters="ABCDEFGH";
    private string currentCoords,leftTile,rightTile,aboveTile,belowTile,goalCoords,startingCoords;
    private int xcoords,ycoords,xGoal,yGoal;
    private int dims;
    private int[,]textures;
    private bool currentlyMoving=false;
    private bool canMoveInThatDirection=true;
    public TextureGenerator t;
    private bool mazeGenerated=false;
    private bool generatingMazeIdleCurrentlyRunning=false;
    private bool tookTooLong=false;
    internal int animSpeed;
    private Config<cannymazesettings> cmSettings;
    public GameObject numbers,gm,currentBox,goalBox,anchor;
    ///<value>The different types of mazes that the module can have. The last five are exclusive to ruleseeds other than 1.</value>
    private string[]mazeNames=new string[]{"Sum","Compare","Tiles","Binary","Avoid","Strict","Walls","Average","Digital","Fours","Movement","Double Binary"};
    private List<string> j;
    private int startingTile,currentTile;
    private int movementsMade=0;
    private List<string> tilesTraversed;
    private List<string> allDirs;
    private List<string> correctPath;
    private float m,b;
    private int[][]anyOneBit=new int[][]{
            new int[]{3,5},
            new int[]{3,6},
            new int[]{1,2,7},
            new int[]{5,6},
            new int[]{1,4,7},
            new int[]{2,4,7},
            new int[]{3,5,6}
    };
    private int[][]anyTwoBits=new int[][]{
            new int[]{2,4,7},
            new int[]{1,4,7},
            new int[]{5,6},
            new int[]{1,2,7},
            new int[]{3,6},
            new int[]{3,5},
            new int[]{1,2,4}
    };
    [Serializable]
    public sealed class cannymazesettings{
        public int animationSpeed=30;
    }

    public static Dictionary<string,object>[]TweaksEditorSettings=new Dictionary<string,object>[]{
        new Dictionary<string,object>{
            {"Filename","cannymaze-settings.json"},
            {"Name","Canny Maze"},
            {"Listings",new List<Dictionary<string,object>>{
                new Dictionary<string,object>{
                    {"Key","animationSpeed"},
                    {"Text","Animation Speed"},
                    {"Description","Set the speed of the module's moving animation in frames.\nShould be from 10 to 60."}
                }
            }}
        }
    };

	void Start(){
        j=new List<string>();
        tilesTraversed=new List<string>();
        allDirs=new List<string>(){"left","right","up","down"};
        correctPath=new List<string>();
        cmSettings=new Config<cannymazesettings>();
        animSpeed=Mathf.Clamp(cmSettings.Read().animationSpeed,10,60);
        cmSettings.Write("{\"animationSpeed\":"+animSpeed+"}");
        numbers.SetActive(false);
        StartCoroutine(Initialization());
        arrowleft.Set(onInteract:()=>{
            if(mazeGenerated&&!Status.IsSolved)
                StartCoroutine(Moving("left",animSpeed));
            Shake(arrowleft,1,Sound.BigButtonPress);
            });
        arrowright.Set(onInteract:()=>{
            if(mazeGenerated&&!Status.IsSolved)
                StartCoroutine(Moving("right",animSpeed));
            Shake(arrowright,1,Sound.BigButtonPress);
            });
        arrowup.Set(onInteract:()=>{
            if(mazeGenerated&&!Status.IsSolved)
                StartCoroutine(Moving("up",animSpeed));
            Shake(arrowup,1,Sound.BigButtonPress);
            });
        arrowdown.Set(onInteract:()=>{
            if(mazeGenerated&&!Status.IsSolved)
                StartCoroutine(Moving("down",animSpeed));
            Shake(arrowdown,1,Sound.BigButtonPress);
            });
        resetButton.Set(onInteract:()=>{
            if(tookTooLong)
                Solve("Solved by pressing the Reset button after generation took too long.");
            else if(mazeGenerated&&!Status.IsSolved){
                Log("Reset the maze.");
                StartCoroutine(Moving("reset",2));
            }
            Shake(resetButton,1,Sound.BigButtonPress);
        });
        maze.Set(onInteract:()=>{
        if(mazeGenerated&&!currentlyMoving&&!Status.IsSolved){
            if(viewingWholeMaze){
                if(numbers.activeInHierarchy){
                    numbers.SetActive(false);
                    t.changeTexture(t.finalTexture);
                }
                currentBox.SetActive(false);
                goalBox.SetActive(false);
                maze.GetComponent<MeshRenderer>().material.mainTextureScale=new Vector2(1f/dims,1f/dims);
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
            }else{
                currentBox.SetActive(true);
                goalBox.SetActive(true);
                maze.GetComponent<MeshRenderer>().material.mainTextureScale=Vector2.one;
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=Vector2.zero;
            }
            viewingWholeMaze=!viewingWholeMaze;
        }
        });
        numbersButton.Set(onInteract:()=>{
            if(tookTooLong)
                Solve("Solved by pressing the Numbers button after generation took too long.");
            if(mazeGenerated&&viewingWholeMaze&&!Status.IsSolved){
                if(!numbers.activeInHierarchy){
                    numbers.SetActive(true);
                    t.changeTexture(t.whiteBG);
                    currentBox.SetActive(false);
                    goalBox.SetActive(false);
                }else{
                    numbers.SetActive(false);
                    t.changeTexture(t.finalTexture);
                    currentBox.SetActive(true);
                    goalBox.SetActive(true);
                }
            }
            Shake(numbersButton,1,Sound.BigButtonPress);
        });
    }

    private float f(float x){
        return (x*-2/dims)-(-2/dims);
    }

    private IEnumerator Initialization(){
        t.changeTexture(t.whiteBG);
        dims=t.gridDimensions;
        textures=t.textureIndices;
        currentPosition=new Vector2((float)UnityEngine.Random.Range(0,dims)/dims,(float)UnityEngine.Random.Range(0,dims)/dims);
        startingPosition=currentPosition;
        maze.GetComponent<MeshRenderer>().material.mainTextureScale=new Vector2(1f/dims,1f/dims);
        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
        xcoords=(int)(currentPosition.x*dims+.01f);
        ycoords=(int)(currentPosition.y*dims+.01f);
        currentCoords=coordLetters[xcoords].ToString()+(dims-ycoords);
        startingCoords=currentCoords;
        if(tookTooLong)
            yield break;
        if(!generatingMazeIdleCurrentlyRunning)
            StartCoroutine(generatingMazeIdle());
        int movements=0;
        int attempts=0;
        startingTile=textures[(dims-ycoords-1),xcoords];
        do{
            xGoal=UnityEngine.Random.Range(0,dims);
            yGoal=UnityEngine.Random.Range(0,dims);
            goalCoords=coordLetters[xGoal].ToString()+(dims-yGoal);
        }while(goalCoords==currentCoords);
        int index;
        currentTile=textures[(dims-ycoords-1),xcoords];
        j=mazeType(false,false);
        while(currentCoords!=goalCoords&&attempts<3){
            if(j.Count==0)
                break;
            index=UnityEngine.Random.Range(0,j.Count);
            correctPath.Add(j.ElementAt(index));
            yield return StartCoroutine(Moving(j.ElementAt(index),2,false,false));
            if(mazeNames[startingTile]!="Movement"&&
              (correctPath.Count>2&&(
              (correctPath.ElementAt(correctPath.Count-1)=="left" &&correctPath.ElementAt(correctPath.Count-2)=="right")
            ||(correctPath.ElementAt(correctPath.Count-1)=="right"&&correctPath.ElementAt(correctPath.Count-2)=="left")
            ||(correctPath.ElementAt(correctPath.Count-1)=="up"   &&correctPath.ElementAt(correctPath.Count-2)=="down")
            ||(correctPath.ElementAt(correctPath.Count-1)=="down" &&correctPath.ElementAt(correctPath.Count-2)=="up")
            ))){
                correctPath.RemoveAt(correctPath.Count-1);
                correctPath.RemoveAt(correctPath.Count-1);
            }
            movements++;
            if(movements>=20){
                yield return StartCoroutine(Moving("reset",2,false));
                movements=0;
                correctPath.Clear();
                attempts++;
            }
        }
        if(attempts!=3&&j.Count!=0){
            string output="";
            for(int r=0;r<dims;r++){
                for(int c=0;c<dims;c++){
                    output+=textures[r,c];
                    if(c!=dims-1)output+=" ";
                }
                if(r!=dims-1)output+="\n";
            }
            Log("Your layout is:\n"+output);
            switch(dims){
                case 5:
                    numbers.GetComponent<TextMesh>().fontSize=30;
                    m=.2f;
                    b=1.1f;
                    anchor.transform.localPosition=new Vector3(.7f,.7f,0);
                    break;
                case 6:
                    numbers.GetComponent<TextMesh>().fontSize=25;
                    m=.167429f;
                    b=1.08143f;
                    anchor.transform.localPosition=new Vector3(2/3f,2/3f,0);
                    break;
                case 7:
                    numbers.GetComponent<TextMesh>().fontSize=22;
                    m=.1405f;
                    b=1.07f;
                    anchor.transform.localPosition=new Vector3(.65f,.65f,0);
                    break;
                case 8:
                    numbers.GetComponent<TextMesh>().fontSize=19;
                    m=.125f;
                    b=1.065f;
                    anchor.transform.localPosition=new Vector3(.625f,.625f,0);
                    break;
            }
            numbers.GetComponent<TextMesh>().text=output;
            currentCoords=startingCoords;
            currentBox.transform.localScale=new Vector3(7f/dims,1,7f/dims);
            goalBox.transform.localScale=new Vector3(7f/dims,1,7f/dims);
            goalBox.transform.localPosition=new Vector3(m*xGoal-b,-.01f,-m*yGoal+b);
            yield return StartCoroutine(Moving("reset",2,false));
            Log("Your maze is: "+(mazeNames[(textures[(dims-ycoords-1),xcoords])-1])+" Maze");
            Log("Your current coordinates are: "+currentCoords);
            Log("Your goal is: "+goalCoords);
            Log("A possible route is: "+String.Join(", ", correctPath.ToArray()));
            yield return StartCoroutine(Moving("reset",2,true));
            mazeGenerated=true;
            gm.SetActive(false);
            t.changeTexture(t.finalTexture);
            maze.GetComponent<MeshRenderer>().material.mainTextureScale=new Vector2(1f/dims,1f/dims);
            maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
            yield break;
        }
        else{
            t.Awake();
            StartCoroutine(Initialization());
            yield break;
        }
    }

    private IEnumerator Moving(string direction,int n,bool logging=true,bool includeBacktracking=true){
        if(viewingWholeMaze||currentlyMoving
        ||(xcoords==0     &&direction=="left")
        ||(xcoords==dims-1&&direction=="right")
        ||(ycoords==dims-1&&direction=="up")
        ||(ycoords==0     &&direction=="down"))
            yield break;
        if(direction!="reset"){
            canMoveInThatDirection = j.Contains(direction) || tilesTraversed.Contains(tileinDirection(direction));
            if(!canMoveInThatDirection){
                Strike("Tried to move "+direction+", not allowed.");
                yield break;
            }
        }
        switch(direction){
            case"up":
                if(currentPosition.y+.01f>=((dims-1f)/dims))
                    yield break;
                else{
                    currentlyMoving=true;
                    currentPosition+=new Vector2(0,1f/(n*dims));//Difference between the integral of movement and the Riemann sum
                    for(int i=n-1;i>0;i--){
                        currentPosition-=new Vector2(0,f(i*1f/n)*1f/n);//Riemann sum
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
                        yield return null;//necessary for the animation to play
                    }
                    currentlyMoving=false;
                }
                break;
            case"down":
                if(currentPosition.y<=.01f)
                    yield break;
                else{
                    currentlyMoving=true;
                    currentPosition-=new Vector2(0,1f/(n*dims));
                    for(int i=n-1;i>0;i--){
                        currentPosition+=new Vector2(0,f(i*1f/n)*1f/n);
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
                        yield return null;
                    }
                    currentlyMoving=false;
                }
                break;
            case"left":
                if(currentPosition.x<=.01f)
                    yield break;
                else{
                    currentlyMoving=true;
                    currentPosition-=new Vector2(1f/(n*dims),0);
                    for(int i=n-1;i>0;i--){
                        currentPosition+=new Vector2(f(i*1f/n)*1f/n,0);
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
                        yield return null;
                    }
                    currentlyMoving=false;
                }
                break;
            case"right":
                if(currentPosition.x+.01f>=((dims-1f)/dims))
                    yield break;
                else{
                    currentlyMoving=true;
                    currentPosition+=new Vector2(1f/(n*dims),0);
                    for(int i=n-1;i>0;i--){
                        currentPosition-=new Vector2(f(i*1f/n)*1f/n,0);
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
                        yield return null;
                    }
                    currentlyMoving=false;
                }
                break;
            case"reset":
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=startingPosition;
                break;
            
        }
        currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
        xcoords=(int)(currentPosition.x*dims+.01f);
        ycoords=(int)(currentPosition.y*dims+.01f);
        currentCoords=coordLetters[xcoords].ToString()+(dims-ycoords);
        currentTile=textures[(dims-ycoords-1),xcoords];
        if(!tilesTraversed.Contains(currentCoords))
            tilesTraversed.Add(currentCoords);
        currentBox.transform.localPosition=new Vector3(m*xcoords-b,-.01f,-m*ycoords+b);
        
        if(xcoords!=0)
            leftTile=coordLetters[xcoords-1].ToString()+(dims-ycoords);
        else leftTile="";

        if(xcoords!=dims-1)
            rightTile=coordLetters[xcoords+1].ToString()+(dims-ycoords);
        else rightTile="";

        if(ycoords!=dims-1)
            aboveTile=coordLetters[xcoords].ToString()+(dims-ycoords-1);
        else aboveTile="";

        if(ycoords!=0)
            belowTile=coordLetters[xcoords].ToString()+(dims-ycoords+1);
        else belowTile="";

        if(logging)
            Log("---");//makes the log a bit easier to read
        if(direction=="reset"){
            tilesTraversed.Clear();
            tilesTraversed.Add(currentCoords);
            movementsMade=0;
        }
        else{
            if(logging)
                Log("Pressed "+direction+", going to "+currentCoords+".");
        }
        if(currentCoords==goalCoords&&logging&&includeBacktracking){
            Solve("Your module is: solved module");
            yield break;
        }
        movementsMade++;
        j=mazeType(logging,includeBacktracking);
        if(logging){
            Log("Your current tile is: "+textures[(dims-ycoords-1),xcoords]);
            Log("Possible directions are: "+String.Join(", ", j.ToArray()));
        }
    }

    ///<summary>A method to determine which type of maze is to be used.</summary>
    ///<returns>A <c>List&lt;string&gt;</c> containing the directions in which the<br/>defuser is allowed to move. This will contain at least one of<br/>any of the following: left, right, up, down.</returns>
    ///<remarks>Note: Incomplete, <c>switch</c> will be inaccurate until all maze<br/>types have been implemented.</remarks>
    private List<string> mazeType(bool logging=true,bool includeBacktracking=true){
        List<string> temp=new List<string>();
        switch(startingTile){
            case 1:
                temp=sumDigitalAverageMaze(logging);
                break;
            case 2:
                temp=compareMaze();
                break;
            case 3:
                temp=tilesMovementMaze(logging);
                break;
            case 4:
                temp=binaryMaze();
                break;
            case 5:
                temp=avoidMaze();
                break;
            case 6:
            case 7:
                temp=strictMaze();
                break;
        }
        if(includeBacktracking){
            foreach(string s in allDirs){
                if(tilesTraversed.Contains(tileinDirection(s))&&!temp.Contains(s))
                    temp.Add(s);
            }
        }
        return temp;
    }

    private string tileinDirection(string direction){
        switch(direction){
            case "left":
                return leftTile;
            case "right":
                return rightTile;
            case "up":
                return aboveTile;
            case "down":
                return belowTile;
            default:
                return "";
        }
    }
    private List<string> sumDigitalAverageMaze(bool logging){
        int sum=0;
        Dictionary<string,int> adjacentTiles=new Dictionary<string,int>();
        if(xcoords!=0){
            adjacentTiles.Add("left",textures[(dims-ycoords-1),xcoords-1]);
            sum+=textures[(dims-ycoords-1),xcoords-1];
        }
        if(xcoords!=dims-1){
            adjacentTiles.Add("right",textures[(dims-ycoords-1),xcoords+1]);
            sum+=textures[(dims-ycoords-1),xcoords+1];
        }
        if(ycoords!=dims-1){
            adjacentTiles.Add("up",textures[(dims-ycoords-2),xcoords]);
            sum+=textures[(dims-ycoords-2),xcoords];
        }
        if(ycoords!=0){
            adjacentTiles.Add("down",textures[(dims-ycoords),xcoords]);
            sum+=textures[(dims-ycoords),xcoords];
        }
        int modulo=0;
        int average=0;
        int digital=0;
        if(mazeNames[startingTile-1]=="Sum"){
            modulo=sum%7+1;
            if(logging)
                Log("The sum of all orthogonally-adjacent tiles is "+sum+". Modulo 7 and adding 1, this is "+modulo+".");
        }
        if(mazeNames[startingTile-1]=="Average"){
            average=(int)((sum/adjacentTiles.Count)+.5f);
            modulo=average%7+1;
            if(logging)
                Log("The average of the sum of all orthogonally-adjacent tiles is "+average+". Modulo 7 and adding 1, this is "+modulo+".");
        }
        if(mazeNames[startingTile-1]=="Digital"){
            digital=(sum/10)+(sum%10);
            while(digital>9)
                digital=(digital/10)+(digital%10);
            modulo=digital%7+1;
            if(logging)
                Log("The digital root of the sum of all orthogonally-adjacent tiles is "+digital+". Modulo 7 and adding 1, this is "+modulo+".");
        }
        for(int i=0;i<adjacentTiles.Count;i++){
            adjacentTiles[adjacentTiles.ElementAt(i).Key]-=modulo;
            if(adjacentTiles[adjacentTiles.ElementAt(i).Key]<0)
                adjacentTiles[adjacentTiles.ElementAt(i).Key]*=-1;//turn the tile numbers into absolute value of distance from modulo
        }
        List<string> possibleDirections=new List<string>();
        int min=adjacentTiles.Aggregate((l,r)=>l.Value<r.Value?l:r).Value;//find the minimum distance
        for(int i=0;i<7;i++){
            if(possibleDirections.Count==0){
                foreach(KeyValuePair<string,int> tile in adjacentTiles){
                    if(!tilesTraversed.Contains(tileinDirection(tile.Key))&&(tile.Value==min+i||tile.Value==min-i))
                        possibleDirections.Add(tile.Key);//get all tile directions that are this distance or the nearest away from modulo
                }
            }
        }
        return possibleDirections;
    }

    private List<string> compareMaze(){
        int horizSum=0;
        int vertSum=0;
        List<string>horizDirs=new List<string>();
        List<string>vertDirs=new List<string>();
        if(xcoords!=0){
            horizSum+=textures[(dims-ycoords-1),xcoords-1];
            horizDirs.Add("left");
        }
        if(xcoords!=dims-1){
            horizSum+=textures[(dims-ycoords-1),xcoords+1];
            horizDirs.Add("right");
        }
        if(ycoords!=dims-1){
            vertSum+=textures[(dims-ycoords-2),xcoords];
            vertDirs.Add("up");
        }
        if(ycoords!=0){
            vertSum+=textures[(dims-ycoords),xcoords];
            vertDirs.Add("down");
        }
        if(horizSum>vertSum)
            return horizDirs;
        if(horizSum<vertSum)
            return vertDirs;
        horizDirs.AddRange(vertDirs);
        return horizDirs;
    }

    private List<string> tilesMovementMaze(bool logging){
        string distinct="";
        int modulo;
        int total=movementsMade;
        if(mazeNames[startingTile-1]=="Tiles"){
            distinct="distinct ";
            total=tilesTraversed.Count;
        }
        modulo=((currentTile*total)%7)+1;
        if(logging)
            Log("The total number of "+distinct+"tiles traversed so far is "+total+", and multiplying by the current tile yields "+(currentTile*total)+". Modulo 7 and adding 1, this is "+modulo+".");
        Dictionary<string,int> adjacentTiles=new Dictionary<string,int>();
        if(xcoords!=0)
            adjacentTiles.Add("left",textures[(dims-ycoords-1),xcoords-1]);
        if(xcoords!=dims-1)
            adjacentTiles.Add("right",textures[(dims-ycoords-1),xcoords+1]);
        if(ycoords!=dims-1)
            adjacentTiles.Add("up",textures[(dims-ycoords-2),xcoords]);
        if(ycoords!=0)
            adjacentTiles.Add("down",textures[(dims-ycoords),xcoords]);
        for(int i=0;i<adjacentTiles.Count;i++){
            adjacentTiles[adjacentTiles.ElementAt(i).Key]-=modulo;
            if(adjacentTiles[adjacentTiles.ElementAt(i).Key]<0)
                adjacentTiles[adjacentTiles.ElementAt(i).Key]*=-1;//turn the tile numbers into absolute value of distance from modulo
        }
        List<string> possibleDirections=new List<string>();
        int min=adjacentTiles.Aggregate((l,r)=>l.Value<r.Value?l:r).Value;//find the minimum distance
        for(int i=0;i<7;i++){
            if(possibleDirections.Count==0){
                foreach(KeyValuePair<string,int> tile in adjacentTiles){
                    if(!tilesTraversed.Contains(tileinDirection(tile.Key))&&(tile.Value==min+i||tile.Value==min-i))
                        possibleDirections.Add(tile.Key);//get all tile directions that are this distance or the nearest away from modulo
                }
            }
        }
        return possibleDirections;
    }

    private List<string> binaryMaze(){
        List<string> dirs=new List<string>();
        int[][]bit;
        switch(mazeNames[startingTile-1]){
            case "Fours":
                return foursMaze();
            case "Double Binary":
                bit=anyTwoBits;
                break;
            case "Binary":
            default:
                bit=anyOneBit;
                break;
        }
        if(xcoords!=0&&
           Array.Exists(bit[currentTile-1],
           t=>t==textures[(dims-ycoords-1),xcoords-1]))
            dirs.Add("left");

        if(xcoords!=dims-1&&
           Array.Exists(bit[currentTile-1],
           t=>t==textures[(dims-ycoords-1),xcoords+1]))
            dirs.Add("right");

        if(ycoords!=dims-1&&
           Array.Exists(bit[currentTile-1],
           t=>t==textures[(dims-ycoords-2),xcoords]))
            dirs.Add("up");

        if(ycoords!=0&&
           Array.Exists(bit[currentTile-1],
           t=>t==textures[(dims-ycoords),xcoords]))
            dirs.Add("down");

        return dirs;
    }

    private List<string> avoidMaze(){
        List<string> dirs=new List<string>();
        if(xcoords!=0
           &&textures[(dims-ycoords-1),xcoords-1]!=1
           &&textures[(dims-ycoords-1),xcoords-1]!=2)
            dirs.Add("left");

        if(xcoords!=dims-1
           &&textures[(dims-ycoords-1),xcoords+1]!=3
           &&textures[(dims-ycoords-1),xcoords+1]!=4)
            dirs.Add("right");

        if(ycoords!=dims-1
           &&textures[(dims-ycoords-2),xcoords]!=5
           &&textures[(dims-ycoords-2),xcoords]!=6)
            dirs.Add("up");

        if(ycoords!=0
           &&textures[(dims-ycoords),xcoords]!=7)
            dirs.Add("down");

        return dirs;
    }

    private List<string> foursMaze(){
        Dictionary<string,int> adjacentTiles=new Dictionary<string,int>();
        if(xcoords!=0)
            adjacentTiles.Add("left",textures[(dims-ycoords-1),xcoords-1]%4);
        if(xcoords!=dims-1)
            adjacentTiles.Add("right",textures[(dims-ycoords-1),xcoords+1]%4);
        if(ycoords!=dims-1)
            adjacentTiles.Add("up",textures[(dims-ycoords-2),xcoords]%4);
        if(ycoords!=0)
            adjacentTiles.Add("down",textures[(dims-ycoords),xcoords]%4);
        List<string> possibleDirections=new List<string>();
        int mostCommon=adjacentTiles.Select(d => d.Value)
                             .GroupBy(k => k)
                             .OrderByDescending(k => k.Count())
                             .First()
                             .Key;
        foreach(KeyValuePair<string,int> tile in adjacentTiles){
            if(!tilesTraversed.Contains(tileinDirection(tile.Key))&&tile.Value==mostCommon)
                possibleDirections.Add(tile.Key);//get all tile directions that are this distance or the nearest away from modulo
        }
        return possibleDirections;
    }

    private List<string> strictMaze(){
        List<string> dirs=new List<string>();
        if(xcoords!=0
           &&textures[(dims-ycoords-1),xcoords-1]<currentTile)
            dirs.Add("left");

        if(xcoords!=dims-1
           &&textures[(dims-ycoords-1),xcoords+1]>currentTile)
            dirs.Add("right");

        if(ycoords!=dims-1
           &&((textures[(dims-ycoords-2),xcoords]%2!=0&&currentTile%2==0)
           || (textures[(dims-ycoords-2),xcoords]%2==0&&currentTile%2!=0)))
            dirs.Add("up");

        if(ycoords!=0
           &&((textures[(dims-ycoords),xcoords]%2!=0&&currentTile%2==0)
           || (textures[(dims-ycoords),xcoords]%2==0&&currentTile%2!=0)))
            dirs.Add("down");

        return dirs;
    }

    private List<string>wallsMaze(){
        return allDirs;//placeholder until i learn to make the Walls Maze
    }

    private IEnumerator generatingMazeIdle(){
        generatingMazeIdleCurrentlyRunning=true;
        gm.GetComponent<TextMesh>().fontSize=45;
        string gen="GENERATING\nMAZE.";
        int totaltime=0;
        while(!mazeGenerated){
            gm.GetComponent<TextMesh>().text=gen;
            yield return new WaitForSeconds(.75f);
            if(totaltime==4&&!mazeGenerated){
                gm.GetComponent<TextMesh>().fontSize=27;
                gm.GetComponent<TextMesh>().text="SORRY THE MODULE\nTOOK SO LONG TO\nLOAD. PRESS EITHER\nOF THE TWO RED\nBUTTONS BELOW TO\nSOLVE IMMEDIATELY.";
                tookTooLong=true;
                yield break;
            }
            gm.GetComponent<TextMesh>().text=gen+".";
            yield return new WaitForSeconds(.75f);
            gm.GetComponent<TextMesh>().text=gen+"..";
            yield return new WaitForSeconds(.75f);
            totaltime++;
        }
        generatingMazeIdleCurrentlyRunning=false;
    }
}
