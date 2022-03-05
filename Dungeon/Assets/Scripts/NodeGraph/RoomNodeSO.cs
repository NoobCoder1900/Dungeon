using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
// ReSharper disable All

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id; //GUID
    [HideInInspector] public List<string> parentRoomNodeIDList = new();
    [HideInInspector] public List<string> childRoomNodeIDList = new();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code

#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging;
    [HideInInspector] public bool isSelected;
     
    /// <summary>
    /// Initialise node
    /// </summary>
    public void Initialise(Rect rect_, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType_)
    {
        this.rect = rect_;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType_;
        
        //load room node type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Draw nodes with node style
    /// </summary>
    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);
        
        EditorGUI.BeginChangeCheck();
        
        // if node has a parent or it's entrance then display as label instead of a popup
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);            
        }
        else
        {
            var selected = roomNodeTypeList.list.FindIndex((x => x == roomNodeType));

            var selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesDisplay());

            roomNodeType = roomNodeTypeList.list[selection];
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
        
        GUILayout.EndArea();
    }

    private string[] GetRoomNodeTypesDisplay()
    {
        var roomArray = new string[roomNodeTypeList.list.Count];
        for (var i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }
        return roomArray;
    }
    
    /// <summary>
    /// Process event for the node
    /// </summary>
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMoveUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMoveDragEvent(currentEvent);
                break;
        }
    }

    /// <summary>
    /// Process mouse drag event
    /// </summary>
    private void ProcessMoveDragEvent(Event currentEvent)
    {
        //process left click drag event
        if (currentEvent.button == 0)
            ProcessLeftMouseDragEvent(currentEvent);
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    private void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Process mouse up events
    /// </summary>
    private void ProcessMoveUpEvent(Event currentEvent)
    {
        //if left click is up  
        if (currentEvent.button == 0)
            ProcessLeftClickUpEvent();
    }

    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
            isLeftClickDragging = false;
    }

    /// <summary>
    /// Process mouse down events
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //left click
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        //right click
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessLeftClickDownEvent()
    {
       Selection.activeObject = this;
        
        //Toggle node selection
        isSelected = !isSelected;
    }
    
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    public bool  AddChildRoomNodeIDToRoomNodes(string childID) 
    {
        if (!IsChildRoomValue(childID)) return false;
        childRoomNodeIDList.Add(childID);
        return true;

    }

    /// <summary>
    /// Check the child node can be validly added to the parent node
    /// </summary>
    private bool IsChildRoomValue(string childID)
    {
        var isConnectedBossNodeAlready = false;
        // check if there is already a connected boss room in the node graph
        foreach (var roomNode in roomNodeGraph.roomNodeList.Where(roomNode => roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0))
        {
            isConnectedBossNodeAlready = true;
        }
        
        // if the child node is boss room and there is already a boss room then return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;
        
        // if the child has a type of none then return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;
        
        // if the child already has a child with childID
        if (roomNodeGraph.GetRoomNode(childID))
            return false;
        
        // if this node ID and the child nodeID are the same then return false
        if (id == childID)
            return false;
        
        // if this childID is already in the parentID list then return false
        if (parentRoomNodeIDList.Contains(childID))
            return false;
        
        // if the child node already has a parent node then return false
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;
        
        // if this node id corridor and the child node is corridor as well then return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;
        
        // If child is not a corridor and this node is not a corridor return false
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;
        
        // If adding a corridor check that this node has < the maximum permitted child corridors
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        // if the child room is an entrance return false - the entrance must always be the top level parent node
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        // If adding a room to a corridor check that this corridor node doesn't already have a room added
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;

    }

    public bool AddParentRoomNodeIDRoomNodes(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

#endif

    #endregion Editor Code
}
