// CS3100 Server 4
// File: room.cs
// Desc: Room Class

using System;
using System.Collections.Generic;

public class Room 
{
    private Queue<Message> recent;  // Queue of most recent messages
    private string name;            // Name of the room
    private int id;                 // Room ID
    private string owner;           // Owner's username/id
    // private List<string> admins;     // List of admins (names/ids)
    private DateTime openTime;      // Time that room was opened
    
    public bool isPrivate;         // Boolean for room access type
    public List<string> users;     // List of users (names/ids)
    
    // Constructs default room
    public Room () {
        owner = "Server";
        name = "Room";
        id = 0;
        openTime = DateTime.Now;
    }
    // Constructs a room with owner 'o', name 'n', and created at time 't', and id 'i'
    public Room (string o, string n, DateTime t, int i){
        owner = o;
        name = n;
        id = i;
        openTime = t;
    }

    // Change the room's name to 'n'
    public void setName(const string n){
        name = n;
    }
    // Returns room's name.
    public string getName(){ 
        return name;
    }
    
    public string getOwner(){
        return owner;
    }
    public int getID(){
        return id;
    }

    // Returns TimeSpan since last message
    public TimeSpan getIdleTime(){
        // TODO: implement this
    }

    // Returns how long room has been open
    public TimeSpan getAge(){
        return DateTime.Now.Subtract(openTime);
    }

}
