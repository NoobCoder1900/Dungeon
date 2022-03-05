using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id; //GUID
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code

#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;
     
    /// <summary>
    /// Initialise node
    /// </summary>
    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;
        
        //load room node type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Draw nodes with nodestyle
    /// </summary>
    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);
        
        EditorGUI.BeginChangeCheck();

        var selected = roomNodeTypeList.list.FindIndex((x => x == roomNodeType));

        var selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesDisplay());

        roomNodeType = roomNodeTypeList.list[selection];

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
            default:
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
    }

    private void ProcessLeftClickDownEvent()
    {
       Selection.activeObject = this;
        
        //Toggle node selection
        isSelected = !isSelected;
    }


#endif

    #endregion Editor Code
}
