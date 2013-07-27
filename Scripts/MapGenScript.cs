using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenScript : MonoBehaviour {
	
	public GameObject[] tileObjs;
	public int mapSize = 10;
	
	private int[][] mapGrid;
	private int[][][] lookupTable;
	private List<Vector2> openNodes;
	private List<Vector2> wallNodes;
	private float tileScale;
	private int index = 0;
	private int indexGrid = 0;
	private bool update = true;	
	
	// Use this for initialization
	void Start () {
		//Initialize everything as empty
		mapGrid = new int[mapSize][];
		lookupTable = new int[mapSize][][];
		for (int i = 0; i < mapSize; i++)
		{
			lookupTable[i] = new int[mapSize][];	
			mapGrid[i] = new int[mapSize];	
			for (int j = 0; j < mapSize; j++)
			{
				mapGrid[i][j] = Random.Range(0, 50);	
				lookupTable[i][j] = new int[] {0, 0};
			}
		}
		//more initializing...
		mapGrid[0][0] = 100;
		wallNodes = new List<Vector2>();
		openNodes = new List<Vector2>();
		openNodes.Add(new Vector2(0,0));
		lookupTable[0][0][0] = 1;
		
		// Make sure we have appropriate models
		if (tileObjs.Length < 5)
		{
			Debug.Log("Not enough models.");
			Debug.Break();
		}
		
		if (tileObjs[0] != null)
		{
			// First find a center for your bounds.
			Vector3 center = Vector3.zero;
			
			foreach (Transform child in tileObjs[0].transform)
			{
			    center += child.gameObject.renderer.bounds.center;   
			}
			center /= tileObjs[0].transform.childCount; //center is average center of children
			
			//Now you have a center, calculate the bounds by creating a zero sized 'Bounds', 
			Bounds bounds = new Bounds(center,Vector3.zero); 
			
			foreach (Transform child in tileObjs[0].transform)
			{
			    bounds.Encapsulate(child.gameObject.renderer.bounds);   
			}
			
			tileScale = bounds.size.x;
		}
		else
		{
			Debug.Log("No tile objects found.");	
			Debug.Break();
		}
	
		UpdateGrid();
	}
	
	private void UpdateGrid()
	{
		// start with the lowest node - it has the heightest weight so it will always
		// get replaced unless there are no more nodes that can be opened
		Vector2 lowestNode = openNodes[indexGrid];
		
		foreach (Vector2 node in openNodes)
		{
			// check node to the left
			if (node.x > 0 && mapGrid[(int)node.x - 1][(int)node.y] < mapGrid[(int)lowestNode.x][(int)lowestNode.y] && CanAdd(node.x - 1, node.y))
			{
				lowestNode = new Vector2(node.x - 1, node.y);
			}
			// check node below
			if (node.y > 0 && mapGrid[(int)node.x][(int)node.y - 1] < mapGrid[(int)lowestNode.x][(int)lowestNode.y] && CanAdd(node.x, node.y - 1))
			{
				lowestNode = new Vector2(node.x, node.y - 1);
			}
			// check node to the right
			if (node.x < mapSize - 1 && mapGrid[(int)node.x + 1][(int)node.y] < mapGrid[(int)lowestNode.x][(int)lowestNode.y] && CanAdd(node.x + 1, node.y))
			{
				lowestNode = new Vector2(node.x + 1, node.y);
			}
			// check node above
			if (node.y < mapSize - 1 && mapGrid[(int)node.x][(int)node.y + 1] < mapGrid[(int)lowestNode.x][(int)lowestNode.y] && CanAdd(node.x, node.y + 1))
			{
				lowestNode = new Vector2(node.x, node.y + 1);
			}
		}
		
		// this means we've considered all closed nodes and not selected any
		if (lowestNode == openNodes[0])
		{
			Debug.Log("Finished. Displaying map");
			for (int i = 0; i < mapSize; i++)
			{
				for (int j = 0; j < mapSize; j++)
				{
					if (lookupTable[i][j][0] == 0)
					{
						wallNodes.Add(new Vector2(i, j));	
					}
				}
			}
			DisplayGrid();
		}
		// keep searching for more nodes to open
		else
		{
			lookupTable[(int)lowestNode.x][(int)lowestNode.y][0] = 1;
			openNodes.Add(lowestNode);				
			UpdateGrid();
		}
	}
	
	// This will look at the nodes around a given node and decide how it should be 
	// rotated to align with the other modules.
	private int Rotation(float pX, float pY, int numSides)
	{
		// cast variables
		int x = (int)pX;
		int y = (int)pY;
		
		// handle the case based on number of open sides (i.e. sides that also contain a module)
		switch (numSides)
		{
			case 0:
			Debug.Log("I think this is an error...");
			Debug.Break();
			return 0;
			break;			
			case 1:
			if (y < mapSize - 1 && lookupTable[x][y+1][0] != 0)
				return 0;
			else if (x < mapSize - 1 && lookupTable[x+1][y][0] != 0)
				return 90;
			else if (y > 0 && lookupTable[x][y-1][0] != 0)
				return 180;
			else
				return 270;
			break;
			case 2:
			if (y < mapSize - 1 && lookupTable[x][y+1][0] != 0)
			{
				if (y > 0 && lookupTable[x][y-1][0] != 0)
				{
					lookupTable[x][y][0] = 5;
					return 0;
				}
				else
				{
					if (x > 0 && lookupTable[x-1][y][0] != 0)
						return -90;
					else
						return 0;
				}
			}
			else if (x < mapSize - 1 && lookupTable[x+1][y][0] != 0)
			{
				if (x > 0 && lookupTable[x-1][y][0] != 0)
				{
					lookupTable[x][y][0] = 5;
					return 90;
				}
				else
				{
					return 90;	
				}
			}				
			else if (y > 0 && lookupTable[x][y-1][0] != 0)
				return 180;
			else
				return 270;
			break;
			case 3:
			if (y == mapSize - 1 || (y < mapSize - 1 && lookupTable[x][y+1][0] == 0))
				return 90;
			else if (x == mapSize - 1 || (x < mapSize - 1 && lookupTable[x+1][y][0] == 0))
				return 180;
			else if (y == 0 || (y > 0 && lookupTable[x][y-1][0] == 0))
				return 270;
			else
				return 0;
			break;			
			case 4:
			default:			
				return 0;
			break;
		}
	}
	
	// Check now many sides are open (i.e. contain a module)
	private int NumOpenSides(float pX, float pY)
	{
		// cast variables
		int x = (int)pX;
		int y = (int)pY;
		
		int sideCount = 0;
		if (x > 0 && lookupTable[x - 1][y][0] != 0)
		{
			sideCount++;
		}				
		if (y > 0 && lookupTable[x][y - 1][0] != 0)
		{
			sideCount++;
		}				
		if (x < mapSize - 1 && lookupTable[x + 1][y][0] != 0)
		{
			sideCount++;
		}				
		if (y < mapSize - 1 && lookupTable[x][y + 1][0] != 0)
		{
			sideCount++;
		}		
		
		return sideCount;
	}
	
	// To be opened, a node must only have at most one side in use, and not already be selected
	private bool CanAdd(float pX, float pY)
	{
		return NumOpenSides(pX, pY) <= 1 && lookupTable[(int)pX][(int)pY][0] == 0;
	}
	
	private void DisplayGrid()
	{
		// First we update each node with its type (based on number of open sides) and rotation
		for (int i = 0; i < mapSize; i++)
		{
			for (int j = 0; j < mapSize; j++)
			{
				if (lookupTable[i][j][0] != 0)
				{
					int numSides = NumOpenSides(i, j);
					lookupTable[i][j][0] = numSides;
					lookupTable[i][j][1] = Rotation(i, j, numSides);
				}
			}
		}
		
		// Tell the game that we should now instantiate the objects
		update = true;
	}
	
	private void Update()
	{
		// We've instantiated them all...
		if (index >= openNodes.Count)
		{
			update = false;
		}
		
		// We should instantiate the next one
		if (update)
		{			
			// This will instantiate the next opened node in the next frame, that way we 
			// don't hang the thread by instantiating them all in one loop
			Vector3 point = openNodes[index];
			int x = (int)point.x;
			int y = (int)point.y;
			GameObject go = GameObject.Instantiate(tileObjs[lookupTable[x][y][0]-1], new Vector3(x, 0, y)
				* tileScale, Quaternion.identity) as GameObject;
			go.transform.eulerAngles = new Vector3(0, lookupTable[x][y][1]+90, 0);
			index++;			
		}
	}
}
