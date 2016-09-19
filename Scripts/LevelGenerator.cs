using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

	public int maxX = 10, maxY = 10;
	[Range(0, 100)]
	public int randomFillPercent = 50;
	public string seed;
	public bool useRandomSeed;
	[Range(2, 6)]
	public int wallsNeeded = 4;
	public float tileSize = 1;
	public int roomThresholdSize = 10, wallThresholdSize = 40;
	public int border = 10;

	int[,] map;

	// Use this for initialization
	void Start () {
		GenerateMap();
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.A)) {
			GenerateMap();
		}

		if (Input.GetKeyDown(KeyCode.S)) {
			SmoothMap();
			MeshGenerator meshGen = GetComponent<MeshGenerator>();
			meshGen.GenerateMesh(map, tileSize);
		}
	}

	void GenerateMap() {
		if (useRandomSeed) {
			seed = Time.time.ToString();
		}

		map = new int[maxX, maxY];
		System.Random pseudoGenerator = new System.Random(seed.GetHashCode());
		for (int x = 0; x < maxX; x++) {
			for (int y = 0; y < maxY; y++) {
				if (y == 0) {
					map[x, y] = 1;
				} else if (y == maxY - 1 || x == 0 || x == maxX - 1) {
					map[x, y] = 0;
				} else {
					map[x, y] = pseudoGenerator.Next(0, 100) < randomFillPercent ? 1 : 0;
				}
			}
		}

		for (int i = 0; i < 5; i++) {
			SmoothMap();
		}

		ProcessMap();

		int[,] borderedMap = new int[maxX, maxY + border];
		for (int x = 0; x < maxX; x++) {
			for (int y = 0; y < maxY + border; y++) {
				if (y < border) {
					borderedMap[x, y] = 1;
				} else {
					borderedMap[x, y] = map[x, y - border];
				}
			}
		}

		MeshGenerator meshGen = GetComponent<MeshGenerator>();
		meshGen.GenerateMesh(borderedMap, tileSize);
	}

	void SmoothMap() {
		for (int x = 0; x < maxX; x++) {
			for (int y = 0; y < maxY; y++) {
				int walls = GetSurroundingWallCount(x, y);
				if (walls > wallsNeeded) {
					map[x, y] = 1;
				} else if (walls < wallsNeeded) {
					map[x, y] = 0;
				}
			}
		}
	}

	int GetSurroundingWallCount(int x, int y) {
		int count = 0;
		for (int ix = x - 1; ix <= x + 1; ix++) {
			for (int iy = y - 1; iy <= y + 1; iy++) {
				if (!IsInMapRange(ix, iy)) {
					if (ix >= 0 || iy <= 1) {
						count++;
					}
					continue;
				}
				if (iy == y && ix == x) {
					continue;
				}
				count += map[ix, iy];
			}
		}
		return count;
	}

	void ProcessMap() {
		List<List<Coord>> wallRegions = GetRegions(1);

		foreach (List<Coord> wallRegion in wallRegions) {
			if (wallRegion.Count < wallThresholdSize) {
				foreach (Coord tile in wallRegion) {
					map[tile.tileX, tile.tileY] = 0;
				}
			}
		}

		List<List<Coord>> roomRegions = GetRegions(0);

		foreach (List<Coord> roomRegion in roomRegions) {
			if (roomRegion.Count < roomThresholdSize) {
				foreach (Coord tile in roomRegion) {
					map[tile.tileX, tile.tileY] = 1;
				}
			}
		}
	}

	List<List<Coord>> GetRegions(int tileType) {
		List<List<Coord>> regions = new List<List<Coord>>();
		int[,] mapFlags = new int[maxX, maxY];

		for (int x = 0; x < maxX; x++) {
			for (int y = 0; y < maxY; y++) {
				if (mapFlags[x,y] == 0 && map[x,y] == tileType) {
					List<Coord> newRegion = GetRegionTiles(x,y);
					regions.Add(newRegion);

					foreach (Coord tile in newRegion) {
						mapFlags[tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}
		return regions;
	}

	List<Coord> GetRegionTiles(int startX, int startY) {
		List<Coord> tiles = new List<Coord>();
		int[,] mapFlags = new int[maxX, maxY];
		int tileType = map[startX, startY];

		Queue<Coord> queue = new Queue<Coord>();
		queue.Enqueue(new Coord(startX, startY));
		mapFlags[startX, startY] = 1;

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();
			tiles.Add(tile);

			for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
				for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
					if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX)) {
						if (mapFlags[x,y] == 0 && map[x,y] == tileType) {
							mapFlags[x,y] = 1;
							queue.Enqueue(new Coord(x, y));
						}
					}
				}
			}
		}
		return tiles;
	}

	bool IsInMapRange(int x, int y) {
		return x >= 0 && x < maxX && y >= 0 && y < maxY;
	}

	void OnDrawGizmos() {
		// if (map != null) {
		// 	for (int x = 0; x < maxX; x++) {
		// 		for (int y = 0; y < maxY; y++) {
		// 			Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
		// 			Vector3 pos = new Vector3(x * tileSize, y * tileSize, 0);
		// 			Gizmos.DrawCube(pos + transform.position, Vector3.one * tileSize);
		// 		}
		// 	}
		// }
	}

	struct Coord {
		public int tileX, tileY;

		public Coord(int x, int y) {
			tileX = x;
			tileY = y;
		}
	}

}
