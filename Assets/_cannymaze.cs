using Wawa.Modules;
using Wawa.Extensions;
using Wawa.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class _cannymaze:ModdedModule{
    internal bool moduleSolved;
    public KMSelectable arrowleft,arrowright,arrowup,arrowdown,maze,numbersButton,resetButton;
    internal bool viewingWholeMaze=false;
    private Vector2 currentPosition,startingPosition;
    private string coordLetters="ABCDEFGH";
    private string currentCoords,leftTile,rightTile,aboveTile,belowTile;
    private int xcoords,ycoords;
    private int dims;
    private int[,]textures;
    private bool currentlyMoving=false;
    private bool canMoveInThatDirection=true;
    public TextureGenerator t;
    internal int n;
    private Config<cannymazesettings> cmSettings;
    public GameObject numbers;
    ///<value>The different types of mazes that the module can have. The last three are exclusive to ruleseeds other than 1.</value>
    private string[]mazeNames=new string[]{"Sum","Compare","Movement","Binary","Avoid","Strict","Walls","Average","Digital","Fours"};
    private List<string> j;
    private int startingTile;
    private List<string> tilesTraversed;
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
        cmSettings=new Config<cannymazesettings>();
        n=Mathf.Clamp(cmSettings.Read().animationSpeed,10,60);
        cmSettings.Write("{\"animationSpeed\":"+n+"}");
        numbers.SetActive(false);
        dims=t.gridDimensions;
        string output="";
        textures=t.textureIndices;
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
                break;
            case 6:
                numbers.GetComponent<TextMesh>().fontSize=25;
                break;
            case 7:
                numbers.GetComponent<TextMesh>().fontSize=22;
                break;
            case 8:
                numbers.GetComponent<TextMesh>().fontSize=19;
                break;
        }
        numbers.GetComponent<TextMesh>().text=output;
        currentPosition=new Vector2((float)UnityEngine.Random.Range(0,dims)/dims,(float)UnityEngine.Random.Range(0,dims)/dims);
        startingPosition=currentPosition;
        maze.GetComponent<MeshRenderer>().material.mainTextureScale=new Vector2(1f/dims,1f/dims);
        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
        StartCoroutine(Moving("init"));
        arrowleft.Set(onInteract:()=>{
            StartCoroutine(Moving("left"));
            Shake(arrowleft,1,Sound.BigButtonPress);
            });
        arrowright.Set(onInteract:()=>{
            StartCoroutine(Moving("right"));
            Shake(arrowright,1,Sound.BigButtonPress);
            });
        arrowup.Set(onInteract:()=>{
            StartCoroutine(Moving("up"));
            Shake(arrowup,1,Sound.BigButtonPress);
            });
        arrowdown.Set(onInteract:()=>{
            StartCoroutine(Moving("down"));
            Shake(arrowdown,1,Sound.BigButtonPress);
            });
        resetButton.Set(onInteract:()=>{
            StartCoroutine(Moving("reset"));
            Shake(resetButton,1,Sound.BigButtonPress);
        });
        maze.Set(onInteract:()=>{
            if(!currentlyMoving){
                if(viewingWholeMaze){
                    if(numbers.activeInHierarchy){
                        numbers.SetActive(false);
                        t.changeTexture(t.finalTexture);
                    }
                    maze.GetComponent<MeshRenderer>().material.mainTextureScale=new Vector2(1f/dims,1f/dims);
                    maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
                }else{
                    maze.GetComponent<MeshRenderer>().material.mainTextureScale=Vector2.one;
                    maze.GetComponent<MeshRenderer>().material.mainTextureOffset=Vector2.zero;
                }
                viewingWholeMaze=!viewingWholeMaze;
                Log("Pressed the maze.");
            }
        });
        numbersButton.Set(onInteract:()=>{
            if(viewingWholeMaze){
                if(!numbers.activeInHierarchy){
                    numbers.SetActive(true);
                    t.changeTexture(t.whiteBG);
                }else{
                    numbers.SetActive(false);
                    t.changeTexture(t.finalTexture);
                }
            }
            Shake(numbersButton,1,Sound.BigButtonPress);
        });
    }

    private float f(float x){
        return (x*-2/dims)-(-2/dims);
    }

    private IEnumerator Moving(string direction){
        if(viewingWholeMaze||currentlyMoving
        ||(xcoords==0     &&direction=="left")
        ||(xcoords==dims-1&&direction=="right")
        ||(ycoords==dims-1&&direction=="up")
        ||(ycoords==0     &&direction=="down"))
            yield break;
        if(direction!="init"&&direction!="reset"){
            canMoveInThatDirection = j.Contains(direction) || tilesTraversed.Contains(tileinDirection(direction));
            if(!canMoveInThatDirection){
                Strike("Tried to move "+direction+", not allowed.");
                yield break;
            }
        }
        switch(direction){
            case"up":
                if(currentPosition.y+.01f>=((dims-1f)/dims)){
                    yield break;
                }
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
                if(currentPosition.y<=.01f){
                    yield break;
                }
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
                if(currentPosition.x<=.01f){
                    yield break;
                }
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
                if(currentPosition.x+.01f>=((dims-1f)/dims)){
                    yield break;
                }
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
            case"init":
                break;
            
        }
        currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
        xcoords=(int)(currentPosition.x*dims+.01f);
        ycoords=(int)(currentPosition.y*dims+.01f);
        currentCoords=coordLetters[xcoords].ToString()+(dims-ycoords);
        tilesTraversed.Add(currentCoords);
        
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

        if(direction=="init"){
            Log("Your maze is: "+(mazeNames[(textures[(dims-ycoords-1),xcoords])-1])+" Maze");
            Log("Your current coordinates are: "+currentCoords);
            startingTile=textures[(dims-ycoords-1),xcoords];
        }else{
            Log("---");//makes the log a bit easier to read
            if(direction=="reset"){
                Log("Reset the maze.");
                tilesTraversed.Clear();
            }
            else Log("Pressed "+direction+", going to "+currentCoords+".");
        }
        Log("Your current tile is: "+textures[(dims-ycoords-1),xcoords]);
        j=mazeType();
        Log("Possible directions are: "+String.Join(", ", j.ToArray()));
    }

    ///<summary>A method to determine which type of maze is to be used.</summary>
    ///<returns>A <c>List&lt;string&gt;</c> containing the directions in which the<br/>defuser is allowed to move. This will contain at least one of<br/>any of the following: left, right, up, down.</returns>
    ///<remarks>Note: Incomplete, <c>switch</c> will be inaccurate until all maze<br/>types have been implemented.</remarks>
    private List<string> mazeType(){
        switch(startingTile){
            case 1:
            case 3:
            case 5:
            case 7:
                return sumDigitalAverageMaze();
            default:
                return compareMaze();
        }
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
    private List<string> sumDigitalAverageMaze(){
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
        if(mazeNames[0]=="Sum"){
            modulo=sum%7+1;
            Log("The sum of all orthogonally-adjacent tiles is "+sum+". Modulo 7 and adding 1, this is "+modulo+".");
        }
        if(mazeNames[0]=="Average"){
            average=(int)((sum/adjacentTiles.Count)+.5f);
            modulo=average%7+1;
            Log("The average of the sum of all orthogonally-adjacent tiles is "+average+". Modulo 7 and adding 1, this is "+modulo+".");
        }
        if(mazeNames[0]=="Digital"){
            digital=(sum/10)+(sum%10);
            while(digital>9)
                digital=(digital/10)+(digital%10);
            modulo=digital%7+1;
            Log("The digital root of the sum of all orthogonally-adjacent tiles is "+digital+". Modulo 7 and adding 1, this is "+modulo+".");
        }
        List<string> possibleDirections=new List<string>();
        for(int i=0;i<adjacentTiles.Count;i++){
            if(tilesTraversed.Contains(tileinDirection(adjacentTiles.ElementAt(i).Key))){
                possibleDirections.Add(adjacentTiles.ElementAt(i).Key);
                Log("Can go "+adjacentTiles.ElementAt(i).Key+" because of backtracking.");//debug
            }
            adjacentTiles[adjacentTiles.ElementAt(i).Key]-=modulo;
            if(adjacentTiles[adjacentTiles.ElementAt(i).Key]<0)
                adjacentTiles[adjacentTiles.ElementAt(i).Key]*=-1;//turn the tile numbers into absolute value of distance from modulo
        }
        int totalSoFar=possibleDirections.Count;
        int min=adjacentTiles.Aggregate((l,r)=>l.Value<r.Value?l:r).Value;//find the minimum distance
        for(int i=0;i<7;i++){
            if(possibleDirections.Count==totalSoFar){
                foreach(KeyValuePair<string,int> tile in adjacentTiles){
                    if((tile.Value==min+i||tile.Value==min-i)&&!possibleDirections.Contains(tile.Key))
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
}
