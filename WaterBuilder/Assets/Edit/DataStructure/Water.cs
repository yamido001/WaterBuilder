using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一片区域的水
/// </summary>
public class Water{

	static int WaterCount = 1;

	WaterShore mSingleShore;	//内圈是水
	WaterShore mInsideShore;	
	WaterShore mOutsideShore;

	List<TriangleShape> mAllTriangles = new List<TriangleShape> ();

	public int ID {
		get;
		private set;
	}

	public string TransName
	{
		get {
			if (null != mSingleShore)
				return ID.ToString () + mSingleShore.ID.ToString ();
			return ID.ToString () + " " + mInsideShore.ID.ToString () + " " + mOutsideShore.ID.ToString ();
		}
	}

	public Water()
	{
		ID = WaterCount++;
	}

	public void SetSingleShore(WaterShore shore)
	{
		mSingleShore = shore;
	}

	public void SetInsideShore(WaterShore shore)
	{
		mInsideShore = shore;
	}

	public void SetOutsideShore(WaterShore shore)
	{
		mOutsideShore = shore;
	}

	public Mesh CreateMesh(Vector3 worldPos)
	{
		if (null != mSingleShore) {
			return CreateInsideMesh (worldPos);
		} else if (null != mInsideShore && null != mOutsideShore) {
//			GeneralRingWaterTriangles ();
//			return CreateRingWaterMesh (worldPos);
			return CreateRingWaterMesh();
//			return null;
		}
		Debug.LogError ("生成水的模型失败,水岸线信息不合法");
		return null;
	}

	public List<WaterShoreSegment> GetAllWaterShoreSegment()
	{
		if (null != mSingleShore)
			return mSingleShore.GetAllWaterShoreSegment ();
		else if (null != mInsideShore && null != mOutsideShore) {
			var ret = mInsideShore.GetAllWaterShoreSegment ();
			ret.AddRange (mOutsideShore.GetAllWaterShoreSegment ());
			return ret;
		}
		return null;
	}

	public bool IsLineIntersection(Vector3 point1, Vector3 point2)
	{
		for (int i = 0; i < mAllTriangles.Count; ++i) {
			if (mAllTriangles [i].IsLineIntersect (point1, point2))
				return true;
		}
		return false;
	}

	#region 生成内圈的水
	Mesh CreateInsideMesh(Vector3 worldPos)
	{
		List<Vector3> allPoints = mSingleShore.GetAllShorePoint ();
		List<TriangleShape> allTriangles = Utils.GenerateTriangles (allPoints);
		return CreateMeshWithTrianglesList (allTriangles);
	}
	#endregion

	Mesh CreateMeshWithTrianglesList(List<TriangleShape> triangleList)
	{
		Mesh retMesh = new Mesh ();
		int trianglesCount = triangleList.Count;
		Vector4[] tangents = new Vector4[trianglesCount * 3];
		Vector3[] vertices = new Vector3[trianglesCount * 3];
		Vector2[] uv = new Vector2[trianglesCount * 3];
		Color[] colors = new Color[trianglesCount * 3];
		int[] triangles = new int[trianglesCount * 3];

		int trianglesIndex = 0;
		for (int i = 0; i < trianglesCount; ++i) {
			TriangleShape waterTriangle = triangleList [i];

			vertices [trianglesIndex * 3 + 0] = waterTriangle.PosArray [0];
			vertices [trianglesIndex * 3 + 1] = waterTriangle.PosArray [1];
			vertices [trianglesIndex * 3 + 2] = waterTriangle.PosArray [2];

			colors [trianglesIndex * 3 + 0] = Color.white;
			colors [trianglesIndex * 3 + 1] = Color.white;
			colors [trianglesIndex * 3 + 2] = Color.white;

			triangles [trianglesIndex * 3 + 0] = trianglesIndex * 3 + 0;
			triangles [trianglesIndex * 3 + 1] = trianglesIndex * 3 + 1;
			triangles [trianglesIndex * 3 + 2] = trianglesIndex * 3 + 2;
			trianglesIndex++;
		}

		Vector4 rangeSize = Utils.GetRangeSize (triangleList);
		for (int i = 0; i < uv.Length; ++i) {
			uv [i] = GetUV (rangeSize.x, rangeSize.y, rangeSize.z, rangeSize.w, vertices [i]);
		}

		retMesh.vertices = vertices;
		retMesh.uv = uv;
		retMesh.colors = colors;
		retMesh.triangles = triangles;

		for (int i = 0; i < tangents.Length; ++i) {
			tangents [i] = new Vector4 (1f, 0f, 0f, 1f);
		}
		retMesh.tangents = tangents;

//		System.Text.StringBuilder sbLog = new System.Text.StringBuilder ();
//		for (int i = 0; i < retMesh.tangents.Length; ++i) {
//			sbLog.Append (retMesh.tangents[i].ToString() + "\n");
//		}
//		Debug.LogError ("切线信息是: \n" + sbLog.ToString());
//
//		System.Text.StringBuilder sbNormalLog = new System.Text.StringBuilder ();
//		for (int i = 0; i < retMesh.normals.Length; ++i) {
//			sbNormalLog.Append (retMesh.normals[i].ToString() + "\n");
//		}
//		Debug.LogError ("法线信息是: \n" + sbNormalLog.ToString());

		return retMesh;
	}

	#region 生成环的水的第二版方法
	Mesh CreateRingWaterMesh()
	{
		List<Vector3> insidePoints = mInsideShore.GetAllShorePoint ();
		List<Vector3> outsidePoints = mOutsideShore.GetAllShorePoint ();

		List<TriangleShape> generateTriangles = new List<TriangleShape> ();

		List<Vector3> cullPointList1 = new List<Vector3> ();
		List<Vector3> cullPointList2 = new List<Vector3> ();

		for (int i = 0; i < insidePoints.Count; i++) {
			Vector3 curInsidePoint = insidePoints [i];
			int nextInsideIndex = Utils.GetRingNextIndex (i, insidePoints.Count, false);
			Vector3 nextInsidePoint = insidePoints[nextInsideIndex];
			bool hasFind = false;
			for (int j = 0; j < outsidePoints.Count; ++j) {
				Vector3 curOutSidePoint = outsidePoints [j];
				int nextOutsideIndex = Utils.GetRingNextIndex (j, outsidePoints.Count, false);
				Vector3 nextOutSidePoint = outsidePoints[nextOutsideIndex];
				if (mInsideShore.IsLineIntersection (curInsidePoint, curOutSidePoint))
					continue;
				if (mInsideShore.IsLineIntersection (nextInsidePoint, nextOutSidePoint))
					continue;
				if (mOutsideShore.IsLineIntersection (curInsidePoint, curOutSidePoint))
					continue;
				if (mOutsideShore.IsLineIntersection (nextInsidePoint, nextOutSidePoint))
					continue;

				//把这四个点单独生成一个多边形
				cullPointList1.Add (curInsidePoint);
				cullPointList1.Add (curOutSidePoint);
				cullPointList1.Add (nextOutSidePoint);
				cullPointList1.Add (nextInsidePoint);

				//把剩余所有的点单独生成多边形
				Utils.CopyList (outsidePoints, cullPointList2, nextOutsideIndex, false);
				Utils.CopyList (insidePoints, cullPointList2, i, true);
				hasFind = true;
				break;
			}
			if (hasFind)
				break;
		}
		generateTriangles.AddRange (Utils.GenerateTriangles (cullPointList1));
		generateTriangles.AddRange (Utils.GenerateTriangles (cullPointList2));
		return CreateMeshWithTrianglesList(generateTriangles);
	}
	#endregion

	#region 生成环状的水的第一版方法
	void GeneralRingWaterTriangles()
	{
		List<Vector3> insidePoints = mInsideShore.GetAllShorePoint ();
		List<Vector3> outsidePoints = mOutsideShore.GetAllShorePoint ();

		int outsideCurPointIndex = -1;
		//先找到外圈的起始点
		float curDisSqr = float.MaxValue;
		for (int j = 0; j < outsidePoints.Count; ++j) {
			Vector3 curInsidePoint = insidePoints[0];
			Vector3 outsidePoint = outsidePoints [j];
			float disSqr = (outsidePoint - curInsidePoint).sqrMagnitude;
			if (disSqr >= curDisSqr) {
				continue;
			}
			//和内圈相交，点不合法
			if (mInsideShore.IsLineIntersection (curInsidePoint, outsidePoint)) {
				continue;
			}
			curDisSqr = disSqr;
			outsideCurPointIndex = j;
		}
		if (outsideCurPointIndex < 0) {
			Debug.LogError ("未找到外圈的起始点");
			return;
		}

		System.Text.StringBuilder sbLog = new System.Text.StringBuilder ();

		UnityEditor.EditorUtility.DisplayProgressBar ("请稍后", "正在生成水的meish", 0f);

		for (int i = 0; i < insidePoints.Count; ++i) {
			Vector3 insidePoint = insidePoints [i];
			int compareCount = 0;
			UnityEditor.EditorUtility.DisplayProgressBar ("请稍后", "正在生成水的Mesh." + i + "/" + insidePoints.Count, (float)i / insidePoints.Count);

			bool isCurReverse = false;
			while (compareCount <= outsidePoints.Count + 1) {
				Vector3 outsidePoint = outsidePoints[outsideCurPointIndex];
				Vector3 outsideNextPoint =  outsidePoints[Utils.GetRingNextIndex(outsideCurPointIndex, outsidePoints.Count, isCurReverse)];

				outsideCurPointIndex = Utils.GetRingNextIndex (outsideCurPointIndex, outsidePoints.Count, isCurReverse);
				compareCount++;

				bool canInOutOutTriangles = true;	//一个内圈点和两个外圈点是否能组成三角形
				if (mInsideShore.IsLineIntersection (insidePoint, outsidePoint))
					canInOutOutTriangles = false;
				if (mInsideShore.IsLineIntersection (insidePoint, outsideNextPoint))
					canInOutOutTriangles = false;
				if (IsLineIntersection (insidePoint, outsidePoint))
					canInOutOutTriangles = false;
				if (IsLineIntersection (insidePoint, outsideNextPoint))
					canInOutOutTriangles = false;

				if (canInOutOutTriangles) {
					TriangleShape triangleObj = new TriangleShape (insidePoint, outsideNextPoint, outsidePoint);
					mAllTriangles.Add (triangleObj);
					sbLog.Append (triangleObj.ToString () + "\n");
				}

				bool canInInOutTriangles = true;	//两个内圈点和一个外圈点是否能组成三角形
				Vector3 insideNextPoint = (i == insidePoints.Count - 1) ? insidePoints[0] : insidePoints[i + 1];
				if (mInsideShore.IsLineIntersection (insidePoint, outsidePoint))
					canInInOutTriangles = false;
				if (mInsideShore.IsLineIntersection (insideNextPoint, outsidePoint))
					canInInOutTriangles = false;
				if (IsLineIntersection (insidePoint, outsidePoint))
					canInInOutTriangles = false;
				if (IsLineIntersection (insideNextPoint, outsidePoint))
					canInInOutTriangles = false;


				if (canInInOutTriangles) {
					TriangleShape triangleObj = new TriangleShape (insideNextPoint, insidePoint, outsidePoint);
					mAllTriangles.Add (triangleObj);
					sbLog.Append (triangleObj.ToString () + "\n");
				}
			}
		}
		UnityEditor.EditorUtility.ClearProgressBar ();
		Debug.LogError ("生成的三角面的信息是: \n" + sbLog.ToString());
	}

	Mesh CreateRingWaterMesh(Vector3 worldPos)
	{
		Mesh retMesh = new Mesh ();

		Vector3[] vertices = new Vector3[mAllTriangles.Count * 3];
		Vector2[] uv = new Vector2[mAllTriangles.Count * 3];
		Color[] colors = new Color[mAllTriangles.Count * 3];
		int[] triangles = new int[mAllTriangles.Count * 3];

		int trianglesIndex = 0;
		float xMin = float.MaxValue;
		float xMax = float.MinValue;
		float zMin = float.MaxValue;
		float zMax = float.MinValue;

		for (int i = 0; i < mAllTriangles.Count; ++i) {

			var triangleInfo = mAllTriangles [i];

			vertices [trianglesIndex * 3 + 0] = triangleInfo.PosArray [0];
			vertices [trianglesIndex * 3 + 1] = triangleInfo.PosArray [1];
			vertices [trianglesIndex * 3 + 2] = triangleInfo.PosArray [2];

			colors [trianglesIndex * 3 + 0] = Color.white;
			colors [trianglesIndex * 3 + 1] = Color.white;
			colors [trianglesIndex * 3 + 2] = Color.white;

			triangles [trianglesIndex * 3 + 0] = trianglesIndex * 3 + 0;
			triangles [trianglesIndex * 3 + 1] = trianglesIndex * 3 + 1;
			triangles [trianglesIndex * 3 + 2] = trianglesIndex * 3 + 2;
			trianglesIndex++;

			xMin = Utils.GetMinValue (xMin, triangleInfo.PosArray [0].x, triangleInfo.PosArray [1].x, triangleInfo.PosArray [2].x);
			xMax = Utils.GetMaxValue (xMax, triangleInfo.PosArray [0].x, triangleInfo.PosArray [1].x, triangleInfo.PosArray [2].x);
			zMin = Utils.GetMinValue (zMin, triangleInfo.PosArray [0].z, triangleInfo.PosArray [1].z, triangleInfo.PosArray [2].z);
			zMax = Utils.GetMaxValue (zMax, triangleInfo.PosArray [0].z, triangleInfo.PosArray [1].z, triangleInfo.PosArray [2].z);
		}

		for (int i = 0; i < uv.Length; ++i) {
			uv [i] = GetUV (xMin, xMax, zMin, zMax, vertices [i]);
		}

		retMesh.vertices = vertices;
		retMesh.uv = uv;
		retMesh.colors = colors;
		retMesh.triangles = triangles;
		return retMesh;
	}
	#endregion

	Vector2 GetUV(float xMin, float xMax, float yMin, float yMax, Vector3 pos)
	{
		Vector2 ret;
		ret.x = (pos.x - xMin) / (xMax - xMin);
		ret.y = (pos.z - yMin) / (yMax - yMin);
		return ret;
	}
}
