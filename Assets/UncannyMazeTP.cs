﻿using System.Collections.Generic;
using UnityEngine;
using wawa.Modules;
using wawa.Extensions;
using wawa.TwitchPlays;
using wawa.TwitchPlays.Domains;

public class UncannyMazeTP : Twitch<UncannyMaze>
{
    protected override void Awake()
    {
        if (IsTP)
        {
            Module.blur = 0;
        }
    }
    public void Start()
    {
        Module.GetComponent<KMBombModule>().Add(onActivate: () =>
        {
            if (IsTP)
                Module.animSpeed = 2;
        }, onPass: () =>
        {
            if (!IsTP && Module.music)
                Module.Play(new Sound("uncanny" + Module.current.UncannyValue));
        }
        );
    }

    [Command("")]
    public IEnumerable<Instruction> Press(string movement)
    {
        List<KMSelectable> buttons = new List<KMSelectable>();
        if (!Module.mazeGenerated && !Module.tookTooLong)
        {
            yield return TwitchString.SendToChatError("{0}, please wait for the maze to generate.");
            yield break;
        }
        yield return null;
        movement = movement.ToLowerInvariant();
        switch (movement)
        {
            case "m":
            case "maze":
                yield return new KMSelectable[] { Module.maze };
                yield break;
            case "n":
            case "numbers":
                if (!Module.viewingWholeMaze)
                {
                    buttons.Add(Module.maze);
                }

                buttons.Add(Module.numbersButton);
                yield return buttons.ToArray();
                yield break;
            case "reset":
                if (Module.viewingWholeMaze)
                {
                    buttons.Add(Module.maze);
                }

                buttons.Add(Module.resetButton);
                yield return buttons.ToArray();
                yield break;
            default:
                break;
        }
        buttons.Clear();
        if (Module.viewingWholeMaze)
        {
            buttons.Add(Module.maze);
        }

        foreach (char c in movement)
        {
            switch (c)
            {
                case 'u':
                    buttons.Add(Module.arrowup);
                    break;
                case 'd':
                    buttons.Add(Module.arrowdown);
                    break;
                case 'l':
                    buttons.Add(Module.arrowleft);
                    break;
                case 'r':
                    buttons.Add(Module.arrowright);
                    break;
                case 'a':
                    buttons.Add(Module.appendButton);
                    break;
                case ' ':
                    break;
                default:
                    yield return TwitchString.SendToChatError("{0}, " + c.ToString() + " is an invalid character.");
                    yield break;
            }
        }
        yield return buttons.ToArray();
    }

    public override IEnumerable<Instruction> ForceSolve()
    {
        if (!Module.mazeGenerated && !Module.tookTooLong)
        {
            yield break;
        }

        Module.Log("Force solved by Twitch mod.");
        if (Module.tookTooLong)
        {
            Module.numbersButton.OnInteract();
            yield return new WaitForSeconds(.05f);
            yield break;
        }
        if (Module.viewingWholeMaze)
        {
            Module.maze.OnInteract();
            yield return new WaitForSeconds(.05f);
        }
        Module.resetButton.OnInteract();
        yield return new WaitForSeconds(.05f);
        foreach (string dir in Module.correctPath)
        {
            switch (dir)
            {
                case "up":
                    Module.arrowup.OnInteract();
                    break;
                case "down":
                    Module.arrowdown.OnInteract();
                    break;
                case "left":
                    Module.arrowleft.OnInteract();
                    break;
                case "right":
                    Module.arrowright.OnInteract();
                    break;
                case "append":
                    Module.appendButton.OnInteract();
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(.05f);
        }
    }
}
