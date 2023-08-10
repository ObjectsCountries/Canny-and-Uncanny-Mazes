using Wawa.Modules;
using Wawa.Extensions;
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
    

	void Start(){
        dims=TextureGenerator.gridDimensions;
        string output="Your layout is:\n";
        textures=TextureGenerator.textureIndices;
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

    private IEnumerator Moving(string direction){
        switch(direction){
            case"up":
                if(!(!viewingWholeMaze&&currentPosition.y+.01f<((dims-1f)/dims))||currentlyMoving)
                    yield break;
                else{
                    currentlyMoving=true;
                    for(int i=0;i<20;i++){
                        currentPosition+=new Vector2(0,1f/dims/20);
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
                        yield return new WaitForSeconds(.02f);
                    }
                    currentlyMoving=false;
                }
                break;
            case"down":
                if(!(!viewingWholeMaze&&currentPosition.y>.01f)||currentlyMoving)
                    yield break;
                else{
                    currentlyMoving=true;
                    for(int i=0;i<20;i++){
                        currentPosition-=new Vector2(0,1f/dims/20);
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
                        yield return new WaitForSeconds(.02f);
                    }
                    currentlyMoving=false;
                }
                break;
            case"left":
                if(!(!viewingWholeMaze&&currentPosition.x>.01f)||currentlyMoving)
                    yield break;
                else{
                    currentlyMoving=true;
                    for(int i=0;i<20;i++){
                        currentPosition-=new Vector2(1f/dims/20,0);
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
                        yield return new WaitForSeconds(.02f);
                    }
                    currentlyMoving=false;
                }
                break;
            case"right":
                if(!(!viewingWholeMaze&&currentPosition.x+.01f<((dims-1f)/dims))||currentlyMoving)
                    yield break;
                else{
                    currentlyMoving=true;
                    for(int i=0;i<20;i++){
                        currentPosition+=new Vector2(1f/dims/20,0);
                        maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
                        yield return new WaitForSeconds(.02f);
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