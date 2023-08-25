using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wawa.Extensions;
using Wawa.TwitchPlays;
using Wawa.TwitchPlays.Domains;

public sealed class cannymazeTP:Twitch<_cannymaze> {
    void Start(){
        Module.GetComponent<KMBombModule>().Set(onActivate:()=>{
            if(IsTP)
                Module.n=2;
        });
    }
	//new public string Help=@"!{0} u/d/l/r to move up, down, left, or right respectively; multiple can be strung together (i.e. !{0} rrulludr). !{0} m/maze to toggle view of the whole maze, !{0} n/numbers to toggle view of numbers when showing whole maze. !{0} reset to reset. If an invalid character is entered, the module will move up until the invalid character is reached. NOTE: Do not use !{0} r to reset; this will register as a command to move right.";
    [Command("")]
	IEnumerable<Instruction> Press(string movement){
		movement=movement.ToLowerInvariant();
		switch(movement){
            case "m":
            case "maze":
                Debug.Log("entered maze command");
                yield return Module.maze;
                Debug.Log("pressed maze button");//for debugging
				yield break;
            case "n":
            case "numbers":
                if(!Module.viewingWholeMaze)
					yield return Module.maze;
                yield return Module.numbersButton;
				yield break;
            case "reset":
                if(Module.viewingWholeMaze)
					yield return Module.maze;
                yield return Module.resetButton;
				yield break;
            default:
                break;
		}
		if(Module.viewingWholeMaze)
			yield return Module.maze;
        foreach(char c in movement){
            switch(c){
                case 'u':
                    yield return Module.arrowup;
                    break;
                case 'd':
                    yield return Module.arrowdown;
                    break;
                case 'l':
                    yield return Module.arrowleft;
                    break;
                case 'r':
                    yield return Module.arrowright;
                    break;
                default:
                    yield break;
            }
        }
	}

	public override IEnumerable<Instruction> ForceSolve(){
		yield return null;//placeholder
	}
}