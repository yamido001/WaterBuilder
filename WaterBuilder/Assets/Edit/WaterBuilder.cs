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

	List<WaterShoreSegment> waterShoreSegment = new List<WaterShoreSegment>();
	List<WaterShore> waterShoreList = new List<WaterShore>();

	public void BuildWater()
	{
		Debug.Log ("BuildWater   " + waterHeight.ToString());

		terrainParse = gameObject.GetComponent<WaterTerrainParse> ();
		waterShoreSegment.Clear ();

		waterShoreSegment = terrainParse.GenerateWaterShoreSegmentList (waterHeight);
		if (waterShoreSegment == null) {
			Debug.LogError ("GenerateWaterShoreSegment return null");
			return;
		}

		waterShoreList = GenerateWaterShoreList (waterShoreSegment);

		CullSmallWaterShoreSegment (waterShoreList, 0.1f);
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
				WaterShore waterInfo = new WaterShore ();
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


	void OnDrawGizmos()
	{
		//画所有的串联起来的边界线
		Color[] allColors = new Color[5];
		allColors [0] = Color.yellow;
		allColors [1] = Color.green;
		allColors [2] = Color.blue;
		allColors [3] = Color.black;
		for (int j = 0; j < waterShoreList.Count; ++j) {
			WaterShore waterSHore = waterShoreList [j];
			Gizmos.color = allColors[j % allColors.Length];
			List<WaterShoreSegment> waterShoreSegments = waterSHore.GetAllWaterShoreSegment ();
			for (int i = 0; i < waterShoreSegments.Count; ++i) {
				WaterShoreSegment waterSHoreSegment = waterShoreSegments [i];
				Vector3 posOne = waterSHoreSegment.posOne;
				Vector3 posTwo = waterSHoreSegment.posTwo;

				posOne.y += 0f;
				posTwo.y += 0f;

				Gizmos.DrawLine (posOne, posTwo);
			}
		}
	}
}
