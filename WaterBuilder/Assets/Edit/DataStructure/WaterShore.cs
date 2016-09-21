using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 水岸线
/// </summary>
public class WaterShore
{
	/// <summary>
	/// 在List中的前后两条线段依次相连
	/// </summary>
	private List<WaterShoreSegment> mWaterShoreSegmentList = new List<WaterShoreSegment> ();

	/// <summary>
	/// 把传入的线放入现有的链表中
	/// </summary>
	/// <returns><c>true</c>, if water line was joined, <c>false</c> otherwise.</returns>
	/// <param name="line">Line.</param>
	public bool  JoinWaterShoreSegment(WaterShoreSegment line)
	{
		if (mWaterShoreSegmentList.Count == 0) {
			mWaterShoreSegmentList.Add (line);
			return true;
		}
		bool needExchange = false;
		if (mWaterShoreSegmentList [0].CanJoinAtFront (line, out needExchange)) {
			if (needExchange) {
				line.ExchangePos ();
			}
			mWaterShoreSegmentList.Insert (0, line);
			return true;
		}
		if (mWaterShoreSegmentList [mWaterShoreSegmentList.Count - 1].CanJoinAtEnd (line, out needExchange)) {
			if (needExchange) {
				line.ExchangePos ();
			}
			mWaterShoreSegmentList.Add(line);
			return true;
		}
		return false;
	}

	/// <summary>
	/// 把传入的海岸线和当前的相连起来
	/// </summary>
	/// <returns><c>true</c>, if water shore was joined, <c>false</c> otherwise.</returns>
	/// <param name="waterShore">Water shore.</param>
	public bool JoinWaterShore(WaterShore waterShore)
	{
		if (mWaterShoreSegmentList.Count == 0) {
			Debug.LogError ("Water shore with no segment");
			return false;
		}
		if (waterShore.mWaterShoreSegmentList.Count == 0) {
			Debug.LogError ("Input Water shore with no segment");
			return false;
		}
		bool needExchange = false;
		if (mWaterShoreSegmentList [0].CanJoinAtFront (waterShore.mWaterShoreSegmentList [waterShore.mWaterShoreSegmentList.Count - 1], out needExchange)) {
			if (!needExchange) {
				JoinWaterShoreAtFront (waterShore);
				return true;
			}
		}

		if (mWaterShoreSegmentList [0].CanJoinAtFront (waterShore.mWaterShoreSegmentList [0], out needExchange)) {
			if (needExchange) {
				waterShore.RevertWaterLines ();
				JoinWaterShoreAtFront (waterShore);
				return true;
			}
		}

		if (mWaterShoreSegmentList [mWaterShoreSegmentList.Count - 1].CanJoinAtEnd (waterShore.mWaterShoreSegmentList [0], out needExchange)) {
			if (!needExchange) {
				JoinWaterShoreAtEnd (waterShore);
				return true;
			}
		}

		if (mWaterShoreSegmentList [mWaterShoreSegmentList.Count - 1].CanJoinAtEnd (waterShore.mWaterShoreSegmentList [waterShore.mWaterShoreSegmentList.Count - 1], out needExchange)) {
			if (needExchange) {
				waterShore.RevertWaterLines ();
				JoinWaterShoreAtEnd (waterShore);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 在前边插入新的水岸线
	/// </summary>
	/// <param name="waterShore">Water shore.</param>
	private void JoinWaterShoreAtFront(WaterShore waterShore)
	{
		List<WaterShoreSegment> waterShoreSegmentList = waterShore.mWaterShoreSegmentList;
		waterShoreSegmentList.AddRange (mWaterShoreSegmentList);
		mWaterShoreSegmentList = waterShoreSegmentList;
		waterShore.mWaterShoreSegmentList = null;
	}

	/// <summary>
	/// 在后边加入新的水岸线
	/// </summary>
	/// <param name="waterShore">Water shore.</param>
	private void JoinWaterShoreAtEnd(WaterShore waterShore)
	{
		mWaterShoreSegmentList.AddRange (waterShore.mWaterShoreSegmentList);
		waterShore.mWaterShoreSegmentList = null;
	}

	/// <summary>
	/// 获取水岸线中所有的线段
	/// </summary>
	/// <returns>The all water shore segment.</returns>
	public List<WaterShoreSegment> GetAllWaterShoreSegment()
	{
		return mWaterShoreSegmentList;
	}

	/// <summary>
	/// 倒叙水岸线的相连的线段
	/// </summary>
	private void RevertWaterLines()
	{
		mWaterShoreSegmentList.Reverse ();
		for (int i = 0; i < mWaterShoreSegmentList.Count; ++i) {
			mWaterShoreSegmentList [i].ExchangePos ();
		}
	}

	/// <summary>
	/// 检查是否有问题
	/// </summary>
	public void CheckError()
	{
		for (int i = 0; i < mWaterShoreSegmentList.Count - 1; ++i) {
			WaterShoreSegment curSegment = mWaterShoreSegmentList [i];
			WaterShoreSegment nextSegment = mWaterShoreSegmentList [i + 1];
			bool needExchange = false;
			if (curSegment.CanJoinAtEnd (nextSegment, out needExchange)) {
				if (!needExchange) {
					continue;
				}
			}
			if (Vector3.Distance (curSegment.posTwo, nextSegment.posOne) > Vector3.Distance (curSegment.posTwo, nextSegment.posTwo)) {
				Debug.LogError ("CheckError    1:  " + curSegment.ToString() + "   " + nextSegment.ToString());
			}
			Debug.LogError ("CheckError    2:  " + curSegment.ToString() + "   " + nextSegment.ToString());
		}
	}

	/// <summary>
	/// 裁减掉极小的不必要的线段
	/// </summary>
	/// <param name="waterLineLength">Water line length.</param>
	public void CullSmallShoreSegment(float waterLineLength)
	{
		if (mWaterShoreSegmentList.Count == 1)
			return;
		for (int i = mWaterShoreSegmentList.Count - 1; i >= 1; --i) {
			WaterShoreSegment curSegment = mWaterShoreSegmentList [i];
			WaterShoreSegment frontSegment = mWaterShoreSegmentList [i - 1];
			if (curSegment.GetLength () < waterLineLength) {
				mWaterShoreSegmentList.RemoveAt (i);
				frontSegment.posTwo = curSegment.posTwo;
			}
		}
	}
}