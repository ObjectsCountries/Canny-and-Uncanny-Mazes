﻿using Wawa.Modules;
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
    private Vector2 currentPhaseIndex;
    private int xcoords,ycoords;
    private int sum,numberofAdjacentPhases,modulo;
    private float average=0;
    private int dims;
    private int[,]textures;
    private List<int>adjacentPhases;
    private bool currentlyMoving=false;
    public TextureGenerator t;
    private int n;
    private Config<cannymazesettings> cmSettings;
    public GameObject numbers;
    ///<value>The different types of mazes that the module can have. The last three are exclusive to ruleseeds other than 1.</value>
    private enum MazeTypes{
        Sum=1,
        Compare=2,
        Movement=3,
        Binary=4,
        Avoid=5,
        Strict=6,
        Walls=7,
        Average=-1,
        Digital=-2,
        Fours=-3
    }
	
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
        cmSettings=new Config<cannymazesettings>();
        n=Mathf.Clamp(cmSettings.Read().animationSpeed,10,60);
        cmSettings.Write("{\"animationSpeed\":"+n+"}");
        /*if(IsTP)*/n=2;
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
        resetButton.Set(onInteract:()=>{
            if(!currentlyMoving&&!viewingWholeMaze)
                StartCoroutine(Moving("reset"));
            Shake(resetButton,1,Sound.BigButtonPress);
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
            Log("Your maze is: "+((MazeTypes)(textures[(dims-ycoords-1),xcoords]))+" Maze");
            Log("Your current coordinates are: "+currentCoords);
        }else{
            Log("---");//makes the log a bit easier to read
            if(direction=="reset")Log("Reset the maze.");
            else Log("Pressed "+direction+", going to "+currentCoords+".");
        }
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
/*
#pragma warning disable 414
    private readonly string TwitchHelpMessage=@"!{0} u/d/l/r to move up, down, left, or right respectively; multiple can be strung together (i.e. !{0} rrulludr). !{0} m/maze to toggle view of the whole maze, !{0} n/numbers to toggle view of numbers when showing whole maze. !{0} reset to reset. If an invalid character is entered, the module will move up until the invalid character is reached. NOTE: Do not use !{0} r to reset; this will register as a command to move right.";
    private readonly string TwitchManualCode="https://ktane.timwi.de/HTML/Canny%20Maze.html";
#pragma warning restore 414
    private const string letters="udlr";
    private List<KMSelectable>buttons=new List<KMSelectable>();
    List<KMSelectable>ProcessTwitchCommand(string command){
        command=command.ToLowerInvariant();
        buttons.Clear();
        switch(command){
            case"m":
            case"maze":
                return new List<KMSelectable>(){maze};
            case"n":
            case"numbers":
                if(!viewingWholeMaze)return new List<KMSelectable>(){maze,numbersButton};
                return new List<KMSelectable>(){numbersButton};
            case"reset":
                if(viewingWholeMaze)return new List<KMSelectable>(){maze,resetButton};
                return new List<KMSelectable>(){resetButton};
            default:
                break;
        }
        if(viewingWholeMaze)buttons.Add(maze);
        foreach(char c in command){
            switch(c){
                case'u':
                    buttons.Add(arrowup);
                    break;
                case'd':
                    buttons.Add(arrowdown);
                    break;
                case'l':
                    buttons.Add(arrowleft);
                    break;
                case'r':
                    buttons.Add(arrowright);
                    break;
                default:
                    return buttons;
            }
        }
        return buttons;
    }

    IEnumerator TwitchHandleForcedSolve(){
        yield return null;//placeholder
    }
    */
}
