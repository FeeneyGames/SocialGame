using Godot;
using System;

public class game : Control
{
    Texture Crown = ResourceLoader.Load<Texture>("res://img/crown.png");
    ItemList List;
    Button Action;

    Godot.Collections.Array<int> Players = new Godot.Collections.Array<int>();
    int Turn = -1;
    RandomNumberGenerator Random = new RandomNumberGenerator();

    public override void _Ready()
    {
        List = GetNode<ItemList>("HBoxContainer/VBoxContainer/ItemList");
        Action = GetNode<Button>("HBoxContainer/VBoxContainer/Action");
    }

    [Master]
    void SetPlayerName(String name)
    {
        int sender = GetTree().GetRpcSenderId();
        Rpc("UpdatePlayerName", sender, name);
    }

    [Sync]
    void UpdatePlayerName(int player, String name)
    {
        int pos = Players.IndexOf(player);
        if(pos != -1)
            List.SetItemText(pos, name);
    }

    [Master]
    void RequestAction(String action)
    {
        int sender = GetTree().GetRpcSenderId();
        if(Players[Turn] != sender)
            Rpc("Log", "Someone is trying to cheat!");
        else
        {
            DoAction(action);
            NextTurn();
        }
    }

    [Sync]
    void DoAction(String action)
    {
        String name = List.GetItemText(Turn);
        int val = Random.RandiRange(0, 99);
        Rpc("Log", $"{name}: {action} {val}");
    }

    [Sync]
    void SetTurn(int turn)
    {
        Turn = turn;
        if(Turn < Players.Count)
        {
            for(int i = 0; i < Players.Count; i++)
                if(i == Turn)
                    List.SetItemIcon(i, Crown);
                else
                    List.SetItemIcon(i, null);
            Action.Disabled = Players[Turn] != GetTree().GetNetworkUniqueId();
        }
    }

    [Sync]
    void DelPlayer(int id)
    {
        int pos = Players.IndexOf(id);
        if(pos != -1)
        {
            Players.RemoveAt(pos);
            List.RemoveItem(pos);
            if(Turn > pos)
                Turn -= 1;
            if(GetTree().IsNetworkServer())
                Rpc("SetTurn", Turn);
        }
    }

    [Sync]
    public void AddPlayer(int id, String name="")
    {
        Players.Add(id);
        if(name == "")
            List.AddItem("... connecting ...", null, false);
        else
            List.AddItem(name, null, false);
    }

    String GetPlayerName(int pos)
    {
        if(pos < List.GetItemCount())
            return List.GetItemText(pos);
        return "Error!";
    }

    void NextTurn()
    {
        Turn += 1;
        if(Turn >= Players.Count)
            Turn = 0;
        Rpc("SetTurn", Turn);
    }

    public void Start()
    {
        SetTurn(0);
    }

    public void Stop()
    {
        Players.Clear();
        List.Clear();
        Turn = 0;
        Action.Disabled = true;
    }

    public void OnPeerAdd(int id)
    {
        if(GetTree().IsNetworkServer())
        {
            for(int i = 0; i < Players.Count; i++)
                RpcId(id, "AddPlayer", Players[i], GetPlayerName(i));
            Rpc("AddPlayer", id, "");
            RpcId(id, "SetTurn", Turn);
        }
    }

    public void OnPeerDel(int id)
    {
        if(GetTree().IsNetworkServer())
        {
            Rpc("DelPlayer", id);
        }
    }

    [Sync]
    void Log(String text)
    {
        GetNode<RichTextLabel>("HBoxContainer/RichTextLabel").AddText(text + "\n");
    }

    void _on_Action_pressed()
    {
        if(GetTree().IsNetworkServer())
        {
            DoAction("roll");
            NextTurn();
        }
        else
            RpcId(1, "RequestAction", "roll");
    }
}
