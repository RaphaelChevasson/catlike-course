using UnityEngine;

public class SkylineGenerator : MonoBehaviour
{
	const float border = 10f;

	[SerializeField]
	SkylineObject[] prefabs;

	[SerializeField]
	SkylineObject gapPrefab;

	[SerializeField]
	float distance;

	[SerializeField]
	FloatRange altitude;

	[SerializeField]
	FloatRange gapLength, sequenceLength;

	[SerializeField]
	float maxYDifference;

	[SerializeField]
	bool singleSequenceStart;

	float sequenceEndX;

	Vector3 endPosition;

	SkylineObject leftmost, rightmost;

	public void FillView (
		TrackingCamera view, float extraGapLength = 0f, float extraSequenceLength = 0f
	)
	{
		FloatRange visibleX = view.VisibleX(distance).GrowExtents(border);
		while (leftmost != rightmost && leftmost.MaxX < visibleX.min)
		{
			leftmost = leftmost.Recycle();
		}

		while (endPosition.x < visibleX.max)
		{
			if (endPosition.x > sequenceEndX)
			{
				StartNewSequence(
					gapLength.RandomValue + extraGapLength,
					sequenceLength.RandomValue + extraSequenceLength
				);
			}
			rightmost = rightmost.Next = GetInstance();
			endPosition = rightmost.PlaceAfter(endPosition);
		}
	}

	public SkylineObject StartNewGame (TrackingCamera view)
	{
		while (leftmost != null)
		{
			leftmost = leftmost.Recycle();
		}

		FloatRange visibleX = view.VisibleX(distance).GrowExtents(border);
		endPosition = new Vector3(visibleX.min, altitude.RandomValue, distance);
		sequenceEndX = singleSequenceStart ?
			visibleX.max : endPosition.x + sequenceLength.RandomValue;
		leftmost = rightmost = GetInstance();
		endPosition = rightmost.PlaceAfter(endPosition);
		FillView(view);
		return leftmost;
	}

	void StartNewSequence (float gap, float sequence)
	{
		if (gapPrefab != null)
		{
			rightmost = rightmost.Next = gapPrefab.GetInstance();
			rightmost.transform.SetParent(transform, false);
			rightmost.FillGap(endPosition, gap);
		}
		endPosition.x += gap;
		sequenceEndX = endPosition.x + sequence;

		if (maxYDifference > 0f)
		{
			endPosition.y = new FloatRange(
				Mathf.Max(endPosition.y - maxYDifference, altitude.min),
				Mathf.Min(endPosition.y + maxYDifference, altitude.max)
			).RandomValue;
		}
		else
		{
			endPosition.y = altitude.RandomValue;
		}
	}

	SkylineObject GetInstance ()
	{
		SkylineObject instance = prefabs[Random.Range(0, prefabs.Length)].GetInstance();
		instance.transform.SetParent(transform, false);
		return instance;
	}
}