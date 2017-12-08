using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(WaterTerrainParse))]
public class WaterBuilder : MonoBehaviour {

	#region 其他脚本的引用
	public WaterTerrainParse terrainParse;
	#endregion

	#region 水的相关参数
	public float waterHeight = 12f;
	#endregion

	List<WaterShore> waterShoreList = new List<WaterShore>();
	List<Water> waterList = new List<Water>();

	public void BuildWater()
	{
//
//		TestData ();
//		return;
		terrainParse = gameObject.GetComponent<WaterTerrainParse> ();

		List<WaterShoreSegment> waterShoreSegment = terrainParse.GenerateWaterShoreSegmentList (waterHeight);
		if (waterShoreSegment == null) {
			Debug.LogError ("GenerateWaterShoreSegment return null");
			return;
		}

		waterShoreList = GenerateWaterShoreList (waterShoreSegment);

		CullSmallWaterShoreSegment (waterShoreList, 0.1f);

		waterList = GenerateWaters (waterShoreList);

		for (int i = 0; i < waterList.Count; ++i) {
			Mesh mesh = waterList [i].CreateMesh (transform.position);
			if (null == mesh)
				continue;
			GameObject child = new GameObject ();
			child.name = waterList [i].TransName;
			child.transform.SetParent (this.transform);
			child.transform.localPosition = Vector3.zero;
			MeshFilter filter = child.AddComponent<MeshFilter> ();
			MeshRenderer render = child.AddComponent<MeshRenderer> ();
			render.material = Resources.Load<Material> ("Material/WaterMaterial" + (i + 1).ToString());
			filter.mesh = mesh;
		}
	}

	private void CullSmallWaterShoreSegment(List<WaterShore> waterShoreList, float cullLength)
	{
		for (int i = 0; i < waterShoreList.Count; ++i) {
			waterShoreList [i].CullSmallShoreSegment (cullLength);
		}
	}

	private List<WaterShore> GenerateWaterShoreList(List<WaterShoreSegment> waterShoreSegmentList)
	{
		List<WaterShore> waterShoreList = new List<WaterShore> ();

		//先把零散的线段第一次刷选，处理成一一系列的WaterInfo
		for (int i = 0; i < waterShoreSegmentList.Count; ++i) {
			WaterShoreSegment waterShoreSegment = waterShoreSegmentList [i];
			bool hasFind = false;
			for (int j = 0; j < waterShoreList.Count; ++j) {
				WaterShore waterShore = waterShoreList [j];
				if(waterShore.JoinWaterShoreSegment(waterShoreSegment))
				{
					hasFind = true;
					break;
				}
			}
			if (!hasFind) {
				WaterShore waterInfo = new WaterShore (waterHeight);
				waterInfo.JoinWaterShoreSegment (waterShoreSegment);
				waterShoreList.Add (waterInfo);
			}
		}
			
		bool hasJoinSuccess = false;
		for (int i = 0; i < waterShoreList.Count; ++i) {
			//之所以j从0开始，因为A连接B失败，但是B连接A有可能成功,可以优化连接函数
			for (int j = 0; j < waterShoreList.Count; ++j) {
				if (i == j)
					continue;
				WaterShore waterShoreA = waterShoreList [i];
				WaterShore waterShoreB = waterShoreList [j];

				//可以为每一个WaterInfo设定唯一表示，并且拼接WaterLine后ID改变，在这里缓存拼接失败的ID对
				if (waterShoreA.JoinWaterShore (waterShoreB)) {
					hasJoinSuccess = true;
					waterShoreList.RemoveAt (j);
					break;
				}
			}

			//成功拼接后，waterInfos中的某一个数据已经无效，从新开始
			if (hasJoinSuccess) {
				i = -1;
				hasJoinSuccess = false;
			}
		}
		Debug.Log ("waterInfos.cout:  " + waterShoreList.Count);

		for (int i = 0; i < waterShoreList.Count; ++i) {
			waterShoreList [i].CheckError ();
		}
		return waterShoreList;
	}

	List<Water> GenerateWaters(List<WaterShore> waterShores)
	{
		//TODO 现在只是对大小进行了排序
		waterShores.Sort (delegate(WaterShore x, WaterShore y) {
			return x.EdgeSize.CompareTo(y.EdgeSize);
		});

		List<Water> ret = new List<Water> ();
		Water waitOutSideWater = null;
		for (int i = 0; i < waterShores.Count; ++i) {
			WaterShore waterShore = waterShores [i];

			bool isInsideWater = waterShore.IsInsideWater;
			if (waterShore.IsInvalidShore)
				continue;

			if (waterShore.IsInsideWater) {
				//水岸线的内圈是
				if (null != waitOutSideWater) {
					waitOutSideWater.SetOutsideShore (waterShore);
					ret.Add (waitOutSideWater);
					waitOutSideWater = null;
				} else {
					Water water = new Water ();
					water.SetSingleShore (waterShore);
					ret.Add (water);
				}
			} else {
				//水岸线外圈是水，需要等待到下一个包含自己并且内圈是水的
				waitOutSideWater = new Water();
				waitOutSideWater.SetInsideShore (waterShore);
			}
		}
		if (null != waitOutSideWater) {
			Debug.LogError ("发现没有匹配成功的");
			ret.Remove (waitOutSideWater);
		}
		Debug.Log ("水的数量 " + ret.Count);
		return ret;
	}

	void OnDrawGizmos()
	{
		return;
		//画所有的串联起来的边界线
		Color[] allColors = new Color[5];
		allColors [0] = Color.yellow;
		allColors [1] = Color.green;
		allColors [2] = Color.blue;
		allColors [3] = Color.black;

		for (int i = 0; i < waterList.Count; ++i) {
			Water water = waterList [i];
			Gizmos.color = allColors[i % allColors.Length];
			List<WaterShoreSegment> waterShoreSegments = water.GetAllWaterShoreSegment ();
			for (int j = 0; j < waterShoreSegments.Count; ++j) {
				WaterShoreSegment waterSHoreSegment = waterShoreSegments [j];
				Vector3 posOne = waterSHoreSegment.posOne;
				Vector3 posTwo = waterSHoreSegment.posTwo;

				posOne.y += 0f;
				posTwo.y += 0f;

				Gizmos.DrawLine (posOne, posTwo);
			}
		}
	}

	void TestData()
	{
		List<Vector3> pointList = new List<Vector3> ();
//		pointList.Add(new Vector3(0f, 0f, 0f));
//		pointList.Add(new Vector3(0f, 0f, 30f));
//		pointList.Add(new Vector3(20f, 0f, 30f));
//		pointList.Add(new Vector3(10f, 0f, 10f));
//		pointList.Add(new Vector3(30f, 0f, 10f));
//		pointList.Add(new Vector3(30f, 0f, 0f));
//		pointList.Reverse ();

		pointList.Add(new Vector3(40f, 0f, 40f));
		pointList.Add(new Vector3(40f, 0f, 50f));
		pointList.Add(new Vector3(10f, 0f, 50f));
		pointList.Add(new Vector3(10f, 0f, 20f));
		pointList.Add(new Vector3(20f, 0f, 10f));
		pointList.Add(new Vector3(60f, 0f, 10f));
		pointList.Add(new Vector3(60f, 0f, 50f));
		pointList.Add(new Vector3(40f, 0f, 50f));
		pointList.Add(new Vector3(40f, 0f, 40f));
		pointList.Add(new Vector3(50f, 0f, 40f));
		pointList.Add(new Vector3(50f, 0f, 20f));
		pointList.Add(new Vector3(20f, 0f, 20f));
		pointList.Add(new Vector3(20f, 0f, 40f));


		List<TriangleShape> triangleList = Utils.GenerateTriangles (pointList);

		Mesh mesh = TestCreateMesh (triangleList);
		if (null == mesh)
			return;
		GameObject child = new GameObject ();
		child.transform.SetParent (this.transform);
		child.transform.localPosition = Vector3.zero;
		MeshFilter filter = child.AddComponent<MeshFilter> ();
		MeshRenderer render = child.AddComponent<MeshRenderer> ();
		render.material = Resources.Load<Material> ("Material/WaterMaterial1");
		filter.mesh = mesh;
	}

	Mesh TestCreateMesh(List<TriangleShape> triangleList)
	{
		Mesh retMesh = new Mesh ();
		int trianglesCount = triangleList.Count;
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

		return retMesh;
	}

	Vector2 GetUV(float xMin, float xMax, float yMin, float yMax, Vector3 pos)
	{
		Vector2 ret;
		ret.x = (pos.x - xMin) / (xMax - xMin);
		ret.y = (pos.z - yMin) / (yMax - yMin);
		return ret;
	}
}
