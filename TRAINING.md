# Bomberman AI Training Workflow

This document describes how to start a training session for the Bomberman AI agent.

## Prerequisites

- Unity Editor open with the "Lobby" scene loaded.
- Python environment `mlagents_env` set up in the root directory.

## Steps to Start Training

1.  **Open a Terminal** in the project root directory.
2.  **Activate the Environment**:
    ```powershell
    .\mlagents_env\Scripts\activate
    ```
3.  **Start mlagents-learn**:
    ```powershell
    mlagents-learn bomberman_config.yaml --run-id=Bomberman_V4
    ```
    *Note: You can change the `--run-id` to keep track of different experiments.*
4.  **Click Play in Unity**.
5.  In the Main Menu, click the **ENTRENAMIENTO IA** button.

## Observations and Rewards

The agent is rewarded for:
- Destroying walls (+0.5)
- Picking up items (+0.3)
- Surviving and winning the round (+2.0)
- Tactical bomb placement (+0.2)

The agent is penalized for:
- Dying (-5.0)
- Placing bombs "at nothing" (-0.1)
- Standing on a bomb (-0.01)
- Moving away from the opponent (slight penalty)
- Passive behavior (slight penalty over time)
