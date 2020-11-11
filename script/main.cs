using Godot;
using System;

public class main : Control
{
    //const members
    private const uint DEF_PORT = 8080;
    private const string PROTO_NAME = "ludus";

    //members referencing nodes
    Button HostBtn,
           ConnectBtn,
           DisconnectBtn;

    LineEdit NameEdit,
             HostEdit;

    Game Game;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //initialize member variables referencing nodes
        HostBtn = GetNode<Button>("Panel/VBoxContainer/HBoxContainer2/HBoxContainer/Host");
        ConnectBtn= GetNode<Button>("Panl/VBoxContainer/HBoxContainer2/HBoxContainer/Connect");
        DisconnectBtn = GetNode<Button>("Panel/VBoxContainer/HBoxContainer2/HBoxContainer/Disconnect");
        NameEdit = GetNode<LineEdit>("Panel/VBoxContainer/HBoxContainer/NameEdit");
        HostEdit = GetNode<LineEdit>("Panel/VBoxContainer/HBoxContainer2/Hostname");
        Game = GetNode<Game>("Panel/VBoxContainer/Game");
        //create connections
        GetTree().Connect("network_peer_disconnected", this, "_peer_disconnected");
        GetTree().Connect("network_peer_connected", this, "_peer_connected");
        //align the AcceptDialog
        GetNode<AcceptDialog>("AcceptDialog").GetLabel().Align = Label.AlignEnum.Center;
        GetNode<AcceptDialog>("AcceptDialog").GetLabel().Valign = Label.VAlign.Center;
        //set the default user name
        NameEdit.Text = "Username";
    }

    public void StartGame()
    {
        HostBtn.Disabled = true;
        NameEdit.Editable = false;
        HostEdit.Editable = false;
        ConnectBtn.Hide();
        DisconnectBtn.Show();
        Game.Start();
    }

    public void StopGame()
    {
        HostBtn.Disabled = false;
        NameEdit.Editable = true;
        HostEdit.Editable = true;
        ConnectBtn.Show();
        DisconnectBtn.Hide();
        Game.Stop();
    }

}
