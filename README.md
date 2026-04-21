# Seyam.Automata

A robust, event-driven .NET finite state machine (FSM) framework featuring hierarchical states, condition-based watchdogs, and global interrupts. 

**Born from the chaos of Red Dead Redemption 2 mission modding, built for any general-purpose .NET application.**

## 📖 About

**Seyam.Automata** goes beyond standard state transitions. When building complex, unpredictable systems (like open-world game missions or complex UI flows), you often encounter edge cases where a state machine needs to instantly abort its current flow and transition to an entirely different state based on global conditions.

Instead of cluttering every single state with edge-case checks, this library introduces a **Watchdog System**. You define pure software `Sensors` that constantly evaluate conditions against a shared state `Context`. When a condition is met, the sensor evaluates and fires an `IInterruptInput`, forcing the machine to transition based on a global routing table—completely overriding the current state.

## ✨ Features

* **Context-Driven Design:** The entire framework is built around a generic `<TContext>`, allowing you to easily inject and share your application's state/data across the Machine, States, and Sensors.
* **Hierarchical State Machines (HSM):** Encapsulate entire FSMs within a single state using `SubMachineState<TContext>` for modular, nested logic.
* **The Watchdog & Sensor System:** Create software-based `Sensor` classes that evaluate logic in the background and safely self-disable when triggered.
* **Global Interrupt Routing:** Map `IInterruptInput` types to specific states in the `Machine`. If a sensor triggers that interrupt, the machine transitions instantly, regardless of its current state.
* **Terminal States:** Use the `ITerminalState` marker interface to signal when a `SubMachineState` has reached its finish line, allowing the parent machine to resume control.

## 🏗️ Core Architecture

* `Machine<TContext>`: The core engine managing states, transitions, and global interrupts.
* `State<TContext>`: The base class for defining isolated logic with `Enter()`, `Update()`, and `Exit()` lifecycle methods. Tracks `TimeInState` automatically.
* `SubMachineState<TContext>`: A state containing an internal `SubMachine` for hierarchical workflows.
* `IInput` / `Input`: Standard triggers used to transition between states.
* `IInterruptInput`: High-priority triggers used for global routing.
* `Watchdog<TContext>`: A background evaluator that manages active sensors. Iterates safely to allow self-removing sensors during evaluation.
* `Sensor<TContext>`: User-defined background logic that evaluates conditions and fires inputs.

## 🚀 Conceptual Example

Here is how you would use the framework to handle a mission scenario where a global condition (the player dying) interrupts the normal flow:

```csharp
using System;
using Seyam.Automata.Core;
using Seyam.Automata.Sensors;

// ==========================================
// 1. Context and Inputs
// ==========================================
public class MissionContext 
{ 
    public int PlayerHealth { get; set; } = 100; 
}

public class EnemySpottedInput : IInput { }
public class PlayerDiedInterrupt : IInterruptInput { }

// ==========================================
// 2. States
// ==========================================
public class PatrolState : State<MissionContext> 
{
    public PatrolState(Machine<MissionContext> m) : base(m, "Patrol") { }

    public override void Enter(MissionContext context) 
    {
        base.Enter(context); // Calls base to track TimeInState
        Console.WriteLine("Entering Patrol: Setting up NPC routes.");
    }

    public override void Update(MissionContext context) 
    {
        // 1. Evaluate some local logic
        bool enemySpotted = CheckRadarForEnemies();

        // 2. If the condition is met, trigger the input to transition to CombatState
        if (enemySpotted)
        {
            Console.WriteLine("Enemy spotted! Triggering transition to Combat...");
            // This tells the Machine to process the transition immediately
            Machine.Fire(new EnemySpottedInput()); 
        }
    }

    public override void Exit(MissionContext context) 
    {
        // Called right before transitioning to the next state
        Console.WriteLine("Exiting Patrol: Cleaning up NPC routes.");
    }

    private bool CheckRadarForEnemies() 
    {
        // Dummy logic for the example. 
        // In reality, you'd check context or game engine state.
        return true; 
    }
}

public class CombatState : State<MissionContext>
{
    public CombatState(Machine<MissionContext> m) : base(m, "Combat") { }

    public override void Enter(MissionContext context)
    {
        base.Enter(context);
        Console.WriteLine("Entering Combat: Weapons hot! Taking cover.");
    }

    public override void Update(MissionContext context)
    {
        // Combat logic goes here
    }
}

public class MissionFailedState : State<MissionContext> 
{
    public MissionFailedState(Machine<MissionContext> m) : base(m, "Failed") { }

    public override void Enter(MissionContext context) 
    {
        base.Enter(context);
        Console.WriteLine("Mission Failed! The player's health dropped to zero.");
        // Logic to trigger UI failure screen, slow down time, etc.
    }

    public override void Update(MissionContext context) 
    {
        // Wait for player to press 'Restart'
    }

    public override void Exit(MissionContext context) 
    {
        Console.WriteLine("Restarting mission, hiding failure UI.");
    }
}

// ==========================================
// 3. Sensors
// ==========================================
public class HealthSensor : Sensor<MissionContext>
{
    protected override bool EvaluateAndFire(MissionContext context, Machine<MissionContext> machine)
    {
        // Watchdog evaluates this constantly in the background
        if (context.PlayerHealth <= 0)
        {
            Console.WriteLine("Watchdog Sensor Triggered: Player Health is 0 or below!");
            machine.Fire(new PlayerDiedInterrupt());
            return true; // Returns true to automatically disable the sensor after firing
        }
        return false;
    }
}

// ==========================================
// 4. Implementation & Setup
// ==========================================

// Create Context and Machine
var context = new MissionContext();
var machine = new Machine<MissionContext>(context);

// Instantiate States
var patrolState = new PatrolState(machine);
var combatState = new CombatState(machine);
var failedState = new MissionFailedState(machine);

// Setup Machine and register states
machine.AddState(patrolState, isStartingState: true);
machine.AddState(combatState);
machine.AddState(failedState);

// Add Standard Transitions
machine.AddTransition<EnemySpottedInput>(from: patrolState, to: combatState);

// Add Global Interrupt Routing (The Watchdog's target)
machine.AddGlobalInterruptTransition<PlayerDiedInterrupt>(to: failedState);

// Setup the Watchdog
var watchdog = new Watchdog<MissionContext>(machine);
watchdog.AddSensor(new HealthSensor());

// Start execution (enters PatrolState)
Console.WriteLine("--- Starting Machine ---");
machine.Start();

// --- Simulation Loop ---
Console.WriteLine("\n--- Frame 1 ---");
// PatrolState Update is called. CheckRadarForEnemies returns true, firing EnemySpottedInput.
// Machine immediately switches to CombatState.
machine.Update(); 
watchdog.Update();

Console.WriteLine("\n--- Frame 2 (Simulating Player Damage) ---");
context.PlayerHealth = 0; // Player takes fatal damage and healh drops to 0
machine.Update(); // Updates CombatState
// Watchdog detects health <= 0, fires PlayerDiedInterrupt, instantly switching to MissionFailedState
watchdog.Update();
```
## 🎮 Why was this made?
If you have ever tried to script a mission for an open-world game, you know the player rarely does what they are supposed to do. Standard FSMs become a nightmare of "spaghetti transitions" trying to account for every possible way the player can break the mission. 

Seyam.Automata was originally built to script Red Dead Redemption 2 mods. The Watchdog pattern emerged as a clean way to handle global failure/success states (like the player abandoning a mission target) without hardcoding checks into every single step of the local logic.

## 🛠️ Usage / Requirements
This library is built targeting .NET Framework 4.8, making it highly compatible with older runtime environments like those often found in game modding tools (e.g., ScriptHookV.NET / ScriptHookRDR2.NET).

Include the project in your solution or compile the library and reference the Seyam.Automata.dll.

## 🤝 Contributing
Pull requests are welcome! Feel free to open an issue for discussion if you'd like to propose a new feature or architectural improvement.
