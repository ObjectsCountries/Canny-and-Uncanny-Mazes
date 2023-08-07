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
    private bool viewingWholeMaze=true;
    private Vector2 currentPosition;

    

	protected override void Awake(){
        currentPosition=new Vector2((float)UnityEngine.Random.Range(0,TextureGenerator.cols)/TextureGenerator.cols,(float)UnityEngine.Random.Range(0,TextureGenerator.rows)/TextureGenerator.rows);
        arrowleft.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.x>0.05f){//set to 0.05f to circumvent floating point shenanigans
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition-new Vector2(1f/TextureGenerator.cols,0);
                currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
            }
            Log("Pressed left.");
            Log(currentPosition*5);
        });
        arrowright.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.x<((TextureGenerator.cols-1f)/TextureGenerator.cols)){
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition+new Vector2(1f/TextureGenerator.cols,0);
                currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
            }
            Log("Pressed right.");
            Log(currentPosition*5);
        });
        arrowup.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.y<((TextureGenerator.rows-1f)/TextureGenerator.rows)){
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition+new Vector2(0,1f/TextureGenerator.rows);
                currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
            }
            Log("Pressed up.");
            Log(currentPosition*5);
        });
        arrowdown.Set(onInteract: () => {
            if(!viewingWholeMaze&&currentPosition.y>0.05f){
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition-new Vector2(0,1f/TextureGenerator.rows);
                currentPosition=maze.GetComponent<MeshRenderer>().material.mainTextureOffset;
            }
            Log("Pressed down.");
            Log(currentPosition*5);
        });
        maze.Set(onInteract:()=>{
            if(viewingWholeMaze){
                maze.GetComponent<MeshRenderer>().material.mainTextureScale=new Vector2(1f/TextureGenerator.cols,1f/TextureGenerator.rows);
                maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
            } else {
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