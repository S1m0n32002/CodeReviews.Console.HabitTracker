﻿using HabitTracker.S1m0n32002.Models;
using Spectre.Console;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection.Emit;
namespace HabitTracker.S1m0n32002.Controllers;

static class MenuController
{
    enum MainMenuChoices
    {
        AddHabit,
        ShowReport,
        ShowHabits,
        Exit
    }

    static Dictionary<string,MainMenuChoices> mainMenuStrs = new()
    {
        { "Add habit" , MainMenuChoices.AddHabit },
        { "Show report" , MainMenuChoices.ShowReport },
        { "Show habits" , MainMenuChoices.ShowHabits},
        { "Exit" , MainMenuChoices.Exit }
    };

    static readonly Stopwatch stopwatch = new();
    static int count = 0;

    static readonly DbController dbController = new();

    public static bool ShowMainMenu()
    {
        Console.Clear();

        var prompt = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Main menu")
                .AddChoices(mainMenuStrs.Keys)
        );

        switch (mainMenuStrs[prompt])
        {
            case MainMenuChoices.AddHabit:
                ShowAddMenu();
                break;
            case MainMenuChoices.ShowReport:
                ShowReport();
                break;
            case MainMenuChoices.ShowHabits:
                ShowHabits();
                break;
            case MainMenuChoices.Exit:
                return false;
        }

        return true;
    }

    /// <summary>
    /// Show add habit menu
    /// </summary>
    static void ShowAddMenu()
    {
        Console.Clear();

        SW();

        PrintRule("Add Habit");

        var namePrompt = new TextPrompt<string>("Enter habit name:") 
        { 
            ShowDefaultValue = true
        };
        namePrompt.DefaultValue("Habit");
        var name = AnsiConsole.Prompt(namePrompt);

        var periodicityPrompt = new SelectionPrompt<Habit.Periodicities>()
        {
            Title = "Select periodicity",
            HighlightStyle = new Style(Color.Blue),
            MoreChoicesText = "[grey](Move up and down to reveal more choices)[/]",
        };

        periodicityPrompt.AddChoices(Enum.GetValues<Habit.Periodicities>()
                                                .Where(x => x != Habit.Periodicities.None));
        
        var periodicity = AnsiConsole.Prompt(periodicityPrompt);

        dbController.AddHabit(name, periodicity);
    }

    /// <summary>
    /// Show the report of all habits
    /// </summary>
    static void ShowReport()
    {
        Console.Clear();

        var habits = dbController.LoadAllHabits();

        BarChart chart = new()
        {
            Label = "Habits",
        };

        int c = 0;

        foreach (Habit habit in habits)
        {
            chart.AddItem(habit.Name, habit.TimesPerPeriod, Color.FromInt32(++c));
        }

        Panel panel = new(chart)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Yellow),
            Header = new PanelHeader("[bold blue]Habits[/]")
            {
                Justification = Justify.Left
            }
        };

        AnsiConsole.Write(panel);
        AnsiConsole.Markup("Press [blue]Enter[/] to exit");
        Console.ReadLine();
    }

    /// <summary>
    /// Show all habits
    /// </summary>
    static void ShowHabits()
    {
        Console.Clear();

        Table table = new()
        {
            BorderStyle = new Style(Color.Yellow),
            ShowRowSeparators = true,
            ShowFooters = true,
            ShowHeaders = true
        };

        table.AddColumns(["[bold blue]Id[/]", "[bold blue]name[/]", "Times", "Periodicity"]);

        foreach (Habit habit in dbController.LoadAllHabits())
        {
            table.AddRow($"{habit.Id}", $"{habit.Name}", $"{habit.TimesPerPeriod}", $"{habit.Periodicity}");
        }

        Panel panel = new(table)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Yellow),
            Header = new PanelHeader("[bold blue]Habits[/]")
            {
                Justification = Justify.Left
            }
        };

        AnsiConsole.Write(panel);
        AnsiConsole.Markup("Press [blue]Enter[/] to exit");
        Console.ReadLine();
    }

    static void PrintRule(string Title)
    {
        Rule rule = new()
        {
            Title = $"[white]{Title}[/]",
            Justification = Justify.Left,
            Style = Style.Parse("yellow")
        };
        AnsiConsole.Write(rule);
    }

    #region 
    /// <summary>
    /// I wonder what this does...
    /// </summary>
    static void SW()
    {
        count++;

        if (!stopwatch.IsRunning)
            stopwatch.Start();

        if (stopwatch.Elapsed.TotalSeconds > 60)
        {
            count = 0;
            stopwatch.Restart();
        }

        if (count > 10)
            throw new FedUpException();
    }

    class FedUpException : Exception
    {
        public FedUpException() : base("What am I? Your psychiatrist? GO AWAY!") { }
    }
    #endregion
}