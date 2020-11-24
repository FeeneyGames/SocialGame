using Godot;
using System;

public class main : Control
{
    //const members
    private const int DEF_PORT = 8080;
    private readonly string[] PROTO_NAME = {"ludus"};

    //members referencing nodes
    Button HostBtn,
           ConnectBtn,
           DisconnectBtn;

    LineEdit NameEdit,
             HostEdit;

    game Game;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //initialize member variables referencing nodes
        HostBtn = GetNode<Button>("Panel/VBoxContainer/HBoxContainer2/HBoxContainer/Host");
        ConnectBtn= GetNode<Button>("Panel/VBoxContainer/HBoxContainer2/HBoxContainer/Connect");
        DisconnectBtn = GetNode<Button>("Panel/VBoxContainer/HBoxContainer2/HBoxContainer/Disconnect");
        NameEdit = GetNode<LineEdit>("Panel/VBoxContainer/HBoxContainer/NameEdit");
        HostEdit = GetNode<LineEdit>("Panel/VBoxContainer/HBoxContainer2/Hostname");
        Game = GetNode<game>("Panel/VBoxContainer/Game");
        //create connections
        GetTree().Connect("network_peer_disconnected", this, "_peer_disconnected");
        GetTree().Connect("network_peer_connected", this, "_peer_connected");
        //align the AcceptDialog
        GetNode<AcceptDialog>("AcceptDialog").GetLabel().Align = Label.AlignEnum.Center;
        GetNode<AcceptDialog>("AcceptDialog").GetLabel().Valign = Label.VAlign.Center;
        //set the default user name
        NameEdit.Text = "Username";
    }

    void StartGame()
    {
        HostBtn.Disabled = true;
        NameEdit.Editable = false;
        HostEdit.Editable = false;
        ConnectBtn.Hide();
        DisconnectBtn.Show();
        Game.Start();
    }

    void StopGame()
    {
        HostBtn.Disabled = false;
        NameEdit.Editable = true;
        HostEdit.Editable = true;
        ConnectBtn.Show();
        DisconnectBtn.Hide();
        Game.Stop();
    }

    void CloseNetwork()
    {
        SafeDisconnect("server_disconnected", this, "CloseNetwork");
        SafeDisconnect("connection_failed", this, "CloseNetwork");
        SafeDisconnect("connected_to_server", this, "Connected");
        StopGame();
        AcceptDialog acceptDialog = GetNode<AcceptDialog>("AcceptDialog");
        acceptDialog.ShowModal();
        acceptDialog.GetCloseButton().GrabFocus();
        GetTree().NetworkPeer = null;
    }

    void SafeDisconnect(string signal, Godot.Object target, string method)
    {
        if(GetTree().IsConnected(signal, target, method))
            GetTree().Disconnect(signal, target, method);
    }

    void Connected()
    {
        Game.Rpc("SetPlayerName", NameEdit.Text);
    }

    void _peer_connected(int id)
    {
        Game.OnPeerAdd(id);
    }

    void _peer_disconnected(int id)
    {
        Game.OnPeerDel(id);
    }

    void _on_Host_pressed()
    {
        WebSocketServer host = new WebSocketServer();
        host.Listen(DEF_PORT, PROTO_NAME, true);
        GetTree().Connect("server_disconnected", this, "CloseNetwork");
        GetTree().NetworkPeer = host;
        Game.AddPlayer(1, NameEdit.Text);
        StartGame();
    }

    void _on_Disconnect_pressed()
    {
        CloseNetwork();
    }

    void _on_Connect_pressed()
    {
        WebSocketClient host = new WebSocketClient();
        host.ConnectToUrl("ws://" + HostEdit.Text + ":" + DEF_PORT.ToString(), PROTO_NAME, true);
        GetTree().Connect("connection_failed", this, "CloseNetwork");
        GetTree().Connect("connected_to_server", this, "Connected");
        GetTree().NetworkPeer = host;
        StartGame();
    }
}
