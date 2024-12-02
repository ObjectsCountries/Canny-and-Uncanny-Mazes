using wawa.Modules;
using wawa.Extensions;
using wawa.IO;
using wawa.Schemas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using KModkit;

public class _cannymaze:ModdedModule{
    public KMSelectable arrowleft,arrowright,arrowup,arrowdown,maze,numbersButton,resetButton;
    public KMRuleSeedable ruleseed;
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
    internal bool mazeGenerated=false;
    private bool generatingMazeIdleCurrentlyRunning=false;
    internal bool tookTooLong=false;
    internal int animSpeed;
    internal bool music;
    private string output;
    private Config<cannymazesettings> cmSettings;
    public GameObject numbers,gm,currentBox,goalBox,anchor,coordsText;
    private List<string> j;
    private int startingTile;
    internal int currentTile;
    private int movementsMade=0;
    private List<string> tilesTraversed;
    private List<string> allDirs;
    internal List<string> correctPath;
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

    private Dictionary<string,List<string>> defaultWallsMaze5x5=new Dictionary<string,List<string>>(){
        {"A1",new List<string>(){"down"}},
        {"B1",new List<string>(){"right"}},
        {"C1",new List<string>(){"left","right","down"}},
        {"D1",new List<string>(){"left"}},
        {"E1",new List<string>(){"down"}},

        {"A2",new List<string>(){"right","up"}},
        {"B2",new List<string>(){"left","right","down"}},
        {"C2",new List<string>(){"left","right","up"}},
        {"D2",new List<string>(){"left","right"}},
        {"E2",new List<string>(){"left","up","down"}},

        {"A3",new List<string>(){"right"}},
        {"B3",new List<string>(){"left","up","down"}},
        {"C3",new List<string>(){"right"}},
        {"D3",new List<string>(){"left","right","down"}},
        {"E3",new List<string>(){"left","up","down"}},

        {"A4",new List<string>(){"down"}},
        {"B4",new List<string>(){"right","up"}},
        {"C4",new List<string>(){"down"}},
        {"D4",new List<string>(){"up","down"}},
        {"E4",new List<string>(){"up"}},

        {"A5",new List<string>(){"right","up"}},
        {"B5",new List<string>(){"left","right"}},
        {"C5",new List<string>(){"left","up"}},
        {"D5",new List<string>(){"right","up"}},
        {"E5",new List<string>(){"left"}}
    };
    private Dictionary<string,List<string>> defaultWallsMaze6x6=new Dictionary<string,List<string>>(){
        {"A1",new List<string>(){"down"}},
        {"B1",new List<string>(){"down"}},
        {"C1",new List<string>(){"right"}},
        {"D1",new List<string>(){"left","right"}},
        {"E1",new List<string>(){"left","down"}},
        {"F1",new List<string>(){"down"}},

        {"A2",new List<string>(){"up","down"}},
        {"B2",new List<string>(){"right","up"}},
        {"C2",new List<string>(){"left","right","down"}},
        {"D2",new List<string>(){"left","down"}},
        {"E2",new List<string>(){"up","down"}},
        {"F2",new List<string>(){"up","down"}},

        {"A3",new List<string>(){"up","down"}},
        {"B3",new List<string>(){"right","down"}},
        {"C3",new List<string>(){"left","up","down"}},
        {"D3",new List<string>(){"up"}},
        {"E3",new List<string>(){"right","up","down"}},
        {"F3",new List<string>(){"left","up"}},

        {"A4",new List<string>(){"right","up"}},
        {"B4",new List<string>(){"left","up","down"}},
        {"C4",new List<string>(){"right","up"}},
        {"D4",new List<string>(){"left","right"}},
        {"E4",new List<string>(){"left","right","up"}},
        {"F4",new List<string>(){"left","down"}},

        {"A5",new List<string>(){"right"}},
        {"B5",new List<string>(){"left","up"}},
        {"C5",new List<string>(){"right","down"}},
        {"D5",new List<string>(){"left","right","down"}},
        {"E5",new List<string>(){"left","right"}},
        {"F5",new List<string>(){"left","up","down"}},

        {"A6",new List<string>(){"right"}},
        {"B6",new List<string>(){"left","right"}},
        {"C6",new List<string>(){"left","up"}},
        {"D6",new List<string>(){"right","up"}},
        {"E6",new List<string>(){"left"}},
        {"F6",new List<string>(){"up"}}
    };
    private Dictionary<string,List<string>> defaultWallsMaze7x7=new Dictionary<string,List<string>>(){
        {"A1",new List<string>(){"down"}},
        {"B1",new List<string>(){"right","down"}},
        {"C1",new List<string>(){"left","right"}},
        {"D1",new List<string>(){"left","right"}},
        {"E1",new List<string>(){"left","down"}},
        {"F1",new List<string>(){"right"}},
        {"G1",new List<string>(){"left","down"}},

        {"A2",new List<string>(){"right","up","down"}},
        {"B2",new List<string>(){"left","up"}},
        {"C2",new List<string>(){"right","down"}},
        {"D2",new List<string>(){"left"}},
        {"E2",new List<string>(){"right","up"}},
        {"F2",new List<string>(){"left","right"}},
        {"G2",new List<string>(){"left","up"}},

        {"A3",new List<string>(){"right","up","down"}},
        {"B3",new List<string>(){"left"}},
        {"C3",new List<string>(){"right","up"}},
        {"D3",new List<string>(){"left","right"}},
        {"E3",new List<string>(){"left","right","down"}},
        {"F3",new List<string>(){"left","right"}},
        {"G3",new List<string>(){"left","down"}},

        {"A4",new List<string>(){"up","down"}},
        {"B4",new List<string>(){"right","down"}},
        {"C4",new List<string>(){"left","down"}},
        {"D4",new List<string>(){"right","down"}},
        {"E4",new List<string>(){"left","right","up"}},
        {"F4",new List<string>(){"left","down"}},
        {"G4",new List<string>(){"up","down"}},

        {"A5",new List<string>(){"right","up"}},
        {"B5",new List<string>(){"left","up","down"}},
        {"C5",new List<string>(){"right","up"}},
        {"D5",new List<string>(){"left","up"}},
        {"E5",new List<string>(){"down"}},
        {"F5",new List<string>(){"up","down"}},
        {"G5",new List<string>(){"up","down"}},

        {"A6",new List<string>(){"right","down"}},
        {"B6",new List<string>(){"left","up"}},
        {"C6",new List<string>(){"right","down"}},
        {"D6",new List<string>(){"left","right"}},
        {"E6",new List<string>(){"left","right","up"}},
        {"F6",new List<string>(){"left","up"}},
        {"G6",new List<string>(){"up","down"}},

        {"A7",new List<string>(){"right","up"}},
        {"B7",new List<string>(){"left"}},
        {"C7",new List<string>(){"right","up"}},
        {"D7",new List<string>(){"left","right"}},
        {"E7",new List<string>(){"left","right"}},
        {"F7",new List<string>(){"left"}},
        {"G7",new List<string>(){"up"}}
    };
    private Dictionary<string,List<string>> defaultWallsMaze8x8=new Dictionary<string,List<string>>(){
        {"A1",new List<string>(){"right","down"}},
        {"B1",new List<string>(){"left"}},
        {"C1",new List<string>(){"down"}},
        {"D1",new List<string>(){"right","down"}},
        {"E1",new List<string>(){"left","right"}},
        {"F1",new List<string>(){"left"}},
        {"G1",new List<string>(){"right","down"}},
        {"H1",new List<string>(){"left","down"}},

        {"A2",new List<string>(){"up","down"}},
        {"B2",new List<string>(){"right"}},
        {"C2",new List<string>(){"left","right","up"}},
        {"D2",new List<string>(){"left","right","up","down"}},
        {"E2",new List<string>(){"left","right"}},
        {"F2",new List<string>(){"left","right"}},
        {"G2",new List<string>(){"left","up","down"}},
        {"H2",new List<string>(){"up"}},

        {"A3",new List<string>(){"up","down"}},
        {"B3",new List<string>(){"right"}},
        {"C3",new List<string>(){"left","down"}},
        {"D3",new List<string>(){"right","up"}},
        {"E3",new List<string>(){"left","down"}},
        {"F3",new List<string>(){"right"}},
        {"G3",new List<string>(){"left","up","down"}},
        {"H3",new List<string>(){"down"}},

        {"A4",new List<string>(){"up","down"}},
        {"B4",new List<string>(){"right","down"}},
        {"C4",new List<string>(){"left","right","up"}},
        {"D4",new List<string>(){"left","right","down"}},
        {"E4",new List<string>(){"left","up"}},
        {"F4",new List<string>(){"right"}},
        {"G4",new List<string>(){"left","right","up"}},
        {"H4",new List<string>(){"left","up","down"}},

        {"A5",new List<string>(){"up","down"}},
        {"B5",new List<string>(){"up","down"}},
        {"C5",new List<string>(){"right","down"}},
        {"D5",new List<string>(){"left","right","up"}},
        {"E5",new List<string>(){"left","right"}},
        {"F5",new List<string>(){"left","right","down"}},
        {"G5",new List<string>(){"left","right"}},
        {"H5",new List<string>(){"left","up","down"}},

        {"A6",new List<string>(){"right","up"}},
        {"B6",new List<string>(){"left","up","down"}},
        {"C6",new List<string>(){"up","down"}},
        {"D6",new List<string>(){"right","down"}},
        {"E6",new List<string>(){"left","right","down"}},
        {"F6",new List<string>(){"left","up"}},
        {"G6",new List<string>(){"down"}},
        {"H6",new List<string>(){"up","down"}},

        {"A7",new List<string>(){"down"}},
        {"B7",new List<string>(){"right","up"}},
        {"C7",new List<string>(){"left","up","down"}},
        {"D7",new List<string>(){"up","down"}},
        {"E7",new List<string>(){"right","up"}},
        {"F7",new List<string>(){"left"}},
        {"G7",new List<string>(){"up","down"}},
        {"H7",new List<string>(){"up","down"}},

        {"A8",new List<string>(){"right","up"}},
        {"B8",new List<string>(){"left","right"}},
        {"C8",new List<string>(){"left","up"}},
        {"D8",new List<string>(){"right","up"}},
        {"E8",new List<string>(){"left","right"}},
        {"F8",new List<string>(){"left","right"}},
        {"G8",new List<string>(){"left","up"}},
        {"H8",new List<string>(){"up"}}
    };
    private Dictionary<string,List<string>> wallsMaze5x5=new Dictionary<string,List<string>>(){
        {"A1",new List<string>(){}},
        {"B1",new List<string>(){}},
        {"C1",new List<string>(){}},
        {"D1",new List<string>(){}},
        {"E1",new List<string>(){}},

        {"A2",new List<string>(){}},
        {"B2",new List<string>(){}},
        {"C2",new List<string>(){}},
        {"D2",new List<string>(){}},
        {"E2",new List<string>(){}},

        {"A3",new List<string>(){}},
        {"B3",new List<string>(){}},
        {"C3",new List<string>(){}},
        {"D3",new List<string>(){}},
        {"E3",new List<string>(){}},

        {"A4",new List<string>(){}},
        {"B4",new List<string>(){}},
        {"C4",new List<string>(){}},
        {"D4",new List<string>(){}},
        {"E4",new List<string>(){}},

        {"A5",new List<string>(){}},
        {"B5",new List<string>(){}},
        {"C5",new List<string>(){}},
        {"D5",new List<string>(){}},
        {"E5",new List<string>(){}}
    };
    private Dictionary<string,List<string>> wallsMaze6x6=new Dictionary<string,List<string>>(){
        {"A1",new List<string>(){}},
        {"B1",new List<string>(){}},
        {"C1",new List<string>(){}},
        {"D1",new List<string>(){}},
        {"E1",new List<string>(){}},
        {"F1",new List<string>(){}},

        {"A2",new List<string>(){}},
        {"B2",new List<string>(){}},
        {"C2",new List<string>(){}},
        {"D2",new List<string>(){}},
        {"E2",new List<string>(){}},
        {"F2",new List<string>(){}},

        {"A3",new List<string>(){}},
        {"B3",new List<string>(){}},
        {"C3",new List<string>(){}},
        {"D3",new List<string>(){}},
        {"E3",new List<string>(){}},
        {"F3",new List<string>(){}},

        {"A4",new List<string>(){}},
        {"B4",new List<string>(){}},
        {"C4",new List<string>(){}},
        {"D4",new List<string>(){}},
        {"E4",new List<string>(){}},
        {"F4",new List<string>(){}},

        {"A5",new List<string>(){}},
        {"B5",new List<string>(){}},
        {"C5",new List<string>(){}},
        {"D5",new List<string>(){}},
        {"E5",new List<string>(){}},
        {"F5",new List<string>(){}},

        {"A6",new List<string>(){}},
        {"B6",new List<string>(){}},
        {"C6",new List<string>(){}},
        {"D6",new List<string>(){}},
        {"E6",new List<string>(){}},
        {"F6",new List<string>(){}}
    };
    private Dictionary<string,List<string>> wallsMaze7x7=new Dictionary<string,List<string>>(){
        {"A1",new List<string>(){}},
        {"B1",new List<string>(){}},
        {"C1",new List<string>(){}},
        {"D1",new List<string>(){}},
        {"E1",new List<string>(){}},
        {"F1",new List<string>(){}},
        {"G1",new List<string>(){}},

        {"A2",new List<string>(){}},
        {"B2",new List<string>(){}},
        {"C2",new List<string>(){}},
        {"D2",new List<string>(){}},
        {"E2",new List<string>(){}},
        {"F2",new List<string>(){}},
        {"G2",new List<string>(){}},

        {"A3",new List<string>(){}},
        {"B3",new List<string>(){}},
        {"C3",new List<string>(){}},
        {"D3",new List<string>(){}},
        {"E3",new List<string>(){}},
        {"F3",new List<string>(){}},
        {"G3",new List<string>(){}},

        {"A4",new List<string>(){}},
        {"B4",new List<string>(){}},
        {"C4",new List<string>(){}},
        {"D4",new List<string>(){}},
        {"E4",new List<string>(){}},
        {"F4",new List<string>(){}},
        {"G4",new List<string>(){}},

        {"A5",new List<string>(){}},
        {"B5",new List<string>(){}},
        {"C5",new List<string>(){}},
        {"D5",new List<string>(){}},
        {"E5",new List<string>(){}},
        {"F5",new List<string>(){}},
        {"G5",new List<string>(){}},

        {"A6",new List<string>(){}},
        {"B6",new List<string>(){}},
        {"C6",new List<string>(){}},
        {"D6",new List<string>(){}},
        {"E6",new List<string>(){}},
        {"F6",new List<string>(){}},
        {"G6",new List<string>(){}},

        {"A7",new List<string>(){}},
        {"B7",new List<string>(){}},
        {"C7",new List<string>(){}},
        {"D7",new List<string>(){}},
        {"E7",new List<string>(){}},
        {"F7",new List<string>(){}},
        {"G7",new List<string>(){}}
    };
    private Dictionary<string,List<string>> wallsMaze8x8=new Dictionary<string,List<string>>(){
        {"A1",new List<string>(){}},
        {"B1",new List<string>(){}},
        {"C1",new List<string>(){}},
        {"D1",new List<string>(){}},
        {"E1",new List<string>(){}},
        {"F1",new List<string>(){}},
        {"G1",new List<string>(){}},
        {"H1",new List<string>(){}},

        {"A2",new List<string>(){}},
        {"B2",new List<string>(){}},
        {"C2",new List<string>(){}},
        {"D2",new List<string>(){}},
        {"E2",new List<string>(){}},
        {"F2",new List<string>(){}},
        {"G2",new List<string>(){}},
        {"H2",new List<string>(){}},

        {"A3",new List<string>(){}},
        {"B3",new List<string>(){}},
        {"C3",new List<string>(){}},
        {"D3",new List<string>(){}},
        {"E3",new List<string>(){}},
        {"F3",new List<string>(){}},
        {"G3",new List<string>(){}},
        {"H3",new List<string>(){}},

        {"A4",new List<string>(){}},
        {"B4",new List<string>(){}},
        {"C4",new List<string>(){}},
        {"D4",new List<string>(){}},
        {"E4",new List<string>(){}},
        {"F4",new List<string>(){}},
        {"G4",new List<string>(){}},
        {"H4",new List<string>(){}},

        {"A5",new List<string>(){}},
        {"B5",new List<string>(){}},
        {"C5",new List<string>(){}},
        {"D5",new List<string>(){}},
        {"E5",new List<string>(){}},
        {"F5",new List<string>(){}},
        {"G5",new List<string>(){}},
        {"H5",new List<string>(){}},

        {"A6",new List<string>(){}},
        {"B6",new List<string>(){}},
        {"C6",new List<string>(){}},
        {"D6",new List<string>(){}},
        {"E6",new List<string>(){}},
        {"F6",new List<string>(){}},
        {"G6",new List<string>(){}},
        {"H6",new List<string>(){}},

        {"A7",new List<string>(){}},
        {"B7",new List<string>(){}},
        {"C7",new List<string>(){}},
        {"D7",new List<string>(){}},
        {"E7",new List<string>(){}},
        {"F7",new List<string>(){}},
        {"G7",new List<string>(){}},
        {"H7",new List<string>(){}},

        {"A8",new List<string>(){}},
        {"B8",new List<string>(){}},
        {"C8",new List<string>(){}},
        {"D8",new List<string>(){}},
        {"E8",new List<string>(){}},
        {"F8",new List<string>(){}},
        {"G8",new List<string>(){}},
        {"H8",new List<string>(){}}
    };

    [Serializable]
    public sealed class cannymazesettings{


        [TweaksSetting.Number("Set the speed of the module's moving animation in frames.\nShould be from 10 to 60. Set to 2 to forgo moving animation.", "Animation Speed")]
        public int cannyAnimationSpeed=30;

        [TweaksSetting.Checkbox("If streaming, disable this to avoid copyright claims.", "Play Music on Solve")]
        public bool cannyPlayMusicOnSolve=true;

        public cannymazesettings(){}

        public cannymazesettings(int speed){
            if (speed == 2){
                cannyAnimationSpeed = 2;
            }
            else {
                cannyAnimationSpeed = Mathf.Clamp(speed, 10, 60);
            }
        }
    }

    static readonly TweaksEditorSettings TweaksEditorSettings = TweaksEditorSettings.CreateListing("Canny Maze", "cannymaze").Register<cannymazesettings>().BuildAndClear();

    /*
    public static Dictionary<string,object>[]TweaksEditorSettings=new Dictionary<string,object>[]{
            new Dictionary<string,object>{
                {"Filename","cannymaze-settings.json"},
                {"Name","Canny Maze"},
                {"Listings",new List<Dictionary<string,object>>{
                    new Dictionary<string,object>{
                        {"Key","cannyAnimationSpeed"},
                        {"Text","Animation Speed"},
                        {"Description","Set the speed of the module's moving animation in frames.\nShould be from 10 to 60. Set to 2 to forgo moving animation."}
                    },
                    new Dictionary<string, object>{
                        {"Key","cannyPlayMusicOnSolve"},
                        {"Text","Play Music On Solve"},
                        {"Description","If streaming, disable this to avoid copyright claims."}
                    }
                }}
            }
        };
    */

    private string[]swap(string[]array,int index1,int index2){
        string[]arr=array;
        string temp=arr[index1];
        arr[index1]=arr[index2];
        arr[index2]=temp;
        return arr;
    }

    private int sumAverageOrDigital=0;
    private int binaryDoubleOrFours=3;    
    ///<value>The different types of mazes that the module can have. Everything after the first seven are exclusive to ruleseeds other than 1.</value>
    private string[]mazeNames=new string[]{"Tiles","Sum","Compare","Binary","Avoid","Strict","Walls","Average","Digital","Movement","Double Binary","Fours"};
    private enum compareMazeType{
        LR_UD_G,
        LR_UD_L,
        LU_RD_G,
        LU_RD_L,
        LD_RU_G,
        LD_RU_L
    }
    private compareMazeType compare=compareMazeType.LR_UD_G;
    private enum strictMazeType{
        Odd_and_Even,
        Prime_and_Composite,
        Less_to_Greater,
        Greater_to_Less
    }
    private strictMazeType strictH=strictMazeType.Less_to_Greater,strictV=strictMazeType.Odd_and_Even;
    private List<int>avoidMazeOrder;
	void Start(){
        var RND=ruleseed.GetRNG();
        avoidMazeOrder=new List<int>(){0,1,2,3,4,5,6,7};
        if(RND.Seed!=1){
            if(RND.Next(2)==0)
                mazeNames=swap(mazeNames,0,9);//tiles & movement mazes
            do{
                sumAverageOrDigital=RND.Next(9);
            }while(sumAverageOrDigital!=1&&sumAverageOrDigital!=7&&sumAverageOrDigital!=8);
            mazeNames=swap(mazeNames,1,sumAverageOrDigital);
            compare=(compareMazeType)RND.Next(6);
            do{
                binaryDoubleOrFours=RND.Next(12);
            }while(binaryDoubleOrFours!=3&&binaryDoubleOrFours!=10&&binaryDoubleOrFours!=11);
            mazeNames=swap(mazeNames,3,binaryDoubleOrFours);
            avoidMazeOrder.Clear();
            for(int i=0;i<8;i++){
                avoidMazeOrder.Add(RND.Next(8));
                while(avoidMazeOrder.Count!=avoidMazeOrder.Distinct().Count()){
                    avoidMazeOrder[i]=RND.Next(8);
                }
            }
            strictH=(strictMazeType)RND.Next(4);
            do{
                strictV=(strictMazeType)RND.Next(4);
            }while(strictH==strictV);
            wallsMaze5x5=wallsMazeGeneration(RND,5);
            wallsMaze6x6=wallsMazeGeneration(RND,6);
            wallsMaze7x7=wallsMazeGeneration(RND,7);
            wallsMaze8x8=wallsMazeGeneration(RND,8);
        }else{
            wallsMaze5x5=defaultWallsMaze5x5;
            wallsMaze6x6=defaultWallsMaze6x6;
            wallsMaze7x7=defaultWallsMaze7x7;
            wallsMaze8x8=defaultWallsMaze8x8;
        }
        j=new List<string>();
        tilesTraversed=new List<string>();
        allDirs=new List<string>(){"left","right","up","down"};
        correctPath=new List<string>();
        cmSettings=new Config<cannymazesettings>("cannymaze-settings.json");
        animSpeed=cmSettings.Read().cannyAnimationSpeed;
        if(cmSettings.Read().cannyAnimationSpeed!=2)
            animSpeed=Mathf.Clamp(cmSettings.Read().cannyAnimationSpeed,10,60);
        music=cmSettings.Read().cannyPlayMusicOnSolve;
        cmSettings.Write(new cannymazesettings(animSpeed));
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
            if(tookTooLong&&!Status.IsSolved)
                Solve("Solved by pressing the Reset button after generation took too long.");
            else if(mazeGenerated&&!Status.IsSolved&&!viewingWholeMaze){
                Log("Reset the maze.");
                StartCoroutine(Moving("reset",2));
            }
            Shake(resetButton,.75f,Sound.BigButtonPress);
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
                    coordsText.SetActive(false);
                    maze.GetComponent<MeshRenderer>().material.mainTextureScale=new Vector2(1f/dims,1f/dims);
                    maze.GetComponent<MeshRenderer>().material.mainTextureOffset=currentPosition;
                }else{
                    currentBox.SetActive(true);
                    goalBox.SetActive(true);
                    coordsText.SetActive(true);
                    maze.GetComponent<MeshRenderer>().material.mainTextureScale=Vector2.one;
                    maze.GetComponent<MeshRenderer>().material.mainTextureOffset=Vector2.zero;
                }
                viewingWholeMaze=!viewingWholeMaze;
            }
            Shake(maze,.75f,Sound.BigButtonPress);
        });
        numbersButton.Set(onInteract:()=>{
            if(tookTooLong&&!Status.IsSolved)
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
            Shake(numbersButton,.75f,Sound.BigButtonPress);
        });
    }

    private float f(float x){
        return (x*-2/dims)-(-2/dims);
    }

    private IEnumerator Initialization(){
        t.changeTexture(t.whiteBG);
        dims=t.gridDimensions;
        textures=t.textureIndices;
        output="";
        for(int r=0;r<dims;r++){
            for(int c=0;c<dims;c++){
                output+=textures[r,c];
                if(c!=dims-1)output+=" ";
            }
            if(r!=dims-1)output+="\n";
        }
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
            try{
            yield return StartCoroutine(Moving(j.ElementAt(index),2,false,false));
            }catch(IndexOutOfRangeException e){
                string[]exceptionLines=e.ToString().Split('\n');
                Log("Ran into an IndexOutOfRangeException. Regenerating… The following is the content of the exception:");
                foreach(string line in exceptionLines)
                    Log(line);
                t.Awake();
                StartCoroutine(Initialization());
                yield break;
            }finally{}//needed for the yield return StartCoroutine in the try block to function properly
            movements++;
            if(movements>=20){
                yield return StartCoroutine(Moving("reset",2,false));
                movements=0;
                correctPath.Clear();
                attempts++;
            }
        }
        if(attempts!=3&&j.Count!=0){
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
            Log("Your layout is:");
            string[]outputLines=output.Split('\n');
            foreach(string line in outputLines)
                Log(line);
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
            canMoveInThatDirection=j.Contains(direction);
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
                    currentPosition.y+=1f/(n*dims);//Difference between the integral of movement and the Riemann sum
                    for(int i=n-1;i>0;i--){
                        currentPosition.y-=f(i*1f/n)/n;//Riemann sum
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
                    currentPosition.y-=1f/(n*dims);
                    for(int i=n-1;i>0;i--){
                        currentPosition.y+=f(i*1f/n)/n;
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
                    currentPosition.x-=1f/(n*dims);
                    for(int i=n-1;i>0;i--){
                        currentPosition.x+=f(i*1f/n)/n;
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
                    currentPosition.x+=1f/(n*dims);
                    for(int i=n-1;i>0;i--){
                        currentPosition.x-=f(i*1f/n)/n;
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
        numbers.GetComponent<TextMesh>().text=output;
        if((dims-ycoords-1)*2*dims+(xcoords*2)+1>(dims-yGoal-1)*2*dims+(xGoal*2)+1){
            numbers.GetComponent<TextMesh>().text=numbers.GetComponent<TextMesh>().text.Insert((dims-ycoords-1)*2*dims+(xcoords*2)+1,"</color>");
            numbers.GetComponent<TextMesh>().text=numbers.GetComponent<TextMesh>().text.Insert((dims-ycoords-1)*2*dims+(xcoords*2),"<color=\"blue\">");
            numbers.GetComponent<TextMesh>().text=numbers.GetComponent<TextMesh>().text.Insert((dims-yGoal-1)*2*dims+(xGoal*2)+1,"</color>");
            numbers.GetComponent<TextMesh>().text=numbers.GetComponent<TextMesh>().text.Insert((dims-yGoal-1)*2*dims+(xGoal*2),"<color=\"red\">");
        }else{
            numbers.GetComponent<TextMesh>().text=numbers.GetComponent<TextMesh>().text.Insert((dims-yGoal-1)*2*dims+(xGoal*2)+1,"</color>");
            numbers.GetComponent<TextMesh>().text=numbers.GetComponent<TextMesh>().text.Insert((dims-yGoal-1)*2*dims+(xGoal*2),"<color=\"red\">");
            numbers.GetComponent<TextMesh>().text=numbers.GetComponent<TextMesh>().text.Insert((dims-ycoords-1)*2*dims+(xcoords*2)+1,"</color>");
            numbers.GetComponent<TextMesh>().text=numbers.GetComponent<TextMesh>().text.Insert((dims-ycoords-1)*2*dims+(xcoords*2),"<color=\"blue\">");
        }
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
        coordsText.GetComponent<TextMesh>().text="CURRENT: "+currentCoords+"\nGOAL: "+goalCoords;
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
    private List<string> mazeType(bool logging=true,bool includeBacktracking=true){
        List<string> temp=new List<string>();
        switch(mazeNames[startingTile-1]){
            case "Sum":
            case "Average":
            case "Digital":
                temp=sumDigitalAverageMaze(logging);
                break;
            case "Compare":
                temp=compareMaze();
                break;
            case "Tiles":
            case "Movement":
                temp=tilesMovementMaze(logging);
                break;
            case "Binary":
            case "Double Binary":
            case "Fours":
                temp=binaryMaze();
                break;
            case "Avoid":
                temp=avoidMaze();
                break;
            case "Strict":
                temp=strictMaze();
                break;
            case "Walls":
                temp=wallsMaze();
                break;
        }
        if(includeBacktracking&&mazeNames[startingTile-1]!="Walls"){
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
        int sum1=0;
        int sum2=0;
        List<string>dirs1=new List<string>();
        List<string>dirs2=new List<string>();
        if(xcoords!=0){
            sum1+=textures[(dims-ycoords-1),xcoords-1];
            dirs1.Add("left");
        }
        if(xcoords!=dims-1){
            if(compare==compareMazeType.LR_UD_G||compare==compareMazeType.LR_UD_L){
                sum1+=textures[(dims-ycoords-1),xcoords+1];
                dirs1.Add("right");
            }else{
                sum2+=textures[(dims-ycoords-1),xcoords+1];
                dirs2.Add("right");
            }
        }
        if(ycoords!=dims-1){
            if(compare==compareMazeType.LU_RD_G||compare==compareMazeType.LU_RD_L){
                sum1+=textures[(dims-ycoords-2),xcoords];
                dirs1.Add("up");
            }else{
                sum2+=textures[(dims-ycoords-2),xcoords];
                dirs2.Add("up");
            }
        }
        if(ycoords!=0){
            if(compare==compareMazeType.LD_RU_G||compare==compareMazeType.LD_RU_L){
                sum1+=textures[(dims-ycoords),xcoords];
                dirs1.Add("down");
            }else{
                sum2+=textures[(dims-ycoords),xcoords];
                dirs2.Add("down");
            }
        }
        if(sum1==sum2){
            dirs1.AddRange(dirs2);
            return dirs1;
        }else{
            if((int)compare%2==0)
                return sum1>sum2?dirs1:dirs2;
            else return sum1<sum2?dirs1:dirs2;
        }
    }

    private List<string> tilesMovementMaze(bool logging){
        string tilesOrMovement="movements made";
        int modulo;
        int total=movementsMade;
        if(mazeNames[startingTile-1]=="Tiles"){
            tilesOrMovement="tiles traversed";
            total=tilesTraversed.Count;
        }
        modulo=((currentTile*total)%7)+1;
        if(logging)
            Log("The total number of "+tilesOrMovement+" so far is "+total+", and multiplying by the current tile yields "+(currentTile*total)+". Modulo 7 and adding 1, this is "+modulo+".");
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
           &&textures[(dims-ycoords-1),xcoords-1]!=avoidMazeOrder[0]
           &&textures[(dims-ycoords-1),xcoords-1]!=avoidMazeOrder[7])
            dirs.Add("left");

        if(xcoords!=dims-1
           &&textures[(dims-ycoords-1),xcoords+1]!=avoidMazeOrder[3]
           &&textures[(dims-ycoords-1),xcoords+1]!=avoidMazeOrder[4])
            dirs.Add("right");

        if(ycoords!=dims-1
           &&textures[(dims-ycoords-2),xcoords]!=avoidMazeOrder[1]
           &&textures[(dims-ycoords-2),xcoords]!=avoidMazeOrder[2])
            dirs.Add("up");

        if(ycoords!=0
           &&textures[(dims-ycoords),xcoords]!=avoidMazeOrder[5]
           &&textures[(dims-ycoords),xcoords]!=avoidMazeOrder[6])
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
                possibleDirections.Add(tile.Key);//get all tile directions that provide the most common modulo 4 value
        }
        return possibleDirections;
    }

    private List<string> strictMaze(){
        List<string> dirs=new List<string>();
        int[]primes=new int[]{2,3,5,7};
        int[]composites=new int[]{1,4,6};
        switch(strictH){
            case strictMazeType.Odd_and_Even:
                if(xcoords!=0
                &&((textures[(dims-ycoords-1),xcoords-1]%2!=0&&currentTile%2==0)
                || (textures[(dims-ycoords-1),xcoords-1]%2==0&&currentTile%2!=0)))
                    dirs.Add("left");
                if(xcoords!=dims-1
                &&((textures[(dims-ycoords-1),xcoords+1]%2!=0&&currentTile%2==0)
                || (textures[(dims-ycoords-1),xcoords+1]%2==0&&currentTile%2!=0)))
                    dirs.Add("right");
                break;
            case strictMazeType.Prime_and_Composite:
                if(xcoords!=0
                &&((primes.Contains(textures[(dims-ycoords-1),xcoords-1])&&composites.Contains(currentTile))
                || (composites.Contains(textures[(dims-ycoords-1),xcoords-1])&&primes.Contains(currentTile))))
                    dirs.Add("left");
                if(xcoords!=dims-1
                &&((primes.Contains(textures[(dims-ycoords-1),xcoords+1])&&composites.Contains(currentTile))
                || (composites.Contains(textures[(dims-ycoords-1),xcoords+1])&&primes.Contains(currentTile))))
                    dirs.Add("right");
                break;
            case strictMazeType.Greater_to_Less:
                if(xcoords!=0
                &&textures[(dims-ycoords-1),xcoords-1]>currentTile)
                    dirs.Add("left");
                if(xcoords!=dims-1
                &&textures[(dims-ycoords-1),xcoords+1]<currentTile)
                    dirs.Add("right");
                break;
            case strictMazeType.Less_to_Greater:
                if(xcoords!=0
                &&textures[(dims-ycoords-1),xcoords-1]<currentTile)
                    dirs.Add("left");
                if(xcoords!=dims-1
                &&textures[(dims-ycoords-1),xcoords+1]>currentTile)
                    dirs.Add("right");
                break;
        }
        switch(strictV){
            case strictMazeType.Odd_and_Even:
                if(ycoords!=dims-1
                &&((textures[(dims-ycoords-2),xcoords]%2!=0&&currentTile%2==0)
                || (textures[(dims-ycoords-2),xcoords]%2==0&&currentTile%2!=0)))
                    dirs.Add("up");
                if(ycoords!=0
                &&((textures[(dims-ycoords),xcoords]%2!=0&&currentTile%2==0)
                || (textures[(dims-ycoords),xcoords]%2==0&&currentTile%2!=0)))
                    dirs.Add("down");
                break;
            case strictMazeType.Prime_and_Composite:
                if(ycoords!=dims-1
                &&((primes.Contains(textures[(dims-ycoords-2),xcoords])&&composites.Contains(currentTile))
                || (composites.Contains(textures[(dims-ycoords-2),xcoords])&&primes.Contains(currentTile))))
                    dirs.Add("up");
                if(ycoords!=0
                &&((primes.Contains(textures[(dims-ycoords),xcoords])&&composites.Contains(currentTile))
                || (composites.Contains(textures[(dims-ycoords),xcoords])&&primes.Contains(currentTile))))
                    dirs.Add("down");
                break;
            case strictMazeType.Greater_to_Less:
                if(ycoords!=dims-1
                &&textures[(dims-ycoords-2),xcoords]<currentTile)
                    dirs.Add("up");
                if(ycoords!=0
                &&textures[(dims-ycoords),xcoords]>currentTile)
                    dirs.Add("down");
                break;
            case strictMazeType.Less_to_Greater:
                if(ycoords!=dims-1
                &&textures[(dims-ycoords-2),xcoords]>currentTile)
                    dirs.Add("up");
                if(ycoords!=0
                &&textures[(dims-ycoords),xcoords]<currentTile)
                    dirs.Add("down");
                break;
        }

        return dirs;
    }

    private List<string> wallsMaze(){
        switch(dims){
            case 5:
                return wallsMaze5x5[currentCoords];
            case 6:
                return wallsMaze6x6[currentCoords];
            case 7:
                return wallsMaze7x7[currentCoords];
            case 8:
            default:
                return wallsMaze8x8[currentCoords];
        }
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
                music=false;
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

    private Dictionary<string,List<string>> wallsMazeGeneration(MonoRandom r,int d){
        Dictionary<string,List<string>> tempMaze;
        switch(d){
            case 5:
                tempMaze=wallsMaze5x5;
                break;
            case 6:
                tempMaze=wallsMaze6x6;
                break;
            case 7:
                tempMaze=wallsMaze7x7;
                break;
            case 8:
            default:
                tempMaze=wallsMaze8x8;
                break;
        }
        int letterNum=r.Next(d);
        string letter,letterLeft,letterRight;
        int number=r.Next(1,d+1);
        int numberUp,numberDown;
        string primCurrent,primLeft,primRight,primUp,primDown;
        List<string>primDirs=new List<string>();
        List<string>primTraversed=new List<string>();
        string primDir;
        string primRandomTile="";
        do{
            letter=coordLetters.Substring(letterNum,1);
            letterLeft="";
            letterRight="";
            if(letter!="A")
                letterLeft=coordLetters.Substring(letterNum-1,1);
            if(letter!=coordLetters.Substring(d-1,1))
                letterRight=coordLetters.Substring(letterNum+1,1);
            numberUp=0;
            numberDown=0;
            if(number!=1)
                numberUp=number-1;
            if(number!=d)
                numberDown=number+1;
            primCurrent=letter+number.ToString();
            primLeft="";
            primRight="";
            primUp="";
            primDown="";
            if(letterLeft!=""){
                primLeft=letterLeft+number.ToString();
                if(!primTraversed.Contains(primLeft))
                    primDirs.Add(primLeft);
            }
            if(letterRight!=""){
                primRight=letterRight+number.ToString();
                if(!primTraversed.Contains(primRight))
                    primDirs.Add(primRight);
            }
            if(numberUp!=0){
                primUp=letter+numberUp.ToString();
                if(!primTraversed.Contains(primUp))
                    primDirs.Add(primUp);
            }
            if(numberDown!=0){
                primDown=letter+numberDown.ToString();
                if(!primTraversed.Contains(primDown))
                    primDirs.Add(primDown);
            }
            if(primDirs.Count==0){
                primRandomTile=primTraversed[r.Next(primTraversed.Count)];
                letterNum=coordLetters.IndexOf(char.Parse(primRandomTile.Substring(0,1)));
                number=Convert.ToInt32(primRandomTile.Substring(1,1));
                continue;
            }
            primDir=primDirs[r.Next(primDirs.Count)];
            if(primDir==primLeft){
                tempMaze[primDir].Add("right");
                tempMaze[primCurrent].Add("left");
            }
            if(primDir==primRight){
                tempMaze[primDir].Add("left");
                tempMaze[primCurrent].Add("right");
            }
            if(primDir==primUp){
                tempMaze[primDir].Add("down");
                tempMaze[primCurrent].Add("up");
            }
            if(primDir==primDown){
                tempMaze[primDir].Add("up");
                tempMaze[primCurrent].Add("down");
            }
            primDirs.Clear();
            if(!primTraversed.Contains(primCurrent))
                primTraversed.Add(primCurrent);
            if(!primTraversed.Contains(primDir))
                primTraversed.Add(primDir);
            primRandomTile=primTraversed[r.Next(primTraversed.Count)];
            letterNum=coordLetters.IndexOf(char.Parse(primRandomTile.Substring(0,1)));
            number=Convert.ToInt32(primRandomTile.Substring(1,1));
        }while(primTraversed.Count<d*d);
        return tempMaze;
    }
}
