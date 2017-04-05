// Raquel Lippincott - 2016
// C# in Unity

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidpointDisplacement : MonoBehaviour {

	public Terrain MyTerrain; // Defined in Unity by user

	private int hmWidth;
	private int depth = 0;
	private float[,] HeightMap = new float[1, 1];

	public enum MapSizes {extraSmall, small, medium, large, extraLarge, giant};
	public MapSizes mapSize; // Defined in Unity by user

	public float startHeight; // Defined in Unity by user
	public float roughness;	// Defined in Unity by user
	  

	private void Initialize() {
		if (mapSize == MapSizes.extraSmall)
			MyTerrain.terrainData.size.Set (257, 257, 257);
		else if (mapSize == MapSizes.small)
			MyTerrain.terrainData.size.Set (513, 513, 513);
		else if (mapSize == MapSizes.medium)
			MyTerrain.terrainData.size.Set (1025, 1025, 1025);
		else if (mapSize == MapSizes.large)
			MyTerrain.terrainData.size.Set (2049, 2049, 2049);
		else if (mapSize == MapSizes.extraLarge)
			MyTerrain.terrainData.size.Set (4097, 4097, 4097);
		else
			MyTerrain.terrainData.size.Set (8193, 8193, 8193);
			
		HeightMap = new float[MyTerrain.terrainData.heightmapWidth, MyTerrain.terrainData.heightmapHeight];
		hmWidth = MyTerrain.terrainData.heightmapWidth;
	}


	// This function takes in a method and calls the corresponding algorithm to create a heightmap.
	private void CreateHM () {

		// Initialize topleft
		int topleft_x = 0;
		int topleft_y = 0;

		// Initialize the four corners of the height map.
		HeightMap [0, 0] = startHeight;
		HeightMap [0, hmWidth - 1] = startHeight;
		HeightMap [hmWidth - 1, 0] = startHeight;
		HeightMap [hmWidth - 1, hmWidth - 1] = startHeight;

		// Call the recursive midpoint-displacement function
		RecursiveMidpointDisplacement (topleft_x, topleft_y, hmWidth, 0);
	}


	private float MidEdgeVal(int corner1_x, int corner1_y, int corner2_x, int corner2_y, float centerHeight, int x) {

		float corner1Height = HeightMap [corner1_x, corner1_y];
		float corner2Height = HeightMap [corner2_x, corner2_y];

		return (corner1Height + corner2Height )/ 2.0f;
	}


	// Recursive implements the midpoint-displacement algorithm.
	private void RecursiveMidpointDisplacement (int topleft_x, int topleft_y, int hmWidth, int x) {

		if (hmWidth == 1) {
			depth = x;
			return;
		}

		// Assume corners are already set. Next steps: set center edges and set center value

		//Set Center of Square
		if (x == 1) {
			HeightMap [topleft_x + (hmWidth - 1) / 2, topleft_y + (hmWidth - 1) / 2] = 
				(HeightMap [topleft_x, topleft_y] + HeightMap [topleft_x + hmWidth - 1, topleft_y] +
				HeightMap [topleft_x, topleft_y + hmWidth - 1] + HeightMap [topleft_x + hmWidth - 1, topleft_y + hmWidth - 1]) / 4.0f
				+ Random.Range (0.03f,  (hmWidth/800.0f) * roughness);
		}
		if (x == 2) {
			HeightMap [topleft_x + (hmWidth - 1) / 2, topleft_y + (hmWidth - 1) / 2] = 
				(HeightMap [topleft_x, topleft_y] + HeightMap [topleft_x + hmWidth - 1, topleft_y] +
					HeightMap [topleft_x, topleft_y + hmWidth - 1] + HeightMap [topleft_x + hmWidth - 1, topleft_y + hmWidth - 1]) / 4.0f
				+ Random.Range (0.01f,  (hmWidth/600.0f) * roughness);
		}
		else if (x < 9)
		{
			HeightMap [topleft_x + (hmWidth - 1) / 2, topleft_y + (hmWidth - 1) / 2] = 
				(HeightMap [topleft_x, topleft_y] + HeightMap [topleft_x + hmWidth - 1, topleft_y] +
					HeightMap [topleft_x, topleft_y + hmWidth - 1] + HeightMap [topleft_x + hmWidth - 1, topleft_y + hmWidth - 1]) / 4.0f
				+ Random.Range (-1.0f  * (hmWidth/800.0f) * roughness,  (hmWidth/800.0f) * roughness);
		}
		else {
			HeightMap [topleft_x + (hmWidth - 1) / 2, topleft_y + (hmWidth - 1) / 2] = 
			(HeightMap [topleft_x, topleft_y] + HeightMap [topleft_x + hmWidth - 1, topleft_y] +
				HeightMap [topleft_x, topleft_y + hmWidth - 1] + HeightMap [topleft_x + hmWidth - 1, topleft_y + hmWidth - 1]) / 4.0f;
		}

		// Set Center of Edges
		// Edge 1: Between top-left and top-right
		HeightMap[topleft_x + (hmWidth-1)/2, topleft_y] = 
			MidEdgeVal(topleft_x, topleft_y, topleft_x + hmWidth - 1, topleft_y, HeightMap[topleft_x + (hmWidth-1)/2, topleft_y + (hmWidth-1)/2], x);

		// Edge 2: Between top-right and bottom-right
		HeightMap [topleft_x + hmWidth - 1, topleft_y + (hmWidth-1)/2] = 
			MidEdgeVal (topleft_x + hmWidth - 1, topleft_y, topleft_x + hmWidth - 1, topleft_y + hmWidth - 1, HeightMap[topleft_x + (hmWidth-1)/2, topleft_y + (hmWidth-1)/2], x);

		// Edge 3: Between bottom-right and bottom-left
		HeightMap [topleft_x + (hmWidth-1)/2, topleft_y + hmWidth - 1] = 
			MidEdgeVal (topleft_x + hmWidth - 1, topleft_y + hmWidth - 1, topleft_x, topleft_y + hmWidth - 1, HeightMap[topleft_x + (hmWidth-1)/2, topleft_y + (hmWidth-1)/2], x);

		// Edge 4: Between bottom-left and top-left
		HeightMap [topleft_x, topleft_y + (hmWidth-1)/2] = 
			MidEdgeVal (topleft_x, topleft_y + hmWidth - 1, topleft_x, topleft_y, HeightMap[topleft_x + (hmWidth-1)/2, topleft_y + (hmWidth-1)/2], x);

		// Set new width
		hmWidth = (hmWidth-1)/2 + 1;

		// Recursively call function with new topleft values and widths
		RecursiveMidpointDisplacement(topleft_x, topleft_y, hmWidth, x+1);
		RecursiveMidpointDisplacement(topleft_x, topleft_y + hmWidth - 1, hmWidth, x+1);
		RecursiveMidpointDisplacement(topleft_x + hmWidth - 1, topleft_y, hmWidth, x+1);
		RecursiveMidpointDisplacement(topleft_x + hmWidth - 1, topleft_y + hmWidth - 1, hmWidth, x+1);

		if(x>2)
			roughness = roughness * 0.99999f;
	}


	// Use this for initialization
	void Start () {
		Initialize ();
		CreateHM ();
		MyTerrain.terrainData.SetHeights (0, 0, HeightMap);
		print ("x = " + depth);
	}
}