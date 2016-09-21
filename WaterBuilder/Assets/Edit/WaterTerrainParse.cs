using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterTerrainParse : MonoBehaviour {

	public const string TestPlaneLayerName = "WaterTestPlane";
	private Terrain terrain;
	private TerrainData terrainData;
	private GameObject waterSurfacePlane;	//为了做射线判读三角面的边是否和水平面相交
	private float waterHeight;

	public List<WaterShoreSegment> GenerateWaterShoreSegmentList(float waterHeight)
	{
		terrain = Terrain.activeTerrain;
		if (terrain == null) {
			Debug.LogError ("Terrain is null");
			return null;
		}
		terrainData = terrain.terrainData;
		this.waterHeight = waterHeight;

		if (!CheckUsable ()) {
			return null;
		}
		BeforeRun ();
		List<WaterShoreSegment> ret = GetAllWaterShoreSegment ();
		EndRun ();
		return ret;
	}

	private bool CheckUsable()
	{
		//planeObj的检查和设置放在这里，是为了减少执行次数
		if (waterSurfacePlane == null) {
			waterSurfacePlane = GameObject.CreatePrimitive (PrimitiveType.Plane);
			waterSurfacePlane.transform.parent = transform;
			waterSurfacePlane.layer = LayerMask.NameToLayer (TestPlaneLayerName);
			GameObject.DestroyImmediate (waterSurfacePlane.GetComponent<MeshCollider> ());
			BoxCollider boxCollider = waterSurfacePlane.AddComponent<BoxCollider> ();
			boxCollider.size = new Vector3(boxCollider.size.x, 0f, boxCollider.size.z);
		}
		//每次生成水时先强制设置缩放和位置，避免因为误操作导致计算出错
		waterSurfacePlane.transform.localScale = new Vector3 (1000f, 1f, 1000f);
		waterSurfacePlane.transform.position = new Vector3(terrain.transform.position.x, terrain.transform.position.y + waterHeight, terrain.transform.position.z);

		return true;
	}

	private void BeforeRun()
	{
		waterSurfacePlane.SetActive (true);
	}

	private void EndRun()
	{
		waterSurfacePlane.SetActive (false);
	}

	private List<WaterShoreSegment> GetAllWaterShoreSegment()
	{
		List<WaterShoreSegment> waterShoreSegmentList = new List<WaterShoreSegment> ();
		float[,] heights = terrainData.GetHeights (0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

		for (int i = 0; i < terrainData.heightmapWidth - 1; ++i) {
			for (int j = 0; j < terrainData.heightmapHeight - 1; ++j) {
				//遍历每一个正方形，每一个正方形对应两个三角面,正方形顶点反向为
				//  1 ----- 2
				//  |       |
				//  |       |
				//  0 ----- 3
				Vector3 pos0 = GetWorldPosByTerrainGrid(heights, i, j);
				Vector3 pos1 = GetWorldPosByTerrainGrid(heights, i, j + 1);
				Vector3 pos2 = GetWorldPosByTerrainGrid(heights, i + 1, j + 1);
				Vector3 pos3 = GetWorldPosByTerrainGrid(heights, i + 1, j);

				//第一个三角面三个顶点为0，1，2；
				WaterShoreSegment waterShoreSegment = GetWaterShoreSegment(pos0, pos1, pos2);
				if (waterShoreSegment != null) {
					waterShoreSegmentList.Add (waterShoreSegment);
				}
				//第二个三角面三个顶点为0，2，3
				waterShoreSegment = GetWaterShoreSegment(pos0, pos2, pos3);
				if (waterShoreSegment != null) {
					waterShoreSegmentList.Add (waterShoreSegment);
				}
			}
		}
		return waterShoreSegmentList;
	}

	/// <summary>
	/// 获取传入的三个顶点组成的三角面和水平面相交的线段
	/// </summary>
	/// <returns>The water shore segment.</returns>
	/// <param name="pos1">Pos1.</param>
	/// <param name="pos2">Pos2.</param>
	/// <param name="pos3">Pos3.</param>
	private WaterShoreSegment GetWaterShoreSegment(Vector3 pos1, Vector3 pos2, Vector3 pos3)
	{
		//判断每一个边是否和水平面相交
		Vector3[] hitPoss = new Vector3[3];
		int curHitIndex = 0;
		if (IsLineIntersectWater (pos1, pos2, out hitPoss[curHitIndex])) {
			++curHitIndex;
		}
		if (IsLineIntersectWater (pos2, pos3, out hitPoss[curHitIndex])) {
			++curHitIndex;
		}
		if (IsLineIntersectWater (pos3, pos1, out hitPoss[curHitIndex])) {
			++curHitIndex;
		}
		//三条边中只有有两个交点才代表三角面和水平面相交
		if (curHitIndex == 2) {
			WaterShoreSegment line = new WaterShoreSegment ();
			line.posOne = hitPoss [0];
			line.posTwo = hitPoss [1];
			return line;
		}
		return null;
	}

	/// <summary>
	/// 线段是否和水平面相交
	/// </summary>
	/// <returns><c>true</c> if this instance is line tangent water the specified pos1 pos2 hitPos; otherwise, <c>false</c>.</returns>
	/// <param name="pos1">Pos1.</param>
	/// <param name="pos2">Pos2.</param>
	/// <param name="hitPos">Hit position.</param>
	private bool IsLineIntersectWater(Vector3 pos1, Vector3 pos2, out Vector3 hitPos)
	{
		hitPos = Vector3.zero;
		RaycastHit hit;
		LayerMask mask = 1 << LayerMask.NameToLayer (TestPlaneLayerName);
		if (Physics.Raycast (pos1, pos2 - pos1, out hit, 1000f, mask.value)) {
			hitPos = hit.point;

			Vector3 dir1 = hitPos - pos1;
			Vector3 dir2 = pos2 - hitPos;
			//只有碰撞的点必须在线段中
			if (Vector3.Dot (dir1, dir2) >= 0) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 根据山体高度图的网格坐标计算出山体表现在世界空间的坐标
	/// </summary>
	/// <returns>The world position by terrain grid.</returns>
	/// <param name="heights">Heights.</param>
	/// <param name="i">The index.</param>
	/// <param name="j">J.</param>
	private Vector3 GetWorldPosByTerrainGrid(float[,] terrainHeights, int i, int j)
	{
		return Vector3.Scale (new Vector3 (i, terrainHeights[j,i], j), terrainData.heightmapScale);
	}
}
