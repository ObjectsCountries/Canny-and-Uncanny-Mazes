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
    private string[]coordLetters=new string[]{"EDCBA","FEDCBA","GFEDCBA","HGFEDCBA"};
    private string currentCoords;
    private Vector2 currentPhaseIndex;
    private int xcoords,ycoords;
    private int sum,numberofAdjacentPhases;
    private float average=0;
    private int dims;
    private int[,]textures;
    

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
        xcoords=(int)(currentPosition.y*dims+.01f);//the +.01f is there to circumvent floating point shenanigans
        ycoords=(int)(currentPosition.x*dims+.01f);
        currentCoords=coordLetters[dims-5][xcoords].ToString()+(ycoords+1);
        Log("Your current coordinates are: "+currentCoords);
        Log("Your current phase is: "+textures[(dims-xcoords-1),ycoords]);
        sum=0;
        numberofAdjacentPhases=0;
        if(ycoords!=0){
            sum+=textures[(dims-xcoords-1),ycoords-1];
            numberofAdjacentPhases++;
        }
        if(ycoords!=dims-1){
            sum+=textures[(dims-xcoords-1),ycoords+1];
            numberofAdjacentPhases++;
        }
        if(xcoords!=0){
            sum+=textures[(dims-xcoords),ycoords];
            numberofAdjacentPhases++;
        }
        if(xcoords!=dims-1){
            sum+=textures[(dims-xcoords-2),ycoords];
            numberofAdjacentPhases++;
        }
        average=(float)sum/numberofAdjacentPhases;
        Log("The sum of all orthogonally-adjacent phases is "+sum+".");
        Log("The average of all orthogonally-adjacent phases is "+average+".");
        arrowleft.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.x>.05f){//set to .05f to circumvent floating point shenanigans
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition-new Vector2(1f/dims,0);
                moving("left");
            }
        });
        arrowright.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.x<((dims-1f)/dims)){
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition+new Vector2(1f/dims,0);
                moving("right");
            }
        });
        arrowup.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.y<((dims-1f)/dims)){
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition+new Vector2(0,1f/dims);
                moving("up");
            }
        });
        arrowdown.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.y>0.05f){
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition-new Vector2(0,1f/dims);
                moving("down");
            }
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

    private void moving(string direction){
        currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
        xcoords=(int)(currentPosition.y*dims+.01f);
        ycoords=(int)(currentPosition.x*dims+.01f);
        currentCoords=coordLetters[dims-5][xcoords].ToString()+(ycoords+1);
        Log("Pressed "+direction+", going to "+currentCoords+".");
        Log("Your current phase is: "+textures[(dims-xcoords-1),(int)ycoords]);
        sum=0;
        numberofAdjacentPhases=0;
        if(ycoords!=0){
            sum+=textures[(dims-xcoords-1),ycoords-1];
            numberofAdjacentPhases++;
        }
        if(ycoords!=dims-1){
            sum+=textures[(dims-xcoords-1),ycoords+1];
            numberofAdjacentPhases++;
        }
        if(xcoords!=0){
            sum+=textures[(dims-xcoords),ycoords];
            numberofAdjacentPhases++;
        }
        if(xcoords!=dims-1){
            sum+=textures[(dims-xcoords-2),ycoords];
            numberofAdjacentPhases++;
        }
        average=(float)sum/numberofAdjacentPhases;
        Log("The sum of all orthogonally-adjacent phases is "+sum+".");
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