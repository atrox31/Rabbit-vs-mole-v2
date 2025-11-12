using System;
using DialogueSystem;
using Enums;

public interface IInteractable
{
    void Interact(PlayerType type, Func<ActionType, bool> setActionType, Action<bool> changeIsBusy);
}