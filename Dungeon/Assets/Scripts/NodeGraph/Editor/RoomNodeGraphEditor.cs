using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.MPE;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    
    //node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;
    
    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]

    static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        //Define the node layout style
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    
        //load room node types
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }
    
    /// <summary>
    /// Open node graph editor window when a node graph editor scriptable
    /// object is double clicked in the inspector
    /// </summary>
    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        var roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (roomNodeGraph == null) return false;
        OpenWindow();
        currentRoomNodeGraph = roomNodeGraph;
        return true;
    }
    
    /// <summary>
    /// Draw editor Gui
    /// </summary>
    void OnGUI()
    {
        //If a scriptable object of type RoomNodeGraphSO has been selected then process
        if (currentRoomNodeGraph == null) return;
        //Process events
        ProcessEvents(Event.current);
        
        //Draw Room Nodes
        DrawRoomNodes();

        if (GUI.changed)
            Repaint();
    }

    private void ProcessEvents(Event currentEvent)
    {
        //Get room node that mouse is over if it's null or not currently being dragged 
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        
        //if mouse isn't over a room node
        if (currentRoomNode == null)
            ProcessRoomNodeGraphEvents(currentEvent);
        else
            currentRoomNode.ProcessEvents(currentEvent);
    }

    /// <summary>
    /// Check to see to mouse is over a room node. if so, return currentRoomNode else return null
    /// </summary>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (var i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Process room node graph events
    /// </summary>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            //Process Mouse Down Event
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            
            default:
                break;
        }
    }
    
    /// <summary>
    /// Process mouse down event on the room node graph (not over a node)
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //Process right click mouse down on graph event (show context menu)
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        
    }

    /// <summary>
    /// Show context menu
    /// </summary>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.ShowAsContext();
    }

    private void CreateRoomNode(object mousePositionObject)
    {
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }  

    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        var mousePosition = (Vector2) mousePositionObject;
        
        //create room node scriptable object asset
        var roomNode = CreateInstance<RoomNodeSO>();
        
        //add room node to current room node graph room node list
        currentRoomNodeGraph.roomNodeList.Add(roomNode);
        
        //set room node values
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph,
            roomNodeType);
        
        //add room node to room node graph scriptable object database
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        
        AssetDatabase.SaveAssets();
    }
    
    /// <summary>
    /// Draw the room node in the graph window
    /// </summary>
    private void DrawRoomNodes()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(roomNodeStyle);
        }

        GUI.changed = true;
    }
}
