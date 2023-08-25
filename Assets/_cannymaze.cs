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
    private string currentCoords;
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
        StartCoroutine(Moving("maze"));
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

    private float f(float j){
        return (j*-2/dims)-(-2/dims);
    }

    private IEnumerator Moving(string direction){
        if(viewingWholeMaze||currentlyMoving)
            yield break;
        if(direction!="maze"&&direction!="reset"){
            canMoveInThatDirection = j.Contains(direction);
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
            case"maze":
                break;
            
        }
        currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
        xcoords=(int)(currentPosition.x*dims+.01f);
        ycoords=(int)(currentPosition.y*dims+.01f);
        currentCoords=coordLetters[xcoords].ToString()+(dims-ycoords);
        if(direction=="maze"){
            Log("Your maze is: "+(mazeNames[(textures[(dims-ycoords-1),xcoords])-1])+" Maze");
            Log("Your current coordinates are: "+currentCoords);
        }else{
            Log("---");//makes the log a bit easier to read
            if(direction=="reset")
                Log("Reset the maze.");
            else Log("Pressed "+direction+", going to "+currentCoords+".");
        }
        Log("Your current tile is: "+textures[(dims-ycoords-1),xcoords]);
        j=mazeType();
    }

    private List<string> mazeType(){
        /*switch(textures[(dims-ycoords-1),xcoords]){
            case 0:*/
                return sumDigitalAverageMaze();
        //}
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
        if(ycoords!=0){
            adjacentTiles.Add("up",textures[(dims-ycoords),xcoords]);
            sum+=textures[(dims-ycoords),xcoords];
        }
        if(ycoords!=dims-1){
            adjacentTiles.Add("down",textures[(dims-ycoords-2),xcoords]);
            sum+=textures[(dims-ycoords-2),xcoords];
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
        for(int i=0;i<adjacentTiles.Count;i++){
            adjacentTiles[adjacentTiles.ElementAt(i).Key]-=modulo;
            if(adjacentTiles[adjacentTiles.ElementAt(i).Key]<0)
                adjacentTiles[adjacentTiles.ElementAt(i).Key]*=-1;//turn the tile numbers into absolute value of distance from modulo
        }
        List<string> possibleDirections=new List<string>();
        int min=adjacentTiles.Aggregate((l,r)=>l.Value<r.Value?l:r).Value;//find the minimum distance
        for(int i=0;i<4;i++){
            if(possibleDirections.Count==0){
                foreach(KeyValuePair<string,int> tile in adjacentTiles){
                    if(tile.Value==min+i||tile.Value==min-i)
                        possibleDirections.Add(tile.Key);//get all tile directions that are this distance or the nearest away from modulo
                }
            }
        }
        Log("Possible directions are: "+String.Join(", ", possibleDirections.ToArray()));
        return possibleDirections;
    }
}
