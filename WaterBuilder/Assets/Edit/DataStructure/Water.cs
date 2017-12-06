using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一片区域的水
/// </summary>
public class Water{

	public class TriangleObject
	{
		public Vector3 pos1;
		public Vector3 pos2;
		public Vector3 pos3;

		public TriangleObject(Vector3 pos1, Vector3 pos2, Vector3 pos3)
		{
			this.pos1 = pos1;
			this.pos2 = pos2;
			this.pos3 = pos3;
			SortPoints();
		}

		public bool IsLineIntersect(Vector3 linePos1, Vector3 linePos2)
		{
			if (linePos1.Equals (pos1) && linePos2.Equals (pos2))
				return false;
			if (linePos1.Equals (pos2) && linePos2.Equals (pos3))
				return false;
			if (linePos1.Equals (pos3) && linePos2.Equals (pos1))
				return false;
			if (Utils.IsLineIntersection (linePos1, linePos2, pos1, pos2))
				return true;
			if (Utils.IsLineIntersection (linePos1, linePos2, pos2, pos3))
				return true;
			if (Utils.IsLineIntersection (linePos1, linePos2, pos3, pos1))
				return true;
			return false;
		}

		protected void SortPoints()
		{
			Vector3 cross = Vector3.Cross (pos2 - pos1, pos3 - pos2);
			if (Vector3.Dot (cross, Vector3.up) < 0) {
				Vector3 temp = pos3;
				pos3 = pos2;
				pos2 = temp;
			}
		}

		public override string ToString ()
		{
			return string.Format ("({0} , {1}, {2}) ({3} , {4}, {5}) ({6} , {7}, {8}) ", pos1.x, pos1.y, pos1.z, pos2.x, pos2.y, pos2.z, pos3.x, pos3.y, pos3.z);
		}
	}

	static int WaterCount = 1;

	WaterShore mSingleShore;	//内圈是水
	WaterShore mInsideShore;	
	WaterShore mOutsideShore;

	List<TriangleObject> mAllTriangles = new List<TriangleObject> ();

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
			GeneralRingWaterTriangles ();
			return CreateRingWaterMesh (worldPos);
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
		List<WaterShoreSegment> segmentList = mSingleShore.GetAllWaterShoreSegment ();

		Vector3 centerPos = Vector3.zero;
		float xMin = float.MaxValue;
		float xMax = float.MinValue;
		float zMin = float.MaxValue;
		float zMax = float.MinValue;
		for (int i = 0; i < segmentList.Count; ++i) {
			WaterShoreSegment shoreSegment = segmentList [i];
			Vector3 posOne = shoreSegment.posOne - worldPos;
			Vector3 posTwo = shoreSegment.posTwo - worldPos;
			centerPos += posOne;
			centerPos += posTwo;
			xMin = Utils.GetMinValue (xMin, posOne.x, posTwo.x);
			xMax = Utils.GetMaxValue (xMax, posOne.x, posTwo.x);
			zMin = Utils.GetMinValue (zMin, posOne.z, posTwo.z);
			zMax = Utils.GetMaxValue (zMax, posOne.z, posTwo.z);
		}
		centerPos /= segmentList.Count * 2;
		Mesh retMesh = new Mesh ();
		int trianglesCount = segmentList.Count;
		Vector3[] vertices = new Vector3[trianglesCount * 3];
		Vector2[] uv = new Vector2[trianglesCount * 3];
		Color[] colors = new Color[trianglesCount * 3];
		int[] triangles = new int[trianglesCount * 3];

		int trianglesIndex = 0;
		for (int i = 0; i < trianglesCount; ++i) {
			WaterShoreSegment shoreSegment = segmentList [i];

			vertices [trianglesIndex * 3 + 0] = shoreSegment.posOne - worldPos;
			vertices [trianglesIndex * 3 + 1] = shoreSegment.posTwo - worldPos;
			vertices [trianglesIndex * 3 + 2] = centerPos;

			colors [trianglesIndex * 3 + 0] = Color.white;
			colors [trianglesIndex * 3 + 1] = Color.white;
			colors [trianglesIndex * 3 + 2] = Color.white;

			triangles [trianglesIndex * 3 + 0] = trianglesIndex * 3 + 2;
			triangles [trianglesIndex * 3 + 1] = trianglesIndex * 3 + 1;
			triangles [trianglesIndex * 3 + 2] = trianglesIndex * 3 + 0;
			trianglesIndex++;
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

	#region 生成环状的水

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
					TriangleObject triangleObj = new TriangleObject (insidePoint, outsideNextPoint, outsidePoint);
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
					TriangleObject triangleObj = new TriangleObject (insideNextPoint, insidePoint, outsidePoint);
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

			vertices [trianglesIndex * 3 + 0] = triangleInfo.pos1;
			vertices [trianglesIndex * 3 + 1] = triangleInfo.pos2;
			vertices [trianglesIndex * 3 + 2] = triangleInfo.pos3;

			colors [trianglesIndex * 3 + 0] = Color.white;
			colors [trianglesIndex * 3 + 1] = Color.white;
			colors [trianglesIndex * 3 + 2] = Color.white;

			triangles [trianglesIndex * 3 + 0] = trianglesIndex * 3 + 0;
			triangles [trianglesIndex * 3 + 1] = trianglesIndex * 3 + 1;
			triangles [trianglesIndex * 3 + 2] = trianglesIndex * 3 + 2;
			trianglesIndex++;

			xMin = Utils.GetMinValue (xMin, triangleInfo.pos1.x, triangleInfo.pos2.x, triangleInfo.pos3.x);
			xMax = Utils.GetMaxValue (xMax, triangleInfo.pos1.x, triangleInfo.pos2.x, triangleInfo.pos3.x);
			zMin = Utils.GetMinValue (zMin, triangleInfo.pos1.z, triangleInfo.pos2.z, triangleInfo.pos3.z);
			zMax = Utils.GetMaxValue (zMax, triangleInfo.pos1.z, triangleInfo.pos2.z, triangleInfo.pos3.z);
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
