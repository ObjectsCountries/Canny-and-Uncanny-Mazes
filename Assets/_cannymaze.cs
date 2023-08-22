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
    public KMSelectable arrowleft,arrowright,arrowup,arrowdown,maze;
    private bool viewingWholeMaze=false;
    private Vector2 currentPosition;
    private string coordLetters="ABCDEFGH";
    private string currentCoords;
    private Vector2 currentPhaseIndex;
    private int xcoords,ycoords;
    private int sum,numberofAdjacentPhases,modulo;
    private float average=0;
    private int dims;
    private int[,]textures;
    private List<int>adjacentPhases;
    private bool currentlyMoving=false;
    public TextureGenerator t;
    private bool TwitchPlaysActive;
    private int n;
    private Config<settings> cmSettings;
	
    [Serializable]
    public sealed class settings{
        public int animationSpeed=30;
    }

    public static Dictionary<string,object>[]cannymazesettings=new Dictionary<string,object>[]{
        new Dictionary<string,object>{
            {"Filename","cannymaze-settings.json"},
            {"Name","Canny Maze"},
            {"Listings",new List<Dictionary<string,object>>{
                new Dictionary<string,object>{
                    {"Key","animationSpeed"},
                    {"Text","Animation Speed"},
                    {"Description","Set the speed of the module's moving animation in frames. Should be from 10 to 60."}
                }
            }}
        }
    };

	void Start(){
        cmSettings=new Config<settings>();
        n=Mathf.Clamp(cmSettings.Read().animationSpeed,10,60);
        cmSettings.Write("{\"animationSpeed\":"+n+"}");
        if(TwitchPlaysActive)n=1;
        dims=t.gridDimensions;
        string output="Your layout is:\n";
        textures=t.textureIndices;
        for(int r=0;r<dims;r++){
            for(int c=0;c<dims;c++){
                output+=textures[r,c];
                if(c!=dims-1)output+=" ";
            }
            if(r!=dims-1)output+="\n";
        }
        Log(output);
        currentPosition=new Vector2((float)UnityEngine.Random.Range(0,dims)/dims,(float)UnityEngine.Random.Range(0,dims)/dims);
        maze.GetComponent<MeshRenderer>().material.mainTextureScale=new Vector2(1f/dims,1f/dims);
        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
        adjacentPhases=new List<int>(){};
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
        maze.Set(onInteract:()=>{
            if(viewingWholeMaze){
                maze.GetComponent<MeshRenderer>().material.mainTextureScale=new Vector2(1f/dims,1f/dims);
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
            }else{
                maze.GetComponent<MeshRenderer>().material.mainTextureScale=Vector2.one;
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=Vector2.zero;
            }
            viewingWholeMaze=!viewingWholeMaze;
            Log("Pressed the maze.");
        });
    }

    private float f(float j){
        return (j*-2/dims)-(-2/dims);
    }

    private IEnumerator Moving(string direction){
        switch(direction){
            case"up":
                if(!(!viewingWholeMaze&&currentPosition.y+.01f<((dims-1f)/dims))||currentlyMoving)
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
                if(!(!viewingWholeMaze&&currentPosition.y>.01f)||currentlyMoving)
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
                if(!(!viewingWholeMaze&&currentPosition.x>.01f)||currentlyMoving)
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
                if(!(!viewingWholeMaze&&currentPosition.x+.01f<((dims-1f)/dims))||currentlyMoving)
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
            case "maze":
                break;
        }
        currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
        xcoords=(int)(currentPosition.x*dims+.01f);
        ycoords=(int)(currentPosition.y*dims+.01f);
        currentCoords=coordLetters[xcoords].ToString()+(dims-ycoords);
        if(direction!="maze")Log("Pressed "+direction+", going to "+currentCoords+".");
        else Log("Your current coordinates are: "+currentCoords);
        Log("Your current phase is: "+textures[(dims-ycoords-1),xcoords]);
        sum=0;
        numberofAdjacentPhases=0;
        adjacentPhases.Clear();
        modulo=0;
        if(xcoords!=0){
            adjacentPhases.Add(textures[(dims-ycoords-1),xcoords-1]);
            sum+=textures[(dims-ycoords-1),xcoords-1];
            numberofAdjacentPhases++;
        }
        if(xcoords!=dims-1){
            adjacentPhases.Add(textures[(dims-ycoords-1),xcoords+1]);
            sum+=textures[(dims-ycoords-1),xcoords+1];
            numberofAdjacentPhases++;
        }
        if(ycoords!=0){
            adjacentPhases.Add(textures[(dims-ycoords),xcoords]);
            sum+=textures[(dims-ycoords),xcoords];
            numberofAdjacentPhases++;
        }
        if(ycoords!=dims-1){
            adjacentPhases.Add(textures[(dims-ycoords-2),xcoords]);
            sum+=textures[(dims-ycoords-2),xcoords];
            numberofAdjacentPhases++;
        }
        average=(float)sum/numberofAdjacentPhases;
        modulo=sum%7+1;
        Log("The sum of all orthogonally-adjacent phases is "+sum+", and the 1+sum modulo 7 is "+modulo+".");
        Log("The average of all orthogonally-adjacent phases is "+average+".");
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage=@"Use !{0} u/d/l/r to navigate the maze (can be strung together), and !{0} maze to toggle between the view of the entire maze and the current position.";
    private readonly string TwitchManualCode="https://ktane.timwi.de/HTML/Canny%20Maze.html";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command){
        yield return null;//placeholder
    }

    IEnumerator TwitchHandleForcedSolve(){
        yield return null;//placeholder
    }
}
