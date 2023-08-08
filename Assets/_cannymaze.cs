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
    

	void Start(){
        int dims=TextureGenerator.gridDimensions;
        string output="Your layout is:\n";
        int[,]textures=TextureGenerator.textureIndices;
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
        currentCoords=coordLetters[dims-5][(int)(currentPosition.y*dims+.01f)].ToString()+(int)(1+currentPosition.x*dims+.01f);
        Log("Your current coordinates are: "+currentCoords);
        arrowleft.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.x>.05f){//set to .05f to circumvent floating point shenanigans
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition-new Vector2(1f/dims,0);
                currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
                currentCoords=coordLetters[dims-5][(int)(currentPosition.y*dims+.01f)].ToString()+(int)(1+currentPosition.x*dims+.01f);//the +.01f is there to, once again, circumvent floating point shenanigans
            Log("Pressed left, going to "+currentCoords+".");
            }
        });
        arrowright.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.x<((dims-1f)/dims)){
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition+new Vector2(1f/dims,0);
                currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
                currentCoords=coordLetters[dims-5][(int)(currentPosition.y*dims+.01f)].ToString()+(int)(1+currentPosition.x*dims+.01f);
            Log("Pressed right, going to "+currentCoords+".");
            }
        });
        arrowup.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.y<((dims-1f)/dims)){
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition+new Vector2(0,1f/dims);
                currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
                currentCoords=coordLetters[dims-5][(int)(currentPosition.y*dims+.01f)].ToString()+(int)(1+currentPosition.x*dims+.01f);
            Log("Pressed up, going to "+currentCoords+".");
            }
        });
        arrowdown.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.y>0.05f){
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition-new Vector2(0,1f/dims);
                currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
                currentCoords=coordLetters[dims-5][(int)(currentPosition.y*dims+.01f)].ToString()+(int)(1+currentPosition.x*dims+.01f);
            Log("Pressed down, going to "+currentCoords+".");
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

#pragma warning disable 414
    private readonly string TwitchHelpMessage=@"Use !{0} x to interact with the module.";
    private readonly string TwitchManualCode="https://ktane.timwi.de/HTML/Template.html";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command){
        yield return null;//placeholder
    }

    IEnumerator TwitchHandleForcedSolve(){
        yield return null;//placeholder
    }
}